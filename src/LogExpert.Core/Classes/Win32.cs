using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LogExpert.Core.Classes
{
    [SupportedOSPlatform("windows")]
    public static class Win32
    {
        #region Fields

        public const long SM_CYVSCROLL = 20;
        public const long SM_CXHSCROLL = 21;
        public const long SM_CXVSCROLL = 2;
        public const long SM_CYHSCROLL = 3;

        #endregion

        #region Public methods
        //TODO: Take out all the dllimport out of LogExpert.Core.
        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(nint hIcon);

        public static Icon LoadIconFromExe(string fileName, int index)
        {
            //IntPtr[] smallIcons = new IntPtr[1];
            //IntPtr[] largeIcons = new IntPtr[1];
            nint smallIcons = new();
            nint largeIcons = new();
            int num = (int)ExtractIconEx(fileName, index, ref largeIcons, ref smallIcons, 1);
            if (num > 0 && smallIcons != nint.Zero)
            {
                Icon icon = (Icon)Icon.FromHandle(smallIcons).Clone();
                DestroyIcon(smallIcons);
                return icon;
            }
            if (num > 0 && largeIcons != nint.Zero)
            {
                Icon icon = (Icon)Icon.FromHandle(largeIcons).Clone();
                DestroyIcon(largeIcons);
                return icon;
            }
            return null;
        }


        public static Icon[,] ExtractIcons(string fileName)
        {
            nint smallIcon = nint.Zero;
            nint largeIcon = nint.Zero;
            int iconCount = (int)ExtractIconEx(fileName, -1, ref largeIcon, ref smallIcon, 0);
            if (iconCount <= 0)
            {
                return null;
            }

            nint smallIcons = new();
            nint largeIcons = new();
            Icon[,] result = new Icon[2, iconCount];

            for (int i = 0; i < iconCount; ++i)
            {
                int num = (int)ExtractIconEx(fileName, i, ref largeIcons, ref smallIcons, 1);
                if (smallIcons != nint.Zero)
                {
                    result[0, i] = (Icon)Icon.FromHandle(smallIcons).Clone();
                    DestroyIcon(smallIcons);
                }
                else
                {
                    result[0, i] = null;
                }
                if (num > 0 && largeIcons != nint.Zero)
                {
                    result[1, i] = (Icon)Icon.FromHandle(largeIcons).Clone();
                    DestroyIcon(largeIcons);
                }
                else
                {
                    result[1, i] = null;
                }
            }
            return result;
        }

        [DllImport("user32.dll")]
        public static extern long GetSystemMetricsForDpi(long index);


        [DllImport("user32.dll")]
        public static extern long GetSystemMetrics(long index);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int vKey);

        #endregion

        #region Private Methods

        /*
  UINT ExtractIconEx(
      LPCTSTR lpszFile,
      int nIconIndex,
      HICON *phiconLarge,
      HICON *phiconSmall,
      UINT nIcons
  );
       * */

        [DllImport("shell32.dll")]
        private static extern uint ExtractIconEx(string fileName,
            int iconIndex,
            ref nint iconsLarge,
            ref nint iconsSmall,
            uint numIcons
        );

        #endregion
    }
}