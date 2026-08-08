using Content.Server._SV.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Shared._SV.Roles;
using Content.Shared._SV.Roles.Components;
using Robust.Server.GameStates;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;

namespace Content.Server._SV.GameTicking.Rules;

public sealed partial class MalfunctioningAiRuleSystem : GameRuleSystem<MalfunctioningAiRuleComponent>
{
    [Dependency] private AntagSelectionSystem _antagssytem = default!;
    [Dependency] private SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfunctioningAiRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<MalfunctioningAiRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);
    }

    public void OnGetBriefing(Entity<MalfunctioningAiRoleComponent> role, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("sv-malf-infection-greeting"));
    }

    public void AfterEntitySelected(Entity<MalfunctioningAiRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        _antagssytem.SendBriefing(args.Session, Loc.GetString("sv-malf-welcome"), Color.Red, ent.Comp.GreetSoundNotification);
    }
}

