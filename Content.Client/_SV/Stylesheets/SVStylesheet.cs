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
///     Sector Vestige's default stylesheet: the upstream Nanotrasen sheet retuned to the neutral-grey
///     <see cref="SVPalettes"/> scheme, reading chrome textures from the fork's own texture root.
/// </summary>
/// <remarks>
///     This deliberately derives from <see cref="NanotrasenStylesheet"/> rather than from
///     <see cref="CommonStylesheet"/>. Nanotrasen's constructor calls
///     <c>GetAllSheetletRules&lt;NanotrasenStylesheet, CommonSheetletAttribute&gt;</c>, so sheetlets declared
///     as <c>Sheetlet&lt;NanotrasenStylesheet&gt;</c> — PDA, Paper, MainMenu, HumanoidProfileEditor,
///     ConstructionMenu and FeedbackPopup — are loaded by that sheet and no other. A sibling class deriving
///     from <see cref="CommonStylesheet"/> would compile and run, but would silently drop all six.
/// </remarks>
public sealed class SVStylesheet : NanotrasenStylesheet
{
    public override string StylesheetName => "SV";

    public static readonly ResPath SvTextureRoot = new("/Textures/Interface/SV");

    /// <remarks>
    ///     Roots are checked in order by <c>BaseStylesheet.TryGetResource</c>, so only the textures actually
    ///     recoloured for the fork need to exist under <see cref="SvTextureRoot"/>; everything else falls
    ///     through to the upstream Nano root.
    /// </remarks>
    public override Dictionary<Type, ResPath[]> Roots => new()
    {
        { typeof(TextureResource), [SvTextureRoot, TextureRoot] },
    };

    public override ColorPalette PrimaryPalette => SVPalettes.Hull;
    public override ColorPalette SecondaryPalette => SVPalettes.Deck;
    public override ColorPalette PositivePalette => SVPalettes.Green;
    public override ColorPalette NegativePalette => SVPalettes.Red;
    public override ColorPalette HighlightPalette => SVPalettes.Steel;

    public SVStylesheet(object config, StylesheetManager man) : base(config, man) { }
}
