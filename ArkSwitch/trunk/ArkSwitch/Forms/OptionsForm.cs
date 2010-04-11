/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using Microsoft.Drawing;

namespace ArkSwitch.Forms
{
    public partial class OptionsForm : Form
    {
        bool _firstStart = true;

        static readonly Bitmap CheckmarkIcon = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ArkSwitch.Images.checkmark.gif"));
        static readonly ImageAttributes CheckmarkImageAttr = new ImageAttributes();
        static readonly StringFormat DefaultStringFormat = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        static readonly Font DefaultFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);

        bool _restartChanges = false;
        ListViewEx.WndHandler _handler;

        /// <summary>
        /// Static constructor...
        /// </summary>
        static OptionsForm()
        {
            // This is needed to use the cheap .NET CF transparency with the check mark image.
            // The code here was written before the addition of the IImage stuff with its alpha transparency capabilities.
            CheckmarkImageAttr.SetColorKey(Color.FromArgb(255, 0, 220), Color.FromArgb(255, 0, 220));
        }

        /// <summary>
        /// Normal constructor...
        /// </summary>
        public OptionsForm()
        {
            InitializeComponent();

            // Initialize NLS strings.
            this.Text = NativeLang.GetNlsString("Options", this.Text);
            mnuExclusions.Text = NativeLang.GetNlsString("Options", mnuExclusions.Text);
            for (int i = 0; i < lsvOptions.Items.Count; i++)
            {
                var item = lsvOptions.Items[i];
                var tag = item.Tag as string;
                if (tag == "icons")
                {
                    item.SubItems[1].Text = NativeLang.GetNlsString("Options", "StartMenuIcons");
                }
                else if (tag == "boot")
                {
                    item.SubItems[1].Text = NativeLang.GetNlsString("Options", "RunAtBoot");
                }
                else if (tag == "tools")
                {
                    item.SubItems[1].Text = NativeLang.GetNlsString("Options", "ToolsShortcut");
                }
                else if (tag == "std")
                {
                    item.SubItems[1].Text = NativeLang.GetNlsString("Options", "WM65Compat");
                }
            }
        }

        #region Event handling
        private void OptionsForm_Activated(object sender, EventArgs e)
        {
            if (_firstStart)
            {
                _firstStart = false;
                lsvOptions.Font = new Font(DefaultFont.Name, DefaultFont.Size * 2, DefaultFont.Style);
                lsvOptions.SetCustomHandling(out _handler);
                lsvOptions.SetControlBorder(false);
                lsvOptions.BackColor = Theming.BackgroundColor;
                if (Theming.BackgroundImage != null) lsvOptions.SetBackgroundImage(Theming.BackgroundImage);
                _handler.ClickEvent += Handler2_ClickEvent;
                _handler.DrawEvent += Handler2_DrawEvent;
            }
            RefreshData();
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ArkSwitch v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) +
                            Environment.NewLine + "Copyright 2010 Arktronic" +
                            Environment.NewLine + "www.arktronic.com");
        }

        private void mnuExclusions_Click(object sender, EventArgs e)
        {
            // Program.IsShowingDialog isn't touched here because this form itself is already a dialog.

            var frm = new ExclusionsForm();
            frm.ShowDialog();
            frm.Dispose();
        }

        void Handler2_DrawEvent(IntPtr hdc, int item, int subitem, bool selected, RectangleF rect)
        {
            var textColor = selected ? Theming.ListTextColorPrimarySelected : Theming.ListTextColorPrimary;

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
                                graphics.DrawImageAlphaChannel(Theming.ListSelectionRectangleImage, new Rectangle((int)rect.X, (int)rect.Y + 1, (int)rect.Width, (int)rect.Height - 1));
                            }
                            else if (Theming.ListSelectionRectangleColor.HasValue)
                            {
                                using (var bg = new SolidBrush(Theming.ListSelectionRectangleColor.Value))
                                {
                                    graphics.FillRectangle(bg, (int)rect.X, (int)rect.Y + 1, (int)rect.Width, (int)rect.Height - 1);
                                }
                            }
                        }
                        else // not selected
                        {
                            if (Theming.ListItemBackgroundImage != null)
                            {
                                // Draw the deselected rectangle image, since we have one.
                                graphics.DrawImageAlphaChannel(Theming.ListItemBackgroundImage, new Rectangle((int)rect.X, (int)rect.Y + 1, (int)rect.Width, (int)rect.Height - 1));
                            }
                            else if (Theming.ListItemBackgroundColor.HasValue)
                            {
                                using (var bg = new SolidBrush(Theming.ListItemBackgroundColor.Value))
                                {
                                    // Draw the deselected rectangle solid color, since we have that, but no image.
                                    graphics.FillRectangle(bg, (int)rect.X, (int)rect.Y + 1, (int)rect.Width, (int)rect.Height - 1);
                                }
                            }
                        }
                        break;
                    case 0:
                        // The first item is the checkmark image.

                        if (lsvOptions.Items[item].Checked) graphics.DrawImage(CheckmarkIcon, Misc.CalculateCenteredScaledDestRect(rect, CheckmarkIcon.Size, true), 0, 0, CheckmarkIcon.Width, CheckmarkIcon.Height, GraphicsUnit.Pixel, CheckmarkImageAttr);
                        break;
                    case 1:
                        // The second item is the text description of the option.

                        var modRect = new RectangleF(rect.X + 4, rect.Y, rect.Width - 4, rect.Height);
                        using (var brush = new SolidBrush(textColor))
                        {
                            graphics.DrawString(lsvOptions.Items[item].SubItems[subitem].Text, DefaultFont, brush, modRect, DefaultStringFormat);
                        }
                        break;
                }
            }
        }

        void Handler2_ClickEvent(Point location, int itemNum, int subitem)
        {
            // Only process stuff if the checkmark area is clicked.
            if (subitem != 0) return;

            var newVal = !lsvOptions.Items[itemNum].Checked;

            // Update the ListView.
            lsvOptions.Items[itemNum].Checked = newVal;
            lsvOptions.Invalidate();

            // Save the changes immediately.
            var item = lsvOptions.Items[itemNum];
            var tag = item.Tag as string;
            if (tag == "icons")
            {
                AppSettings.SetStartMenuIcons(newVal);
                // This requires a restart.
                _restartChanges = true;
            }
            else if (tag == "boot")
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "ArkSwitch.lnk");
                if (newVal)
                {
                    // Create the file.
                    if (!File.Exists(path))
                    {
                        var sr = new StreamWriter(path, false);
                        var app = "\"" + Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName + "\"";
                        sr.Write(app.Length + "#" + app);
                        sr.Flush();
                        sr.Close();
                    }
                }
                else
                {
                    // Delete the file.
                    if (File.Exists(path)) File.Delete(path);
                }
            }
            else if (tag == "tools")
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), @"Tools\ArkSwitch.lnk");
                if (newVal)
                {
                    // Create the file.
                    if (!File.Exists(path))
                    {
                        var dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        var sr = new StreamWriter(path, false);
                        var app = "\"" + Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName + "\"";
                        sr.Write(app.Length + "#" + app);
                        sr.Flush();
                        sr.Close();
                    }
                }
                else
                {
                    // Delete the file.
                    if (File.Exists(path)) File.Delete(path);
                }
            }
            else if (tag == "std")
            {
                AppSettings.SetTaskbarTakeover(!newVal);
                if (newVal)
                    MessageBox.Show(NativeLang.GetNlsString("Options", "WM65Warning"));
                // This requires a restart.
                _restartChanges = true;
            }
        }

        private void OptionsForm_Resize(object sender, EventArgs e)
        {
            ResetColumns();
        }

        private void OptionsForm_Closing(object sender, CancelEventArgs e)
        {
            if (_restartChanges)
                MessageBox.Show(NativeLang.GetNlsString("Options", "RestartChanges"));

            // Clean up.
            _handler.ClickEvent -= Handler2_ClickEvent;
            _handler.DrawEvent -= Handler2_DrawEvent;
            _handler = null;
        }
        #endregion

        #region Methods
        private void RefreshData()
        {
            for (int i = 0; i < lsvOptions.Items.Count; i++)
            {
                var item = lsvOptions.Items[i];
                var tag = item.Tag as string;
                if (tag == "icons")
                {
                    item.Checked = AppSettings.GetStartMenuIcons();
                }
                else if (tag == "boot")
                {
                    item.Checked = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "ArkSwitch.lnk"));
                }
                else if (tag == "tools")
                {
                    item.Checked = File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), @"Tools\ArkSwitch.lnk"));
                }
                else if (tag == "std")
                {
                    item.Checked = !AppSettings.GetTaskbarTakeover();
                }
            }
        }

        private void ResetColumns()
        {
            lsvOptions.Columns[0].Width = (int)Math.Round(lsvOptions.Size.Width * 0.14583);
            lsvOptions.Columns[1].Width = lsvOptions.ClientSize.Width - lsvOptions.Columns[0].Width - 3;
        }
        #endregion
    }
}