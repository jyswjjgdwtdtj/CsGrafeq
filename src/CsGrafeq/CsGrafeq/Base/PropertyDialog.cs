using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq.Base
{
    public partial class PropertyDialog : Form
    {
        public PropertyDialog(Dictionary<string, (string init, Type t)> obj)
        {
            InitializeComponent();
            SetValue(obj);
        }
        private Label[] Labels;
        private TextBox[] TextBoxes;
        KeyValuePair<string, (string init, Type t)>[] kvp;
        private void SetValue(Dictionary<string,(string init,Type t)> obj)
        {
            Labels = new Label[obj.Count];
            TextBoxes= new TextBox[obj.Count];
            kvp=obj.ToArray();
            for (int i = 0; i < obj.Count; i++)
            {
                Panel panel= new Panel();
                Label label=Labels[i]=new Label();
                TextBox txtbox= TextBoxes[i]=new TextBox();
                panel.Controls.Add(label);
                panel.Controls.Add(txtbox);
                panel.Dock = System.Windows.Forms.DockStyle.Fill;
                panel.Size = new System.Drawing.Size(333, 25);
                panel.AutoSize = true;
                label.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                label.Location = new System.Drawing.Point(2, 4);
                label.Size = new System.Drawing.Size(44, 20);
                label.TabIndex = 0;
                label.Text =kvp[i].Key;
                label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                txtbox.BackColor = System.Drawing.Color.White;
                txtbox.Location = new System.Drawing.Point(43, 1);
                txtbox.Size = new System.Drawing.Size(103, 21);
                txtbox.Text = kvp[i].Value.init;
                verticalFlowLayoutPanel1.Controls.Add(panel);
            }
            Button c=new Button(), o=new Button();
            c.Location = new System.Drawing.Point(177, 12);
            c.Size = new System.Drawing.Size(67, 29);
            c.Text = "确定";
            c.UseVisualStyleBackColor = true;
            c.Click += new System.EventHandler(button1_Click);

            o.Location = new System.Drawing.Point(250, 12);
            o.Size = new System.Drawing.Size(67, 29);
            o.Text = "取消";
            o.UseVisualStyleBackColor = true;
            o.Click += new System.EventHandler(button2_Click);
            Panel pp = new Panel();
            pp.Controls.Add(c);
            pp.Controls.Add(o);
            pp.Dock = System.Windows.Forms.DockStyle.Fill;
            pp.Size = new System.Drawing.Size(333, 50);
            verticalFlowLayoutPanel1.Controls.Add(pp);
        }
        public Dictionary<string,string> returndic = new Dictionary<string,string>();
        public bool OK=false;
        private void button1_Click(object sender, EventArgs e)
        {
           try
            {
                returndic.Clear();
                for(int i = 0; i < kvp.Length; i++)
                {
                    if (kvp[i].Value.t == typeof(double))
                        returndic.Add(kvp[i].Key, double.Parse(TextBoxes[i].Text).ToString());
                    else
                        returndic.Add(kvp[i].Key, TextBoxes[i].Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("坐标值不合法"+ex.Message);
                return;
            }
            OK=true;    
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close ();
        }

    }
}
