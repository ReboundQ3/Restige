using Content.Shared._SV.Silicon.MalfunctioningAi;
using Content.Shared.Doors.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Client.Player;
using Robust.Shared.Utility;

namespace Content.Client._SV.Silicon.MalfunctioningAi;

/// <summary>
/// Client half of the malfunctioning AI antag. Abstract systems aren't registered, so this exists to
/// make the shared verb handler run client-side, and to fill the AI radial menu - which only ever
/// happens on the client.
/// </summary>
public sealed partial class MalfunctioningAiSystem : SharedMalfunctioningAiSystem
{
    [Dependency] private IPlayerManager _player = default!;

    private static readonly ResPath AiActionsRsi = new("/Textures/Interface/Actions/actions_ai.rsi");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoorComponent, GetStationAiRadialEvent>(OnDoorOverloadGetRadial);
    }

    private void OnDoorOverloadGetRadial(Entity<DoorComponent> ent, ref GetStationAiRadialEvent args)
    {
        if (_player.LocalEntity is not { } ai || !IsMalfunctioning(ai))
            return;

        args.Actions.Add(new StationAiRadial
        {
            Sprite = new SpriteSpecifier.Rsi(AiActionsRsi, "door_overcharge_on"),
            Tooltip = Loc.GetString("sv-malfai-overload-door"),
            Event = new MalfAiOverloadDoorEvent(),
        });
    }
}
