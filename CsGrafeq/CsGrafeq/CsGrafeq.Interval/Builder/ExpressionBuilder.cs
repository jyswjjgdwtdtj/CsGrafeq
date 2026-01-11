using CsGrafeq.Compiler;

namespace CsGrafeq;

public static class EntityBuilder
{
    static EntityBuilder()
    {
        x = GetVariable('x');
        y = GetVariable('y');
    }

    public static Entity x { get; }

    public static Entity y { get; }

    public static Entity GetVariable(char varname)
    {
        if (varname < 'a' || varname > 'z')
            throw new ArgumentException(nameof(varname));
        var exp = new Entity();
        exp.Elements.Add(new Element(ElementType.Variable, varname.ToString(), 0));
        return exp;
    }

    public static Entity GetNumber(double num)
    {
        var exp = new Entity();
        exp.Elements.Add(new Element(ElementType.Number, num.ToString(), 0));
        return exp;
    }

    public static Entity FromString(string s)
    {
        var exp = new Entity();
        exp.Elements.AddRange(s.GetTokens().GetElements());
        return exp;
    }

    public static ComparedEntity Less(Entity a, Entity b)
    {
        var EntityCompared = new ComparedEntity();
        EntityCompared.Elements.AddRange(a.Elements);
        EntityCompared.Elements.AddRange(b.Elements);
        EntityCompared.Elements.Add(new Element(ElementType.Function, "Less", 2));
        return EntityCompared;
    }

    public static ComparedEntity Greater(Entity a, Entity b)
    {
        var EntityCompared = new ComparedEntity();
        EntityCompared.Elements.AddRange(a.Elements);
        EntityCompared.Elements.AddRange(b.Elements);
        EntityCompared.Elements.Add(new Element(ElementType.Function, "Greater", 2));
        return EntityCompared;
    }

    public static ComparedEntity LessEqual(Entity a, Entity b)
    {
        var EntityCompared = new ComparedEntity();
        EntityCompared.Elements.AddRange(a.Elements);
        EntityCompared.Elements.AddRange(b.Elements);
        EntityCompared.Elements.Add(new Element(ElementType.Function, "LessEqual", 2));
        return EntityCompared;
    }

    public static ComparedEntity GreaterEqual(Entity a, Entity b)
    {
        var EntityCompared = new ComparedEntity();
        EntityCompared.Elements.AddRange(a.Elements);
        EntityCompared.Elements.AddRange(b.Elements);
        EntityCompared.Elements.Add(new Element(ElementType.Function, "GreaterEqual", 2));
        return EntityCompared;
    }

    public static ComparedEntity Equal(Entity a, Entity b)
    {
        var EntityCompared = new ComparedEntity();
        EntityCompared.Elements.AddRange(a.Elements);
        EntityCompared.Elements.AddRange(b.Elements);
        EntityCompared.Elements.Add(new Element(ElementType.Function, "Equal", 2));
        return EntityCompared;
    }

    private static Entity Call(Entity exp1, string name)
    {
        var newexp = new Entity();
        newexp.Elements.AddRange(exp1.Elements);
        newexp.Elements.Add(new Element(ElementType.Function, name, 1));
        return newexp;
    }

    private static Entity Call(Entity exp1, Entity exp2, string name)
    {
        var newexp = new Entity();
        newexp.Elements.AddRange(exp1.Elements);
        newexp.Elements.AddRange(exp2.Elements);
        newexp.Elements.Add(new Element(ElementType.Function, name, 2));
        return newexp;
    }

    public static Entity Add(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Add");
    }

    public static Entity Subtract(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Subtract");
    }

    public static Entity Multiply(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Multiply");
    }

    public static Entity Divide(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Divide");
    }

    public static Entity Mod(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Mod");
    }

    public static Entity Pow(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Pow");
    }

    public static Entity Sgn(Entity exp1)
    {
        return Call(exp1, "Sgn");
    }

    public static Entity Median(Entity exp1, Entity exp2, Entity exp3)
    {
        var newexp = new Entity();
        newexp.Elements.AddRange(exp1.Elements);
        newexp.Elements.AddRange(exp2.Elements);
        newexp.Elements.AddRange(exp3.Elements);
        newexp.Elements.Add(new Element(ElementType.Function, "Median", 3));
        return newexp;
    }

    public static Entity Exp(Entity exp1)
    {
        return Call(exp1, "Exp");
    }

    public static Entity Ln(Entity exp1)
    {
        return Call(exp1, "Ln");
    }

    public static Entity Lg(Entity exp1)
    {
        return Call(exp1, "Lg");
    }

    public static Entity Log(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Log");
    }

    public static Entity Sqrt(Entity exp1)
    {
        return Call(exp1, "Sqrt");
    }

    public static Entity Root(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Root");
    }

    public static Entity Min(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Min");
    }

    public static Entity Max(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Max");
    }

    public static Entity Sin(Entity exp1)
    {
        return Call(exp1, "Sin");
    }

    public static Entity Cos(Entity exp1)
    {
        return Call(exp1, "Cos");
    }

    public static Entity Tan(Entity exp1)
    {
        return Call(exp1, "Tan");
    }

    public static Entity Arcsin(Entity exp1)
    {
        return Call(exp1, "Arcsin");
    }

    public static Entity Arccos(Entity exp1)
    {
        return Call(exp1, "Arccos");
    }

    public static Entity Arctan(Entity exp1)
    {
        return Call(exp1, "Arctan");
    }

    public static Entity Floor(Entity exp1)
    {
        return Call(exp1, "Floor");
    }

    public static Entity Ceil(Entity exp1)
    {
        return Call(exp1, "Ceil");
    }

    public static Entity GCD(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "GCD");
    }

    public static Entity LCM(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "LCM");
    }

    public static Entity Factorial(Entity exp1)
    {
        return Call(exp1, "Factorial");
    }

    public static ComparedEntity Union(ComparedEntity exp1, ComparedEntity exp2)
    {
        var EntityCompared = new ComparedEntity();
        EntityCompared.Elements.AddRange(exp1.Elements);
        EntityCompared.Elements.AddRange(exp2.Elements);
        EntityCompared.Elements.Add(new Element(ElementType.Function, "Union", 2));
        return EntityCompared;
    }

    public static ComparedEntity Intersect(ComparedEntity exp1, ComparedEntity exp2)
    {
        var EntityCompared = new ComparedEntity();
        EntityCompared.Elements.AddRange(exp1.Elements);
        EntityCompared.Elements.AddRange(exp2.Elements);
        EntityCompared.Elements.Add(new Element(ElementType.Function, "Intersect", 2));
        return EntityCompared;
    }
}

public class EntityBase
{
    internal List<Element> Elements = new();

    internal EntityBase()
    {
    }
}

public class Entity : EntityBase
{
    internal Entity()
    {
    }

    public static Entity operator +(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Add");
    }

    public static Entity operator -(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Subtract");
    }

    public static Entity operator -(Entity exp)
    {
        return Call(exp, "Neg");
    }

    public static Entity operator *(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Multiply");
    }

    public static Entity operator /(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Divide");
    }

    public static Entity operator ^(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Pow");
    }

    public static Entity operator %(Entity exp1, Entity exp2)
    {
        return Call(exp1, exp2, "Mod");
    }

    private static Entity Call(Entity exp1, string name)
    {
        var newexp = new Entity();
        newexp.Elements.AddRange(exp1.Elements);
        newexp.Elements.Add(new Element(ElementType.Operator, name, 1));
        return newexp;
    }

    private static Entity Call(Entity exp1, Entity exp2, string name)
    {
        var newexp = new Entity();
        newexp.Elements.AddRange(exp1.Elements);
        newexp.Elements.AddRange(exp2.Elements);
        newexp.Elements.Add(new Element(ElementType.Operator, name, 2));
        return newexp;
    }

    public static ComparedEntity operator ==(Entity exp1, Entity exp2)
    {
        return EntityBuilder.Equal(exp1, exp2);
    }

    public static ComparedEntity operator !=(Entity exp1, Entity exp2)
    {
        throw new NotImplementedException();
    }

    public static ComparedEntity operator <(Entity exp1, Entity exp2)
    {
        return EntityBuilder.Less(exp1, exp2);
    }

    public static ComparedEntity operator >(Entity exp1, Entity exp2)
    {
        return EntityBuilder.Greater(exp1, exp2);
    }

    public static ComparedEntity operator <=(Entity exp1, Entity exp2)
    {
        return EntityBuilder.LessEqual(exp1, exp2);
    }

    public static ComparedEntity operator >=(Entity exp1, Entity exp2)
    {
        return EntityBuilder.GreaterEqual(exp1, exp2);
    }

    public static implicit operator Entity(double num)
    {
        return EntityBuilder.GetNumber(num);
    }

    public static implicit operator Entity(char var)
    {
        return EntityBuilder.GetVariable(var);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class ComparedEntity : EntityBase
{
    internal ComparedEntity()
    {
    }

    public static ComparedEntity operator &(ComparedEntity exp1, ComparedEntity exp2)
    {
        return EntityBuilder.Intersect(exp1, exp2);
    }

    public static ComparedEntity operator |(ComparedEntity exp1, ComparedEntity exp2)
    {
        return EntityBuilder.Union(exp1, exp2);
    }
}