using Content.Server._SV.Records;

namespace Content.Server._SV.Records.Components;

/// <summary>
/// Stores the key to the entities character records.
/// </summary>
[RegisterComponent]
[Access(typeof(RecordsSystem))]
public sealed partial class RecordsKeyStoreComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public RecordKey Key;

    public RecordsKeyStoreComponent(RecordKey key)
    {
        Key = key;
    }
}
