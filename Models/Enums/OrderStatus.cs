// FILE: Models/Enums/OrderStatus.cs
// PURPOSE: The single place "what are the valid order states" is defined,
//          instead of loose strings scattered through the code
//          (04-BACKEND-ARCHITECTURE.md section 3). Matches the state
//          diagram in 07-REALTIME-ORDER-FLOW.md section 4 exactly:
//
//              Pending -> InPreparation -> Ready -> Delivered
//              Pending -> Cancelled
//              InPreparation -> Cancelled
//
// LOGIC:   Data/AppDbContext.cs stores this as TEXT in the database
//          (HasConversion<string>()) so the enum name IS the stored value
//          and IS the JSON value the frontend already expects
//          ("Pending", "InPreparation", "Ready", "Delivered", "Cancelled" -
//          see kitchen/js/kitchen.js's STATUS_META keys). Services/OrderService.cs
//          is the one place that enforces which transitions are legal.

namespace ScanEat.Api.Models.Enums;

public enum OrderStatus
{
    Pending,
    InPreparation,
    Ready,
    Delivered,
    Cancelled
}
