using Content.Shared.NPC.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._SV.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(MalfunctioningAiRuleSystem))]
public sealed partial class MalfunctioningAiRuleComponent : Component
{
    [DataField]
    public ProtoId<NpcFactionPrototype> SiliconFaction = "MalfunctioningSilicons";

    [DataField]
    public bool GiveBriefing = true;

    [DataField]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/_SV/Ambience/Ambience/Antag/malf.ogg");
}
