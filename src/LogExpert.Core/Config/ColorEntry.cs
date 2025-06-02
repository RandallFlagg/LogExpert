#region

using System.Drawing;

#endregion

namespace LogExpert.Core.Config;

[Serializable]
public class ColorEntry
{
    #region cTor

    public ColorEntry(string fileName, Color color)
    {
        FileName = fileName;
        Color = color;
    }

    #endregion

    public Color Color { get; }

    public string FileName { get; }

    #region Fields

    #endregion
}