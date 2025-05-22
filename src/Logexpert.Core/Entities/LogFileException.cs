namespace LogExpert.Core.Entities
{
    public class LogFileException : ApplicationException
    {
        #region cTor

        public LogFileException(string msg)
            : base(msg)
        {
        }

        public LogFileException(string msg, Exception inner)
            : base(msg, inner)
        {
        }

        #endregion
    }
}