using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq
{
    public class DoubleBufferedControl:UserControl
    {
        public delegate void RenderHandler(Graphics g);
        public delegate void VoidHandler();
        public EventList<Graphics> DBRender=new EventList<Graphics>(5);
        public EventList<(Graphics,Rectangle)> DBRenderTo=new EventList<(Graphics, Rectangle)>(5);
        public EventList<Graphics> DBPrint=new EventList<Graphics>(5);
        protected Graphics TargetGraphics;
        internal int width, height;
        protected bool loaded = false;
        protected BufferedGraphics buf;
        protected Color Color_White, Color_Black, Color_A;
        protected SolidBrush Brush_White, Brush_Black;
        protected Pen Pen_White, Pen_Black;
        public DoubleBufferedControl()
        {
            Initialize();
        }
        protected void Initialize()
        {
            Font = new Font("Consolas", 15);
            Color_White = Color.White;
            Color_Black = Color.Black;
            Color_A = Color.FromArgb(0, 255, 255, 255);
            Pen_White = new Pen(Color_White, 1);
            Pen_Black = new Pen(Color_Black, 1);
            Brush_White = new SolidBrush(Color_White);
            Brush_Black = new SolidBrush(Color_Black);
            TargetGraphics = CreateGraphics();
            buf = TargetGraphics.GetBuffer(ClientRectangle);
        }
        protected void CallDBRender(Graphics g)
        {
            DBRender.Invoke(g);
        }
        protected void CallDBRenderTo(Graphics g,Rectangle r)
        {
            DBRenderTo.Invoke((g,r));
        }
        protected void CallDBPrint(Graphics g)
        {
            DBPrint.Invoke(g);
        }
        public void Render()
        {
            Render(TargetGraphics);
        }
        protected virtual void Render(Graphics rt)
        {
            if (!loaded || (!Visible))
                return;
            Graphics graphics = buf.Graphics;
            graphics.Clear(Color_White);
            CallDBRender(graphics);
            buf.Render();
        }
        protected virtual bool RenderTo(Graphics g,Rectangle rectangle)
        {
            BufferedGraphics buf = g.GetBuffer(rectangle);
            Graphics graphics = buf.Graphics;
            graphics.Clear(Color_White);
            CallDBRenderTo(graphics,rectangle);
            buf.Render();
            return true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            width = ClientSize.Width; height = ClientSize.Height;
            CallDBPrint(e.Graphics);
            loaded = true;
            Render(TargetGraphics);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            width = ClientSize.Width; height = ClientSize.Height;
            buf = TargetGraphics.GetBuffer(ClientRectangle);
            Render(TargetGraphics);
        }
        public class EventList<T1>
        {
            private int Capacity;
            private Action<T1>[] EventArray;
            public EventList(int capacity)
            {
                Capacity = capacity;
                EventArray = new Action<T1>[Capacity];
            }
            public EventList():this(10) { }
            public void Add(int index,Action<T1> item)
            {
                if(-1<index&&index<Capacity)
                    EventArray[index] = item;
                else
                    throw new IndexOutOfRangeException();
            }
            public void Invoke(T1 arg)
            {
                for (int i = 0; i < Capacity; i++)
                {
                    EventArray[i]?.Invoke(arg);
                }
            }
            public static EventList<T1> operator+(EventList<T1> e,(int,Action<T1>) item)
            {
                e.Add(item.Item1, item.Item2);
                return e;
            }
        }
    }
}
