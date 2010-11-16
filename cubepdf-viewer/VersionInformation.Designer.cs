﻿namespace Cube
{
    partial class VersionInformation
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.OKButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Version = new System.Windows.Forms.Label();
            this.cubePDFLink = new System.Windows.Forms.LinkLabel();
            this.Logo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).BeginInit();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(193, 88);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 4;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(56, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "CubePDF Viewer";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(57, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Copyright(C) 2010 CubeSoft.";
            // 
            // Version
            // 
            this.Version.AutoSize = true;
            this.Version.Location = new System.Drawing.Point(57, 29);
            this.Version.Name = "Version";
            this.Version.Size = new System.Drawing.Size(44, 12);
            this.Version.TabIndex = 1;
            this.Version.Text = "Version";
            // 
            // cubePDFLink
            // 
            this.cubePDFLink.AutoSize = true;
            this.cubePDFLink.Location = new System.Drawing.Point(57, 63);
            this.cubePDFLink.Name = "cubePDFLink";
            this.cubePDFLink.Size = new System.Drawing.Size(211, 12);
            this.cubePDFLink.TabIndex = 3;
            this.cubePDFLink.TabStop = true;
            this.cubePDFLink.Text = "http://www.cube-soft.jp/cubepdfviewer/";
            this.cubePDFLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.cubePDFLink_LinkClicked);
            // 
            // Logo
            // 
            this.Logo.Location = new System.Drawing.Point(12, 29);
            this.Logo.Name = "Logo";
            this.Logo.Size = new System.Drawing.Size(32, 32);
            this.Logo.TabIndex = 5;
            this.Logo.TabStop = false;
            // 
            // VersionInformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(283, 123);
            this.Controls.Add(this.Logo);
            this.Controls.Add(this.cubePDFLink);
            this.Controls.Add(this.Version);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VersionInformation";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "バージョン情報";
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label Version;
        private System.Windows.Forms.LinkLabel cubePDFLink;
        private System.Windows.Forms.PictureBox Logo;

    }
}