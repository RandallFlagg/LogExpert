using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogExpert.Core.Classes.Columnizer;

public class ClfColumnizer : ILogLineColumnizer
{
    #region Fields

    private readonly Regex lineRegex = new("(.*) (-) (.*) (\\[.*\\]) (\".*\") (.*) (.*) (\".*\") (\".*\")");

    protected CultureInfo cultureInfo = new("de-DE");
    protected int timeOffset;

    #endregion

    #region cTor

    // anon-212-34-174-126.suchen.de - - [08/Mar/2008:00:41:10 +0100] "GET /wiki/index.php?title=Bild:Poster_small.jpg&printable=yes&printable=yes HTTP/1.1" 304 0 "http://www.captain-kloppi.de/wiki/index.php?title=Bild:Poster_small.jpg&printable=yes" "gonzo1[P] +http://www.suchen.de/faq.html" 

    public ClfColumnizer()
    {
    }

    #endregion

    #region Public methods

    public bool IsTimeshiftImplemented()
    {
        return true;
    }

    public void SetTimeOffset(int msecOffset)
    {
        timeOffset = msecOffset;
    }

    public int GetTimeOffset()
    {
        return timeOffset;
    }

    public DateTime GetTimestamp(LogExpert.ILogLineColumnizerCallback callback, ILogLine line)
    {
        IColumnizedLogLine cols = SplitLine(callback, line);
        if (cols == null || cols.ColumnValues.Length < 8)
        {
            return DateTime.MinValue;
        }

        if (cols.ColumnValues[2].FullValue.Length == 0)
        {
            return DateTime.MinValue;
        }

        try
        {
            var dateTime = DateTime.ParseExact(cols.ColumnValues[2].FullValue, "dd/MMM/yyyy:HH:mm:ss zzz",
                new CultureInfo("en-US"));
            return dateTime;
        }
        catch (Exception)
        {
            return DateTime.MinValue;
        }
    }

    public void PushValue(LogExpert.ILogLineColumnizerCallback callback, int column, string value, string oldValue)
    {
        if (column == 2)
        {
            try
            {
                var newDateTime =
                    DateTime.ParseExact(value, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
                var oldDateTime =
                    DateTime.ParseExact(oldValue, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
                var mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                var mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                timeOffset = (int)(mSecsNew - mSecsOld);
            }
            catch (FormatException)
            {
            }
        }
    }

    public string GetName()
    {
        return "Webserver CLF Columnizer";
    }

    public string GetDescription()
    {
        return "Common Logfile Format used by webservers.";
    }

    public int GetColumnCount()
    {
        return 8;
    }

    public string[] GetColumnNames()
    {
        return ["IP", "User", "Date/Time", "Request", "Status", "Bytes", "Referrer", "User agent"];
    }

    public IColumnizedLogLine SplitLine(LogExpert.ILogLineColumnizerCallback callback, ILogLine line)
    {
        ColumnizedLogLine cLogLine = new()
        {
            LogLine = line
        };

        var columns = new Column[8]
        {
            new() {FullValue = "", Parent = cLogLine},
            new() {FullValue = "", Parent = cLogLine},
            new() {FullValue = "", Parent = cLogLine},
            new() {FullValue = "", Parent = cLogLine},
            new() {FullValue = "", Parent = cLogLine},
            new() {FullValue = "", Parent = cLogLine},
            new() {FullValue = "", Parent = cLogLine},
            new() {FullValue = "", Parent = cLogLine}
        };

        cLogLine.ColumnValues = columns.Select(a => a as IColumn).ToArray();

        var temp = line.FullLine;
        if (temp.Length > 1024)
        {
            // spam 
            temp = temp.Substring(0, 1024);
            columns[3].FullValue = temp;
            return cLogLine;
        }
        // 0         1         2         3         4         5         6         7         8         9         10        11        12        13        14        15        16
        // anon-212-34-174-126.suchen.de - - [08/Mar/2008:00:41:10 +0100] "GET /wiki/index.php?title=Bild:Poster_small.jpg&printable=yes&printable=yes HTTP/1.1" 304 0 "http://www.captain-kloppi.de/wiki/index.php?title=Bild:Poster_small.jpg&printable=yes" "gonzo1[P] +http://www.suchen.de/faq.html" 

        if (lineRegex.IsMatch(temp))
        {
            Match match = lineRegex.Match(temp);
            GroupCollection groups = match.Groups;
            if (groups.Count == 10)
            {
                columns[0].FullValue = groups[1].Value;
                columns[1].FullValue = groups[3].Value;
                columns[3].FullValue = groups[5].Value;
                columns[4].FullValue = groups[6].Value;
                columns[5].FullValue = groups[7].Value;
                columns[6].FullValue = groups[8].Value;
                columns[7].FullValue = groups[9].Value;

                var dateTimeStr = groups[4].Value.Substring(1, 26);

                // dirty probing of date/time format (much faster than DateTime.ParseExact()
                if (dateTimeStr[2] == '/' && dateTimeStr[6] == '/' && dateTimeStr[11] == ':')
                {
                    if (timeOffset != 0)
                    {
                        try
                        {
                            var dateTime = DateTime.ParseExact(dateTimeStr, "dd/MMM/yyyy:HH:mm:ss zzz",
                                new CultureInfo("en-US"));
                            dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, timeOffset));
                            var newDate = dateTime.ToString("dd/MMM/yyyy:HH:mm:ss zzz",
                                new CultureInfo("en-US"));
                            columns[2].FullValue = newDate;
                        }
                        catch (Exception)
                        {
                            columns[2].FullValue = "n/a";
                        }
                    }
                    else
                    {
                        columns[2].FullValue = dateTimeStr;
                    }
                }
                else
                {
                    columns[2].FullValue = dateTimeStr;
                }
            }
        }
        else
        {
            columns[3].FullValue = temp;
        }

        return cLogLine;
    }

    #endregion
}