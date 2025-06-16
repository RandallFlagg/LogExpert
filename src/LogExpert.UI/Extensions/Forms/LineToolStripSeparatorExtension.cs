using System.Runtime.Versioning;

namespace LogExpert.UI.Extensions.Forms;

[SupportedOSPlatform("windows")]
internal class LineToolStripSeparatorExtension : ToolStripSeparator
{
    public LineToolStripSeparatorExtension ()
    {
        Paint += OnExtendedToolStripSeparatorPaint;
    }

    private void OnExtendedToolStripSeparatorPaint (object sender, PaintEventArgs e)
    {
        // Get the separator's width and height.
        var toolStripSeparator = (ToolStripSeparator)sender;
        var width = toolStripSeparator.Width;
        var height = toolStripSeparator.Height;

        // Choose the colors for drawing.
        // I've used Color.White as the foreColor.
        //TODO change to white if the background color is darker;
        Color foreColor = Color.FromKnownColor(KnownColor.Black);
        // Color.Teal as the backColor.
        Color backColor = ColorMode.BackgroundColor;

        // Fill the background.
        using SolidBrush backbrush = new(backColor);
        e.Graphics.FillRectangle(backbrush, 0, 0, width, height);

        // Draw the line.
        using Pen pen = new(foreColor);
        e.Graphics.DrawLine(pen, width / 2, 4, width / 2, height - 4);
    }
}
