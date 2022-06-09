namespace EtumrepMMO.Server.WinForms
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.Tab_Main = new System.Windows.Forms.TabControl();
            this.Tab_Settings = new System.Windows.Forms.TabPage();
            this.Grid_Settings = new System.Windows.Forms.PropertyGrid();
            this.Tab_Logs = new System.Windows.Forms.TabPage();
            this.RTB_Logs = new System.Windows.Forms.RichTextBox();
            this.Button_Start = new System.Windows.Forms.Button();
            this.Button_Stop = new System.Windows.Forms.Button();
            this.Bar_Load = new System.Windows.Forms.ProgressBar();
            this.ActiveConnection = new System.Windows.Forms.TextBox();
            this.Connections = new System.Windows.Forms.Label();
            this.Etumreps = new System.Windows.Forms.Label();
            this.Authenticated = new System.Windows.Forms.Label();
            this.Tab_Main.SuspendLayout();
            this.Tab_Settings.SuspendLayout();
            this.Tab_Logs.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tab_Main
            // 
            this.Tab_Main.Controls.Add(this.Tab_Settings);
            this.Tab_Main.Controls.Add(this.Tab_Logs);
            this.Tab_Main.Location = new System.Drawing.Point(14, 87);
            this.Tab_Main.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Main.Name = "Tab_Main";
            this.Tab_Main.SelectedIndex = 0;
            this.Tab_Main.Size = new System.Drawing.Size(907, 419);
            this.Tab_Main.TabIndex = 0;
            // 
            // Tab_Settings
            // 
            this.Tab_Settings.Controls.Add(this.Grid_Settings);
            this.Tab_Settings.Location = new System.Drawing.Point(4, 24);
            this.Tab_Settings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Settings.Name = "Tab_Settings";
            this.Tab_Settings.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Settings.Size = new System.Drawing.Size(899, 391);
            this.Tab_Settings.TabIndex = 0;
            this.Tab_Settings.Text = "Settings";
            this.Tab_Settings.UseVisualStyleBackColor = true;
            // 
            // Grid_Settings
            // 
            this.Grid_Settings.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Grid_Settings.CategorySplitterColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Grid_Settings.CommandsBorderColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Grid_Settings.DisabledItemForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.Grid_Settings.HelpBackColor = System.Drawing.SystemColors.ControlLight;
            this.Grid_Settings.HelpBorderColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Grid_Settings.LineColor = System.Drawing.SystemColors.ControlDark;
            this.Grid_Settings.Location = new System.Drawing.Point(7, 7);
            this.Grid_Settings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Grid_Settings.Name = "Grid_Settings";
            this.Grid_Settings.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.Grid_Settings.SelectedItemWithFocusBackColor = System.Drawing.SystemColors.ButtonFace;
            this.Grid_Settings.SelectedItemWithFocusForeColor = System.Drawing.SystemColors.Highlight;
            this.Grid_Settings.Size = new System.Drawing.Size(877, 378);
            this.Grid_Settings.TabIndex = 0;
            this.Grid_Settings.ViewBackColor = System.Drawing.SystemColors.ControlDark;
            this.Grid_Settings.ViewBorderColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Grid_Settings.ViewForeColor = System.Drawing.SystemColors.InfoText;
            // 
            // Tab_Logs
            // 
            this.Tab_Logs.AutoScroll = true;
            this.Tab_Logs.Controls.Add(this.RTB_Logs);
            this.Tab_Logs.Location = new System.Drawing.Point(4, 24);
            this.Tab_Logs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Logs.Name = "Tab_Logs";
            this.Tab_Logs.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Logs.Size = new System.Drawing.Size(899, 391);
            this.Tab_Logs.TabIndex = 1;
            this.Tab_Logs.Text = "Logs";
            this.Tab_Logs.UseVisualStyleBackColor = true;
            // 
            // RTB_Logs
            // 
            this.RTB_Logs.BackColor = System.Drawing.SystemColors.ControlDark;
            this.RTB_Logs.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.RTB_Logs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RTB_Logs.Location = new System.Drawing.Point(4, 3);
            this.RTB_Logs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.RTB_Logs.Name = "RTB_Logs";
            this.RTB_Logs.ReadOnly = true;
            this.RTB_Logs.Size = new System.Drawing.Size(891, 385);
            this.RTB_Logs.TabIndex = 0;
            this.RTB_Logs.Text = "";
            // 
            // Button_Start
            // 
            this.Button_Start.Location = new System.Drawing.Point(720, 14);
            this.Button_Start.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button_Start.Name = "Button_Start";
            this.Button_Start.Size = new System.Drawing.Size(88, 27);
            this.Button_Start.TabIndex = 1;
            this.Button_Start.Text = "Start";
            this.Button_Start.UseVisualStyleBackColor = true;
            this.Button_Start.Click += new System.EventHandler(this.Button_Start_Click);
            // 
            // Button_Stop
            // 
            this.Button_Stop.Location = new System.Drawing.Point(816, 14);
            this.Button_Stop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button_Stop.Name = "Button_Stop";
            this.Button_Stop.Size = new System.Drawing.Size(88, 27);
            this.Button_Stop.TabIndex = 2;
            this.Button_Stop.Text = "Stop";
            this.Button_Stop.UseVisualStyleBackColor = true;
            this.Button_Stop.Click += new System.EventHandler(this.Button_Stop_Click);
            // 
            // Bar_Load
            // 
            this.Bar_Load.Location = new System.Drawing.Point(12, 54);
            this.Bar_Load.Name = "Bar_Load";
            this.Bar_Load.Size = new System.Drawing.Size(688, 10);
            this.Bar_Load.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.Bar_Load.TabIndex = 3;
            this.Bar_Load.Visible = false;
            // 
            // ActiveConnection
            // 
            this.ActiveConnection.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ActiveConnection.Enabled = false;
            this.ActiveConnection.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ActiveConnection.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ActiveConnection.Location = new System.Drawing.Point(12, 23);
            this.ActiveConnection.Name = "ActiveConnection";
            this.ActiveConnection.ReadOnly = true;
            this.ActiveConnection.Size = new System.Drawing.Size(688, 25);
            this.ActiveConnection.TabIndex = 4;
            this.ActiveConnection.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ActiveConnection.Visible = false;
            // 
            // Connections
            // 
            this.Connections.AutoSize = true;
            this.Connections.Location = new System.Drawing.Point(720, 54);
            this.Connections.Name = "Connections";
            this.Connections.Size = new System.Drawing.Size(0, 15);
            this.Connections.TabIndex = 5;
            // 
            // Etumreps
            // 
            this.Etumreps.AutoSize = true;
            this.Etumreps.Location = new System.Drawing.Point(720, 84);
            this.Etumreps.Name = "Etumreps";
            this.Etumreps.Size = new System.Drawing.Size(0, 15);
            this.Etumreps.TabIndex = 6;
            // 
            // Authenticated
            // 
            this.Authenticated.AutoSize = true;
            this.Authenticated.Location = new System.Drawing.Point(720, 69);
            this.Authenticated.Name = "Authenticated";
            this.Authenticated.Size = new System.Drawing.Size(0, 15);
            this.Authenticated.TabIndex = 7;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(933, 519);
            this.Controls.Add(this.Authenticated);
            this.Controls.Add(this.Etumreps);
            this.Controls.Add(this.Connections);
            this.Controls.Add(this.ActiveConnection);
            this.Controls.Add(this.Bar_Load);
            this.Controls.Add(this.Button_Stop);
            this.Controls.Add(this.Button_Start);
            this.Controls.Add(this.Tab_Main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.FormClosing += Main_FormClosing;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "EtumrepMMO.Server";
            this.Tab_Main.ResumeLayout(false);
            this.Tab_Settings.ResumeLayout(false);
            this.Tab_Logs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl Tab_Main;
        private System.Windows.Forms.TabPage Tab_Settings;
        private System.Windows.Forms.TabPage Tab_Logs;
        private System.Windows.Forms.RichTextBox RTB_Logs;
        private System.Windows.Forms.Button Button_Start;
        private System.Windows.Forms.Button Button_Stop;
        private System.Windows.Forms.PropertyGrid Grid_Settings;
        private ProgressBar Bar_Load;
        private TextBox ActiveConnection;
        private Label Connections;
        private Label Etumreps;
        private Label Authenticated;
    }
}
