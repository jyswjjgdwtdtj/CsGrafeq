using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry.Shapes.Getter
{
    /*internal abstract class TextGetter:Getter
    {
        public abstract string GetText();
    }
    internal class TextGetter_FromString:TextGetter
    {
        private string Text;
        public TextGetter_FromString(string text)
        {
            SetText(text);
        }
        public override string GetText()
        {
            return Text;
        }
        public void SetText(String s)
        {
            Text = s;
        }

        public override void AddToChangeEvent(ShapeChangeHandler handler)
        {
        }
    }
    internal class TextGetter_FromDistance : TextGetter
    {
        public NumberGetter NumberGetter;
        public TextGetter_FromDistance(NumberGetter NumberGetter)
        {
            NumberGetter = NumberGetter;
        }
        public override string GetText() {
            return NumberGetter.GetNumber().ToString();
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler)
        {
            NumberGetter.AddToChangeEvent(handler);
        }
    }*/
}
