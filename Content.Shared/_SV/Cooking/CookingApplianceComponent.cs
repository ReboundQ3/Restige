
using Robust.Shared.Containers;

namespace Content.Shared._SV.Cooking;

public sealed partial class CookingApplianceComponent : Component
{
    [DataField]
    public State State = State.Off;

    [DataField]
    public string CotnainerId = "cooking_appliance_container";

    [DataField]
    public Container Storage = default!;

}

public enum State
{
    On,
    Off,
    Broken
}
