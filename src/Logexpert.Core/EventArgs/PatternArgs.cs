namespace LogExpert.Core.EventArgs
{
    public class PatternArgs
    {
        #region Properties

        public int EndLine { get; set; } = 0;

        public int Fuzzy { get; set; } = 6;

        public int MaxDiffInBlock { get; set; } = 5;

        public int MaxMisses { get; set; } = 5;

        public int MinWeight { get; set; } = 15;

        public int StartLine { get; set; } = 0;

        #endregion
    }
}