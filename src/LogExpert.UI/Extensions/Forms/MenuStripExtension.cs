using System.Runtime.Versioning;

namespace LogExpert.UI.Extensions.Forms;

[SupportedOSPlatform("windows")]
internal class ExtendedMenuStripRenderer : ToolStripProfessionalRenderer
{
    public ExtendedMenuStripRenderer() : base(new MenuSelectedColors()) { }
}
