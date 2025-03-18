using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal static class Win32Helper
    {
        #region Public methods

        public static Control ControlAtPoint(Point pt)
        {
#if WINDOWS
            return Control.FromChildHandle(NativeMethods.WindowFromPoint(pt));
#else
            foreach (Form form in Application.OpenForms)
            {
                if (FindControlAtPoint(form, pt) != null)
                {
                    return form;
                }
            }

            return null;
#endif
        }
        
        private static Control FindControlAtPoint(Form form, Point point)
        {
            return form.Controls.Cast<Control>().FirstOrDefault(control => control.Bounds.Contains(point));
        }

        public static uint MakeLong(int low, int high)
        {
            return (uint) ((high << 16) + low);
        }

        #endregion
    }
}