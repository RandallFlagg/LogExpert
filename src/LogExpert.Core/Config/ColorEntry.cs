#region

using System.Drawing;

#endregion

namespace LogExpert.Core.Config;

[Serializable]
public class ColorEntry (string fileName, Color color)
{
    public Color Color { get; } = color;

    public string FileName { get; } = fileName;

    #region Fields

    #endregion
}