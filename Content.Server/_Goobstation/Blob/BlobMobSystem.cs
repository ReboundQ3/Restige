// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Speech;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Damage.Systems;

namespace Content.Server._Goobstation.Blob;

public sealed class BlobMobSystem : SharedBlobMobSystem
{
    [Dependency] private DamageableSystem _damageableSystem = default!;
    [Dependency] private ReplacementAccentSystem _accent = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobMobComponent, BlobMobGetPulseEvent>(OnPulsed);

        SubscribeLocalEvent<BlobSpeakComponent, TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<BlobSpeakComponent, TransformSpeakerNameEvent>(OnSpokeName);
        SubscribeLocalEvent<BlobSpeakComponent, SpeakAttemptEvent>(OnSpokeCan, after: new []{ typeof(SpeechSystem) });
    }

    /// <summary>
    /// Blob speech is gibberish to everyone nearby, but stays readable on the hivemind channel.
    /// </summary>
    private void OnTransformSpeech(Entity<BlobSpeakComponent> ent, ref TransformSpeechEvent args)
    {
        if (args.Channel != null && args.Channel.ID == ent.Comp.HivemindChannel.Id)
            return;

        args.Message = _accent.ApplyReplacements(args.Message, ent.Comp.Accent, ent);
    }

    private void OnSpokeName(Entity<BlobSpeakComponent> ent, ref TransformSpeakerNameEvent args)
    {
        if (!ent.Comp.OverrideName)
        {
            return;
        }
        args.VoiceName = Loc.GetString(ent.Comp.Name);
    }

    private void OnSpokeCan(Entity<BlobSpeakComponent> ent, ref SpeakAttemptEvent args)
    {
        if (HasComp<BlobCarrierComponent>(ent))
        {
            return;
        }
        args.Uncancel();
    }

    private void OnPulsed(EntityUid uid, BlobMobComponent component, BlobMobGetPulseEvent args) =>
        _damageableSystem.TryChangeDamage(uid, component.HealthOfPulse);
}
