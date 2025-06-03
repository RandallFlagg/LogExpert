using System.Text;

using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Entities;

namespace LogExpert.Core.Classes.Persister;

public class PersistenceData
{
    #region Fields

    private SortedList<int, Entities.Bookmark> bookmarkList = [];
    private int bookmarkListPosition = 300;
    private bool bookmarkListVisible;
    private string columnizerName;
    private int currentLine = -1;
    public Encoding encoding;
    public string fileName;
    public bool filterAdvanced;
    public List<FilterParams> filterParamsList = [];
    public int filterPosition = 222;
    public bool filterSaveListVisible;
    public List<FilterTabData> filterTabDataList = [];
    public bool filterVisible;
    public int firstDisplayedLine = -1;
    public bool followTail = true;
    public string highlightGroupName;
    public int lineCount;

    public bool multiFile;
    public int multiFileMaxDays;
    public List<string> multiFileNames = [];
    public string multiFilePattern;
    public SortedList<int, RowHeightEntry> rowHeightList = [];
    public string sessionFileName;
    public bool showBookmarkCommentColumn;
    public string tabName;

    public string settingsSaveLoadLocation;

    public SortedList<int, Entities.Bookmark> BookmarkList { get => bookmarkList; set => bookmarkList = value; }
    public int BookmarkListPosition { get => bookmarkListPosition; set => bookmarkListPosition = value; }
    public bool BookmarkListVisible { get => bookmarkListVisible; set => bookmarkListVisible = value; }
    public string ColumnizerName { get => columnizerName; set => columnizerName = value; }
    public int CurrentLine { get => currentLine; set => currentLine = value; }

    #endregion
}
