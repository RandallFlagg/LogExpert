﻿using System;
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
            SftpFileSystem sftFileSystem = sftpFileSystem;
            Uri = fileUri;
            _remoteFileName = Uri.PathAndQuery;

            int port = Uri.Port != -1 ? Uri.Port : 22;

            bool success = false;
            bool cancelled = false;
            if (sftFileSystem.ConfigData.UseKeyfile)
            {
                lock (_sshKeyMonitor) // prevent multiple password dialogs when opening multiple files at once
                {
                    while (sftFileSystem.PrivateKeyFile == null)
                    {
                        PrivateKeyPasswordDialog dlg = new();
                        DialogResult dialogResult = dlg.ShowDialog();
                        if (dialogResult == DialogResult.Cancel)
                        {
                            cancelled = true;
                            break;
                        }

                        PrivateKeyFile privateKeyFile = new(sftFileSystem.ConfigData.KeyFile, dlg.Password);

                        if (privateKeyFile != null)
                        {
                            sftFileSystem.PrivateKeyFile = privateKeyFile;
                        }
                        else
                        {
                            MessageBox.Show("Loading key file failed");
                        }
                    }
                }

                if (cancelled == false)
                {
                    success = false;
                    Credentials credentials = sftFileSystem.GetCredentials(Uri, true, true);
                    while (success == false)
                    {
                        //Add ConnectionInfo object
                        _sftp = new SftpClient(Uri.Host, credentials.UserName, sftFileSystem.PrivateKeyFile);

                        if (_sftp != null)
                        {
                            _sftp.Connect();
                            success = true;
                        }

                        if (success == false)
                        {
                            FailedKeyDialog dlg = new();
                            DialogResult res = dlg.ShowDialog();
                            dlg.Dispose();
                            if (res == DialogResult.Cancel)
                            {
                                return;
                            }

                            if (res == DialogResult.OK)
                            {
                                break; // go to user/pw auth
                            }

                            // retries with disabled cache
                            credentials = sftFileSystem.GetCredentials(Uri, false, true);
                        }
                    }
                }
            }

            if (success == false)
            {
                // username/password auth
                Credentials credentials = sftFileSystem.GetCredentials(Uri, true, false);
                _sftp = new SftpClient(Uri.Host, port, credentials.UserName, credentials.Password);

                if (_sftp == null)
                {
                    // first fail -> try again with disabled cache
                    credentials = sftFileSystem.GetCredentials(Uri, false, false);
                    _sftp = new SftpClient(Uri.Host, port, credentials.UserName, credentials.Password);

                    if (_sftp == null)
                    {
                        // 2nd fail -> abort
                        MessageBox.Show("Authentication failed!");
                        //MessageBox.Show(sftp.LastErrorText);
                        return;
                    }
                }
                else
                {
                    _sftp.Connect();
                }
            }

            if (_sftp.IsConnected == false)
            {
                MessageBox.Show("Sftp is not connected");
                return;
            }

            OriginalLength = _lastLength = Length;
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
