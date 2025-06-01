namespace LogExpert.Core.Classes.Bookmark;

public static class BookmarkExporter
{
    #region Fields

    private const string replacementForNewLine = @"\n";

    #endregion

    #region Public methods

    public static void ExportBookmarkList (SortedList<int, Entities.Bookmark> bookmarkList, string logfileName,
        string fileName)
    {
        FileStream fs = new(fileName, FileMode.Create, FileAccess.Write);
        StreamWriter writer = new(fs);
        writer.WriteLine("Log file name;Line number;Comment");
        foreach (Entities.Bookmark bookmark in bookmarkList.Values)
        {
            string line = $"{logfileName};{bookmark.LineNum};{bookmark.Text.Replace(replacementForNewLine, @"\" + replacementForNewLine, StringComparison.OrdinalIgnoreCase).Replace("\r\n", replacementForNewLine, StringComparison.OrdinalIgnoreCase)}";
            writer.WriteLine(line);
        }
        writer.Close();
        fs.Close();
    }

    public static void ImportBookmarkList (string logfileName, string fileName, SortedList<int, Entities.Bookmark> bookmarkList)
    {
        using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
        using StreamReader reader = new(fs);
        if (!reader.EndOfStream)
        {
            _ = reader.ReadLine(); // skip "Log file name;Line number;Comment"
        }

        while (!reader.EndOfStream)
        {
            try
            {
                var line = reader.ReadLine();
                line = line.Replace(replacementForNewLine, "\r\n", StringComparison.OrdinalIgnoreCase).Replace("\\\r\n", replacementForNewLine, StringComparison.OrdinalIgnoreCase);

                // Line is formatted: logfileName ";" bookmark.LineNum ";" bookmark.Text;
                var firstSeparator = line.IndexOf(';', StringComparison.OrdinalIgnoreCase);
                var secondSeparator = line.IndexOf(';', firstSeparator + 1);

                var fileStr = line[..firstSeparator];
                var lineStr = line.Substring(firstSeparator + 1, secondSeparator - firstSeparator - 1);
                var comment = line[(secondSeparator + 1)..];

                if (int.TryParse(lineStr, out var lineNum))
                {
                    Entities.Bookmark bookmark = new(lineNum, comment);
                    bookmarkList.Add(lineNum, bookmark);
                }
                else
                {
                    //!!!log error: skipping a line entry
                }
            }
            catch
            {
                //!!!
            }
        }
    }

    #endregion
}