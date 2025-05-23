﻿using LogExpert;

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Forms;

[assembly: SupportedOSPlatform("windows")]
namespace FlashIconHighlighter
{
    internal class FlashIconPlugin : IKeywordAction
    {
        #region Properties

        public string Text
        {
            get { return GetName(); }
        }

        #endregion

        #region IKeywordAction Member

        public void Execute(string keyword, string param, ILogExpertCallback callback, ILogLineColumnizer columnizer)
        {
            FormCollection openForms = Application.OpenForms;
            foreach (Form form in openForms)
            {
                if (form.TopLevel && form.Name.Equals("LogTabWindow") && form.Text.Contains(callback.GetFileName()))
                {
                    form.BeginInvoke(FlashWindow, [form]);
                }
            }
        }

        private void FlashWindow(Form form)
        {
            FLASHWINFO fw = new FLASHWINFO();

            fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO)));
            fw.hwnd = form.Handle;
            fw.dwFlags = 14;
            fw.uCount = 0;

            Win32Stuff.FlashWindowEx(ref fw);
        }

        public string GetDescription()
        {
            return "Let the taskbar icon flash ";
        }

        public string GetName()
        {
            return "Flash Icon";
        }

        #endregion
    }
}