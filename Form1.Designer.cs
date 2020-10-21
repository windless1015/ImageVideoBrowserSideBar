namespace TestImagVideoBrowserSideBar
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.imageVideoBrowserSideBar1 = new TestImagVideoBrowserSideBar.ImageVideoBrowserSideBar();
            this.SuspendLayout();
            // 
            // imageVideoBrowserSideBar1
            // 
            this.imageVideoBrowserSideBar1.dataPath = null;
            this.imageVideoBrowserSideBar1.Location = new System.Drawing.Point(262, 12);
            this.imageVideoBrowserSideBar1.Name = "imageVideoBrowserSideBar1";
            this.imageVideoBrowserSideBar1.Size = new System.Drawing.Size(386, 720);
            this.imageVideoBrowserSideBar1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 744);
            this.Controls.Add(this.imageVideoBrowserSideBar1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageVideoBrowserSideBar imageVideoBrowserSideBar1;
    }
}

