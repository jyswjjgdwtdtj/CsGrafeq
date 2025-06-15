using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Layout;
using System.Windows.Forms;

namespace CsGrafeq.Base
{
    public class FlowLayout : LayoutEngine
    {
        public FlowLayout() {  }
        public override bool Layout(
            object container,
            LayoutEventArgs layoutEventArgs)
        {
            Control parent = container as Control;
            Rectangle parentDisplayRectangle = parent.DisplayRectangle;
            Point nextControlLocation = parentDisplayRectangle.Location;
            foreach (Control c in parent.Controls)
            {
                if (!c.Visible)
                {
                    continue;
                }
                nextControlLocation.Offset(c.Margin.Left, c.Margin.Top);
                c.Location = nextControlLocation;
                if (c.AutoSize)
                {
                    c.Size = c.GetPreferredSize(parentDisplayRectangle.Size);
                }
                nextControlLocation.X = parentDisplayRectangle.X;
                nextControlLocation.Y += c.Height + c.Margin.Bottom;
            }
            return false;
        }
    }
}
