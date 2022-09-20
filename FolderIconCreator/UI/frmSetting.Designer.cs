namespace FolderIconCreator.UI
{
    partial class frmSetting
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
            this.label2 = new System.Windows.Forms.Label();
            this.btnDir = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.chkDefForeImage = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnBackground = new System.Windows.Forms.Button();
            this.lblBackPath = new System.Windows.Forms.Label();
            this.lblForePath = new System.Windows.Forms.Label();
            this.btnForeground = new System.Windows.Forms.Button();
            this.btnBackDel = new System.Windows.Forms.Button();
            this.btnForeDel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(2, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 18);
            this.label2.TabIndex = 7;
            this.label2.Text = "フォルダアイコン変更対象フォルダ";
            // 
            // btnDir
            // 
            this.btnDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDir.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnDir.Location = new System.Drawing.Point(336, 23);
            this.btnDir.Name = "btnDir";
            this.btnDir.Size = new System.Drawing.Size(28, 28);
            this.btnDir.TabIndex = 6;
            this.btnDir.Text = "...";
            this.btnDir.UseVisualStyleBackColor = true;
            this.btnDir.Click += new System.EventHandler(this.btnDir_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(2, 28);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(332, 19);
            this.textBox1.TabIndex = 5;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(5, 107);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(256, 256);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(2, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 18);
            this.label1.TabIndex = 9;
            this.label1.Text = "生成されるフォルダアイコン";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button1.Location = new System.Drawing.Point(188, 53);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 30);
            this.button1.TabIndex = 10;
            this.button1.Text = "確定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "アイコンファイル(*.ico)|*.ico|すべてのファイル(*.*)|*.*;";
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button2.Location = new System.Drawing.Point(279, 53);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 30);
            this.button2.TabIndex = 11;
            this.button2.Text = "戻す";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button1_Click);
            // 
            // chkDefForeImage
            // 
            this.chkDefForeImage.AutoSize = true;
            this.chkDefForeImage.Location = new System.Drawing.Point(91, 374);
            this.chkDefForeImage.Name = "chkDefForeImage";
            this.chkDefForeImage.Size = new System.Drawing.Size(68, 16);
            this.chkDefForeImage.TabIndex = 12;
            this.chkDefForeImage.Text = "デフォルト";
            this.chkDefForeImage.UseVisualStyleBackColor = true;
            this.chkDefForeImage.CheckedChanged += new System.EventHandler(this.chkTransparent_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.Location = new System.Drawing.Point(2, 438);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 18);
            this.label3.TabIndex = 13;
            this.label3.Text = "背景画像";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label4.Location = new System.Drawing.Point(2, 372);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 18);
            this.label4.TabIndex = 14;
            this.label4.Text = "前景画像";
            // 
            // btnBackground
            // 
            this.btnBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBackground.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnBackground.Location = new System.Drawing.Point(336, 455);
            this.btnBackground.Name = "btnBackground";
            this.btnBackground.Size = new System.Drawing.Size(28, 28);
            this.btnBackground.TabIndex = 15;
            this.btnBackground.Text = "...";
            this.btnBackground.UseVisualStyleBackColor = true;
            this.btnBackground.Click += new System.EventHandler(this.btnBackground_Click);
            // 
            // lblBackPath
            // 
            this.lblBackPath.AutoSize = true;
            this.lblBackPath.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblBackPath.Location = new System.Drawing.Point(12, 460);
            this.lblBackPath.Name = "lblBackPath";
            this.lblBackPath.Size = new System.Drawing.Size(32, 18);
            this.lblBackPath.TabIndex = 16;
            this.lblBackPath.Text = "なし";
            // 
            // lblForePath
            // 
            this.lblForePath.AutoSize = true;
            this.lblForePath.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblForePath.Location = new System.Drawing.Point(12, 401);
            this.lblForePath.Name = "lblForePath";
            this.lblForePath.Size = new System.Drawing.Size(32, 18);
            this.lblForePath.TabIndex = 17;
            this.lblForePath.Text = "なし";
            // 
            // btnForeground
            // 
            this.btnForeground.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnForeground.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnForeground.Location = new System.Drawing.Point(336, 396);
            this.btnForeground.Name = "btnForeground";
            this.btnForeground.Size = new System.Drawing.Size(28, 28);
            this.btnForeground.TabIndex = 18;
            this.btnForeground.Text = "...";
            this.btnForeground.UseVisualStyleBackColor = true;
            this.btnForeground.Click += new System.EventHandler(this.btnBackground_Click);
            // 
            // btnBackDel
            // 
            this.btnBackDel.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnBackDel.Location = new System.Drawing.Point(57, 433);
            this.btnBackDel.Name = "btnBackDel";
            this.btnBackDel.Size = new System.Drawing.Size(28, 28);
            this.btnBackDel.TabIndex = 19;
            this.btnBackDel.Text = "消";
            this.btnBackDel.UseVisualStyleBackColor = true;
            this.btnBackDel.Click += new System.EventHandler(this.btnBackground_Click);
            // 
            // btnForeDel
            // 
            this.btnForeDel.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnForeDel.Location = new System.Drawing.Point(57, 367);
            this.btnForeDel.Name = "btnForeDel";
            this.btnForeDel.Size = new System.Drawing.Size(28, 28);
            this.btnForeDel.TabIndex = 20;
            this.btnForeDel.Text = "消";
            this.btnForeDel.UseVisualStyleBackColor = true;
            this.btnForeDel.Click += new System.EventHandler(this.btnBackground_Click);
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 489);
            this.Controls.Add(this.btnForeground);
            this.Controls.Add(this.btnBackground);
            this.Controls.Add(this.btnForeDel);
            this.Controls.Add(this.btnBackDel);
            this.Controls.Add(this.lblForePath);
            this.Controls.Add(this.lblBackPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkDefForeImage);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnDir);
            this.Controls.Add(this.textBox1);
            this.Name = "frmSetting";
            this.Text = "frmSetting";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmSetting_FormClosed);
            this.Shown += new System.EventHandler(this.frmSetting_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDir;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox chkDefForeImage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnBackground;
        private System.Windows.Forms.Label lblBackPath;
        private System.Windows.Forms.Label lblForePath;
        private System.Windows.Forms.Button btnForeground;
        private System.Windows.Forms.Button btnBackDel;
        private System.Windows.Forms.Button btnForeDel;
    }
}