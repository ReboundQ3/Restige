using Content.Client.Examine;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client.Stylesheets.Sheetlets.Hud;

[CommonSheetlet]
public sealed class ExamineButtonSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        // The examine button sits directly on the world, so its idle state stays fully transparent and
        // only the interaction states tint.
        var examineButtonColorContext = Color.Transparent;
        var examineButtonColorContextHover = sheet.PrimaryPalette.Element;
        var examineButtonColorContextPressed = sheet.PrimaryPalette.PressedElement;
        var examineButtonColorContextDisabled = sheet.SecondaryPalette.Text;

        var buttonContext = new StyleBoxTexture { Texture = Texture.White };

        return
        [
            E<ExamineButton>()
                .Class(ExamineButton.StyleClassExamineButton)
                .Prop(ContainerButton.StylePropertyStyleBox, buttonContext),
            E<ExamineButton>()
                .Class(ExamineButton.StyleClassExamineButton)
                .PseudoNormal()
                .Prop(Control.StylePropertyModulateSelf, examineButtonColorContext),
            E<ExamineButton>()
                .Class(ExamineButton.StyleClassExamineButton)
                .PseudoHovered()
                .Prop(Control.StylePropertyModulateSelf, examineButtonColorContextHover),
            E<ExamineButton>()
                .Class(ExamineButton.StyleClassExamineButton)
                .PseudoPressed()
                .Prop(Control.StylePropertyModulateSelf, examineButtonColorContextPressed),
            E<ExamineButton>()
                .Class(ExamineButton.StyleClassExamineButton)
                .PseudoDisabled()
                .Prop(Control.StylePropertyModulateSelf, examineButtonColorContextDisabled),
        ];
    }
}
