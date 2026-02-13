using Content.Server._SV.Records.Components;
using Content.Shared._SV.Records;
using Content.Server.StationRecords.Systems;
using Content.Shared.Forensics.Components;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.StationRecords;
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
            Log.Error($"Null JobId in CharacterRecordsSystem::OnPlayerSpawn for character {args.Profile.Name} played by {args.Player.Name}");
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
}

