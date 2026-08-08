using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Serialization;

namespace Content.Shared._SV.Silicon.MalfunctioningAi;

public abstract partial class SharedMalfunctioningAiSystem
{
    [Dependency] private SharedExplosionSystem _sharedExplosion = default!;
    private void InitializeAirlock()
    {
        SubscribeLocalEvent<DoorComponent, MalfAiOverloadDoorEvent>(OnOverloadDoor);
    }

    private void OnOverloadDoor(EntityUid ent, DoorComponent component, MalfAiOverloadDoorEvent args)
    {
        if (!IsMalfunctioning(args.User))
            return;

        _sharedExplosion.QueueExplosion(ent, "Minibomb", 1, 1, 1, 1, 1, false);
        if (TryComp<DamageableComponent>(ent, out var comp))
        {
            DamageSpecifier explosionDamage = new();
        }

    }
}

/// <summary> Event for a malfunctioning AI overloading a door. </summary>
[Serializable, NetSerializable]
public sealed class MalfAiOverloadDoorEvent : BaseStationAiAction;
