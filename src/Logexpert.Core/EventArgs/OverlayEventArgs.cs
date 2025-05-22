using LogExpert.Core.Entities;

namespace LogExpert.Core.EventArgs
{
    public class OverlayEventArgs(BookmarkOverlay overlay) : System.EventArgs
    {
        #region Properties

        public BookmarkOverlay BookmarkOverlay { get; set; } = overlay;

        #endregion
    }
}