using LogExpert.Core.Classes.Highlight;

namespace LogExpert.Core.Entities
{
    [Serializable]
    public class HilightGroup : ICloneable
    {
        #region Properties

        public string GroupName { get; set; } = string.Empty;

        public List<HilightEntry> HilightEntryList { get; set; } = [];

        public object Clone()
        {
            HilightGroup clone = new()
            {
                GroupName = GroupName
            };

            foreach (HilightEntry entry in HilightEntryList)
            {
                clone.HilightEntryList.Add((HilightEntry)entry.Clone());
            }

            return clone;
        }

        #endregion
    }
}