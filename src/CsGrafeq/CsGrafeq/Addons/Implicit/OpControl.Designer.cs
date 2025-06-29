namespace CsGrafeq.Addons.Implicit
{
    partial class OpControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        public System.ComponentModel.IContainer components = null;

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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.InputBox = new System.Windows.Forms.TextBox();
            this.FlowPanel = new CsGrafeq.Base.VerticalFlowLayoutPanel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.InputBox);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(324, 30);
            this.panel1.TabIndex = 1;
            // 
            // InputBox
            // 
            this.InputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputBox.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputBox.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.InputBox.Location = new System.Drawing.Point(0, 1);
            this.InputBox.Name = "InputBox";
            this.InputBox.Size = new System.Drawing.Size(321, 28);
            this.InputBox.TabIndex = 0;
            this.InputBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // FlowPanel
            // 
            this.FlowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowPanel.BackColor = System.Drawing.Color.White;
            this.FlowPanel.Location = new System.Drawing.Point(0, 30);
            this.FlowPanel.Name = "FlowPanel";
            this.FlowPanel.Size = new System.Drawing.Size(324, 438);
            this.FlowPanel.TabIndex = 0;
            // 
            // OpControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.FlowPanel);
            this.Name = "OpControl";
            this.Size = new System.Drawing.Size(324, 468);
            this.Tag = "隐函数";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.TextBox InputBox;
        public Base.VerticalFlowLayoutPanel FlowPanel;
    }
}
