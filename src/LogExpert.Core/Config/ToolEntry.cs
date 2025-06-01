using System;

using LogExpert.Core.Classes;

namespace LogExpert.Core.Config
{
    [Serializable]
    public class ToolEntry
    {
        #region Fields

        public string args = "";
        public string cmd = "";
        public string columnizerName = "";
        public string iconFile;
        public int iconIndex;
        public bool isFavourite;
        public string name;
        public bool sysout;
        public string workingDir = "";

        #endregion

        #region Public methods

        public override string ToString()
        {
            return Util.IsNull(name) ? cmd : name;
        }

        public ToolEntry Clone()
        {
            ToolEntry clone = new();
            clone.cmd = cmd;
            clone.args = args;
            clone.name = name;
            clone.sysout = sysout;
            clone.columnizerName = columnizerName;
            clone.isFavourite = isFavourite;
            clone.iconFile = iconFile;
            clone.iconIndex = iconIndex;
            clone.workingDir = workingDir;
            return clone;
        }

        #endregion
    }
}