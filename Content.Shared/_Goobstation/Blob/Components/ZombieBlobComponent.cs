// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Radio;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Blob.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ZombieBlobComponent : Component
{
    public List<string> OldFactions = new();

    [AutoNetworkedField]
    public EntityUid BlobPodUid = default!;

    public float? OldColdDamageThreshold = null;

    [ViewVariables]
    public Dictionary<string, int> DisabledFixtureMasks { get; } = new();

    [DataField("greetSoundNotification")]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/Ambience/Antag/zombie_start.ogg");

    [DataField, AutoNetworkedField]
    public bool CanShoot = false;

    /// <summary>
    /// Hivemind radio channel given to this zombie by its blob pod, removed again when it stops being a zombie.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> HivemindChannel = "Blobmind";

    /// <summary>
    /// Radio components the zombie did not have before being zombified, so shutdown only removes what was added.
    /// </summary>
    public bool AddedTransmitter;
    public bool AddedActiveRadio;
    public bool AddedReceiver;
}
