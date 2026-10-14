using Robust.Shared.Containers;

namespace Content.Shared._SV.Cooking;

public sealed partial class CookingApplianceSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CookingApplianceComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<CookingApplianceComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Storage = _container.EnsureContainer<Container>(ent, ent.Comp.CotnainerId);
    }
}
