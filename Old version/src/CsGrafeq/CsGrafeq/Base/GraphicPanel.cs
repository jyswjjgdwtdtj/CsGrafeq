using CsGrafeq.Addons.Implicit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.Base.Values;
using System.Resources;
using CsGrafeq.Geometry;
using CsGrafeq.Geometry.Shapes.Getter;
using CsGrafeq.Geometry.Shapes;
using System.Reflection;
using Microsoft.VisualBasic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Runtime.CompilerServices;


namespace CsGrafeq.Base
{
    public partial class GraphicPanel : UserControl
    {
        private DisplayerBase Player=new DisplayerBase();
        private OperationPanel OpPanel=new OperationPanel();
        private DragPanel DgPanel=new DragPanel();
        private GeometryPad GP;
        public GraphicPanel()
        {
            InitializeComponent();
            SuspendLayout();
            Player.Dock = DockStyle.Fill;
            Player.PanelWidth = 200;
            OpPanel.Location = System.Drawing.Point.Empty;
            OpPanel.Width = Player.PanelWidth;
            OpPanel.Height = Height;
            OpPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            OpPanel.BackColor = Color.White;
            DgPanel.Width = 3;
            DgPanel.Left = Player.PanelWidth;
            DgPanel.Padding = new Padding(0);
            DgPanel.Margin = new Padding(0);
            DgPanel.BackColor = Color.FromArgb(200, 200, 200);
            DgPanel.Height = Height;
            DgPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            DgPanel.Cursor = Cursors.SizeWE;
            Controls.Add(DgPanel);
            Controls.Add(OpPanel);
            Controls.Add(Player);
            Player.SendToBack();
            Player.AddonListChanged += Player_AddonListChanged;
            Player.AppendAddon(new GeometryPad());
            Player.AppendAddon(new ImplicitFunctionPad());
            DgPanel.BringToFront();
            OpPanel.BringToFront();

            ResumeLayout(false);
            int DownX = 0;
            bool IsDown = false;
            DgPanel.MouseDown += (s, e) =>
            {
                DownX = e.X;
                IsDown = true;
            };
            DgPanel.MouseMove += (s, e) =>
            {
                if (IsDown)
                {
                    int a = DgPanel.Left + e.X - DownX;
                    if (a > Width / 2)
                        a = Width / 2;
                    if (a < 20)
                        a = 20;
                    if (a >= 20 && a <= Width / 2 && a != DgPanel.Left)
                    {
                        SuspendLayout();
                        DgPanel.Left = a;
                        OpPanel.Width = DgPanel.Left;
                        Player.PanelWidth = OpPanel.Width;
                        Player.ReRenderAxis();
                        OpPanel.Refresh();
                        ResumeLayout();
                    }
                }
            };
            DgPanel.MouseUp += (s, e) =>
            {
                IsDown = false;
            };
            OpPanel.checkBox1.CheckedChanged += CheckBox_CheckedChanged;
            OpPanel.checkBox2.CheckedChanged += CheckBox_CheckedChanged;
            OpPanel.checkBox3.CheckedChanged += CheckBox_CheckedChanged;
        }

        private void Player_AddonListChanged(object sender, EventArgs e)
        {
            OpPanel.SuspendLayout();
            var p = OpPanel.tabPage_Axis;
            foreach(TabPage page in OpPanel.tabControl_Operation.TabPages.Cast<TabPage>().ToArray())
            {
                if (page == p)
                    continue;
                if (!Player.Addons.MyContains((addon) =>
                {
                    return page.Controls.Contains(addon.Addon.OpControl);
                }))
                {
                    OpPanel.tabControl_Operation.TabPages.Remove(page);
                }
            }
            IEnumerable<TabPage> tabpages = OpPanel.tabControl_Operation.TabPages.Cast<TabPage>();
            foreach (AddonClass addonclass in Player.Addons)
            {
                if (!tabpages.MyContains((tabpage) =>
                {
                    return tabpage.Controls.Contains(addonclass.Addon.OpControl);
                }))
                {
                    TabPage page = new TabPage();
                    Control c = addonclass.Addon.OpControl;
                    page.Controls.Add(c);
                    page.Text = c.Tag as string;
                    c.Dock = DockStyle.Fill;
                    OpPanel.tabControl_Operation.TabPages.Add(page);
                }
            }
            OpPanel.ResumeLayout(true);
            OpPanel.Refresh();
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Player.DrawAxisLine=OpPanel.checkBox1.Checked;
            Player.DrawAxisNumber = OpPanel.checkBox2.Checked;
            Player.DrawAxisGrid = OpPanel.checkBox3.Checked;
            Player.ReRenderAxis();
        }
        private static void ForceToPaint(Control c)
        {
            c.GetType().GetMethod("OnPaint", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(c, new object[] { new PaintEventArgs(c.CreateGraphics(), c.ClientRectangle) });
            foreach (Control i in c.Controls)
                ForceToPaint(i);
        }
        private class DragPanel : Panel
        {
        }
    }

}
namespace CsGrafeq
{
    internal static partial class ExMethods
    {
        public static bool MyContains<T>(this IEnumerable<T> values,Func<T,bool> func)
        {
            foreach(var v in values)
                if (func(v))
                    return true;
            return false;
        }
    }
}