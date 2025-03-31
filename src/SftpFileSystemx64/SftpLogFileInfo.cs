using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using LogExpert;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace SftpFileSystem
{
    internal class SftpLogFileInfo : ILogFileInfo
    {
        #region Static/Constants
        //TODO Add to Options
        private const int RETRY_COUNT = 20;
        private const int RETRY_SLEEP = 250;

        #endregion

        #region Private Fields

        private readonly ILogExpertLogger _logger;
        private readonly string _remoteFileName;

        private readonly SftpClient _sftp;
        private readonly object _sshKeyMonitor = new();
        private DateTime _lastChange = DateTime.Now;
        private long _lastLength;

        #endregion

        #region Ctor

        internal SftpLogFileInfo(SftpFileSystem sftpFileSystem, Uri fileUri, ILogExpertLogger logger)
        {
            _logger = logger;
            Uri = fileUri;
            _remoteFileName = Uri.PathAndQuery;
            int port = Uri.Port != -1 ? Uri.Port : 22;

            // Attempt to initialize the SFTP connection
            if (!InitializeSftpConnection(sftpFileSystem, port))
            {
                MessageBox.Show("SFTP connection failed.");
                return;
            }

            OriginalLength = _lastLength = Length;
        }

        private bool InitializeSftpConnection(SftpFileSystem sftpFileSystem, int port)
        {
            if (sftpFileSystem.ConfigData.UseKeyfile)
            {
                if (!LoadPrivateKey(sftpFileSystem))
                {
                    return false;
                }

                // Attempt connection with keyfile
                return TryConnectWithKeyfile(sftpFileSystem, port);
            }

            // Fallback to username/password authentication
            return TryConnectWithCredentials(sftpFileSystem, port);
        }

        private bool LoadPrivateKey(SftpFileSystem sftpFileSystem)
        {
            lock (_sshKeyMonitor)
            {
                while (sftpFileSystem.PrivateKeyFile == null)
                {
                    using PrivateKeyPasswordDialog dlg = new();
                    if (dlg.ShowDialog() == DialogResult.Cancel)
                    {
                        return false;
                    }

                    try
                    {
                        sftpFileSystem.PrivateKeyFile = new PrivateKeyFile(sftpFileSystem.ConfigData.KeyFile, dlg.Password);
                    }
                    catch
                    {
                        MessageBox.Show("Loading key file failed.");
                    }
                }
            }
            return true;
        }

        private bool TryConnectWithKeyfile(SftpFileSystem sftpFileSystem, int port)
        {
            sftpFileSystem.GetCredentials(Uri, true, true);

            while (true)
            {
                _sftp.Connect();

                if (_sftp.IsConnected)
                {
                    return true;
                }

                using FailedKeyDialog dlg = new();
                if (dlg.ShowDialog() == DialogResult.Cancel)
                {
                    return false;
                }

                // Retry with cache disabled
                sftpFileSystem.GetCredentials(Uri, false, true);
            }
        }

        private bool TryConnectWithCredentials(SftpFileSystem sftpFileSystem, int port)
        {
            sftpFileSystem.GetCredentials(Uri, true, false);
            _sftp.Connect();

            if (!_sftp.IsConnected)
            {
                // Retry with cache disabled
                sftpFileSystem.GetCredentials(Uri, false, false);
                _sftp.Connect();

                if (!_sftp.IsConnected)
                {
                    MessageBox.Show("Authentication failed!");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Interface ILogFileInfo

        public string DirectoryName
        {
            get
            {
                string full = FullName;
                int i = full.LastIndexOf(DirectorySeparatorChar);
                if (i != -1)
                {
                    return full.Substring(0, i);
                }

                return ".";
            }
        }

        public char DirectorySeparatorChar => '/';

        public bool FileExists
        {
            get
            {
                try
                {
                    SftpFile file = (SftpFile) _sftp.Get(_remoteFileName);
                    long len = file.Attributes.Size;
                    return len != -1;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return false;
                }
            }
        }

        public string FileName
        {
            get
            {
                string full = FullName;
                int i = full.LastIndexOf(DirectorySeparatorChar);
                return full.Substring(i + 1);
            }
        }

        public string FullName => Uri.ToString();

        public long Length
        {
            get
            {
                SftpFile file = (SftpFile)_sftp.Get(_remoteFileName);
                return file.Attributes.Size;
            }
        }

        public long OriginalLength { get; }

        public int PollInterval
        {
            get
            {
                TimeSpan diff = DateTime.Now - _lastChange;
                if (diff.TotalSeconds < 4)
                {
                    return 400;
                }

                if (diff.TotalSeconds < 30)
                {
                    return (int)diff.TotalSeconds * 100;
                }

                return 5000;
            }
        }

        public Uri Uri { get; }

        public bool FileHasChanged()
        {
            if (Length != _lastLength)
            {
                _lastLength = Length;
                _lastChange = DateTime.Now;
                return true;
            }

            return false;
        }

        public Stream OpenStream()
        {
            int retry = RETRY_COUNT;
            while (true)
            {
                try
                {
                    return _sftp.OpenRead(_remoteFileName);
                }
                catch (IOException)
                {
                    //First remove a try then check if its less or 0
                    if (--retry <= 0)
                    {
                        throw;
                    }

                    Thread.Sleep(RETRY_SLEEP);
                }
            }
        }

        #endregion
    }
}
