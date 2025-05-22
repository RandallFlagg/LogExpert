using LogExpert.Core.Config;

using System.Runtime.Versioning;

namespace LogExpert.UI.Extensions.Forms;

[SupportedOSPlatform("windows")]
public class MenuSelectedColors : ProfessionalColorTable
{
    public override Color ImageMarginGradientBegin => ColorMode.MenuBackgroundColor;

    public override Color ImageMarginGradientMiddle => ColorMode.MenuBackgroundColor;

    public override Color ImageMarginGradientEnd => ColorMode.MenuBackgroundColor;

    public override Color ToolStripDropDownBackground => ColorMode.MenuBackgroundColor;

    public override Color MenuBorder => ColorMode.MenuBackgroundColor;

    public override Color MenuItemBorder => ColorMode.MenuBackgroundColor;

    public override Color MenuItemSelected => ColorMode.HoverMenuBackgroundColor;

    public override Color MenuItemSelectedGradientBegin => ColorMode.HoverMenuBackgroundColor;

    public override Color MenuItemSelectedGradientEnd => ColorMode.HoverMenuBackgroundColor;

    public override Color MenuItemPressedGradientBegin => ColorMode.MenuBackgroundColor;

    public override Color MenuItemPressedGradientEnd => ColorMode.MenuBackgroundColor;
}