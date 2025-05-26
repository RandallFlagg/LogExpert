using LogExpert.Core.Entities;

namespace LogExpert.Core.Interface
{
    /// <summary>
    /// To be implemented by the bookmark window. Will be informed from LogWindow about changes in bookmarks.
    /// </summary>
    //TODO: Not in use!
    public interface IBookmarkView
    {
        #region Properties

        bool LineColumnVisible { set; }

        #endregion

        #region Public methods

        void UpdateView();

        void BookmarkTextChanged(Bookmark bookmark);

        void SelectBookmark(int lineNum);

        void SetBookmarkData(IBookmarkData bookmarkData);

        #endregion
    }
}