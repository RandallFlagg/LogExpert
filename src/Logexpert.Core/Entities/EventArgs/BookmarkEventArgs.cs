namespace LogExpert.Core.Entities.EventArgs
{
    public class BookmarkEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public BookmarkEventArgs(Bookmark bookmark)
        {
            Bookmark = bookmark;
        }

        #endregion

        #region Properties

        public Bookmark Bookmark { get; }

        #endregion
    }
}