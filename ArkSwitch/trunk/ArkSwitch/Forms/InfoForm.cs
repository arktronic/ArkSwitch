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
        static readonly string NumModulesString = NativeLang.GetNlsString("AppInfo", "NumberOfModules");
        static readonly string NumThreadsString = NativeLang.GetNlsString("AppInfo", "NumberOfThreads");
        static readonly Font MainFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
        static readonly Font SubFont = new Font(FontFamily.GenericSerif, 8, FontStyle.Regular);
        static readonly StringFormat DefaultStringFormat = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

        TaskItem _task;
        ProcessItem _proc;
        bool _processMode;

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
            if (_task == null) return;
            var ex = new ExcludeNewForm { ExePathName = _task.ExePath };
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
            if (MessageBox.Show(NativeLang.GetNlsString("AppInfo", "KillConfirmMsg"),
                                NativeLang.GetNlsString("AppInfo", "KillConfirmTitle"), MessageBoxButtons.YesNo,
                                MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;
            Cursor.Current = Cursors.WaitCursor;
            OpenNETCF.Windows.Forms.ApplicationEx.DoEvents();
            TaskMgmt.Instance.KillProcess(_processMode ? _proc.ProcessId : _task.ProcessId);
            Program.TheForm.RefreshData();
            Cursor.Current = Cursors.Default;
            Close();
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
            var icon = ExeIconMgmt.GetIconForExe(_processMode ? _proc.ExePath : _task.ExePath, true);
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

            // Initialize all the task or process strings that will be drawn.
            var mainString = _processMode ? Path.GetFileName(_proc.ExePath) : Path.GetFileName(_task.ExePath);
            var substring = _processMode
                                ? string.Concat(NativeLang.GetNlsString("AppInfo", "Slot"), " ", _proc.SlotNumber)
                                : _task.Title;
            var location = Path.GetDirectoryName(_processMode ? _proc.ExePath : _task.ExePath);
            var numModules = TaskMgmt.Instance.GetProcessModules(_processMode ? _proc.ProcessId : _task.ProcessId).ToString();
            var numThreads = TaskMgmt.Instance.GetProcessThreads(_processMode ? _proc.ProcessId : _task.ProcessId).ToString();

            // Output the filename and window title (or slot number in case of a raw process).

            rect.X = 6;
            rect.Width = ClientSize.Width - 70;
            var mainStringSize = e.Graphics.MeasureString(mainString, MainFont);
            var subStringSize = e.Graphics.MeasureString(substring, SubFont);
            var combinedHeight = mainStringSize.Height + subStringSize.Height + 1;
            var y = rect.Height / 2 - combinedHeight / 2 + rect.Y;
            var mainRect = new RectangleF(rect.X + 2, y, rect.Width - 2, mainStringSize.Height);
            var subRect = new RectangleF(rect.X + 2, y + mainStringSize.Height + 1, rect.Width - 2, subStringSize.Height);
            using (var brushPrimary = new SolidBrush(Theming.AppInfoTextColorPrimary))
            using (var brushSecondary = new SolidBrush(Theming.AppInfoTextColorSecondary))
            using (var pen = new Pen(Theming.AppInfoDelimiterColor, 2))
            {
                e.Graphics.DrawString(mainString, MainFont, brushPrimary, mainRect, DefaultStringFormat);
                e.Graphics.DrawString(substring, SubFont, brushSecondary, subRect, DefaultStringFormat);
                e.Graphics.DrawLine(pen, 0, (int)rect.Y + (int)rect.Height + 7, ClientSize.Width, (int)rect.Y + (int)rect.Height + 7);
            }

            // Output the rest of the strings.
            rect.Y += 84;
            rect.Height = combinedHeight + 3;
            if (!location.EndsWith(@"\")) location += @"\";
            DrawStrings(e.Graphics, LocatedInString, location, rect);
            rect.Y += rect.Height + 2;
            DrawStrings(e.Graphics, NumModulesString, numModules, rect);
            rect.Y += rect.Height + 2;
            DrawStrings(e.Graphics, NumThreadsString, numThreads, rect);
        }
        #endregion

        #region Methods
        internal void PopulateTask(TaskItem item)
        {
            _processMode = false;
            mnuExclude.Enabled = true;
            _task = item;
            // Redraw everything.
            Invalidate();
        }

        internal void PopulateProc(ProcessItem item)
        {
            _processMode = true;
            mnuExclude.Enabled = false;
            _proc = item;
            // Redraw everything.
            Invalidate();
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