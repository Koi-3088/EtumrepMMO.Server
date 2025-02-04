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
            this.Tab_QueueDisplay = new System.Windows.Forms.TabPage();
            this.LV_Concurrent = new System.Windows.Forms.ListView();
            this.TB_QueueList = new System.Windows.Forms.TextBox();
            this.LV_QueueList = new System.Windows.Forms.ListView();
            this.TB_CurrentlyServing = new System.Windows.Forms.TextBox();
            this.Tab_Settings = new System.Windows.Forms.TabPage();
            this.Grid_Settings = new System.Windows.Forms.PropertyGrid();
            this.Tab_Logs = new System.Windows.Forms.TabPage();
            this.RTB_Logs = new System.Windows.Forms.RichTextBox();
            this.Button_Start = new System.Windows.Forms.Button();
            this.Button_Stop = new System.Windows.Forms.Button();
            this.PB_Ready = new System.Windows.Forms.PictureBox();
            this.Tab_Main.SuspendLayout();
            this.Tab_QueueDisplay.SuspendLayout();
            this.Tab_Settings.SuspendLayout();
            this.Tab_Logs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PB_Ready)).BeginInit();
            this.SuspendLayout();
            // 
            // Tab_Main
            // 
            this.Tab_Main.Controls.Add(this.Tab_QueueDisplay);
            this.Tab_Main.Controls.Add(this.Tab_Settings);
            this.Tab_Main.Controls.Add(this.Tab_Logs);
            this.Tab_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tab_Main.Location = new System.Drawing.Point(0, 0);
            this.Tab_Main.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Main.Name = "Tab_Main";
            this.Tab_Main.SelectedIndex = 0;
            this.Tab_Main.Size = new System.Drawing.Size(680, 357);
            this.Tab_Main.TabIndex = 0;
            // 
            // Tab_QueueDisplay
            // 
            this.Tab_QueueDisplay.BackColor = System.Drawing.Color.DarkGray;
            this.Tab_QueueDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Tab_QueueDisplay.Controls.Add(this.LV_Concurrent);
            this.Tab_QueueDisplay.Controls.Add(this.TB_QueueList);
            this.Tab_QueueDisplay.Controls.Add(this.LV_QueueList);
            this.Tab_QueueDisplay.Controls.Add(this.TB_CurrentlyServing);
            this.Tab_QueueDisplay.Location = new System.Drawing.Point(4, 24);
            this.Tab_QueueDisplay.Name = "Tab_QueueDisplay";
            this.Tab_QueueDisplay.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_QueueDisplay.Size = new System.Drawing.Size(672, 329);
            this.Tab_QueueDisplay.TabIndex = 0;
            this.Tab_QueueDisplay.Text = "Queue Display";
            // 
            // LV_Concurrent
            // 
            this.LV_Concurrent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LV_Concurrent.AutoArrange = false;
            this.LV_Concurrent.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LV_Concurrent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LV_Concurrent.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.LV_Concurrent.Location = new System.Drawing.Point(157, 6);
            this.LV_Concurrent.MultiSelect = false;
            this.LV_Concurrent.Name = "LV_Concurrent";
            this.LV_Concurrent.Size = new System.Drawing.Size(507, 76);
            this.LV_Concurrent.TabIndex = 11;
            this.LV_Concurrent.UseCompatibleStateImageBehavior = false;
            this.LV_Concurrent.View = System.Windows.Forms.View.List;
            // 
            // TB_QueueList
            // 
            this.TB_QueueList.BackColor = System.Drawing.SystemColors.ControlDark;
            this.TB_QueueList.Location = new System.Drawing.Point(6, 88);
            this.TB_QueueList.Name = "TB_QueueList";
            this.TB_QueueList.ReadOnly = true;
            this.TB_QueueList.Size = new System.Drawing.Size(145, 23);
            this.TB_QueueList.TabIndex = 10;
            this.TB_QueueList.Text = "Queue:";
            this.TB_QueueList.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LV_QueueList
            // 
            this.LV_QueueList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LV_QueueList.AutoArrange = false;
            this.LV_QueueList.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LV_QueueList.Location = new System.Drawing.Point(157, 88);
            this.LV_QueueList.Name = "LV_QueueList";
            this.LV_QueueList.Size = new System.Drawing.Size(507, 233);
            this.LV_QueueList.TabIndex = 9;
            this.LV_QueueList.UseCompatibleStateImageBehavior = false;
            this.LV_QueueList.View = System.Windows.Forms.View.List;
            // 
            // TB_CurrentlyServing
            // 
            this.TB_CurrentlyServing.BackColor = System.Drawing.SystemColors.ControlDark;
            this.TB_CurrentlyServing.Location = new System.Drawing.Point(6, 6);
            this.TB_CurrentlyServing.Name = "TB_CurrentlyServing";
            this.TB_CurrentlyServing.ReadOnly = true;
            this.TB_CurrentlyServing.Size = new System.Drawing.Size(145, 23);
            this.TB_CurrentlyServing.TabIndex = 8;
            this.TB_CurrentlyServing.Text = "Currently serving:";
            this.TB_CurrentlyServing.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Tab_Settings
            // 
            this.Tab_Settings.Controls.Add(this.Grid_Settings);
            this.Tab_Settings.Location = new System.Drawing.Point(4, 24);
            this.Tab_Settings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Settings.Name = "Tab_Settings";
            this.Tab_Settings.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Settings.Size = new System.Drawing.Size(672, 329);
            this.Tab_Settings.TabIndex = 1;
            this.Tab_Settings.Text = "Settings";
            this.Tab_Settings.UseVisualStyleBackColor = true;
            // 
            // Grid_Settings
            // 
            this.Grid_Settings.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Grid_Settings.CategorySplitterColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Grid_Settings.CommandsBorderColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Grid_Settings.DisabledItemForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.Grid_Settings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Grid_Settings.HelpBackColor = System.Drawing.SystemColors.ControlLight;
            this.Grid_Settings.HelpBorderColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Grid_Settings.LineColor = System.Drawing.SystemColors.ControlDark;
            this.Grid_Settings.Location = new System.Drawing.Point(4, 3);
            this.Grid_Settings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Grid_Settings.Name = "Grid_Settings";
            this.Grid_Settings.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.Grid_Settings.SelectedItemWithFocusBackColor = System.Drawing.SystemColors.ButtonFace;
            this.Grid_Settings.SelectedItemWithFocusForeColor = System.Drawing.SystemColors.Highlight;
            this.Grid_Settings.Size = new System.Drawing.Size(664, 323);
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
            this.Tab_Logs.Size = new System.Drawing.Size(672, 329);
            this.Tab_Logs.TabIndex = 2;
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
            this.RTB_Logs.Size = new System.Drawing.Size(664, 323);
            this.RTB_Logs.TabIndex = 0;
            this.RTB_Logs.Text = "";
            // 
            // Button_Start
            // 
            this.Button_Start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Start.Location = new System.Drawing.Point(563, 3);
            this.Button_Start.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button_Start.Name = "Button_Start";
            this.Button_Start.Size = new System.Drawing.Size(56, 22);
            this.Button_Start.TabIndex = 1;
            this.Button_Start.Text = "Start";
            this.Button_Start.UseVisualStyleBackColor = true;
            this.Button_Start.Click += new System.EventHandler(this.Button_Start_Click);
            // 
            // Button_Stop
            // 
            this.Button_Stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button_Stop.Location = new System.Drawing.Point(620, 3);
            this.Button_Stop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button_Stop.Name = "Button_Stop";
            this.Button_Stop.Size = new System.Drawing.Size(56, 22);
            this.Button_Stop.TabIndex = 2;
            this.Button_Stop.Text = "Stop";
            this.Button_Stop.UseVisualStyleBackColor = true;
            this.Button_Stop.Click += new System.EventHandler(this.Button_Stop_Click);
            // 
            // PB_Ready
            // 
            this.PB_Ready.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PB_Ready.BackColor = System.Drawing.SystemColors.ControlLight;
            this.PB_Ready.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PB_Ready.Location = new System.Drawing.Point(559, -1);
            this.PB_Ready.Name = "PB_Ready";
            this.PB_Ready.Size = new System.Drawing.Size(120, 29);
            this.PB_Ready.TabIndex = 8;
            this.PB_Ready.TabStop = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(680, 357);
            this.Controls.Add(this.Button_Stop);
            this.Controls.Add(this.Button_Start);
            this.Controls.Add(this.PB_Ready);
            this.Controls.Add(this.Tab_Main);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EtumrepMMO.Server";
            this.FormClosing += Main_FormClosing;
            this.Tab_Main.ResumeLayout(false);
            this.Tab_QueueDisplay.ResumeLayout(false);
            this.Tab_QueueDisplay.PerformLayout();
            this.Tab_Settings.ResumeLayout(false);
            this.Tab_Logs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PB_Ready)).EndInit();
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
        private TabPage Tab_QueueDisplay;
        private TextBox TB_QueueList;
        private ListView LV_QueueList;
        private TextBox TB_CurrentlyServing;
        private PictureBox PB_Ready;
        private ListView LV_Concurrent;
    }
}
