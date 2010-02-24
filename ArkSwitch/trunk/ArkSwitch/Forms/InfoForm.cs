/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Drawing;

namespace ArkSwitch.Forms
{
    public partial class InfoForm : Form
    {
        static readonly string LocatedInString = NativeLang.GetNlsString("AppInfo", "LocatedIn");
        static readonly string MemoryUsageString = NativeLang.GetNlsString("AppInfo", "MemoryUsage");
        static readonly string EfficiencyIndexString = NativeLang.GetNlsString("AppInfo", "EfficiencyIndex");
        static readonly Font MainFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
        static readonly Font SubFont = new Font(FontFamily.GenericSerif, 8, FontStyle.Regular);
        static readonly StringFormat DefaultStringFormat = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

        TaskItem _item;

        public InfoForm()
        {
            InitializeComponent();

            // Initialize NLS strings.
            this.Text = NativeLang.GetNlsString("AppInfo", this.Text);
            mnuExclude.Text = NativeLang.GetNlsString("AppInfo", mnuExclude.Text);
            mnuKill.Text = NativeLang.GetNlsString("AppInfo", mnuKill.Text);
        }

        #region Event handling
        private void mnuExclude_Click(object sender, EventArgs e)
        {
            if (_item == null) return;
            var ex = new ExcludeNewForm { ExePathName = _item.ExePath };
            try
            {
                if (ex.ShowDialog() == DialogResult.OK)
                {
                    var excl = ex.ExePathName.ToLower().Trim();
                    if (excl.Length > 0 && !Program.ExcludedExes.Contains(excl))
                    {
                        Program.ExcludedExes.Add(excl);
                        AppSettings.SetExcludedExes();
                        Program.TheForm.RefreshData();
                        Close();
                    }
                }
            }
            finally
            {
                ex.Dispose();
            }
        }

        private void mnuKill_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(NativeLang.GetNlsString("AppInfo", "KillConfirmMsg"), NativeLang.GetNlsString("AppInfo", "KillConfirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                TaskMgmt.Instance.KillProcess(_item.ProcessId);
                Program.TheForm.RefreshData();
                Close();
            }
        }

        private void InfoForm_Activated(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Default;
            BackColor = Theming.AppInfoBackgroundColor;
        }

        private void InfoForm_Paint(object sender, PaintEventArgs e)
        {
            // Draw the background image, if we have one.
            if (Theming.AppInfoBackgroundImage != null)
            {
                e.Graphics.DrawImage(Theming.AppInfoBackgroundImage, 0, 0);
            }

            // Draw the application's icon.
            var rect = new RectangleF(ClientSize.Width - 70, 6, 64, 75);
            var icon = ExeIconMgmt.GetIconForExe(_item.ExePath, true);
            if (icon != null)
            {
                if (icon.Height <= rect.Height && icon.Width <= rect.Width)
                    e.Graphics.DrawIcon(icon, (int)rect.Width / 2 - icon.Width / 2 + (int)rect.X, (int)rect.Height / 2 - icon.Height / 2 + (int)rect.Y);
                else
                {
                    var bmp = ExeIconMgmt.GetBitmapFromIcon(icon, true);
                    e.Graphics.DrawImage(bmp, Misc.CalculateCenteredScaledDestRect(rect, bmp.Size, false), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                    bmp.Dispose();
                }
            }
            else
                e.Graphics.DrawImageAlphaChannel(MainForm.NoIconImage, Misc.CalculateCenteredScaledDestRect(rect, MainForm.NoIconImageSize, false));

            // Output the filename and window title.
            rect.X = 6;
            rect.Width = ClientSize.Width - 70;
            var mainStringSize = e.Graphics.MeasureString(Path.GetFileName(_item.ExePath), MainFont);
            var subStringSize = e.Graphics.MeasureString(_item.Title, SubFont);
            var combinedHeight = mainStringSize.Height + subStringSize.Height + 1;
            var y = rect.Height / 2 - combinedHeight / 2 + rect.Y;
            var mainRect = new RectangleF(rect.X + 2, y, rect.Width - 2, mainStringSize.Height);
            var subRect = new RectangleF(rect.X + 2, y + mainStringSize.Height + 1, rect.Width - 2, subStringSize.Height);
            using (var brushPrimary = new SolidBrush(Theming.AppInfoTextColorPrimary))
            using (var brushSecondary = new SolidBrush(Theming.AppInfoTextColorSecondary))
            using (var pen = new Pen(Theming.AppInfoDelimiterColor, 2))
            {
                e.Graphics.DrawString(Path.GetFileName(_item.ExePath), MainFont, brushPrimary, mainRect, DefaultStringFormat);
                e.Graphics.DrawString(_item.Title, SubFont, brushSecondary, subRect, DefaultStringFormat);
                e.Graphics.DrawLine(pen, 0, (int)rect.Y + (int)rect.Height + 7, ClientSize.Width, (int)rect.Y + (int)rect.Height + 7);
            }

            // Output the rest of the strings.
            rect.Y += 84;
            rect.Height = combinedHeight + 3;
            var loc = Path.GetDirectoryName(_item.ExePath);
            if (!loc.EndsWith(@"\")) loc += @"\";
            DrawStrings(e.Graphics, LocatedInString, loc, rect);
            rect.Y += rect.Height + 2;
            DrawStrings(e.Graphics, MemoryUsageString, TaskMgmt.FormatMemoryString(_item.HeapSize), rect);
            rect.Y += rect.Height + 2;
            DrawStrings(e.Graphics, EfficiencyIndexString, CalculateEfficiencyIndex(), rect);
        }
        #endregion

        #region Methods
        internal void PopulateTask(TaskItem item)
        {
            _item = item;
            // Redraw everything.
            Invalidate();
        }

        /// <summary>
        /// This function calculates the efficiency index.
        /// When I first wrote it, I thought it made a lot of sense. Now, I think it makes some sense, but not too much.
        /// Memory management in WM6.x is interesting, especially concerning the application memory slots, so in the end,
        /// it appears that most information about device memory is pretty useless.
        /// Nevertheless, I'm keeping this here for now, as it is still an indicator of sorts about how an application
        /// functions.
        /// </summary>
        /// <returns></returns>
        string CalculateEfficiencyIndex()
        {
            var mods = TaskMgmt.Instance.GetProcessModules(_item.ProcessId);
            int eff = 100;
            foreach (var mod in mods)
            {
                var slot = TaskMgmt.Instance.GetSlotNumber(mod.BaseAddress);
                if (slot >= 59)
                {
                    eff += (mod.GlobalUsage > 1 ? 5 : 1);
                }
                else if (slot == 1)
                {
                    eff += (mod.GlobalUsage > 1 ? 1 : -5);
                }
                else if (slot == 0)
                {
                    eff--;
                }
            }
            eff = (int)(eff / (mods.Count / 10f));
            return eff.ToString();
        }

        /// <summary>
        /// Draws a single piece of information on the provided graphics object.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <param name="rect"></param>
        void DrawStrings(Graphics graphics, string str1, string str2, RectangleF rect)
        {
            var firstStringSize = graphics.MeasureString(str1, SubFont);
            var secondStringSize = graphics.MeasureString(str2, MainFont);
            var combinedHeight = firstStringSize.Height + secondStringSize.Height + 1;
            var y = rect.Height / 2 - combinedHeight / 2 + rect.Y;
            var firstRect = new RectangleF(rect.X + 2, y, rect.Width - 2, firstStringSize.Height);
            var secondRect = new RectangleF(rect.X + 2, y + firstStringSize.Height + 1, rect.Width - 2, secondStringSize.Height);
            using (var brushPrimary = new SolidBrush(Theming.AppInfoTextColorPrimary))
            using (var brushSecondary = new SolidBrush(Theming.AppInfoTextColorSecondary))
            using (var pen = new Pen(Theming.AppInfoDelimiterColor, 2))
            {
                graphics.DrawString(str1, SubFont, brushSecondary, firstRect, DefaultStringFormat);
                graphics.DrawString(str2, MainFont, brushPrimary, secondRect, DefaultStringFormat);
                graphics.DrawLine(pen, 0, (int)rect.Y + (int)rect.Height, ClientSize.Width, (int)rect.Y + (int)rect.Height);
            }
        }
        #endregion
    }
}