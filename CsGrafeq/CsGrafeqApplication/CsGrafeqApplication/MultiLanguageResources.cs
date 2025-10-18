using System;
using System.Collections;
using System.Collections.Generic;
namespace CsGrafeqApplication;

public class MultiLanguageResources
{
    public static MultiLanguageResources Instance { get; private set; }=new MultiLanguageResources();
    public MultiLanguageData AngleText { get; } = new MultiLanguageData { English = "Angle", Chinese = "角" };
    public MultiLanguageData CantBeMovedText { get; } = new MultiLanguageData { English = "Unmovable", Chinese = "无法移动" };
    public MultiLanguageData CircleText { get; } = new MultiLanguageData { English = "Circle", Chinese = "圆" };
    public MultiLanguageData CsGrafeqText { get; } = new MultiLanguageData { English = "CsGrafeq", Chinese = "CsGrafeq 几何画板" };
    public MultiLanguageData DeleteText { get; } = new MultiLanguageData { English = "Delete", Chinese = "删除" };
    public MultiLanguageData DeselectAllText { get; } = new MultiLanguageData { English = "Deselect All", Chinese = "取消全选" };
    public MultiLanguageData FunctionText { get; } = new MultiLanguageData { English = "Function", Chinese = "函数" };
    public MultiLanguageData HalfLineText { get; } = new MultiLanguageData { English = "Half", Chinese = "射线" };
    public MultiLanguageData LanguageText { get; } = new MultiLanguageData { English = "Language", Chinese = "语言" };
    public MultiLanguageData PointText { get; } = new MultiLanguageData { English = "Point", Chinese = "点" };
    public MultiLanguageData PolygonText { get; } = new MultiLanguageData { English = "Polygon", Chinese = "多边形" };
    public MultiLanguageData RedoText { get; } = new MultiLanguageData { English = "Redo", Chinese = "取消撤销" };
    public MultiLanguageData SegmentText { get; } = new MultiLanguageData { English = "Segment", Chinese = "线段" };
    public MultiLanguageData SelectAllText { get; } = new MultiLanguageData { English = "Select All", Chinese = "全选" };
    public MultiLanguageData SettingText { get; } = new MultiLanguageData { English = "Setting", Chinese = "设置" };
    public MultiLanguageData StraightText { get; } = new MultiLanguageData { English = "Straight", Chinese = "直线" };
    public MultiLanguageData ThemeText { get; } = new MultiLanguageData { English = "Theme", Chinese = "主题" };
    public MultiLanguageData UndoText { get; } = new MultiLanguageData { English = "Undo", Chinese = "撤销" };
    public MultiLanguageData ZoomInText { get; } = new MultiLanguageData { English = "Zoom In", Chinese = "放大" };
    public MultiLanguageData ZoomOutText { get; } = new MultiLanguageData { English = "Zoom Out", Chinese = "缩小" };
    public MultiLanguageData ControlledText { get; } = new MultiLanguageData { English = "Controlled", Chinese = "控制" };
    public MultiLanguageData MovingOpText { get; } = new MultiLanguageData { English = "Moving Optimization", Chinese = "移动优化" };
    public MultiLanguageData ZoomingOpText { get; } = new MultiLanguageData { English = "Zooming Optimization (Sometimes may let performance worse)", Chinese = "缩放优化 (有时可能会导致性能下降)" };
    // Present only in en-us.resx — zh-hans.resx has no corresponding key. Chinese side left empty.
    public MultiLanguageData OnText { get; } = new MultiLanguageData { English = "On", Chinese = "位于" };

    public IReadOnlyDictionary<string, MultiLanguageData> All { get; init; }
    public MultiLanguageResources()
    {
        All=new Dictionary<string, MultiLanguageData>(StringComparer.OrdinalIgnoreCase)
        {
            ["AngleText"] = AngleText,
            ["CantBeMovedText"] = CantBeMovedText,
            ["CircleText"] = CircleText,
            ["CsGrafeqText"] = CsGrafeqText,
            ["DeleteText"] = DeleteText,
            ["DeselectAllText"] = DeselectAllText,
            ["FunctionText"] = FunctionText,
            ["HalfLineText"] = HalfLineText,
            ["LanguageText"] = LanguageText,
            ["PointText"] = PointText,
            ["PolygonText"] = PolygonText,
            ["RedoText"] = RedoText,
            ["SegmentText"] = SegmentText,
            ["SelectAllText"] = SelectAllText,
            ["SettingText"] = SettingText,
            ["StraightText"] = StraightText,
            ["ThemeText"] = ThemeText,
            ["UndoText"] = UndoText,
            ["ZoomInText"] = ZoomInText,
            ["ZoomOutText"] = ZoomOutText,
            ["ControlledText"] = ControlledText,
            ["MovingOpText"] = MovingOpText,
            ["ZoomingOpText"] = ZoomingOpText,
            ["OnText"] = OnText
        };
    }
}