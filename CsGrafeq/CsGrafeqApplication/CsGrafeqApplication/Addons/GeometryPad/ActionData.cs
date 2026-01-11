using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using CsGrafeq.I18N;

namespace CsGrafeqApplication.Addons.GeometryPad;

internal class ActionData
{
    /// <summary>
    ///     是否为多点构造 如拟合直线，多边形
    /// </summary>
    public bool IsMultiPoint { get; set; } = false;

    /// <summary>
    ///     构造函数
    /// </summary>
    public required ConstructorInvoker GetterConstructor { get; init; }

    /// <summary>
    ///     构造函数参数类型
    /// </summary>
    public required List<ShapeArg> Args { get; init; }

    /// <summary>
    ///     名称
    /// </summary>
    public required MultiLanguageData Name { get; init; }

    /// <summary>
    ///     简介
    /// </summary>
    public required MultiLanguageData Description { get; init; }

    /// <summary>
    ///     自身类型
    /// </summary>
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
    Circle = 0b10000,
    Polygon = 0b100000,
    Angle = 0b1000000,
    Text = 0b10000000
}

/// <summary>
///     具有名字的可观测列表
/// </summary>
internal class HasNameActionList : ObservableCollection<ActionData>
{
    public HasNameActionList(MultiLanguageData multiLanguageData)
    {
        Name = multiLanguageData;
    }

    public MultiLanguageData Name { get; init; }
}