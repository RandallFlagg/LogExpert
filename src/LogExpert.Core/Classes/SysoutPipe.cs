using NLog;

using System.Diagnostics;
using System.Text;

namespace LogExpert.Core.Classes
{
    public class SysoutPipe
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly StreamReader sysout;
        private StreamWriter writer;

        #endregion

        #region cTor

        public SysoutPipe(StreamReader sysout)
        {
            this.sysout = sysout;
            FileName = Path.GetTempFileName();
            _logger.Info("sysoutPipe created temp file: {0}", FileName);

            FileStream fStream = new(FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(fStream, Encoding.Unicode);

            Thread thread = new(new ThreadStart(ReaderThread))
            {
                IsBackground = true
            };
            thread.Start();
        }

        #endregion

        #region Properties

        public string FileName { get; }

        #endregion

        #region Public methods

        public void ClosePipe()
        {
            writer.Close();
            writer = null;
        }


        public void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            writer.WriteLine(e.Data);
        }

        public void ProcessExitedEventHandler(object sender, System.EventArgs e)
        {
            //ClosePipe();
            if (sender.GetType() == typeof(Process))
            {
                ((Process)sender).Exited -= ProcessExitedEventHandler;
                ((Process)sender).OutputDataReceived -= DataReceivedEventHandler;
            }
        }

        #endregion

        protected void ReaderThread()
        {
            char[] buff = new char[256];

            while (true)
            {
                try
                {
                    int read = sysout.Read(buff, 0, 256);
                    if (read == 0)
                    {
                        break;
                    }
                    writer.Write(buff, 0, read);
                }
                catch (IOException e)
                {
                    _logger.Error(e);
                    break;
                }
            }

            ClosePipe();
        }
    }
}