using System.Runtime.Versioning;

namespace LogExpert.UI.Extensions.Forms;

[SupportedOSPlatform("windows")]
public class ExtendedMenuStripRenderer : ToolStripProfessionalRenderer
{
    public ExtendedMenuStripRenderer() : base(new MenuSelectedColors()) { }
}
