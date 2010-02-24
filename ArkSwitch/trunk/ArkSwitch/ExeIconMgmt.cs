/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace ArkSwitch
{
    static class ExeIconMgmt
    {
        public static Icon GetIconForExe(string path, bool large)
        {
            IntPtr[] icons = null;
            try
            {
                icons = new[] { IntPtr.Zero };
                uint readCount = ExtractIconEx(path, 0, large ? icons : null, large ? null : icons, 1);
                if (readCount < 1) return null;
                IntPtr iconPtr = icons[0];
                if (iconPtr == IntPtr.Zero) return null;
                return (Icon)Icon.FromHandle(iconPtr).Clone();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (icons != null && icons.Length > 0 && icons[0] != IntPtr.Zero)
                {
                    // It is very important to free any used memory.
                    DestroyIcon(icons[0]);
                }
            }

        }

        public static Bitmap GetBitmapFromIcon(Icon icon, bool whiteFill)
        {
            if (icon == null) return null;
            var bmp = new Bitmap(icon.Width, icon.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(new SolidBrush(whiteFill ? Color.White : Color.FromArgb(255, 0, 220)), 0, 0, bmp.Width, bmp.Height);
                g.DrawIcon(icon, 0, 0);
            }
            return bmp;
        }

        #region P/Invoke related stuff
        [DllImport("coredll.dll")]
        static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("coredll.dll")]
        static extern int DestroyIcon(IntPtr hIcon);
        #endregion
    }
}
