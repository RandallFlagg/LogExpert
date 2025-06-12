using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LogExpert.UI.Extensions;

[SupportedOSPlatform("windows")]
internal static partial class Win32 //NativeMethods
{
    #region Fields

    public const long SM_CYVSCROLL = 20;
    public const long SM_CXHSCROLL = 21;
    public const long SM_CXVSCROLL = 2;
    public const long SM_CYHSCROLL = 3;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    #endregion

    #region Library Imports
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon (nint hIcon);

    [LibraryImport("User32.dll")]
    public static partial int SetForegroundWindow (nint hWnd);

    [LibraryImport("user32.dll")]
    public static partial long GetSystemMetricsForDpi (long index);

    [LibraryImport("user32.dll")]
    public static partial long GetSystemMetrics (long index);

    [LibraryImport("user32.dll")]
    public static partial short GetKeyState (int vKey);

    /*
    UINT ExtractIconEx(
    LPCTSTR lpszFile,
    int nIconIndex,
    HICON *phiconLarge,
    HICON *phiconSmall,
    UINT nIcons
    );
    * */
    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint ExtractIconEx (
        string fileName,
        int iconIndex,
        ref nint iconsLarge,
        ref nint iconsSmall,
        uint numIcons
    );

    #region TitleBarDarkMode
    [LibraryImport("dwmapi.dll")]
    public static partial int DwmSetWindowAttribute (nint hwnd, int attr, ref int attrValue, int attrSize);
    #endregion
    #endregion

    #region Public methods

    public static Icon LoadIconFromExe (string fileName, int index)
    {
        //IntPtr[] smallIcons = new IntPtr[1];
        //IntPtr[] largeIcons = new IntPtr[1];
        nint smallIcons = new();
        nint largeIcons = new();
        var num = (int)ExtractIconEx(fileName, index, ref largeIcons, ref smallIcons, 1);
        if (num > 0 && smallIcons != nint.Zero)
        {
            var icon = (Icon)Icon.FromHandle(smallIcons).Clone();
            DestroyIcon(smallIcons);
            return icon;
        }
        if (num > 0 && largeIcons != nint.Zero)
        {
            var icon = (Icon)Icon.FromHandle(largeIcons).Clone();
            DestroyIcon(largeIcons);
            return icon;
        }
        return null;
    }

    public static Icon[,] ExtractIcons (string fileName)
    {
        var smallIcon = nint.Zero;
        var largeIcon = nint.Zero;
        var iconCount = (int)ExtractIconEx(fileName, -1, ref largeIcon, ref smallIcon, 0);
        if (iconCount <= 0)
        {
            return null;
        }

        nint smallIcons = new();
        nint largeIcons = new();
        var result = new Icon[2, iconCount];

        for (var i = 0; i < iconCount; ++i)
        {
            var num = (int)ExtractIconEx(fileName, i, ref largeIcons, ref smallIcons, 1);
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

    #endregion

    #region Private Methods

    public static bool UseImmersiveDarkMode (nint handle, bool enabled)
    {

        var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
        if (IsWindows10OrGreater(18985))
        {
            attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
        }

        var useImmersiveDarkMode = enabled ? 1 : 0;
        return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;

    }

    private static bool IsWindows10OrGreater (int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }

    #endregion TitleBarDarkMode

}