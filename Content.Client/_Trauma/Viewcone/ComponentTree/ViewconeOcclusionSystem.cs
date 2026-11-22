using System.Numerics;
using Content.Shared._Trauma.Viewcone.Components;
using Robust.Client.GameObjects;
using Robust.Shared.ComponentTrees;
using Robust.Shared.Physics;

namespace Content.Client._Trauma.Viewcone.ComponentTree;

/// <summary>
/// Handles gathering sprites to modify alpha in the viewcone overlays
/// </summary>
public sealed partial class ViewconeOcclusionSystem : ComponentTreeSystem<ViewconeOccludableTreeComponent, ViewconeOccludableComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;

    protected override bool DoFrameUpdate => true;
    protected override bool DoTickUpdate => false;
    protected override bool Recursive => false;

    protected override Box2 ExtractAabb(in ComponentTreeEntry<ViewconeOccludableComponent> entry, Vector2 pos, Angle rot)
    {
        return _sprite.CalculateBounds((entry.Uid, _spriteQuery.Comp(entry.Uid)), pos, rot, default).CalcBoundingBox();
    }
}
