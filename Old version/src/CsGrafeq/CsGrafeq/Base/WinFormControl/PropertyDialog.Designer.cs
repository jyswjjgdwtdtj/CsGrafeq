namespace CsGrafeq.Base
{
    partial class PropertyDialog
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
            this.verticalFlowLayoutPanel1 = new CsGrafeq.Base.VerticalFlowLayoutPanel();
            this.SuspendLayout();
            // 
            // verticalFlowLayoutPanel1
            // 
            this.verticalFlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verticalFlowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.verticalFlowLayoutPanel1.Name = "verticalFlowLayoutPanel1";
            this.verticalFlowLayoutPanel1.Size = new System.Drawing.Size(339, 144);
            this.verticalFlowLayoutPanel1.TabIndex = 0;
            // 
            // PropertyDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(339, 144);
            this.Controls.Add(this.verticalFlowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PropertyDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "属性";
            this.ResumeLayout(false);

        }

        #endregion

        private VerticalFlowLayoutPanel verticalFlowLayoutPanel1;
    }
}