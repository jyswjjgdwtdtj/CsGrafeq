using CsGrafeq.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.Base.Values;

namespace CsGrafeq.Addons.InkPad
{
    public class InkPad:Addon
    {
        private Pen FirstPen=new Pen(Color_Black),SecondPen=new Pen(Color.Red);
        private DateTime a=DateTime.Now;
        public InkPad() {
            _AddonMode = AddonMode.ForPixel;
            Bitmap =new Bitmap(1,1);
            Graphics=Bitmap.GetGraphics();
            Graphics.Clear(Color_Transparent);
            _RenderMode=RenderMode.All;
        }
        private Bitmap Bitmap;
        private Graphics Graphics;
        protected override void Render(Graphics graphics, Rectangle rect)
        {
            Size s = Size;
            RefreshOwnerArguments();
            if (s != Size)
            {
                Bitmap bitmap = new Bitmap(Size.Width, Size.Height);
                Graphics.Dispose();
                Graphics = bitmap.GetGraphics();
                Graphics.Clear(Color_Transparent);
                Graphics.DrawImage(Bitmap, 0, 0);
                Bitmap.Dispose();
                Bitmap = bitmap;
            }
            graphics.DrawImage(Bitmap, 0, 0);
        }
        private bool MouseDown = false;
        private Point lastpoint;
        protected override bool OnMouseDown(MouseEventArgs e)
        {
            MouseDown = true;
            lastpoint = e.Location;
            return false;
        }
        protected override bool OnMouseMove(AddonMouseMoveEventArgs e)
        {
            if (!MouseDown)
                return false;
            Graphics.DrawLine(e.Button==MouseButtons.Left?FirstPen:SecondPen,lastpoint,e.MouseEventArgs.Location);
            lastpoint = e.MouseEventArgs.Location;
            AskForRender();
            return false;
        }
        protected override bool OnMouseUp(MouseEventArgs e)
        {
            MouseDown = false;
            Graphics.DrawLine(e.Button == MouseButtons.Left ? FirstPen : SecondPen, lastpoint, e.Location);
            AskForRender();
            return false;
        }
        protected override void OnLoaded()
        {
            Bitmap=new Bitmap(Size.Width, Size.Height);
            Graphics = Bitmap.GetGraphics();
            Graphics.Clear(Color_Transparent);
        }

    }
}
