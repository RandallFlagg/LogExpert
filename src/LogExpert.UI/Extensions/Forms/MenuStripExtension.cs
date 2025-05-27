using System.Runtime.Versioning;
using System.Windows.Forms;

namespace LogExpert.UI.Extensions.Forms;

[SupportedOSPlatform("windows")]
public class ExtendedMenuStripRenderer : ToolStripProfessionalRenderer
{
    public ExtendedMenuStripRenderer() : base(new MenuSelectedColors()) { }
}
