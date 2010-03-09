namespace ArkSwitch.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu ArkMenu;

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
            this.ArkMenu = new System.Windows.Forms.MainMenu();
            this.mnuKill = new System.Windows.Forms.MenuItem();
            this.mnuMenu = new System.Windows.Forms.MenuItem();
            this.mnuMenuSwitchTo = new System.Windows.Forms.MenuItem();
            this.mnuMenuKill = new System.Windows.Forms.MenuItem();
            this.mnuMenuTaskDetails = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.mnuMenuOptions = new System.Windows.Forms.MenuItem();
            this.mnuMenuActivationField = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.mnuMenuQuit = new System.Windows.Forms.MenuItem();
            this.lsvTasks = new System.Windows.Forms.ListView();
            this.colIcon = new System.Windows.Forms.ColumnHeader();
            this.colText = new System.Windows.Forms.ColumnHeader();
            this.colX = new System.Windows.Forms.ColumnHeader();
            this.tmrRamRefresh = new System.Windows.Forms.Timer();
            this.pbxTopBar = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // ArkMenu
            // 
            this.ArkMenu.MenuItems.Add(this.mnuKill);
            this.ArkMenu.MenuItems.Add(this.mnuMenu);
            // 
            // mnuKill
            // 
            this.mnuKill.Text = "CloseAll";
            this.mnuKill.Click += new System.EventHandler(this.mnuKill_Click);
            // 
            // mnuMenu
            // 
            this.mnuMenu.MenuItems.Add(this.mnuMenuSwitchTo);
            this.mnuMenu.MenuItems.Add(this.mnuMenuKill);
            this.mnuMenu.MenuItems.Add(this.mnuMenuTaskDetails);
            this.mnuMenu.MenuItems.Add(this.menuItem5);
            this.mnuMenu.MenuItems.Add(this.mnuMenuOptions);
            this.mnuMenu.MenuItems.Add(this.mnuMenuActivationField);
            this.mnuMenu.MenuItems.Add(this.menuItem2);
            this.mnuMenu.MenuItems.Add(this.mnuMenuQuit);
            this.mnuMenu.Text = "Menu";
            // 
            // mnuMenuSwitchTo
            // 
            this.mnuMenuSwitchTo.Text = "MenuSwitchTo";
            this.mnuMenuSwitchTo.Click += new System.EventHandler(this.mnuMenuSwitchTo_Click);
            // 
            // mnuMenuKill
            // 
            this.mnuMenuKill.Text = "MenuCloseTask";
            this.mnuMenuKill.Click += new System.EventHandler(this.mnuMenuKill_Click);
            // 
            // mnuMenuTaskDetails
            // 
            this.mnuMenuTaskDetails.Text = "MenuTaskDetails";
            this.mnuMenuTaskDetails.Click += new System.EventHandler(this.mnuMenuTaskDetails_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Text = "-";
            // 
            // mnuMenuOptions
            // 
            this.mnuMenuOptions.Text = "MenuOptions";
            this.mnuMenuOptions.Click += new System.EventHandler(this.mnuMenuOptions_Click);
            // 
            // mnuMenuActivationField
            // 
            this.mnuMenuActivationField.Text = "MenuActivationField";
            this.mnuMenuActivationField.Click += new System.EventHandler(this.mnuMenuActivationField_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Text = "-";
            // 
            // mnuMenuQuit
            // 
            this.mnuMenuQuit.Text = "MenuQuit";
            this.mnuMenuQuit.Click += new System.EventHandler(this.mnuMenuQuit_Click);
            // 
            // lsvTasks
            // 
            this.lsvTasks.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lsvTasks.Columns.Add(this.colIcon);
            this.lsvTasks.Columns.Add(this.colText);
            this.lsvTasks.Columns.Add(this.colX);
            this.lsvTasks.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lsvTasks.FullRowSelect = true;
            this.lsvTasks.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lsvTasks.Location = new System.Drawing.Point(0, 23);
            this.lsvTasks.Name = "lsvTasks";
            this.lsvTasks.Size = new System.Drawing.Size(240, 245);
            this.lsvTasks.TabIndex = 0;
            this.lsvTasks.View = System.Windows.Forms.View.Details;
            // 
            // colIcon
            // 
            this.colIcon.Text = "";
            this.colIcon.Width = 70;
            // 
            // colText
            // 
            this.colText.Text = "Text";
            this.colText.Width = 60;
            // 
            // colX
            // 
            this.colX.Text = "";
            this.colX.Width = 64;
            // 
            // tmrRamRefresh
            // 
            this.tmrRamRefresh.Interval = 800;
            this.tmrRamRefresh.Tick += new System.EventHandler(this.tmrRamRefresh_Tick);
            // 
            // pbxTopBar
            // 
            this.pbxTopBar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pbxTopBar.Location = new System.Drawing.Point(0, 0);
            this.pbxTopBar.Name = "pbxTopBar";
            this.pbxTopBar.Size = new System.Drawing.Size(240, 23);
            this.pbxTopBar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxTopBar.Paint += new System.Windows.Forms.PaintEventHandler(this.pbxTopBar_Paint);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.pbxTopBar);
            this.Controls.Add(this.lsvTasks);
            this.Menu = this.ArkMenu;
            this.Name = "MainForm";
            this.Text = "ArkSwitch";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem mnuKill;
        private System.Windows.Forms.MenuItem mnuMenu;
        private System.Windows.Forms.MenuItem mnuMenuSwitchTo;
        private System.Windows.Forms.MenuItem mnuMenuKill;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem mnuMenuQuit;
        private System.Windows.Forms.ListView lsvTasks;
        private System.Windows.Forms.ColumnHeader colIcon;
        private System.Windows.Forms.ColumnHeader colText;
        private System.Windows.Forms.ColumnHeader colX;
        private System.Windows.Forms.MenuItem mnuMenuTaskDetails;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem mnuMenuOptions;
        private System.Windows.Forms.MenuItem mnuMenuActivationField;
        private System.Windows.Forms.Timer tmrRamRefresh;
        private System.Windows.Forms.PictureBox pbxTopBar;
    }
}