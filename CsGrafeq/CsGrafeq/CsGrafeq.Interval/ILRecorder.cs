using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace CsGrafeq.Interval;

public class ILRecorder
{
    private readonly ILGenerator IL;
    private readonly StringBuilder sb = new();

    public ILRecorder(ILGenerator il)
    {
        IL = il;
    }

    public void Emit(OpCode op)
    {
        sb.AppendLine(op.Name);
        IL.Emit(op);
    }

    public void Emit(OpCode op, byte b)
    {
        sb.AppendLine(op.Name + " " + b);
        IL.Emit(op, b);
    }

    public void Emit(OpCode op, ConstructorInfo ci)
    {
        sb.AppendLine(op.Name + " " + ci);
        IL.Emit(op, ci);
    }

    public void Emit(OpCode op, double d)
    {
        sb.AppendLine(op.Name + " " + d);
        IL.Emit(op, d);
    }

    public void Emit(OpCode op, FieldInfo fi)
    {
        sb.AppendLine(op.Name + " " + fi);
        IL.Emit(op, fi);
    }

    public void Emit(OpCode op, float f)
    {
        sb.AppendLine(op.Name + " " + f);
        IL.Emit(op, f);
    }

    public void Emit(OpCode op, int i)
    {
        sb.AppendLine(op.Name + " " + i);
        IL.Emit(op, i);
    }

    public void Emit(OpCode op, long l)
    {
        sb.AppendLine(op.Name + " " + l);
        IL.Emit(op, l);
    }

    public void Emit(OpCode op, MethodInfo l)
    {
        if (l == null)
            throw new Exception(nameof(l) + "不能为null");
        sb.AppendLine(op.Name + " " + l.ToString().Split('\r')[0]);
        IL.Emit(op, l);
    }

    public void Emit(OpCode op, string l)
    {
        sb.AppendLine(op.Name + " " + l);
        IL.Emit(op, l);
    }

    public void Emit(OpCode op, Type l)
    {
        sb.AppendLine(op.Name + " " + l);
        IL.Emit(op, l);
    }

    public void EmitCalli(OpCode opcode, CallingConvention cc, Type? ret, Type[]? para)
    {
        sb.AppendLine(
            $"{opcode.Name} CallingConventions:{cc},ReturnType:{ret},Parameters:[{string.Join(", ", para ?? Array.Empty<Type>())}]");
        IL.EmitCalli(opcode, cc, ret, para);
    }

    public string GetRecord()
    {
        return sb.ToString();
    }
}