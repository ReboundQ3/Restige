using Robust.Shared.GameStates;
namespace Content.Shared._Trauma.Viewcone.Components;

/// <summary>
/// Intended to be used on inventory items, mutations or status effects (i.e. this is relayed).
/// Modifies the viewcone angle of the relevant entity multiplicatively.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ViewconeModifierComponent : Component
{
    [DataField(required: true)]
    public float AngleModifier = 1f;
}
