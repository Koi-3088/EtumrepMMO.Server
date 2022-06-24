﻿namespace EtumrepMMO.Server.WinForms
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
            this.TB_QueueList = new System.Windows.Forms.TextBox();
            this.LV_QueueList = new System.Windows.Forms.ListView();
            this.TB_CurrentlyServing = new System.Windows.Forms.TextBox();
            this.TB_ActiveConnections = new System.Windows.Forms.TextBox();
            this.Tab_Settings = new System.Windows.Forms.TabPage();
            this.Grid_Settings = new System.Windows.Forms.PropertyGrid();
            this.Tab_Logs = new System.Windows.Forms.TabPage();
            this.RTB_Logs = new System.Windows.Forms.RichTextBox();
            this.Button_Start = new System.Windows.Forms.Button();
            this.Button_Stop = new System.Windows.Forms.Button();
            this.Label_Connections = new System.Windows.Forms.Label();
            this.Label_Etumreps = new System.Windows.Forms.Label();
            this.Label_Authenticated = new System.Windows.Forms.Label();
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
            this.Tab_Main.Location = new System.Drawing.Point(14, 87);
            this.Tab_Main.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Main.Name = "Tab_Main";
            this.Tab_Main.SelectedIndex = 0;
            this.Tab_Main.Size = new System.Drawing.Size(907, 419);
            this.Tab_Main.TabIndex = 0;
            // 
            // Tab_QueueDisplay
            // 
            this.Tab_QueueDisplay.BackColor = System.Drawing.Color.DarkGray;
            this.Tab_QueueDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Tab_QueueDisplay.Controls.Add(this.TB_QueueList);
            this.Tab_QueueDisplay.Controls.Add(this.LV_QueueList);
            this.Tab_QueueDisplay.Controls.Add(this.TB_CurrentlyServing);
            this.Tab_QueueDisplay.Controls.Add(this.TB_ActiveConnections);
            this.Tab_QueueDisplay.Location = new System.Drawing.Point(4, 24);
            this.Tab_QueueDisplay.Name = "Tab_QueueDisplay";
            this.Tab_QueueDisplay.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_QueueDisplay.Size = new System.Drawing.Size(899, 391);
            this.Tab_QueueDisplay.TabIndex = 0;
            this.Tab_QueueDisplay.Text = "Queue Display";
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
            this.LV_QueueList.AutoArrange = false;
            this.LV_QueueList.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LV_QueueList.Location = new System.Drawing.Point(157, 88);
            this.LV_QueueList.Name = "LV_QueueList";
            this.LV_QueueList.Size = new System.Drawing.Size(716, 297);
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
            // TB_ActiveConnections
            // 
            this.TB_ActiveConnections.BackColor = System.Drawing.SystemColors.ControlDark;
            this.TB_ActiveConnections.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TB_ActiveConnections.Enabled = false;
            this.TB_ActiveConnections.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.TB_ActiveConnections.ForeColor = System.Drawing.SystemColors.ControlText;
            this.TB_ActiveConnections.Location = new System.Drawing.Point(157, 6);
            this.TB_ActiveConnections.Multiline = true;
            this.TB_ActiveConnections.Name = "TB_ActiveConnections";
            this.TB_ActiveConnections.ReadOnly = true;
            this.TB_ActiveConnections.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TB_ActiveConnections.Size = new System.Drawing.Size(716, 76);
            this.TB_ActiveConnections.TabIndex = 4;
            this.TB_ActiveConnections.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TB_ActiveConnections.WordWrap = false;
            // 
            // Tab_Settings
            // 
            this.Tab_Settings.Controls.Add(this.Grid_Settings);
            this.Tab_Settings.Location = new System.Drawing.Point(4, 24);
            this.Tab_Settings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Settings.Name = "Tab_Settings";
            this.Tab_Settings.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Tab_Settings.Size = new System.Drawing.Size(899, 391);
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
            this.RTB_Logs.Size = new System.Drawing.Size(891, 385);
            this.RTB_Logs.TabIndex = 0;
            this.RTB_Logs.Text = "";
            // 
            // Button_Start
            // 
            this.Button_Start.Location = new System.Drawing.Point(94, 12);
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
            this.Button_Stop.Location = new System.Drawing.Point(94, 43);
            this.Button_Stop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button_Stop.Name = "Button_Stop";
            this.Button_Stop.Size = new System.Drawing.Size(88, 27);
            this.Button_Stop.TabIndex = 2;
            this.Button_Stop.Text = "Stop";
            this.Button_Stop.UseVisualStyleBackColor = true;
            this.Button_Stop.Click += new System.EventHandler(this.Button_Stop_Click);
            // 
            // Label_Connections
            // 
            this.Label_Connections.AutoSize = true;
            this.Label_Connections.Location = new System.Drawing.Point(214, 18);
            this.Label_Connections.Name = "Label_Connections";
            this.Label_Connections.Size = new System.Drawing.Size(0, 15);
            this.Label_Connections.TabIndex = 5;
            // 
            // Label_Etumreps
            // 
            this.Label_Etumreps.AutoSize = true;
            this.Label_Etumreps.Location = new System.Drawing.Point(214, 47);
            this.Label_Etumreps.Name = "Label_Etumreps";
            this.Label_Etumreps.Size = new System.Drawing.Size(0, 15);
            this.Label_Etumreps.TabIndex = 6;
            // 
            // Label_Authenticated
            // 
            this.Label_Authenticated.AutoSize = true;
            this.Label_Authenticated.Location = new System.Drawing.Point(214, 32);
            this.Label_Authenticated.Name = "Label_Authenticated";
            this.Label_Authenticated.Size = new System.Drawing.Size(0, 15);
            this.Label_Authenticated.TabIndex = 7;
            // 
            // PB_Ready
            // 
            this.PB_Ready.BackColor = System.Drawing.SystemColors.ControlLight;
            this.PB_Ready.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PB_Ready.Location = new System.Drawing.Point(14, 12);
            this.PB_Ready.Name = "PB_Ready";
            this.PB_Ready.Size = new System.Drawing.Size(61, 58);
            this.PB_Ready.TabIndex = 8;
            this.PB_Ready.TabStop = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(933, 519);
            this.Controls.Add(this.PB_Ready);
            this.Controls.Add(this.Label_Authenticated);
            this.Controls.Add(this.Label_Etumreps);
            this.Controls.Add(this.Label_Connections);
            this.Controls.Add(this.Button_Stop);
            this.Controls.Add(this.Button_Start);
            this.Controls.Add(this.Tab_Main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "Main";
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
        private TextBox TB_ActiveConnections;
        private Label Label_Connections;
        private Label Label_Etumreps;
        private Label Label_Authenticated;
        private TabPage Tab_QueueDisplay;
        private TextBox TB_QueueList;
        private ListView LV_QueueList;
        private TextBox TB_CurrentlyServing;
        private PictureBox PB_Ready;
    }
}