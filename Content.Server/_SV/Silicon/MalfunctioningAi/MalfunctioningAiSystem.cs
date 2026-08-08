using Content.Server._SV.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Shared._SV.Silicon.MalfunctioningAi;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._SV.Silicon.MalfunctioningAi;

/// <summary>
/// Server half of the malfunctioning AI antag. Antag assignment is authoritative, so it only happens here.
/// </summary>
public sealed partial class MalfunctioningAiSystem : SharedMalfunctioningAiSystem
{
    [Dependency] private AntagSelectionSystem _antag = default!;

    private static readonly EntProtoId DefaultMalfRule = "SVMalfunctioningAi";

    protected override void MakeMalfunctioning(EntityUid ai)
    {
        // Needs a player attached - an unoccupied or ghosted AI has nobody to hand the role to.
        if (!TryComp<ActorComponent>(ai, out var actor))
            return;

        _antag.ForceMakeAntag<MalfunctioningAiRuleComponent>(actor.PlayerSession, DefaultMalfRule);
    }
}
