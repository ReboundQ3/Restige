// SPDX-FileCopyrightText: 2026 Sector-Vestige contributors
// SPDX-FileCopyrightText: 2026 Sector Vestige contributors (modifications)
// SPDX-FileCopyrightText: 2026 ReboundQ3 <22770594+ReboundQ3@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._SV.Sponsors;

/// <summary>
/// A Patreon patron tier. Data-driven so colour/name changes are YAML/loc edits.
/// Internal ids stay tier1/tier2/tier3 forever; display names are loc-only.
/// </summary>
[Prototype]
public sealed partial class PatronTierPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>Loc key for the public display name (e.g. "Passenger").</summary>
    [DataField]
    public LocId DisplayName;

    /// <summary>Colour applied to the patron's OOC name.</summary>
    [DataField(required: true)]
    public Color OocColor;
}
