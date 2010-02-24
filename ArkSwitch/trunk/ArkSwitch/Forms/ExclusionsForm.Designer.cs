namespace ArkSwitch.Forms
{
    partial class ExclusionsForm
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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.mnuAdd = new System.Windows.Forms.MenuItem();
            this.mnuRemove = new System.Windows.Forms.MenuItem();
            this.lsvExclusions = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.tmrReset = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.mnuAdd);
            this.mainMenu1.MenuItems.Add(this.mnuRemove);
            // 
            // mnuAdd
            // 
            this.mnuAdd.Text = "AddNew";
            this.mnuAdd.Click += new System.EventHandler(this.mnuAdd_Click);
            // 
            // mnuRemove
            // 
            this.mnuRemove.Text = "Remove";
            this.mnuRemove.Click += new System.EventHandler(this.mnuRemove_Click);
            // 
            // lsvExclusions
            // 
            this.lsvExclusions.Columns.Add(this.colName);
            this.lsvExclusions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsvExclusions.FullRowSelect = true;
            this.lsvExclusions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lsvExclusions.Location = new System.Drawing.Point(0, 0);
            this.lsvExclusions.Name = "lsvExclusions";
            this.lsvExclusions.Size = new System.Drawing.Size(240, 268);
            this.lsvExclusions.TabIndex = 0;
            this.lsvExclusions.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "";
            this.colName.Width = 60;
            // 
            // tmrReset
            // 
            this.tmrReset.Interval = 300;
            this.tmrReset.Tick += new System.EventHandler(this.tmrReset_Tick);
            // 
            // Exclusions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lsvExclusions);
            this.Menu = this.mainMenu1;
            this.MinimizeBox = false;
            this.Name = "Exclusions";
            this.Text = "Title";
            this.Activated += new System.EventHandler(this.Exclusions_Activated);
            this.Resize += new System.EventHandler(this.Exclusions_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem mnuAdd;
        private System.Windows.Forms.MenuItem mnuRemove;
        private System.Windows.Forms.ListView lsvExclusions;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.Timer tmrReset;
    }
}