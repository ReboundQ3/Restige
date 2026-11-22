using Content.Shared.Inventory;

namespace Content.Shared._Trauma.Viewcone;

/// <summary>
/// Relayed through the feet inventory slot to allow worn clothing to cancel
/// the viewcone footstep effect, e.g. silent shoes.
/// </summary>
[ByRefEvent]
public record struct CanSpawnFootstepsEvent(bool Cancelled = false) : IInventoryRelayEvent
{
    readonly SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.FEET;
}
