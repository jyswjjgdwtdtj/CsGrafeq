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
    public partial class ScriptDialog : Form
    {
        public bool OK=false;
        public ScriptDialog(string script)
        {
            InitializeComponent();
            Script=script;
        }
        public string Script
        {
            get=>textBox1.Text;
            set=>textBox1.Text = value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OK = true;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
