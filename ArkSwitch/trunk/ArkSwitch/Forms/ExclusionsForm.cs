/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Drawing;

namespace ArkSwitch.Forms
{
    public partial class ExclusionsForm : Form
    {
        bool _firstStart = true;
        ListViewEx.WndHandler _handler;
        static readonly StringFormat DefaultStringFormat = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        static readonly Font DefaultFont = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular);

        public ExclusionsForm()
        {
            InitializeComponent();

            // Initialize NLS strings.
            this.Text = NativeLang.GetNlsString("Exclusions", this.Text);
            mnuAdd.Text = NativeLang.GetNlsString("Exclusions", mnuAdd.Text);
            mnuRemove.Text = NativeLang.GetNlsString("Exclusions", mnuRemove.Text);
        }

        #region Event handling
        private void Exclusions_Activated(object sender, EventArgs e)
        {
            if (_firstStart)
            {
                _firstStart = false;
                lsvExclusions.Font = new Font(DefaultFont.Name, DefaultFont.Size * 2, DefaultFont.Style);
                lsvExclusions.SetCustomHandling(out _handler);
                lsvExclusions.SetControlBorder(false);
                lsvExclusions.BackColor = Theming.BackgroundColor;
                if (Theming.BackgroundImage != null) lsvExclusions.SetBackgroundImage(Theming.BackgroundImage);
                _handler.DrawEvent += Handler_DrawEvent;
            }
            PopulateExclusions();
        }

        private void Handler_DrawEvent(IntPtr hdc, int item, int subitem, bool selected, RectangleF rect)
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
                        else if (Theming.ListItemBackgroundColor.HasValue)
                        {
                            using (var bg = new SolidBrush(Theming.ListItemBackgroundColor.Value))
                            {
                                graphics.FillRectangle(bg, (int)rect.X, (int)rect.Y + 1, (int)rect.Width, (int)rect.Height - 1);
                            }
                        }
                        break;
                    case 0:
                        using (var brush = new SolidBrush(textColor))
                        {
                            graphics.DrawString(lsvExclusions.Items[item].SubItems[subitem].Text, DefaultFont, brush, rect, DefaultStringFormat);
                        }
                        break;
                }
            }
        }

        private void Exclusions_Resize(object sender, EventArgs e)
        {
            ResetColumns();
        }

        private void tmrReset_Tick(object sender, EventArgs e)
        {
            // The timer is used for a hack to get the ListView sized properly.
            ResetColumns();
            tmrReset.Enabled = false;
        }

        private void mnuAdd_Click(object sender, EventArgs e)
        {
            var ex = new ExcludeNewForm();
            var res = ex.ShowDialog();
            var newExe = ex.ExePathName.Trim().ToLower();
            if (res == DialogResult.OK && newExe.Length > 0 && !Program.ExcludedExes.Contains(newExe))
            {
                Program.ExcludedExes.Add(newExe);
                AppSettings.SetExcludedExes();
                PopulateExclusions();
            }
            ex.Dispose();
        }

        private void mnuRemove_Click(object sender, EventArgs e)
        {
            if (lsvExclusions.SelectedIndices.Count < 1) return;
            Program.ExcludedExes.Remove(lsvExclusions.Items[lsvExclusions.SelectedIndices[0]].Text);
            AppSettings.SetExcludedExes();
            PopulateExclusions();
        }
        #endregion

        #region Methods
        private void ResetColumns()
        {
            lsvExclusions.Columns[0].Width = lsvExclusions.ClientSize.Width - 3;
        }

        /// <summary>
        /// Populates the list of excluded EXEs.
        /// </summary>
        private void PopulateExclusions()
        {
            lsvExclusions.Items.Clear();
            foreach (var item in Program.ExcludedExes)
                lsvExclusions.Items.Add(new ListViewItem(item));
            tmrReset.Enabled = true;
        }
        #endregion
    }
}