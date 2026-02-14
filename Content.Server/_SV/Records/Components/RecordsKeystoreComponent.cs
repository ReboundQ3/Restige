namespace Content.Server._SV.Records;

/// <summary>
/// Stores the key to the entities character records.
/// </summary>
[RegisterComponent]
[Access(typeof(RecordsSystem))]
public sealed partial class RecordKeyStorageComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public RecordKey Key;

    public RecordKeyStorageComponent(RecordKey key)
    {
        Key = key;
    }
}
