﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntiKeyLogger
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice,
        IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll")]
        private static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        public static extern bool CloseDesktop(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        public static extern IntPtr GetThreadDesktop(int dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        enum DESKTOP_ACCESS : uint
        {
            DESKTOP_NONE = 0,
            DESKTOP_READOBJECTS = 0x0001,
            DESKTOP_CREATEWINDOW = 0x0002,
            DESKTOP_CREATEMENU = 0x0004,
            DESKTOP_HOOKCONTROL = 0x0008,
            DESKTOP_JOURNALRECORD = 0x0010,
            DESKTOP_JOURNALPLAYBACK = 0x0020,
            DESKTOP_ENUMERATE = 0x0040,
            DESKTOP_WRITEOBJECTS = 0x0080,
            DESKTOP_SWITCHDESKTOP = 0x0100,

            GENERIC_ALL = (DESKTOP_READOBJECTS | DESKTOP_CREATEWINDOW | DESKTOP_CREATEMENU |
                            DESKTOP_HOOKCONTROL | DESKTOP_JOURNALRECORD | DESKTOP_JOURNALPLAYBACK |
                            DESKTOP_ENUMERATE | DESKTOP_WRITEOBJECTS | DESKTOP_SWITCHDESKTOP),
        }

        static void Main(string[] args)
        {
            // old desktop's handle, obtained by getting the current desktop assigned for this thread
            IntPtr hOldDesktop = GetThreadDesktop(GetCurrentThreadId());

            // new desktop's handle, assigned automatically by CreateDesktop
            IntPtr hNewDesktop = CreateDesktop("RandomDesktopName",
            IntPtr.Zero, IntPtr.Zero, 0, (uint)DESKTOP_ACCESS.GENERIC_ALL, IntPtr.Zero);

            // switching to the new desktop
            SwitchDesktop(hNewDesktop);

            // Random login form: used for testing / not required
            string passwd = "";

            // running on a different thread, this way SetThreadDesktop won't fail
            Task.Factory.StartNew(() =>
            {
                // assigning the new desktop to this thread - 
                // so the Form will be shown in the new desktop)
                SetThreadDesktop(hNewDesktop);

                Form loginWnd = new Form();
                TextBox passwordTextBox = new TextBox();
                passwordTextBox.Location = new Point(10, 30);
                passwordTextBox.Width = 250;
                passwordTextBox.Font = new Font("Arial", 20, FontStyle.Regular);

                loginWnd.Controls.Add(passwordTextBox);
                loginWnd.FormClosing += (sender, e) => { passwd = passwordTextBox.Text; };

                Application.Run(loginWnd);

            }).Wait();  // waits for the task to finish

            // end of login form

            // if got here, the form is closed => switch back to the old desktop
            SwitchDesktop(hOldDesktop);

            // disposing the secure desktop since it's no longer needed
            CloseDesktop(hNewDesktop);

            Console.WriteLine("Password, typed inside secure desktop: " + passwd);
            Console.ReadLine();
        }
    }
}