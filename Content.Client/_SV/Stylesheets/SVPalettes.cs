// SPDX-FileCopyrightText: 2026 Sector-Vestige contributors
// SPDX-FileCopyrightText: 2026 ReboundQ3 <22770594+ReboundQ3@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Stylesheets.Palette;

namespace Content.Client._SV.Stylesheets;

/// <summary>
///     Sector Vestige's "Ferrite" palette registry — a true neutral grey scheme at the same tonal weight as
///     the upstream Nanotrasen styling, with the NanoTrasen gold replaced by pale steel.
/// </summary>
/// <remarks>
///     Mirrors the shape of <see cref="Palettes"/>. Every palette is derived from a single base hex by
///     <see cref="ColorPalette.FromHexBase"/>, which walks lightness and chroma in Oklab to generate the
///     element / hovered / pressed / disabled / background / text ramp — so retheming means editing one hex
///     per role, not a table of shades.
///
///     <para>
///     <see cref="Deck"/> is tuned so its derived <c>Background</c> lands on <c>#25252A</c>, byte-identical
///     to the stock window texture. That is deliberate: the ~53 panels still carrying a hardcoded
///     <c>#1B1B1E</c> or <c>#25252A</c> continue to match until they are individually migrated.
///     </para>
/// </remarks>
public static class SVPalettes
{
    // Structural tones. Hull carries interactive chrome (buttons, headers), Deck carries surfaces
    // (window backgrounds, inset panels).
    public static readonly ColorPalette Hull = ColorPalette.FromHexBase("#62626b", lightnessShift: 0.06f, chromaShift: 0.0015f);
    public static readonly ColorPalette Deck = ColorPalette.FromHexBase("#545459", lightnessShift: 0.06f);

    // Highlight tone. Replaces the NanoTrasen gold on window titles and accents.
    public static readonly ColorPalette Steel = ColorPalette.FromHexBase("#a8c6d8", lightnessShift: 0.05f, chromaShift: 0.005f);

    /// <summary>
    ///     De-emphasised foreground text — sub-labels, placeholders, disabled captions, scrollbar grabbers.
    /// </summary>
    /// <remarks>
    ///     The five structural roles are all surface tones; their <c>Text</c> shades sit too dark to use as
    ///     foreground on a dark panel. Upstream filled that gap with scattered <c>Color.Gray</c> and
    ///     <c>Color.DarkGray</c> literals, which is what this ramp replaces. Its base is the
    ///     <c>Color.DarkGray</c> value those literals were reaching for, so the shades land where the old
    ///     hardcoded greys did: <c>Base</c> ≈ DarkGray, <c>PressedElement</c> ≈ Gray,
    ///     <c>DisabledElement</c> ≈ the old <c>#757575</c> footer text.
    /// </remarks>
    public static readonly ColorPalette Muted = ColorPalette.FromHexBase("#a9a9a9");

    // Status tones. Kept saturated enough to read as information rather than theming.
    public static readonly ColorPalette Green = ColorPalette.FromHexBase("#4f8459", lightnessShift: 0.06f, chromaShift: 0.008f);
    public static readonly ColorPalette Amber = ColorPalette.FromHexBase("#b5883c", lightnessShift: 0.06f, chromaShift: 0.010f);
    public static readonly ColorPalette Red = ColorPalette.FromHexBase("#a34649", lightnessShift: 0.06f, chromaShift: 0.014f);

    public static readonly StatusPalette Status = new([Red.Base, Amber.Base, Green.Base]);

    // Intended to be used with `ModulateSelf` to darken / lighten something.
    public static readonly ColorPalette AlphaModulate = ColorPalette.FromHexBase("#ffffff");
}
