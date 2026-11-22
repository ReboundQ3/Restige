using System.Linq;
using Robust.Client.GameObjects;

namespace Content.Client._Trauma.Sprite;

/// <summary>
/// Modifies sprite visibility. Used to avoid conflicts with multiple different systems/overlays changing visibility.
/// </summary>
public sealed partial class SpriteVisibilitySystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;
    [Dependency] private EntityQuery<SpriteVisibilityComponent> _query = default!;

    [SubscribeLocalEvent]
    private void OnStartup(Entity<SpriteVisibilityComponent> ent, ref ComponentStartup args)
    {
        if (!_spriteQuery.TryComp(ent, out var comp) || comp.Color.A >= 1f)
            return;

        ent.Comp.VisibilityModifiers[nameof(SpriteComponent)] = comp.Color.A;
    }

    /// <param name="alpha">Sprite color alpha, use a value of 1 or more to remove the modifier and 0 or less to hide the sprite.</param>
    public void UpdateVisibilityModifiers(EntityUid uid, string key, float alpha)
    {
        if (!_spriteQuery.TryComp(uid, out var comp) || TerminatingOrDeleted(uid))
            return;

        if (alpha >= 1f)
            RemoveVisibilityModifier((uid, comp), key);
        else
            AddVisibilityModifier((uid, comp), key, alpha);
    }

    private void AddVisibilityModifier(Entity<SpriteComponent> ent, string key, float modifier)
    {
        modifier = MathF.Max(modifier, 0f);

        var comp = EnsureComp<SpriteVisibilityComponent>(ent);
        if (comp.VisibilityModifiers.TryGetValue(key, out var old) && old == modifier)
            return;

        comp.VisibilityModifiers[key] = modifier;
        ReCalculateSpriteVisibility((ent, ent.Comp, comp));
    }

    private void RemoveVisibilityModifier(Entity<SpriteComponent?, SpriteVisibilityComponent?> ent, string key)
    {
        if (!_spriteQuery.Resolve(ent, ref ent.Comp1))
            return;

        if (!_query.Resolve(ent, ref ent.Comp2, false))
        {
            SetSpriteVisibility(ent!, 1f);
            return;
        }

        ent.Comp2.VisibilityModifiers.Remove(key);
        if (ent.Comp2.VisibilityModifiers.Count == 0)
        {
            RemCompDeferred(ent, ent.Comp2);
            SetSpriteVisibility(ent!, 1f);
            return;
        }

        if (ent.Comp2.VisibilityModifiers.Count == 1 &&
            ent.Comp2.VisibilityModifiers.TryGetValue(nameof(SpriteComponent), out var alpha))
        {
            RemCompDeferred(ent, ent.Comp2);
            SetSpriteVisibility(ent!, alpha);
            return;
        }

        ReCalculateSpriteVisibility(ent!);
    }

    private void SetSpriteVisibility(Entity<SpriteComponent> ent, float visibility)
    {
        var e = ent.AsNullable();
        visibility = Math.Clamp(visibility, 0f, 1f);
        var visible = visibility > 0f;
        _sprite.SetVisible(e, visible);
        if (visible)
            _sprite.SetColor(e, ent.Comp.Color.WithAlpha(visibility));
    }

    private void ReCalculateSpriteVisibility(Entity<SpriteComponent, SpriteVisibilityComponent> ent)
    {
        var visibility = ent.Comp2.VisibilityModifiers.Values.Aggregate(1f, (x, y) => x * y);
        SetSpriteVisibility(ent, visibility);
    }

    public float GetModifier(SpriteVisibilityComponent comp)
        => comp.VisibilityModifiers.Values.Aggregate(1f, (x, y) => x * y);
}
