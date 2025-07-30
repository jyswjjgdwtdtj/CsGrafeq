using CsGrafeq.Base;
using ScriptCompilerEngine.CompileEngine;
using ScriptCompilerEngine.ScriptNative.InternalMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry.Shapes.Getter
{
    internal abstract class TextGetter:Getter
    {
        public abstract string GetText();
    }
    internal class TextGetter_FromScript : TextGetter
    {
        private NoArgFunc ScriptFunc=()=> { return "";};
        private string _Script = "'返回所要显示的字符串";
        public TextGetter_FromScript(string script)
        {
            SetScript(script);
        }
        public TextGetter_FromScript()
        {
            Adjust();
        }
        public void SetScript(string script)
        {
            if (script == _Script)
                return;
            _Script = script;
            ScriptFunc = ScriptCompilerEngine.CompileEngine.CompileEngine.CompileNoArgFunc(script);
            Console.WriteLine(CompileEngine.LastILRecord);
        }
        public string GetScript()
        {
            return _Script;
        }
        public override string GetText()
        {
            try
            {
                return Method.ObjectToString(ScriptFunc.Invoke());
            }
            catch
            {
                return "";
            }
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
        }
        public override bool Adjust()
        {
            ScriptDialog dlg = new ScriptDialog(_Script);
            dlg.ShowDialog();
            if (dlg.OK)
            {
                SetScript(dlg.Script);
                return true;
            }
            return false;
        }
    }
}
