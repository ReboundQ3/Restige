// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Managers;
using Content.Shared._Goobstation.Blob;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._Goobstation.Administration.Systems;

/// <summary>
/// Admin antag verbs for Goob-Station antagonists, ported from Goob's GoobAdminVerbSystem.Antag.
/// </summary>
public sealed partial class GoobAdminVerbSystem : EntitySystem
{
    [Dependency] private IAdminManager _adminManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetVerbsEvent<Verb>>(AddAntagVerbs);
    }

    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor)
            || !_adminManager.HasAdminFlag(actor.PlayerSession, AdminFlags.Fun))
            return;

        if (!HasComp<MindContainerComponent>(args.Target))
            return;

        // Blob
        Verb blobAntag = new()
        {
            Text = Loc.GetString("admin-verb-text-make-blob"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/_Goobstation/Blob/Actions/blob.rsi"), "blobFactory"),
            Act = () =>
            {
                EnsureComp<BlobCarrierComponent>(args.Target).HasMind = HasComp<ActorComponent>(args.Target);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-text-make-blob"),
        };
        // Goob checked SiliconComponent (Einstein Engines IPCs), which Restige doesn't have; borgs are the closest match.
        if (!HasComp<BorgChassisComponent>(args.Target))
            args.Verbs.Add(blobAntag);
    }
}
