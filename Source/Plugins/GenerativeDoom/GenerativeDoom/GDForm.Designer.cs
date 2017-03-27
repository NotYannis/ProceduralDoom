namespace GenerativeDoom
{
    partial class GDForm
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
            this.BtnClose = new System.Windows.Forms.Button();
            this.btnDoMagic = new System.Windows.Forms.Button();
            this.btnMakeEasy = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lbCategories = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // BtnClose
            // 
            this.BtnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnClose.Location = new System.Drawing.Point(72, 179);
            this.BtnClose.Margin = new System.Windows.Forms.Padding(2);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(60, 32);
            this.BtnClose.TabIndex = 0;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // btnDoMagic
            // 
            this.btnDoMagic.Location = new System.Drawing.Point(72, 82);
            this.btnDoMagic.Margin = new System.Windows.Forms.Padding(1);
            this.btnDoMagic.Name = "btnDoMagic";
            this.btnDoMagic.Size = new System.Drawing.Size(81, 36);
            this.btnDoMagic.TabIndex = 1;
            this.btnDoMagic.Text = "Make Hard";
            this.btnDoMagic.UseVisualStyleBackColor = true;
            this.btnDoMagic.Click += new System.EventHandler(this.btnDoMagic_Click);
            // 
            // btnMakeEasy
            // 
            this.btnMakeEasy.Location = new System.Drawing.Point(72, 12);
            this.btnMakeEasy.Margin = new System.Windows.Forms.Padding(1);
            this.btnMakeEasy.Name = "btnMakeEasy";
            this.btnMakeEasy.Size = new System.Drawing.Size(81, 33);
            this.btnMakeEasy.TabIndex = 2;
            this.btnMakeEasy.Text = "Make Easy";
            this.btnMakeEasy.UseVisualStyleBackColor = true;
            this.btnMakeEasy.Click += new System.EventHandler(this.btnAnalysis_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(72, 47);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(1);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(81, 33);
            this.btnGenerate.TabIndex = 3;
            this.btnGenerate.Text = "Make Medium";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lbCategories
            // 
            this.lbCategories.FormattingEnabled = true;
            this.lbCategories.ItemHeight = 14;
            this.lbCategories.Location = new System.Drawing.Point(14, 214);
            this.lbCategories.Margin = new System.Windows.Forms.Padding(1);
            this.lbCategories.Name = "lbCategories";
            this.lbCategories.Size = new System.Drawing.Size(178, 116);
            this.lbCategories.TabIndex = 4;
            // 
            // GDForm
            // 
            this.AcceptButton = this.BtnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.BtnClose;
            this.ClientSize = new System.Drawing.Size(216, 340);
            this.Controls.Add(this.lbCategories);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnMakeEasy);
            this.Controls.Add(this.btnDoMagic);
            this.Controls.Add(this.BtnClose);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GDForm";
            this.Text = "Generative Doom";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GDForm_FormClosing);
            this.Load += new System.EventHandler(this.GDForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Button btnDoMagic;
        private System.Windows.Forms.Button btnMakeEasy;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ListBox lbCategories;
    }
}