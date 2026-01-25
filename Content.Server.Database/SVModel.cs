// File to store as much SV related database things outside of Model.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Content.Server.Database;

public static class SVModel
{
    /// <summary>
    /// Stores SV Character data separately from the main Profile. This is done to work around a bug
    /// in EFCore migrations as CV described in their model CDModel.cs
    /// </summary>
    public class SVProfile
    {
        public int Id { get; set; }

        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        public float Height { get; set; } = 1f;

        [Column("character_records", TypeName = "jsonb")]
        public JsonDocument? CharacterRecords { get; set; }

        public List<CharacterRecordEntry> CharacterRecordEntries { get; set; } = new();

    }
    public enum DbRecordEntryType : byte
    {
        Medical = 0, Security = 1, Employment = 2, Centcomm = 3, Admin = 4,
    }

    [Table("SV_character_record_entries"), Index(nameof(Id))]
    public sealed class CharacterRecordEntry
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string LastEditedDate { get; set; } = null!;

        public string LastEditedBy { get; set; } = null!;

        public string Author { get; set; } = null!;

        public string StampedBy { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DbRecordEntryType Type { get; set; }

        public int SVProfileId { get; set; }
        public SVProfile SVProfile { get; set; } = null!;
    }
}
