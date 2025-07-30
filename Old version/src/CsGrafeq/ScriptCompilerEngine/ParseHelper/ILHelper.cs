using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic.Logging;
using ScriptCompilerEngine.Internals;
using Microsoft.VisualBasic;

namespace ScriptCompilerEngine.ParseHelper
{
    public class ILHelper
    {
        private ILGenerator IL;
        private StringBuilder Recorder = new StringBuilder();
        private BiDictionary<string,Label> Labels= new BiDictionary<string,Label>();
        private BiDictionary<string,LocalBuilder> Locals= new BiDictionary<string,LocalBuilder>();
        public LocalBuilder ReturnVariable;
        public readonly string Ret = "ret".ToLower();
        public ILHelper(ILGenerator il, string[] ArguementName)
        {
            IL = il;
            ReturnVariable = DeclareLocal(typeof(object), Ret);
            for(int i = 0; i < ArguementName.Length; i++)
            {
                var loc = DeclareLocal(typeof(object), ArguementName[i]);
                il.Emit(OpCodes.Ldarg,i);
                il.Emit(OpCodes.Stloc, loc);
            }
        }
        public void Emit(OpCode op)
        {
            Recorder.AppendLine(op.Name);
            IL.Emit(op);
        }
        public void Emit(OpCode op,Label l)
        {
            Recorder.AppendLine(op.Name+" " + Labels.Backward[l]);
            IL.Emit(op,l);
        }
        public void Emit(OpCode op, byte b)
        {
            Recorder.AppendLine(op.Name + " " + b.ToString()); IL.Emit(op, b);
        }
        public void Emit(OpCode op, ConstructorInfo ci)
        {
            Recorder.AppendLine(op.Name + " " + ci.ToString()); IL.Emit(op, ci);
        }
        public void Emit(OpCode op, double d)
        {
            Recorder.AppendLine(op.Name + " " + d.ToString()); IL.Emit(op, d);
        }
        public void Emit(OpCode op, FieldInfo fi)
        {
            Recorder.AppendLine(op.Name + " " + fi.ToString()); IL.Emit(op, fi);
        }
        public void Emit(OpCode op, float f)
        {
            Recorder.AppendLine(op.Name + " " + f.ToString()); IL.Emit(op, f);
        }
        public void Emit(OpCode op, int i)
        {
            Recorder.AppendLine(op.Name + " " + i.ToString()); IL.Emit(op, i);
        }
        public void LoadVariable(LocalBuilder loc)
        {
            IL.Emit(OpCodes.Ldloc, loc);
            Recorder.AppendLine("Ldloc " + Locals.Backward[loc]);
        }
        public void LoadVariable(string name)
        {
            name = name.ToLower();
            IL.Emit(OpCodes.Ldloc, Locals.Forward[name]);
            Recorder.AppendLine("Ldloc " + name);
        }
        public void SetVariable(string name)
        {
            name = name.ToLower();
            IL.Emit(OpCodes.Stloc, Locals.Forward[name]);
            Recorder.AppendLine("Stloc " + name);
        }
        public void SetVariable(LocalBuilder loc)
        {
            IL.Emit(OpCodes.Stloc, loc);
            Recorder.AppendLine("Stloc " + Locals.Backward[loc]);
        }
        
        public void Emit(OpCode op, long l)
        {
            Recorder.AppendLine(op.Name + " " + l.ToString()); IL.Emit(op, l);
        }
        public void Emit(OpCode op, MethodInfo l)
        {
            Recorder.AppendLine(op.Name + " " + l.ToString().Split('\r')[0]); IL.Emit(op, l);
        }
        public void Emit(OpCode op, string l)
        {
            Recorder.AppendLine(op.Name + " " + l.ToString()); IL.Emit(op, l);
        }
        public void Emit(OpCode op, Type l)
        {
            Recorder.AppendLine(op.Name + " " + l.ToString()); IL.Emit(op, l);
        }
        public Label DefineLabel(string name)
        {
            Label label = IL.DefineLabel();
            Labels.Add("Label_"+Labels.Count,label);
            return label;
        }
        public Label DefineLabel()
        {
            return DefineLabel(Labels.Count.ToString());
        }
        public LocalBuilder DeclareLocal(Type t, string name)
        {
            name=name.ToLower();
            LocalBuilder local = IL.DeclareLocal(t);
            Locals.Add(name, local);
            return local;
        }
        public LocalBuilder DeclareLocal(Type t)
        {
            return DeclareLocal(t, "_"+ Locals.Count);
        }
        public void MarkLabel(string name)
        {
            Recorder.AppendLine("Label:label_" + name);
            IL.MarkLabel(Labels.Forward[name]);
        }
        public void MarkLabel(Label label)
        {
            Recorder.AppendLine("Label:" + Labels.Backward[label] );
            IL.MarkLabel(label);
        }
        public string GetRecord()
        {
            return Recorder.ToString();
        }
        public LocalBuilder GetLocal(string s)
        {
            s=s.ToLower();
            return Locals.Forward[s];
        }
        public bool ContainsVariable(string s)
        {
            if (Locals.Contains_ForwardKey(s))
                return true;
            return false;
        }
    }
}
