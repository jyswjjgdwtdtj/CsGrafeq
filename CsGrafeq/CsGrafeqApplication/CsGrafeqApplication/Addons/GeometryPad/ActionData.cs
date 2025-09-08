using System;
using CsGrafeq.Collections;

namespace CsGrafeqApplication.Addons.GeometryPad;

internal struct ActionData
{
    public UseShape ShapesToUse;
    public string Name { get; }
    public string Description { get; }

    public ActionData(string name, string description, UseShape shapesToUse)
    {
        Name = name;
        Description = description;
        ShapesToUse = shapesToUse;
    }

    [Flags]
    internal enum UseShape
    {
        None = 0b0000,
        Point = 0b0001,
        Line = 0b0010,
        Circle = 0b0100,
        Polygon = 0b1000
    }
}

internal class HasNameActionList : HasNameList<ActionData>
{
    public HasNameActionList(string name) : base(name)
    {
    }
}