using Avalonia.Controls.Shapes;
using CsGrafeqApp;
using CsGrafeqApp.Classes;
using ExCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeqApp.Shapes.ShapeGetter
{
    public abstract class AngleGetter:Getter
    {
        public abstract AngleData GetAngle();
        public readonly struct AngleData
        {
            public readonly double Angle;
            public readonly Vec AnglePoint;
            public readonly Vec Point1;
            public readonly Vec Point2;
            public AngleData(double angle, Vec ap, Vec p1, Vec p2)
            {
                Angle = angle;
                AnglePoint = ap;
                Point1 = p1;
                Point2 = p2;
            }

        }
    }
    public class AngleGetter_FromThreePoint:AngleGetter
    {
        private Point AnglePoint,Point1,Point2;
        public AngleGetter_FromThreePoint(Point anglePoint,Point point1,Point point2)
        {
            AnglePoint = anglePoint;
            Point1 = point1;
            Point2 = point2;
        }
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            AnglePoint.ShapeChanged += handler;
            AnglePoint.SubShapes.Add(subShape);
            Point1.ShapeChanged += handler;
            Point2.SubShapes.Add(subShape);
            Point2.ShapeChanged += handler;
            Point1.SubShapes.Add(subShape);
        }
        public override AngleData GetAngle()
        {
            double aa = ((Point2.Location- AnglePoint.Location).Arg2() - (Point1.Location - AnglePoint.Location).Arg2()) / Math.PI * 180;
            aa = aa.Mod(360);
            if (aa > 180)
                aa = 360 - aa;
            return new AngleData(aa,AnglePoint.Location,Point1.Location,Point2.Location);
        }
        public override string ActionName => "Angle";
        public override Shape[] Parameters => [AnglePoint,Point1,Point2];
    }
}
