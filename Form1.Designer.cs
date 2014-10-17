namespace Botten_Anna
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.WB = new System.Windows.Forms.WebBrowser();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // WB
            // 
            this.WB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WB.Location = new System.Drawing.Point(0, 0);
            this.WB.Margin = new System.Windows.Forms.Padding(1, 1, 3, 3);
            this.WB.MaximumSize = new System.Drawing.Size(1123, 883);
            this.WB.MinimumSize = new System.Drawing.Size(1123, 883);
            this.WB.Name = "WB";
            this.WB.Size = new System.Drawing.Size(1123, 883);
            this.WB.TabIndex = 0;
            this.WB.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            this.WB.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.WB_PreviewKeyDown);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1123, 883);
            this.Controls.Add(this.WB);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser WB;
        private System.Windows.Forms.Timer timer1;

    }
}

