/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Drawing;
using System.IO;

namespace ArkSwitch.Forms
{
    public partial class MainForm : Form
    {
        bool _firstStart = true;

        static readonly Font TaskListMainFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
        static readonly Font TaskListSubFont = new Font(FontFamily.GenericSerif, 8, FontStyle.Regular);
        static readonly Font MemoryBarFontLeft = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
        static readonly Font MemoryBarFontRight = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular);
        static readonly StringFormat TaskListStringFormat = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
        static readonly StringFormat MemoryBarStringFormatLeft = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        static readonly StringFormat MemoryBarStringFormatRight = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        static readonly IImagingFactory ImgFactory = ImagingFactory.GetImaging();
        public static readonly IImage NoIconImage;
        public static readonly Size NoIconImageSize;
        static bool _needScrollbar;
        static bool _usingStartIcons;

        ListViewEx.WndHandler _handler;

        string _statusString;

        /// <summary>
        /// Static constructor...
        /// </summary>
        static MainForm()
        {
            // Cache the no-app-icon image.
            using (var st = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArkSwitch.Images.Application.png"))
            {
                var buf = new byte[st.Length];
                st.Read(buf, 0, (int)st.Length);
                ImgFactory.CreateImageFromBuffer(buf, (uint)buf.Length, BufferDisposalFlag.BufferDisposalFlagNone, out NoIconImage);
                ImageInfo info;
                NoIconImage.GetImageInfo(out info);
                NoIconImageSize = new Size((int)info.Width, (int)info.Height);
                buf = null;
            }
        }

        /// <summary>
        /// Normal constructor...
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Initialize some default stuff.
            lsvTasks.Font = new Font(FontFamily.GenericSansSerif, TaskListMainFont.Size + TaskListSubFont.Size + 3, FontStyle.Regular);
            _usingStartIcons = AppSettings.GetStartMenuIcons();
            if (_usingStartIcons) StartIconMgmt.CacheStartMenuIcons();
            pbxTopBar.Image = Theming.StatusBarImage;
            Visible = false;

            // Initialize NLS strings.
            mnuKill.Text = NativeLang.GetNlsString("Main", mnuKill.Text);
            mnuMenu.Text = NativeLang.GetNlsString("Main", mnuMenu.Text);
            mnuMenuSwitchTo.Text = NativeLang.GetNlsString("Main", mnuMenuSwitchTo.Text);
            mnuMenuKill.Text = NativeLang.GetNlsString("Main", mnuMenuKill.Text);
            mnuMenuTaskDetails.Text = NativeLang.GetNlsString("Main", mnuMenuTaskDetails.Text);
            mnuMenuOptions.Text = NativeLang.GetNlsString("Main", mnuMenuOptions.Text);
            mnuMenuActivationField.Text = NativeLang.GetNlsString("Main", mnuMenuActivationField.Text);
            mnuMenuQuit.Text = NativeLang.GetNlsString("Main", mnuMenuQuit.Text);
        }

        #region Event handling
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Oddly enough, nothing needs to be done here.
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (_firstStart)
            {
                _firstStart = false;
                lsvTasks.SetCustomHandling(out _handler);
                lsvTasks.SetControlBorder(false);
                lsvTasks.BackColor = Theming.BackgroundColor;
                if (Theming.BackgroundImage != null) lsvTasks.SetBackgroundImage(Theming.BackgroundImage);
                _handler.DrawEvent += Handler_DrawEvent;
                _handler.ClickEvent += Handler_ClickEvent;
            }
            RefreshData();
        }

        /// <summary>
        /// Draws the text on the status bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pbxTopBar_Paint(object sender, PaintEventArgs e)
        {
            using (Brush brushLeft = new SolidBrush(Theming.StatusBarTextColorPrimary))
            using (Brush brushRight = new SolidBrush(Theming.StatusBarTextColorSecondary))
            {
                e.Graphics.DrawString(NativeLang.GetNlsString("Main", "Programs"), MemoryBarFontLeft, brushLeft, new RectangleF(4, 0, pbxTopBar.Width - 4, pbxTopBar.Height - 2), MemoryBarStringFormatLeft);
                e.Graphics.DrawString(_statusString, MemoryBarFontRight, brushRight, new RectangleF(0, 0, pbxTopBar.Width - 4, pbxTopBar.Height - 2), MemoryBarStringFormatRight);
            }
        }

        /// <summary>
        /// This event occurs when the ListView is clicked.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="item"></param>
        /// <param name="subitem"></param>
        void Handler_ClickEvent(Point location, int item, int subitem)
        {
            if (item < 0) return;
            switch (subitem)
            {
                case 0:
                    TaskDetails(item);
                    break;
                case 1:
                    SwitchToTask(item);
                    break;
                case 2:
                    CloseTask(item, true);
                    break;
            }
        }

        /// <summary>
        /// This event occurs when specific parts of the ListView are to be drawn.
        /// </summary>
        /// <param name="hdc"></param>
        /// <param name="item"></param>
        /// <param name="subitem"></param>
        /// <param name="selected"></param>
        /// <param name="rect"></param>
        void Handler_DrawEvent(IntPtr hdc, int item, int subitem, bool selected, RectangleF rect)
        {
            var task = (TaskItem)lsvTasks.Items[item].Tag;

            using (var graphics = Graphics.FromHdc(hdc))
            {
                switch (subitem)
                {
                    case -1:
                        // This is the prepaint event for the entire item.

                        if (selected)
                        {
                            if (Theming.ListSelectionRectangleImage != null)
                            {
                                // Draw the selection rectangle image, since we have one.
                                graphics.DrawImageAlphaChannel(Theming.ListSelectionRectangleImage, new Rectangle((int)rect.X, (int)rect.Y + 1, (int)rect.Width, (int)rect.Height - 1));
                            }
                            else if (Theming.ListSelectionRectangleColor.HasValue)
                            {
                                using (var bg = new SolidBrush(Theming.ListSelectionRectangleColor.Value))
                                {
                                    // Draw the selection rectangle solid color, since we have that, but no image.
                                    graphics.FillRectangle(bg, (int)rect.X, (int)rect.Y + 1, (int)rect.Width, (int)rect.Height - 1);
                                }
                            }
                        }
                        else // not selected
                        {
                            if(Theming.ListItemBackgroundImage != null)
                            {
                                // Draw the deselected rectangle image, since we have one.
                                graphics.DrawImageAlphaChannel(Theming.ListItemBackgroundImage, new Rectangle((int)rect.X, (int)rect.Y + 1, (int)rect.Width, (int)rect.Height - 1));
                            }
                            else if (Theming.ListItemBackgroundColor.HasValue)
                            {
                                using (var bg = new SolidBrush(Theming.ListItemBackgroundColor.Value))
                                {
                                    // Draw the deselected rectangle solid color, since we have that, but no image.
                                    graphics.FillRectangle(bg, (int) rect.X, (int) rect.Y + 1, (int) rect.Width, (int) rect.Height - 1);
                                }
                            }
                        }
                        break;
                    case 0:
                        // The first item is the application icon.

                        try
                        {
                            // If we're using Start menu icons, then try to get the one for the current task.
                            var customIcon = _usingStartIcons ? StartIconMgmt.GetCustomIconPathForExe(task.ExePath) : null;
                            if (customIcon != null && File.Exists(customIcon))
                            {
                                // Retrieve it.
                                IImage img;
                                ImgFactory.CreateImageFromFile(customIcon, out img);
                                ImageInfo info;
                                img.GetImageInfo(out info);
                                var imgSize = new Size((int)info.Width, (int)info.Height);

                                // Draw it.
                                graphics.DrawImageAlphaChannel(img, Misc.CalculateCenteredScaledDestRect(rect, imgSize, false));
                            }
                            else
                            {
                                // Get the icon from the EXE itself.
                                var icon = ExeIconMgmt.GetIconForExe(task.ExePath, true);
                                if (icon != null)
                                {
                                    if (icon.Height <= rect.Height && icon.Width <= rect.Width)
                                    {
                                        // If the icon is smaller or equal to the size of the space we have for it, just draw it directly, in the center.
                                        graphics.DrawIcon(icon, (int)rect.Width / 2 - icon.Width / 2 + (int)rect.X, (int)rect.Height / 2 - icon.Height / 2 + (int)rect.Y);
                                    }
                                    else
                                    {
                                        // The icon is too big, so we have to resize it. Since there is no method provided to draw resized icons, we need a bitmap instead.

                                        // Get the bitmap representation of the icon.
                                        var bmp = ExeIconMgmt.GetBitmapFromIcon(icon, true);
                                        // Draw the bitmap, resizing it (and keeping the aspect ratio).
                                        graphics.DrawImage(bmp, Misc.CalculateCenteredScaledDestRect(rect, bmp.Size, false), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                                        bmp.Dispose();
                                    }
                                }
                                else
                                {
                                    // Draw the generic application icon.
                                    graphics.DrawImageAlphaChannel(NoIconImage, Misc.CalculateCenteredScaledDestRect(rect, NoIconImageSize, false));
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show("An error has occurred. ArkSwitch will try to continue. Please report this!" + Environment.NewLine + "Error: " + ex.Message);
                        }
                        break;
                    case 1:
                        // The second item is the task title and memory info.

                        // Generate the strings, determine their sizes, etc...
                        var infoString = Path.GetFileName(task.ExePath);
                        var nameStringSize = graphics.MeasureString(task.Title, TaskListMainFont);
                        var infoStringSize = graphics.MeasureString(infoString, TaskListSubFont);
                        var combinedHeight = nameStringSize.Height + infoStringSize.Height + 1;
                        var y = rect.Height / 2 - combinedHeight / 2 + rect.Y;
                        var titleRect = new RectangleF(rect.X + 2, y, rect.Width - 2, nameStringSize.Height);
                        var infoRect = new RectangleF(rect.X + 2, y + nameStringSize.Height + 1, rect.Width - 2, infoStringSize.Height);

                        // Draw the strings.
                        using (var brushPrimary = new SolidBrush(selected ? Theming.ListTextColorPrimarySelected : Theming.ListTextColorPrimary))
                        using (var brushSecondary = new SolidBrush(selected ? Theming.ListTextColorSecondarySelected : Theming.ListTextColorSecondary))
                        {
                            graphics.DrawString(task.Title, TaskListMainFont, brushPrimary, titleRect, TaskListStringFormat);
                            graphics.DrawString(infoString, TaskListSubFont, brushSecondary, infoRect, TaskListStringFormat);
                        }

                        break;
                    case 2:
                        // The third item is the X icon.

                        var modRect = new Rectangle((int)rect.X + 2, (int)rect.Y + 2, (int)rect.Width - 4, (int)rect.Height - 4);
                        var xImg = selected ? Theming.XSelectedImage : Theming.XDeselectedImage;
                        var xImgSize = selected ? Theming.XSelectedImageSize : Theming.XDeselectedImageSize;
                        graphics.DrawImageAlphaChannel(xImg, Misc.CalculateCenteredScaledDestRect(modRect, xImgSize, true));
                        break;
                }
            }
        }

        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            // Clean up.
            _handler.DrawEvent -= Handler_DrawEvent;
            _handler.ClickEvent -= Handler_ClickEvent;
            _handler = null;
            Program.TheForm = null;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            using (var b = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(b))
            {
                pbxTopBar.Height = (int)g.MeasureString("Testing", TaskListSubFont).Height + 5;
            }
            pbxTopBar.Top = 0;
            pbxTopBar.Left = 0;
            pbxTopBar.Width = ClientSize.Width;
            lsvTasks.Top = pbxTopBar.Height;
            lsvTasks.Left = 0;
            lsvTasks.Height = ClientSize.Height - pbxTopBar.Height - 1;
            lsvTasks.Width = ClientSize.Width;
            if (lsvTasks.Items.Count > 0) _needScrollbar = (lsvTasks.Height < lsvTasks.GetRequiredSize(lsvTasks.Items.Count).Height);
            ResetColumns();
        }

        private void mnuMenuQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuMenuOptions_Click(object sender, EventArgs e)
        {
            Program.IsShowingDialog = true;
            try
            {
                var frm = new OptionsForm();
                frm.ShowDialog();
                frm.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FATAL ERROR: " + ex.Message);
                Close();
            }
            Program.IsShowingDialog = false;
        }

        private void mnuKill_Click(object sender, EventArgs e)
        {
            if (lsvTasks.Items.Count < 1) return;
            if (MessageBox.Show(NativeLang.GetNlsString("Main", "CloseAllMsg"), NativeLang.GetNlsString("Main", "CloseAllTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                for (int i = 0; i < lsvTasks.Items.Count; i++)
                    CloseTask(i, false);

                // Hide this form after closing everything, since there's (theoretically) nothing left to close.
                Visible = false;
            }
        }

        private void mnuMenuKill_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lsvTasks.SelectedIndices.Count; i++)
            {
                CloseTask(lsvTasks.SelectedIndices[i], true);
            }
        }

        private void mnuMenuSwitchTo_Click(object sender, EventArgs e)
        {
            if (lsvTasks.SelectedIndices.Count < 1) return;
            SwitchToTask(lsvTasks.SelectedIndices[0]);
        }

        private void mnuMenuTaskDetails_Click(object sender, EventArgs e)
        {
            if (lsvTasks.SelectedIndices.Count < 1) return;
            TaskDetails(lsvTasks.SelectedIndices[0]);
        }

        private void mnuMenuActivationField_Click(object sender, EventArgs e)
        {
            if (!Program.Handle.IsAllocated)
            {
                // If WM65 compatibility mode is on, we can't do anything with the activation field.
                MessageBox.Show(NativeLang.GetNlsString("Main", "ActivationFieldNotAvailableMsg"));
                return;
            }

            Program.IsShowingDialog = true;
            try
            {
                var frm = new ActivationRelocationForm();
                Program.SetActivationFieldRelocationMode(true);
                var res = frm.ShowDialog();
                frm.Dispose();

                if (res == DialogResult.OK)
                {
                    // We don't need to tell the native DLL the new info because it already has it. Just store it in the registry.
                    AppSettings.SetActivationFieldValue(1, Program.ProcessEventsInstance.X);
                    AppSettings.SetActivationFieldValue(2, Program.ProcessEventsInstance.Y);
                }
                else
                {
                    // Reset back to the previous values.
                    Program.ProcessEventsInstance.EventType = 255;
                    Program.ProcessEventsInstance.X = (uint)AppSettings.GetActivationFieldValue(1);
                    Program.ProcessEventsInstance.Y = (uint)AppSettings.GetActivationFieldValue(2);
                    Program.Register(Program.Handle.AddrOfPinnedObject());
                }

                Program.SetActivationFieldRelocationMode(false);
            }
            catch (Exception ex)
            {
                // If there's an activation field-related problem, it's safer to just quit.
                MessageBox.Show("FATAL ERROR: " + ex.Message);
                Close();
            }
            Program.IsShowingDialog = false;
        }

        private void tmrRamRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                tmrRamRefresh.Enabled = false;
                RefreshSystemRamInfo();
            }
            catch (ObjectDisposedException)
            {
                // Ignore...
            }
        }
        #endregion

        #region Methods
        private void ResetColumns()
        {
            lsvTasks.Columns[0].Width = (int)Math.Round(lsvTasks.Size.Width * 0.14583);
            lsvTasks.Columns[2].Width = lsvTasks.Columns[0].Width;

            lsvTasks.Columns[1].Width = lsvTasks.Size.Width - lsvTasks.Columns[0].Width - lsvTasks.Columns[2].Width - (_needScrollbar ? GetSystemMetrics(SM_CXVSCROLL) : 0) - 5;
        }

        internal void RefreshData()
        {
            // Refresh tasks.
            lock(this)
            {
                var tasks = TaskMgmt.Instance.GetTasks();
                var heightNeeded = lsvTasks.GetRequiredSize(tasks.Count).Height + 2;
                _needScrollbar = (lsvTasks.Height <= heightNeeded);
                ResetColumns();
                lsvTasks.Items.Clear();
                foreach (var task in tasks)
                {
                    lsvTasks.Items.Add(new ListViewItem("") { Tag = task });
                }

                // Refresh system summary.
                RefreshSystemRamInfo();
            }
        }

        internal void RefreshSystemRamInfo()
        {
            var freeProcs = 32 - TaskMgmt.Instance.GetNumProcesses();
            var newStatus = string.Format(NativeLang.GetNlsString("Main", "FreeTotalBar"), freeProcs, "32");

            try
            {
                if (_statusString != newStatus)
                {
                    // This is a trick to continue updating the status bar after the user has done something.
                    // It's needed for times when the OS takes a while to update its totals.
                    tmrRamRefresh.Enabled = false;
                    tmrRamRefresh.Enabled = true;
                }
                _statusString = newStatus;
                // Signal the PictureBox to redraw itself.
                pbxTopBar.Invalidate();
            }
            catch (ObjectDisposedException)
            {
                // Ignore...
            }
        }
        #endregion

        private void SwitchToTask(int item)
        {
            lock(this)
            {
                var task = (TaskItem)lsvTasks.Items[item].Tag;
                TaskMgmt.Instance.ActivateWindow(task.HWnd);
                Visible = false;
            }
        }

        private void CloseTask(int item, bool refresh)
        {
            Cursor.Current = Cursors.WaitCursor;
            lock(this)
            {
                var task = (TaskItem)lsvTasks.Items[item].Tag;
                TaskMgmt.Instance.CloseWindow(task.HWnd);
                if (refresh)
                {
                    RefreshData();

                    // Always do the timer refresh trick after closing something.
                    tmrRamRefresh.Enabled = false;
                    tmrRamRefresh.Enabled = true;
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void TaskDetails(int item)
        {
            Program.IsShowingDialog = true;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var frm = new InfoForm();
                lock(this)
                {
                    frm.PopulateTask((TaskItem)lsvTasks.Items[item].Tag);
                }
                frm.ShowDialog();
                frm.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FATAL ERROR: " + ex.Message);
                Close();
            }
            Program.IsShowingDialog = false;
        }

        #region P/Invoke related stuff
        [DllImport("coredll.dll")]
        static extern int GetSystemMetrics(int nIndex);

        const int SM_CXVSCROLL = 2;
        #endregion
    }
}