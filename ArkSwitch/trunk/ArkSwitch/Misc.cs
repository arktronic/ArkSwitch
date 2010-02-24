/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace ArkSwitch
{
    static class Misc
    {
        #region P/Invoke-related stuff
        const int GWL_STYLE = -16;
        const int WS_BORDER = 0x00800000;
        const int SWP_NOSIZE = 0x1;
        const int SWP_NOMOVE = 0x2;
        const int SWP_FRAMECHANGED = 0x20;

        [DllImport("coredll.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("coredll.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uflags);

        [DllImport("coredll.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        #endregion

        public static void SetControlBorder(this Control control, bool borderOn)
        {
            var style = GetWindowLong(control.Handle, GWL_STYLE);

            if (borderOn)
                style |= WS_BORDER;
            else
                style &= ~WS_BORDER;

            SetWindowLong(control.Handle, GWL_STYLE, style);
            SetWindowPos(control.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);
        }

        public static string GetApplicationDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
        }

        public static Rectangle CalculateCenteredScaledDestRect(Rectangle dest, Size srcSize, bool scaleDownOnly)
        {
            return CalculateCenteredScaledDestRect(new RectangleF(dest.X, dest.Y, dest.Width, dest.Height), srcSize, scaleDownOnly);
        }

        public static Rectangle CalculateCenteredScaledDestRect(RectangleF dest, Size srcSize, bool scaleDownOnly)
        {
            // Local vars.
            int pixelDelta;
            var x = (int)dest.X;
            var y = (int)dest.Y;

            // Scale.
            if (dest.Width < dest.Height)
            {
                // Width is the limiting factor.
                pixelDelta = (int)dest.Width - srcSize.Width;
                // Width is now optimal. Check if height is too high. It's okay if it's low.
                if ((int)dest.Height < srcSize.Height + pixelDelta)
                {
                    // Adjust pixelDelta to compensate.
                    pixelDelta -= (srcSize.Height + pixelDelta - (int)dest.Height);
                }
            }
            else
            {
                // Height is the limiting factor (or height == width).
                pixelDelta = (int)dest.Height - srcSize.Height;
                // Height is now optimal. Check if width is too high. It's okay if it's low.
                if ((int)dest.Width < srcSize.Width + pixelDelta)
                {
                    // Adjust pixelDelta to compensate.
                    pixelDelta -= (srcSize.Width + pixelDelta - (int)dest.Width);
                }
            }

            if (pixelDelta > 0 && scaleDownOnly)
            {
                // Center only.
                x = (int)dest.Width / 2 - srcSize.Width / 2 + (int)dest.X;
                y = (int)dest.Height / 2 - srcSize.Height / 2 + (int)dest.Y;
                // Return the result.
                return new Rectangle(x, y, srcSize.Width, srcSize.Height);
            }

            // Center.
            if ((int)dest.Width != srcSize.Width + pixelDelta)
                x = (int)dest.Width / 2 - (srcSize.Width + pixelDelta) / 2 + (int)dest.X;
            if ((int)dest.Height != srcSize.Height + pixelDelta)
                y = (int)dest.Height / 2 - (srcSize.Height + pixelDelta) / 2 + (int)dest.Y;
            // Return the result.
            return new Rectangle(x, y, srcSize.Width + pixelDelta, srcSize.Height + pixelDelta);
        }
    }
}
