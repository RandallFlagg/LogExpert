using LogExpert.Core.Entities;

namespace LogExpert.Core.EventArguments;

public class BookmarkEventArgs : EventArgs
{
    public BookmarkEventArgs (Bookmark bookmark)
    {
        Bookmark = bookmark;
    }

    public BookmarkEventArgs () { }

    public static new readonly BookmarkEventArgs Empty = new();

    #region Properties

    public Bookmark Bookmark { get; }

    #endregion
}