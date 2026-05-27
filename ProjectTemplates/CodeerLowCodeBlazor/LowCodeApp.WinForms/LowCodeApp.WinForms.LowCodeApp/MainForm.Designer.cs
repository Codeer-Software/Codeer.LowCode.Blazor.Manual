namespace LowCodeApp.WinForms.LowCodeApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _blazorWebView = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
            _menuStrip = new MenuStrip();
            _etcToolStripMenuItem = new ToolStripMenuItem();
            _developerToolToolStripMenuItem = new ToolStripMenuItem();
            _menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // _blazorWebView
            // 
            _blazorWebView.Dock = DockStyle.Fill;
            _blazorWebView.Location = new Point(0, 24);
            _blazorWebView.Name = "_blazorWebView";
            _blazorWebView.Size = new Size(800, 426);
            _blazorWebView.StartPath = "/";
            _blazorWebView.TabIndex = 0;
            _blazorWebView.Text = "blazorWebView1";
            // 
            // _menuStrip
            // 
            _menuStrip.Items.AddRange(new ToolStripItem[] { _etcToolStripMenuItem });
            _menuStrip.Location = new Point(0, 0);
            _menuStrip.Name = "_menuStrip";
            _menuStrip.Size = new Size(800, 24);
            _menuStrip.TabIndex = 1;
            _menuStrip.Text = "menuStrip1";
            // 
            // _etcToolStripMenuItem
            // 
            _etcToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { _developerToolToolStripMenuItem });
            _etcToolStripMenuItem.Name = "_etcToolStripMenuItem";
            _etcToolStripMenuItem.Size = new Size(35, 20);
            _etcToolStripMenuItem.Text = "etc";
            // 
            // _developerToolToolStripMenuItem
            // 
            _developerToolToolStripMenuItem.Name = "_developerToolToolStripMenuItem";
            _developerToolToolStripMenuItem.Size = new Size(180, 22);
            _developerToolToolStripMenuItem.Text = "Developer Tool";
            _developerToolToolStripMenuItem.Click += _developerToolToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(_blazorWebView);
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;
            Name = "MainForm";
            Text = "WinForms + Codeer.LowCode.Blazor";
            _menuStrip.ResumeLayout(false);
            _menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView _blazorWebView;
        private MenuStrip _menuStrip;
        private ToolStripMenuItem _etcToolStripMenuItem;
        private ToolStripMenuItem _developerToolToolStripMenuItem;
    }
}
