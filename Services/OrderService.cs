// FILE: Services/OrderService.cs
// PURPOSE: The core business logic of the whole system
//          (09-ROADMAP.md step 6: "the most important single piece of the
//          backend"). Implements the full request lifecycle traced in
//          04-BACKEND-ARCHITECTURE.md section 4 and the sequence diagram in
//          07-REALTIME-ORDER-FLOW.md section 3.
// LOGIC:   - CreateOrderAsync NEVER trusts a price, name, or table sent by
//            the browser: the table is looked up by its QrToken and must be
//            active; every item's real Product/Offer is re-read from the
//            database and its CURRENT price is what gets charged
//            (02-SYSTEM-ARCHITECTURE.md section 5). ItemNameSnapshot +
//            UnitPrice are then copied onto the OrderItem so this moment's
//            price/name survive even if the Product/Offer changes later.
//          - Tax is a flat 14% of Subtotal, matching the VAT rate already
//            hardcoded in the customer app's own updateTotals()
//            (customer/js/app.js) - kept identical so a customer's cart
//            preview and the final charged total always agree.
//          - UpdateStatusAsync enforces the exact state machine from
//            07-REALTIME-ORDER-FLOW.md section 4 (Pending -> InPreparation
//            -> Ready -> Delivered, with Cancelled reachable from the first
//            two) - an invalid jump (e.g. Pending straight to Delivered) is
//            rejected with a clear message instead of silently applied.
//          - Every state-changing method broadcasts over OrderHub
//            immediately after saving - "the moment the order is saved, it's
//            broadcast" (07-REALTIME-ORDER-FLOW.md section 4) - using the
//            exact event names and argument shapes
//            kitchen/js/signalr-client.js already listens for:
//            NewOrder(order) and OrderStatusChanged(orderId, newStatus) as
//            two separate arguments, not one combined object.

using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ScanEat.Api.Data;
using ScanEat.Api.Helpers;
using ScanEat.Api.Hubs;
using ScanEat.Api.Models.DTOs;
using ScanEat.Api.Models.Entities;
using ScanEat.Api.Models.Enums;

namespace ScanEat.Api.Services;

public class OrderService : IOrderService
{
    private const decimal VatRate = 0.14m;

    // Legal forward transitions - see class header comment and
    // 07-REALTIME-ORDER-FLOW.md section 4's state diagram.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> ValidTransitions = new()
    {
        [OrderStatus.Pending] = new[] { OrderStatus.InPreparation, OrderStatus.Cancelled },
        [OrderStatus.InPreparation] = new[] { OrderStatus.Ready, OrderStatus.Cancelled },
        [OrderStatus.Ready] = new[] { OrderStatus.Delivered },
        [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
    };

    private readonly AppDbContext _db;
    private readonly IHubContext<OrderHub> _hub;

    public OrderService(AppDbContext db, IHubContext<OrderHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var table = await _db.RestaurantTables.FirstOrDefaultAsync(t => t.QrToken == dto.TableToken);
        if (table is null || !table.IsActive)
        {
            // Deliberately vague about WHICH problem it is (missing vs.
            // inactive) - both mean the same thing to the customer: scan the
            // real QR code on the table again. See
            // 06-QR-CODE-AND-TABLE-FLOW.md section 5's two-step check.
            throw ApiException.BadRequest(
                "This table link isn't valid or is no longer active. Please scan the QR code on your table again.");
        }

        if (dto.Items.Count == 0)
        {
            throw ApiException.BadRequest("Your order has no items.");
        }

        var orderItems = new List<OrderItem>();
        decimal subtotal = 0m;

        foreach (var line in dto.Items)
        {
            if (line.Quantity < 1)
            {
                throw ApiException.BadRequest("Item quantity must be at least 1.");
            }

            var type = line.Type.Trim().ToLowerInvariant();
            var notes = string.IsNullOrWhiteSpace(line.Notes) ? null : line.Notes.Trim();

            if (type == "product")
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == line.Id);
                if (product is null)
                {
                    throw ApiException.BadRequest($"Product #{line.Id} was not found.");
                }
                if (!product.IsAvailable)
                {
                    throw ApiException.BadRequest($"'{product.Name}' is currently sold out.");
                }

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ItemNameSnapshot = product.Name,
                    UnitPrice = product.Price, // server-truth price - never trusts the browser
                    Quantity = line.Quantity,
                    Notes = notes,
                });
                subtotal += product.Price * line.Quantity;
            }
            else if (type == "offer")
            {
                var offer = await _db.Offers.Include(o => o.OfferItems).ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == line.Id);
                if (offer is null)
                {
                    throw ApiException.BadRequest($"Offer #{line.Id} was not found.");
                }
                if (!offer.IsActive)
                {
                    throw ApiException.BadRequest($"'{offer.Title}' is no longer available.");
                }

                if (offer.OfferItems.Any(item => item.Product?.IsAvailable != true))
                {
                    throw ApiException.BadRequest($"An item in '{offer.Title}' is currently sold out.");
                }
                var components = offer.OfferItems.Select(item =>
                    new OrderComponentDto(item.Product!.Name, (long)item.Quantity * line.Quantity));
                orderItems.Add(new OrderItem
                {
                    ComponentsJson = JsonSerializer.Serialize(components),
                    OfferId = offer.Id,
                    ItemNameSnapshot = offer.Title,
                    UnitPrice = offer.NewPrice, // server-truth price - never trusts the browser
                    Quantity = line.Quantity,
                    Notes = notes,
                });
                subtotal += offer.NewPrice * line.Quantity;
            }
            else
            {
                throw ApiException.BadRequest($"Unknown item type '{line.Type}'. Expected 'product' or 'offer'.");
            }
        }

        var tax = Math.Round(subtotal * VatRate, 2, MidpointRounding.AwayFromZero);
        var total = subtotal + tax;
        var now = DateTime.UtcNow;

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.Pending,
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            CreatedAt = now,
            UpdatedAt = now,
            Items = orderItems,
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var detail = MapToDetailDto(order, table.TableNumber);
        await _hub.Clients.All.SendAsync("NewOrder", detail);

        return new OrderResponseDto
        {
            OrderId = order.Id,
            Status = order.Status.ToString(),
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
        };
    }

    public async Task<List<OrderDetailDto>> GetOrdersAsync(string? statusCsv)
    {
        IQueryable<Order> query = _db.Orders.Include(o => o.Table).Include(o => o.Items);

        var requestedStatuses = ParseStatusCsv(statusCsv);
        query = requestedStatuses.Count > 0
            ? query.Where(o => requestedStatuses.Contains(o.Status))
            // No/empty filter: default to every order still "in flight" -
            // matches what a Kitchen Display page load needs.
            : query.Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled);

        var orders = await query.OrderBy(o => o.CreatedAt).ToListAsync();
        return orders.Select(o => MapToDetailDto(o, o.Table!.TableNumber)).ToList();
    }

    public async Task<OrderDetailDto> UpdateStatusAsync(int orderId, string newStatusRaw)
    {
        if (!Enum.TryParse<OrderStatus>(newStatusRaw, ignoreCase: true, out var newStatus))
        {
            throw ApiException.BadRequest($"'{newStatusRaw}' is not a recognized order status.");
        }

        var order = await _db.Orders.Include(o => o.Table).Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw ApiException.NotFound($"Order #{orderId} was not found.");

        if (!ValidTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(newStatus))
        {
            throw ApiException.BadRequest($"Cannot move an order from '{order.Status}' to '{newStatus}'.");
        }

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Two separate positional arguments - matches
        // kitchen/js/signalr-client.js's
        // `connection.on("OrderStatusChanged", (orderId, newStatus) => ...)`.
        await _hub.Clients.All.SendAsync("OrderStatusChanged", order.Id, order.Status.ToString());

        return MapToDetailDto(order, order.Table!.TableNumber);
    }

    private static List<OrderStatus> ParseStatusCsv(string? statusCsv)
    {
        if (string.IsNullOrWhiteSpace(statusCsv))
        {
            return new List<OrderStatus>();
        }

        return statusCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Enum.TryParse<OrderStatus>(s, ignoreCase: true, out var parsed) ? (OrderStatus?)parsed : null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();
    }

    // Maps an Order (with Items loaded) to the shape both
    // GET /api/staff/orders and the NewOrder broadcast use - see
    // OrderDetailDto.cs header comment for why these two must never drift apart.
    private static OrderDetailDto MapToDetailDto(Order order, string tableNumber) => new()
    {
        Id = order.Id,
        TableNumber = tableNumber,
        Status = order.Status.ToString(),
        Items = order.Items.Select(i => new OrderDetailItemDto
        {
            Quantity = i.Quantity,
            ItemNameSnapshot = i.ItemNameSnapshot,
            UnitPrice = i.UnitPrice,
            Notes = i.Notes,
            Components = i.ComponentsJson is null ? [] :
                JsonSerializer.Deserialize<List<OrderComponentDto>>(i.ComponentsJson) ?? [],
        }).ToList(),
        Subtotal = order.Subtotal,
        Tax = order.Tax,
        Total = order.Total,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
    };
}
