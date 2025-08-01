using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Reactive;
using Avalonia.ReactiveUI;

using Publics;
using CsGrafeqApp.Attributes.Validation;
using CsGrafeqApp.Shapes.ShapeGetter;
using CsGrafeqApp.Classes;

namespace CsGrafeqApp.Shapes
{
    public class Point:Shape
    {
        private Vec _Location;
        public Vec Location
        {
            get { return _Location; }
        }
        public double LocationX
        {
            get { return Location.X; }
            set {
                if (PointGetter is PointGetter_Movable pm)
                {
                    pm.SetControlPoint(new Vec(value,LocationY));
                    RefreshValues();
                    return;
                }
                throw new Exception();
            }
        }
        public double LocationY
        {
            get { return Location.Y; }
            set
            {
                if (PointGetter is PointGetter_Movable pm)
                {
                    pm.SetControlPoint(new Vec(LocationX, value));
                    RefreshValues();
                    return;
                }
                throw new Exception();
            }
        }
        public PointGetter PointGetter;
        public readonly DistinctList<TextGetter> TextGetters=new DistinctList<TextGetter>();
        public Point(PointGetter pointgetter){
            //if(pointgetter is PointGetter_FromPoint)
                //throw new ArgumentNullException(nameof(pointgetter)+" 不能为指向点");
            PointGetter = pointgetter;
            PointGetter.AddToChangeEvent(RefreshValues,this);
            RefreshValues();
        }
        public override void RefreshValues()
        {
            Vec p = PointGetter.GetPoint();
            this.RaiseAndSetIfChanged(ref _Location.X, p.X, nameof(LocationX));
            this.RaiseAndSetIfChanged(ref _Location.Y, p.Y, nameof(LocationY));
            InvokeEvent();
        }
        public override PointGetter Getter
        {
            get => PointGetter;
        }
        protected override string TypeName
        {
            get => "Point";
        }
        public override string Description => "";
        public override Vec HitTest(Vec vec)
        {
            return (vec - Location);
        }
    }
}
