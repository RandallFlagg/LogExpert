using LogExpert.Core.Entities;

namespace LogExpert.Core.EventArgs
{
    public class BookmarkEventArgs(Bookmark bookmark) : System.EventArgs
    {
        #region Properties

        public Bookmark Bookmark { get; } = bookmark;

        #endregion
    }
}