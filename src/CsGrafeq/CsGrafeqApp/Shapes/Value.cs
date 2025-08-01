using CsGrafeqApp.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsGrafeqApp.Shapes;
using CsGrafeqApp.Shapes.ShapeGetter;

namespace CsGrafeqApp.Shapes
{
    public sealed class Value<T>:Shape
    {
        private T _Value;
        private string ValueTypeName = typeof(T).GetType().Name;
        public Value(T value)
        {
            _Value = value;
            CanSelected = false;
            CanPointOver = false;
            Name = _Value.ToString().IfNullRetEmpty();
        }
        
        public override string Description => throw new NotImplementedException();
        public override Getter Getter => throw new NotImplementedException();
        public override Vec HitTest(Vec vec)=> throw new NotImplementedException();
        public override void InvokeEvent() => throw new NotImplementedException();
        protected override string TypeName => ValueTypeName;
        public override void RefreshValues() => throw new NotImplementedException();
    }
}
