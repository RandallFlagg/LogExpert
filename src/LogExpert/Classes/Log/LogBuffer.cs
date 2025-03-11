using System.Collections.Generic;
using NLog;

namespace LogExpert.Classes.Log
{
    public class LogBuffer
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

#if DEBUG
        private readonly IList<long> _filePositions = new List<long>(); // File position for every line
#endif

        private readonly IList<ILogLine> _logLines = new List<ILogLine>();
        private long _size;

        #endregion Fields

        #region Properties

        public long StartPos { get; set; } = 0;

        public bool IsFull => LineCount >= MaxLines;
        //public bool IsFull => Size >= MaxSize;
        //public bool IsFull => SizeTest >= MaxSize;

        public long Size
        {
            get => _size;
            set
            {
                _size = value;
#if DEBUG
                if (_filePositions.Count > 0 && _size < _filePositions[^1] - StartPos)
                {
                    _logger.Error("LogBuffer overall Size must be greater than last line file position!");
                }
#endif
            }
        }

        public int StartLine { get; set; } = 0;

        public int LineCount { get; private set; }

        public bool IsDisposed { get; private set; }

        public ILogFileInfo FileInfo { get; set; }

        public int DroppedLinesCount { get; set; } = 0;

        public int PrevBuffersDroppedLinesSum { get; set; } = 0;

        private int MaxLines { get; }

        private int MaxSize { get; }
        private long _sizeTest = 0;
        public long SizeTest
        {
            get
            {
                return _sizeTest;
            }

            internal set
            {
                _sizeTest = value - SizeTest;
            }
        }

        #endregion Properties

        #region Constructor

        public LogBuffer(ILogFileInfo fileInfo, int maxLines, int maxSize = int.MaxValue)
        {
            FileInfo = fileInfo;
            MaxLines = maxLines;
            MaxSize = maxSize;
        }

        #endregion Constructor

        #region Public Methods

        public void AddLine(ILogLine line, long filePos)
        {
            _logLines.Add(line);
#if DEBUG
            _filePositions.Add(filePos);
#endif
            LineCount++;
            IsDisposed = false;
        }

        public void ClearLines()
        {
            _logLines.Clear();
            LineCount = 0;
        }

        public void DisposeContent()
        {
            _logLines.Clear();
            IsDisposed = true;
#if DEBUG
            DisposeCount++;
#endif
        }

        public ILogLine GetLineOfBlock(int num) => num >= 0 && num < _logLines.Count ? _logLines[num] : null;

#if DEBUG
        public long GetFilePosForLineOfBlock(int line) => line >= 0 && line < _filePositions.Count ? _filePositions[line] : -1;
#endif

#if DEBUG

        public long DisposeCount { get; private set; }

#endif

        #endregion Public Methods
    }
}
