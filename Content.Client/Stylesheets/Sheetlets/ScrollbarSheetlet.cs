using Content.Client._SV.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class ScrollbarSheetlet : Sheetlet<PalettedStylesheet>
{
    public const int DefaultGrabberSize = 10;

    // Scrollbar grabbers float over arbitrary content, so they stay translucent rather than taking a
    // surface colour from the palette.
    private static readonly Color GrabberNormal = SVPalettes.Muted.PressedElement.WithAlpha(0.35f);
    private static readonly Color GrabberHover = SVPalettes.Muted.Element.WithAlpha(0.35f);
    private static readonly Color GrabberGrabbed = SVPalettes.Muted.Base.WithAlpha(0.35f);

    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        var vScrollBarGrabberNormal = new StyleBoxFlat
        {
            BackgroundColor = GrabberNormal, ContentMarginLeftOverride = DefaultGrabberSize,
            ContentMarginTopOverride = DefaultGrabberSize,
        };
        var vScrollBarGrabberHover = new StyleBoxFlat
        {
            BackgroundColor = GrabberHover, ContentMarginLeftOverride = DefaultGrabberSize,
            ContentMarginTopOverride = DefaultGrabberSize,
        };

        var vScrollBarGrabberGrabbed = new StyleBoxFlat
        {
            BackgroundColor = GrabberGrabbed, ContentMarginLeftOverride = DefaultGrabberSize,
            ContentMarginTopOverride = DefaultGrabberSize,
        };

        var hScrollBarGrabberNormal = new StyleBoxFlat
        {
            BackgroundColor = GrabberNormal, ContentMarginTopOverride = DefaultGrabberSize,
        };

        var hScrollBarGrabberHover = new StyleBoxFlat
        {
            BackgroundColor = GrabberHover, ContentMarginTopOverride = DefaultGrabberSize,
        };

        var hScrollBarGrabberGrabbed = new StyleBoxFlat
        {
            BackgroundColor = GrabberGrabbed, ContentMarginTopOverride = DefaultGrabberSize,
        };

        return
        [
            E<VScrollBar>().Prop(ScrollBar.StylePropertyGrabber, vScrollBarGrabberNormal),
            E<VScrollBar>().PseudoHovered().Prop(ScrollBar.StylePropertyGrabber, vScrollBarGrabberHover),
            E<VScrollBar>().PseudoPressed().Prop(ScrollBar.StylePropertyGrabber, vScrollBarGrabberGrabbed),
            E<HScrollBar>().Prop(ScrollBar.StylePropertyGrabber, hScrollBarGrabberNormal),
            E<HScrollBar>().PseudoHovered().Prop(ScrollBar.StylePropertyGrabber, hScrollBarGrabberHover),
            E<HScrollBar>().PseudoPressed().Prop(ScrollBar.StylePropertyGrabber, hScrollBarGrabberGrabbed),
        ];
    }
}
