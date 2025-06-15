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
    public partial class OperationPanel : UserControl
    {
        public OperationPanel()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint,true);
        }
        private void noFlashTabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush=new SolidBrush(Color.Black);
            TabControl tabControl1 = noFlashTabControl2;
            TabPage _tabPage = tabControl1.TabPages[e.Index];
            Rectangle _tabBounds = tabControl1.GetTabRect(e.Index);

            e.DrawBackground();
            if (e.State == DrawItemState.Selected)
            {
                g.FillRectangle(new SolidBrush(Color.White), e.Bounds);
            }
            else
            {
            }
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, tabControl1.GetTabRect(tabControl1.TabCount-1).Bottom,_tabBounds.Width,tabControl1.Height));
            Font _tabFont = e.Font;
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
