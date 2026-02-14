using Content.Server.StationRecords.Systems;
using Content.Server.StationRecords;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Content.Shared._SV.Records;
using Content.Shared.Forensics.Components;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Server._SV.Records;

public sealed class RecordsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn, after: [typeof(StationRecordsSystem)]);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        if (!HasComp<StationRecordsComponent>(args.Station))
        {
            Log.Error("Tried to add Records on a station without StationRecords");
            return;
        }

        if (!HasComp<RecordsComponent>(args.Station))
            AddComp<RecordsComponent>(args.Station);
        if (string.IsNullOrEmpty(args.JobId))
        {
            Log.Error($"Null JobId in RecordsSystem::OnPlayerSpawn for character {args.Profile.Name} played by {args.Player.Name}");
            return;
        }

        if (HasComp<SkipLoadingCharacterRecordsComponent>(args.Mob))
            return;

        var profile = args.Profile;
        // Be robust: if records are missing, initialize them to defaults instead of bailing.
        if (profile.SVCharacterRecords == null)
        {
            profile.SVCharacterRecords = PlayerProvidedRecords.DefaultRecords();
        }
        else
        {
            profile.SVCharacterRecords.EnsureValid();
        }

        var player = args.Mob;

        if (!_prototype.TryIndex(args.JobId, out JobPrototype? jobPrototype))
        {
            throw new ArgumentException($"Invalid job prototype ID: {args.JobId}");
        }

        TryComp<FingerprintComponent>(player, out var fingerprintComponent);
        TryComp<DnaComponent>(player, out var dnaComponent);

        var jobTitle = jobPrototype.LocalizedName;
        var stationRecordsKey = FindStationRecordsKey(player);

        // Grab the title from the station records if they exist to support our job title system
        if (stationRecordsKey != null && _records.TryGetRecord<GeneralStationRecord>(stationRecordsKey.Value, out var stationRecords))
        {
            jobTitle = stationRecords.JobTitle;
        }

        var records = new FullCharacterRecords(
            pRecords: new PlayerProvidedRecords(profile.SVCharacterRecords),
            stationRecordsKey: stationRecordsKey?.Id,
            name: profile.Name,
            age: profile.Age,
            species: profile.Species,
            jobTitle: jobTitle,
            jobIcon: jobPrototype.Icon,
            gender: profile.Gender,
            sex: profile.Sex,
            fingerprint: fingerprintComponent?.Fingerprint,
            dna: dnaComponent?.DNA,
            owner: player);
        AddRecord(args.Station, args.Mob, records);
    }

    private StationRecordKey? FindStationRecordsKey(EntityUid uid)
    {
        if (!_inventory.TryGetSlotEntity(uid, "id", out var idUid))
            return null;

        var keyStorageEntity = idUid;
        if (TryComp<PdaComponent>(idUid, out var pda) && pda.ContainedId is { } id)
        {
            keyStorageEntity = id;
        }

        if (!TryComp<StationRecordKeyStorageComponent>(keyStorageEntity, out var storage))
        {
            return null;
        }

        return storage.Key;
    }

    private void AddRecord(EntityUid station, EntityUid player, FullCharacterRecords records, RecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return;

        var key = recordsDb.CreateNewKey();
        recordsDb.Records.Add(key, records);
        var playerKey = new RecordKey { Station = station, Index = key };
        AddComp(player, new RecordKeyStorageComponent(playerKey));

        RaiseLocalEvent(station, new RecordsModifiedEvent());
    }

    private void DelRecord(EntityUid station, EntityUid player, RecordType type, int index, RecordKeyStorageComponent? key, RecordsComponent? recordsDb)
    {
        if (!Resolve(station, ref recordsDb) || !Resolve(player, ref key))
            return;

        if (!recordsDb.Records.TryGetValue(key.Key.Index, out var value))
            return;

        var record = value.PRecords;

        switch (type)
        {
            case RecordType.Employment:
                record.EmploymentEntries.RemoveAt(index);
                break;
            case RecordType.Medical:
                record.MedicalEntries.RemoveAt(index);
                break;
            case RecordType.Security:
                record.SecurityEntries.RemoveAt(index);
                break;
            case RecordType.CentComm:
                record.CentCommEntries.RemoveAt(index);
                break;
        }
    }

    private IDictionary<uint, FullCharacterRecords> GetRecord(EntityUid station, RecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return new Dictionary<uint, FullCharacterRecords>();

        return recordsDb.Records;
    }

    public sealed class RecordsModifiedEvent : EntityEventArgs;
}

