// SPDX-FileCopyrightText: 2026 Sector-Vestige contributors
// SPDX-FileCopyrightText: 2026 ReboundQ3 <22770594+ReboundQ3@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Palette;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.ResourceManagement;
using Robust.Shared.Utility;

namespace Content.Client._SV.Stylesheets;

/// <summary>
///     Sector Vestige's system stylesheet, used by admin, debug and launcher UIs.
/// </summary>
/// <remarks>
///     Upstream <see cref="SystemStylesheet"/> declares no texture roots at all and relies on the
///     <c>GetTextureOr</c> fallback to Nano, so without this subclass every admin and launcher window would
///     keep upstream chrome while the rest of the game moved to the fork's scheme.
///
///     <para>
///     The two structural roles are swapped relative to <see cref="SVStylesheet"/>, so admin windows sit on
///     the lighter Hull grey instead of the darker Deck grey. That keeps tooling visually distinct from
///     in-round UI, which is the same distinction upstream draws by giving System its own palette.
///     </para>
/// </remarks>
public sealed class SVSystemStylesheet : SystemStylesheet
{
    public override string StylesheetName => "SVSystem";

    /// <remarks>
    ///     Upstream declares an empty root list and leans entirely on the <c>GetTextureOr</c> Nano fallback.
    ///     Naming Nano explicitly as the second root preserves that behaviour while letting the fork's own
    ///     textures take precedence where they exist.
    /// </remarks>
    public override Dictionary<Type, ResPath[]> Roots => new()
    {
        { typeof(TextureResource), [SVStylesheet.SvTextureRoot, NanotrasenStylesheet.TextureRoot] },
    };

    public override ColorPalette PrimaryPalette => SVPalettes.Deck;
    public override ColorPalette SecondaryPalette => SVPalettes.Hull;
    public override ColorPalette PositivePalette => SVPalettes.Green;
    public override ColorPalette NegativePalette => SVPalettes.Red;
    public override ColorPalette HighlightPalette => SVPalettes.Steel;

    public SVSystemStylesheet(object config, StylesheetManager man) : base(config, man) { }
}
