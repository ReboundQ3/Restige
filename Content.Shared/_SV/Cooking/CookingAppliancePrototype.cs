using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

[Prototype]
public sealed partial class CookingAppliancePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public LocId Name { get; private set; } = string.Empty;

    [DataField]
    public CookingApplianceType Type { get; private set; } = CookingApplianceType.Null;
}

public enum CookingApplianceType
{
    Grill,
    Microwave,
    Bake,
    Boil,
    Null
}
