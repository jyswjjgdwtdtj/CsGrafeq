using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsGrafeq.Addons.Implicit;
using CsGrafeq.Addons;

namespace CsGrafeq.Addons.Implicit
{
    public partial class ExpressionBar : UserControl
    {
        public ImplicitFunctionPad.ImplicitFunc func;
        public ExpressionBar()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
