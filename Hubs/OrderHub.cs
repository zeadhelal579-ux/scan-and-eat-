// FILE: Hubs/OrderHub.cs
// PURPOSE: SignalR's real-time layer (04-BACKEND-ARCHITECTURE.md section 3),
//          mapped at "/hubs/orders" in Program.cs. This is what the Kitchen
//          Display connects to; OrderService is what tells it when to
//          broadcast. Full contract in 07-REALTIME-ORDER-FLOW.md section 2.
// LOGIC:   - [Authorize] on the whole hub: only staff (Kitchen and Admin)
//            connect to it - customers never open a live connection in
//            Phase 1. Program.cs configures the JWT bearer handler to also
//            accept the token from an "access_token" query string
//            parameter specifically for requests under /hubs, because
//            browsers cannot attach a custom Authorization header to the
//            WebSocket handshake SignalR's JS client uses - this is exactly
//            what kitchen/js/signalr-client.js's
//            `accessTokenFactory: () => Auth.getToken()` relies on.
//          - The hub class itself has no custom methods: every broadcast
//            (NewOrder, OrderStatusChanged) is server-initiated from
//            OrderService via IHubContext<OrderHub>, never called by a
//            client - so there is nothing for clients to invoke here.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ScanEat.Api.Hubs;

[Authorize]
public class OrderHub : Hub
{
}
