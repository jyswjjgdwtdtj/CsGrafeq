namespace CsGrafeq.Base
{
    partial class OperationPanel
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl_Operation = new CsGrafeq.Base.NoFlashTabControl();
            this.tabPage_Axis = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_Axis = new System.Windows.Forms.TableLayoutPanel();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.tabControl_Operation.SuspendLayout();
            this.tabPage_Axis.SuspendLayout();
            this.tableLayoutPanel_Axis.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_Operation
            // 
            this.tabControl_Operation.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabControl_Operation.Controls.Add(this.tabPage_Axis);
            this.tabControl_Operation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_Operation.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl_Operation.HotTrack = true;
            this.tabControl_Operation.ItemSize = new System.Drawing.Size(60, 18);
            this.tabControl_Operation.Location = new System.Drawing.Point(0, 0);
            this.tabControl_Operation.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_Operation.Multiline = true;
            this.tabControl_Operation.Name = "tabControl_Operation";
            this.tabControl_Operation.Padding = new System.Drawing.Point(0, 0);
            this.tabControl_Operation.SelectedIndex = 0;
            this.tabControl_Operation.Size = new System.Drawing.Size(492, 626);
            this.tabControl_Operation.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl_Operation.TabIndex = 2;
            this.tabControl_Operation.TabStop = false;
            this.tabControl_Operation.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.noFlashTabControl1_DrawItem);
            // 
            // tabPage_Axis
            // 
            this.tabPage_Axis.Controls.Add(this.tableLayoutPanel_Axis);
            this.tabPage_Axis.Location = new System.Drawing.Point(22, 4);
            this.tabPage_Axis.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_Axis.Name = "tabPage_Axis";
            this.tabPage_Axis.Size = new System.Drawing.Size(466, 618);
            this.tabPage_Axis.TabIndex = 1;
            this.tabPage_Axis.Text = "坐标轴";
            // 
            // tableLayoutPanel_Axis
            // 
            this.tableLayoutPanel_Axis.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel_Axis.ColumnCount = 1;
            this.tableLayoutPanel_Axis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Axis.Controls.Add(this.checkBox3, 0, 2);
            this.tableLayoutPanel_Axis.Controls.Add(this.checkBox2, 0, 1);
            this.tableLayoutPanel_Axis.Controls.Add(this.checkBox1, 0, 0);
            this.tableLayoutPanel_Axis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_Axis.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_Axis.Name = "tableLayoutPanel_Axis";
            this.tableLayoutPanel_Axis.RowCount = 5;
            this.tableLayoutPanel_Axis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel_Axis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel_Axis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel_Axis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel_Axis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel_Axis.Size = new System.Drawing.Size(466, 618);
            this.tableLayoutPanel_Axis.TabIndex = 0;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Location = new System.Drawing.Point(3, 53);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(108, 16);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.Text = "显示坐标轴网格";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(3, 28);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(108, 16);
            this.checkBox2.TabIndex = 1;
            this.checkBox2.Text = "显示坐标轴数字";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(3, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(84, 16);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "显示坐标轴";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // OperationPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tabControl_Operation);
            this.Name = "OperationPanel";
            this.Size = new System.Drawing.Size(492, 626);
            this.tabControl_Operation.ResumeLayout(false);
            this.tabPage_Axis.ResumeLayout(false);
            this.tableLayoutPanel_Axis.ResumeLayout(false);
            this.tableLayoutPanel_Axis.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        public NoFlashTabControl tabControl_Operation;
        public System.Windows.Forms.TabPage tabPage_Axis;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Axis;
        public System.Windows.Forms.CheckBox checkBox3;
        public System.Windows.Forms.CheckBox checkBox2;
        public System.Windows.Forms.CheckBox checkBox1;
    }
}
