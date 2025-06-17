#region

using System.Drawing;

#endregion

namespace LogExpert.Core.Config;

[Serializable]
public record ColorEntry (string FileName, Color Color);
