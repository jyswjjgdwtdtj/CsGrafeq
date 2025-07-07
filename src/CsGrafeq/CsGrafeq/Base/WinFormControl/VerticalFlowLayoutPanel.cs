using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace CsGrafeq.Base
{
    public class VerticalFlowLayoutPanel:Panel
    {
        private FlowLayout flowLayout=new FlowLayout();
        public override LayoutEngine LayoutEngine => flowLayout;
        public VerticalFlowLayoutPanel()
        {

        }
    }
}
