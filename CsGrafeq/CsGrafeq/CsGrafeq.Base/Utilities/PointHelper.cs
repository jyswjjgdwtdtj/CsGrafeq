using sysPoint=System.Drawing.Point;
using sysPointF=System.Drawing.PointF;
using avaPoint=Avalonia.Point;
namespace CsGrafeq.Utilities;

public static class PointHelper
{
    extension(sysPoint p)
    {
        public avaPoint ToAvaloniaPoint()
        {
            return new avaPoint(p.X, p.Y);
        }
    }

    extension(sysPointF p)
    {
        public avaPoint ToAvaloniaPoint()
        {
            return new avaPoint(p.X, p.Y);
        }

        public sysPointF Add(sysPointF p2)
        {
            return new sysPointF(p.X + p2.X,p.Y + p2.Y);
        }
        public sysPointF Add(float x, float y)
        {
            return new sysPointF(p.X + x,p.Y + y);
        }
    }

    extension(avaPoint p)
    {
        public sysPoint ToSysPoint()
        {
            return new sysPoint((int)p.X, (int)p.Y);
        }
        public sysPointF ToSysPointF()
        {
            return new sysPointF((int)p.X, (int)p.Y);
        }
    }
}