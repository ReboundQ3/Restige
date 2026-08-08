using Content.Shared._SV.Roles.Components;
using Content.Shared.Administration.Managers;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._SV.Silicon.MalfunctioningAi;

public abstract partial class SharedMalfunctioningAiSystem : EntitySystem
{
    [Dependency] private ISharedAdminManager _admin = default!;
    [Dependency] private SharedStationAiSystem _stationAi = default!;
    [Dependency] private SharedMindSystem _minds = default!;
    [Dependency] private SharedRoleSystem _roles = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);

        InitializeAirlock();
    }

    /// <summary>
    /// Whether an AI is a malfunctioning AI antag. Accepts either the AI itself or the core holding it.
    /// </summary>
    public bool IsMalfunctioning(EntityUid entity)
    {
        if (TryComp<StationAiCoreComponent>(entity, out var core) &&
            _stationAi.TryGetHeld((entity, core), out var held))
        {
            entity = held.Value;
        }

        return _minds.TryGetMind(entity, out var mindId, out _) &&
            _roles.MindHasRole<MalfunctioningAiRoleComponent>(mindId);
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (!_admin.IsAdmin(args.User))
            return;

        if (!TryComp<StationAiCoreComponent>(args.Target, out var core))
            return;

        if (!_stationAi.TryGetHeld((args.Target, core), out var held))
            return;

        if (IsMalfunctioning(held.Value))
            return;

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("sv-create-antag-malfai"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Mobs/Silicon/station_ai.rsi"), "ai_angel_dead"),
            Act = () => MakeMalfunctioning(held.Value),
            Impact = LogImpact.High,
        });
    }

    /// <summary>
    /// Turns the AI held in a core into a malfunctioning AI antag. No-op on the client.
    /// </summary>
    protected virtual void MakeMalfunctioning(EntityUid ai)
    {
    }
}
