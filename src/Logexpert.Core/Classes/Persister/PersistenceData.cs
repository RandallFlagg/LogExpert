using System.Text;

using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Entities;

namespace LogExpert.Core.Classes.Persister;

public class PersistenceData
{
    #region Fields

    public SortedList<int, Entities.Bookmark> bookmarkList = [];
    public int bookmarkListPosition = 300;
    public bool bookmarkListVisible;
    public string columnizerName;
    public int currentLine = -1;
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

    #endregion
}
