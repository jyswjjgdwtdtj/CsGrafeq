using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq.Addons.Implicit
{
    public partial class OpControl : UserControl
    {
        public OpControl()
        {
            InitializeComponent();
            InputBox.KeyPress += (s, e) =>
            {
                if (
                 (e.KeyChar == '.') || (e.KeyChar == '\b') ||
                ('0' <= e.KeyChar && e.KeyChar <= '9') ||
                ('a' <= e.KeyChar && e.KeyChar <= 'z') ||
                (e.KeyChar == '%') || (e.KeyChar == '^') || (e.KeyChar == '*') ||
                 (e.KeyChar == '+') || (e.KeyChar == '(') || (e.KeyChar == ')') ||
                (e.KeyChar == '-') || (e.KeyChar == '/') || (e.KeyChar == ',') ||
                (e.KeyChar == '=') || (e.KeyChar == '<') || (e.KeyChar == '>') ||
                (e.KeyChar == '{') || (e.KeyChar == '}') ||
                (e.KeyChar == 1) ||
                (e.KeyChar == 3) ||
                (e.KeyChar == 22) ||
                (e.KeyChar == 24)
                )
                {
                }
                else if (('A' <= e.KeyChar && e.KeyChar <= 'Z'))
                {
                    e.KeyChar = (char)(e.KeyChar - 'A' + 'a');
                }
                else
                {
                    e.Handled = true;
                }
            };
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
