using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exception = Mono.WebBrowser.Exception;

namespace LogExpert.Config
{
    public static class ColorMode
    {
        // Bright Theme
        // https://paletton.com/#uid=15-0u0k00sH00kJ0pq+00RL00RL
        private static readonly Color BrightBookmarkDefaultSystemColor = SystemColors.Control; // Important: only supports SystemColors
        private static readonly Color LessBrightBackgroundColor = Color.FromArgb(208, 205, 206);
        private static readonly Color BrightBackgroundColor = Color.FromArgb(221, 221, 221);
        private static readonly Color BrighterBackgroundColor = Color.FromArgb(253, 253, 253);
        private static readonly Color BrightForeColor = Color.FromArgb(0, 0, 0);

        // Dark Theme
        // https://paletton.com/#uid=15-0u0k005U0670008J003Y003Y
        private static readonly Color DarkBookmarkDefaultSystemColor = SystemColors.ControlDarkDark; // Important: only supports SystemColors
        private static readonly Color LessLessDarkBackgroundColor = Color.FromArgb(90, 90, 90);
        private static readonly Color LessDarkBackgroundColor = Color.FromArgb(67, 67, 67);
        private static readonly Color DarkBackgroundColor = Color.FromArgb(45, 45, 45);
        private static readonly Color DarkerBackgroundColor = Color.FromArgb(30, 30, 30);
        private static readonly Color DarkForeColor = Color.FromArgb(255, 255, 255);

        // Default
        public static Color BackgroundColor = BrightBackgroundColor;
        public static Color DockBackgroundColor = BrighterBackgroundColor;
        public static Color BookmarksDefaultBackgroundColor = BrightBookmarkDefaultSystemColor;
        public static Color ForeColor = BrightForeColor;
        public static Color MenuBackgroundColor = BrighterBackgroundColor;
        public static Color HoverMenuBackgroundColor = LessBrightBackgroundColor;
        public static Color ActiveTabColor = BrighterBackgroundColor;
        public static Color InactiveTabColor = LessBrightBackgroundColor;
        public static Color TabsBackgroundStripColor = LessBrightBackgroundColor;


        public static bool DarkModeEnabled;

        public static void LoadColorMode()
        {
            var preferences = ConfigManager.Settings.Preferences;

            if (preferences.darkMode)
            {
                SetDarkMode();
            }
            else
            {
                SetBrightMode();
            }
        }

        private static void SetDarkMode()
        {
            BackgroundColor = DarkBackgroundColor;
            ForeColor = DarkForeColor;
            MenuBackgroundColor = DarkerBackgroundColor;
            DockBackgroundColor = LessDarkBackgroundColor;
            HoverMenuBackgroundColor = LessDarkBackgroundColor;
            BookmarksDefaultBackgroundColor = DarkBookmarkDefaultSystemColor;
            TabsBackgroundStripColor = LessDarkBackgroundColor;
            ActiveTabColor = LessLessDarkBackgroundColor;
            InactiveTabColor = LessDarkBackgroundColor;
            DarkModeEnabled = true;
        }

        private static void SetBrightMode()
        {
            BackgroundColor = BrightBackgroundColor;
            ForeColor = BrightForeColor;
            MenuBackgroundColor = BrighterBackgroundColor;
            DockBackgroundColor = BrighterBackgroundColor;
            BookmarksDefaultBackgroundColor = BrightBookmarkDefaultSystemColor;
            HoverMenuBackgroundColor = LessBrightBackgroundColor;
            TabsBackgroundStripColor = BrighterBackgroundColor;
            ActiveTabColor = BrighterBackgroundColor;
            InactiveTabColor = LessBrightBackgroundColor;
            DarkModeEnabled = false;
        }

        #region TitleBarDarkMode
        #if WINDOWS2
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        #elif LINUX_WAYLAND
        [DllImport("libwayland-client.so")]
    private static extern IntPtr wl_display_connect(string name);

    [DllImport("libwayland-client.so")]
    private static extern void wl_display_disconnect(IntPtr display);

    [DllImport("libwayland-client.so")]
    private static extern IntPtr wl_registry_bind(IntPtr registry, uint name, IntPtr interfacePointer, uint version);

    [DllImport("libwayland-client.so")]
    private static extern IntPtr wl_surface_create(IntPtr compositor);
        #endif

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

            #if LINUX_WAYLAND
        // This is a hypothetical function to set an attribute
        // You would need to implement the logic by interacting with Wayland protocols.
        private static bool SetWaylandWindowAttribute(IntPtr handle, int attribute, ref int value, int size)
        {
            // Hypothetical implementation
            // In Wayland, you'd need to interact with a protocol, such as xdg_toplevel or others.

            Console.WriteLine($"Setting attribute {attribute} to value {value}");
            // Simulate success
            return true;
        }

        public static bool SetDarkMode(IntPtr handle)
        {
            int useDarkMode = 1; // Enable dark mode
            const int darkModeAttribute = 20; // Hypothetical attribute ID
            return SetWaylandWindowAttribute(handle, darkModeAttribute, ref useDarkMode, sizeof(int));
        }
        #endif
        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {

            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            if (IsWindows10OrGreater(18985))
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            int useImmersiveDarkMode = enabled ? 1 : 0;
            #if WINDOWS2
            return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            #elif LINUX_WAYLAND
            IntPtr display = wl_display_connect(null);
        if (display == IntPtr.Zero)
        {
            Console.WriteLine("Failed to connect to Wayland display!");
            throw new ApplicationException("Failed to connect to Wayland display!");
        }

        Console.WriteLine("Connected to Wayland display!");

        // Use a hypothetical handle (replace with actual Wayland surface handle)
        IntPtr windowHandle = IntPtr.Zero;

        var result = SetDarkMode(windowHandle);

        wl_display_disconnect(display);
            
            return result;
            #endif

        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        #endregion TitleBarDarkMode

    }
}
