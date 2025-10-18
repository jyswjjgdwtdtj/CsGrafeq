using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using CsGrafeq.Collections;

namespace CsGrafeqApplication.Addons.GeometryPad;

internal class ActionData
{
    public bool IsMultiPoint = false;
    public required ConstructorInvoker GetterConstructor { get; init; }
    public required List<ShapeArg> Args { get; init; }
    public required MultiLanguageData Name { get; init; }
    public required MultiLanguageData Description { get; init; }
    public required ShownShapeArg Self { get; init; }
}

[Flags]
internal enum ShapeArg
{
    None = 0b0000,
    Point = 0b0001,
    Line = 0b0010,
    Circle = 0b0100,
    Polygon = 0b1000
}
[Flags]
internal enum ShownShapeArg
{
    None = 0b0000,
    Point = 0b0001,
    Straight = 0b0010,
    Half = 0b0100,
    Segment = 0b1000,
    Circle=0b10000,
    Polygon = 0b100000,
    Angle= 0b1000000
}

internal class HasNameActionList : ObservableCollection<ActionData>
{
    public MultiLanguageData Name { get; init; }
    public HasNameActionList(MultiLanguageData multiLanguageData)
    {
        Name=multiLanguageData;
    }
}