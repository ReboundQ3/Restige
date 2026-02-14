using Content.Shared._SV.Records;

namespace Content.Server._SV.Records;

/// <summary>
/// The component on the station that stores records after the round starts.
/// </summary>
[RegisterComponent]
[Access(typeof(RecordsSystem))]
public sealed partial class RecordsComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<uint, FullCharacterRecords> Records = new();

    [ViewVariables(VVAccess.ReadOnly)]
    private uint _nextKey = 1;

    /// <summary>
    /// Creates a key has never been used previously
    /// </summary>
    public uint CreateNewKey()
    {
        return _nextKey++;
    }
}

public sealed record RecordKey
{
    public EntityUid Station { get; init; }
    public uint Index { get; init; }
}
