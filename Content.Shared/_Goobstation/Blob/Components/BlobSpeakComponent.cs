// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Radio;
using Content.Shared.Speech.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Blob.Components;

//[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[RegisterComponent, NetworkedComponent]
public sealed partial class BlobSpeakComponent : Component
{
    /// <summary>
    /// Replacement accent that turns local speech into blob noises.
    /// </summary>
    [DataField]
    public ProtoId<ReplacementAccentPrototype> Accent = "blob";

    /// <summary>
    /// Messages sent on this channel keep their text, so blobs can still understand each other.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> HivemindChannel = "Blobmind";

    /// <summary>
    /// Hide entity name
    /// </summary>
    [DataField]
    public bool OverrideName = false; // Goob Edit, no overriding default name.

    [DataField]
    public LocId Name = "speak-vv-blob";
}
