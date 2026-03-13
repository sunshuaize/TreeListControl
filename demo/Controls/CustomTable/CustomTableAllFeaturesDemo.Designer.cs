namespace demo.Controls.CustomTable
{
    partial class CustomTableAllFeaturesDemo
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
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
            // 
            // tblMain
            // 
            this.tblMain = new CustomTable();
            this.tblMain.Location = new System.Drawing.Point(20, 20);
            this.tblMain.Size = new System.Drawing.Size(960, 450);
            this.tblMain.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
            this.tblMain.TabIndex = 0;
            // 
            // lblInfo
            // 
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblInfo.Location = new System.Drawing.Point(20, 485);
            this.lblInfo.Size = new System.Drawing.Size(960, 70);
            this.lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
            this.lblInfo.BackColor = System.Drawing.Color.White;
            this.lblInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblInfo.Padding = new System.Windows.Forms.Padding(10);
            this.lblInfo.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
            this.lblInfo.TabIndex = 1;
            // 
            // pnlButtons
            // 
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.pnlButtons.Location = new System.Drawing.Point(20, 480);
            this.pnlButtons.Size = new System.Drawing.Size(960, 80);
            this.pnlButtons.Visible = false;
            this.pnlButtons.TabIndex = 2;
            // 
            // CustomTableAllFeaturesDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.tblMain);
            this.Name = "CustomTableAllFeaturesDemo";
            this.Text = "CustomTable 功能演示";
            this.ResumeLayout(false);

        }

        #endregion

        private CustomTable tblMain;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Panel pnlButtons;
    }
}
