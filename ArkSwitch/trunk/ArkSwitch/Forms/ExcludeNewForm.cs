/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Windows.Forms;

namespace ArkSwitch.Forms
{
    public partial class ExcludeNewForm : Form
    {
        public ExcludeNewForm()
        {
            InitializeComponent();

            // Initialize NLS strings.
            this.Text = NativeLang.GetNlsString("ExcludeEXE", this.Text);
            lblText.Text = NativeLang.GetNlsString("ExcludeEXE", lblText.Text);
            menuItem1.Text = NativeLang.GetNlsString("ExcludeEXE", menuItem1.Text);
            menuItem2.Text = NativeLang.GetNlsString("ExcludeEXE", menuItem2.Text);
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public string ExePathName
        {
            get
            {
                return tbxExe.Text;
            }
            set
            {
                tbxExe.Text = value;
            }
        }
    }
}