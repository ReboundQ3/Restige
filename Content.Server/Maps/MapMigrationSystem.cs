using System.Collections.Generic;
using System.IO;
using System.Linq;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Server.Maps;

/// <summary>
///     Performs basic map migration operations by listening for engine <see cref="MapLoaderSystem"/> events.
/// </summary>
public sealed partial class MapMigrationSystem : EntitySystem
{
    [Dependency] private IResourceManager _resMan = default!;

    /// <summary>
    ///     SV - Migration files are read in order. A later file overrides an earlier one for the same prototype id.
    /// </summary>
    private static readonly ResPath[] MigrationFiles =
    [
        new("/migration.yml"),
        new("/_SV/migration.yml"),
    ];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BeforeEntityReadEvent>(OnBeforeReadEvent);

#if DEBUG
        var mappings = ReadMigrations();

        // Verify that all of the entries map to valid entity prototypes.
        foreach (var newId in mappings.Values) // SV - Foreach loop though the
        {
            if (!string.IsNullOrEmpty(newId) && newId != "null")
                DebugTools.Assert(ProtoMan.HasIndex<EntityPrototype>(newId), $"{newId} is not an entity prototype.");
        }
#endif
    }

    /// <summary>
    /// SV - Rewritten to handle a handle a list of ResPath's instead of a singular one.
    /// </summary>
    private Dictionary<string, string> ReadMigrations()
    {
        var mappings = new Dictionary<string, string>();

        foreach (var path in MigrationFiles)
        {
            if (!_resMan.TryContentFileRead(path, out var stream))
                continue;

            using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
            var document = DataNodeParser.ParseYamlStream(reader).FirstOrDefault();

            if (document?.Root is not MappingDataNode fileMappings)
                continue;

            foreach (var (key, value) in fileMappings)
            {
                if (value is not ValueDataNode valueNode)
                    continue;

                if (mappings.ContainsKey(key))
                    Log.Warning($"Migration for {key} in {path} overrides an earlier migration file.");

                mappings[key] = valueNode.Value;
            }
        }

        return mappings;
    }

    /// <summary>
    /// SV - Rewritten to handle the list from ReadMigrations
    /// </summary>
    private void OnBeforeReadEvent(BeforeEntityReadEvent ev)
    {
        foreach (var (key, value) in ReadMigrations())
        {
            if (string.IsNullOrWhiteSpace(value) || value == "null")
                ev.DeletedPrototypes.Add(key);
            else
                ev.RenamedPrototypes.Add(key, value);
        }
    }
}
