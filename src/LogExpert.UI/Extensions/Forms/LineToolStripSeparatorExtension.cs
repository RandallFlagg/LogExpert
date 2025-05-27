﻿using LogExpert.Core.Config;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace LogExpert.UI.Extensions.Forms;

[SupportedOSPlatform("windows")]
public class LineToolStripSeparatorExtension : ToolStripSeparator
{
    public LineToolStripSeparatorExtension()
    {
        Paint += OnExtendedToolStripSeparatorPaint;
    }

    private void OnExtendedToolStripSeparatorPaint(object sender, PaintEventArgs e)
    {
        // Get the separator's width and height.
        ToolStripSeparator toolStripSeparator = (ToolStripSeparator)sender;
        int width = toolStripSeparator.Width;
        int height = toolStripSeparator.Height;

        // Choose the colors for drawing.
        // I've used Color.White as the foreColor.
        Color foreColor = ColorMode.ForeColor;
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
