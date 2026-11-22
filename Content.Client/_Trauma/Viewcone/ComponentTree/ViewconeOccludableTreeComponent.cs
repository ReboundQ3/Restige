using Content.Shared._Trauma.Viewcone.Components;
using Robust.Shared.ComponentTrees;
using Robust.Shared.Physics;

namespace Content.Client._Trauma.Viewcone.ComponentTree;

[RegisterComponent]
public sealed partial class ViewconeOccludableTreeComponent : Component, IComponentTreeComponent<ViewconeOccludableComponent>
{
    public DynamicTree<ComponentTreeEntry<ViewconeOccludableComponent>> Tree { get; set; }
}
