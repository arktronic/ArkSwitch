namespace ArkSwitch.Forms
{
    partial class OptionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem();
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem();
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem();
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.mnuAbout = new System.Windows.Forms.MenuItem();
            this.mnuExclusions = new System.Windows.Forms.MenuItem();
            this.lsvOptions = new System.Windows.Forms.ListView();
            this.colCheck = new System.Windows.Forms.ColumnHeader();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.mnuAbout);
            this.mainMenu1.MenuItems.Add(this.mnuExclusions);
            // 
            // mnuAbout
            // 
            this.mnuAbout.Text = "About";
            this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
            // 
            // mnuExclusions
            // 
            this.mnuExclusions.Text = "Exclusions";
            this.mnuExclusions.Click += new System.EventHandler(this.mnuExclusions_Click);
            // 
            // lsvOptions
            // 
            this.lsvOptions.Columns.Add(this.colCheck);
            this.lsvOptions.Columns.Add(this.colName);
            this.lsvOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsvOptions.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Regular);
            this.lsvOptions.FullRowSelect = true;
            this.lsvOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listViewItem1.Checked = true;
            listViewItem1.Tag = "boot";
            listViewItem1.Text = "";
            listViewItem1.SubItems.Add("Run at boot time");
            listViewItem2.Checked = true;
            listViewItem2.Tag = "icons";
            listViewItem2.Text = "";
            listViewItem2.SubItems.Add("Use Start Menu icons");
            listViewItem3.Tag = "tools";
            listViewItem3.Text = "";
            listViewItem3.SubItems.Add("Enable Tools shortcut");
            listViewItem4.Tag = "std";
            listViewItem4.Text = "";
            listViewItem4.SubItems.Add("WM6.5 compatibility");
            this.lsvOptions.Items.Add(listViewItem1);
            this.lsvOptions.Items.Add(listViewItem2);
            this.lsvOptions.Items.Add(listViewItem3);
            this.lsvOptions.Items.Add(listViewItem4);
            this.lsvOptions.Location = new System.Drawing.Point(0, 0);
            this.lsvOptions.Name = "lsvOptions";
            this.lsvOptions.Size = new System.Drawing.Size(240, 268);
            this.lsvOptions.TabIndex = 0;
            this.lsvOptions.View = System.Windows.Forms.View.Details;
            // 
            // colCheck
            // 
            this.colCheck.Text = "";
            this.colCheck.Width = 70;
            // 
            // colName
            // 
            this.colName.Text = "";
            this.colName.Width = 160;
            // 
            // OptionsForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lsvOptions);
            this.Menu = this.mainMenu1;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.Text = "Title";
            this.Activated += new System.EventHandler(this.OptionsForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.OptionsForm_Closing);
            this.Resize += new System.EventHandler(this.OptionsForm_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem mnuAbout;
        private System.Windows.Forms.MenuItem mnuExclusions;
        private System.Windows.Forms.ListView lsvOptions;
        private System.Windows.Forms.ColumnHeader colCheck;
        private System.Windows.Forms.ColumnHeader colName;
    }
}