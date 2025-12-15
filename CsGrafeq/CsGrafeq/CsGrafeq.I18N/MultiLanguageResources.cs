namespace CsGrafeq.I18N;

public class MultiLanguageResources
{
    public MultiLanguageResources()
    {
        All = new Dictionary<string, MultiLanguageData>(StringComparer.OrdinalIgnoreCase)
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
            ["OnText"] = OnText,
            ["LightText"] = LightText,
            ["DarkText"] = DarkText,
            ["FollowSystemText"] = FollowSystemText
        };
    }

    public static MultiLanguageResources Instance { get; private set; } = new();
    public MultiLanguageData LightText { get; } = new() { English = "Light", Chinese = "亮色" };
    public MultiLanguageData DarkText { get; } = new() { English = "Dark", Chinese = "暗色" };
    public MultiLanguageData FollowSystemText { get; } = new() { English = "Follow System", Chinese = "跟随系统" };
    public MultiLanguageData AngleText { get; } = new() { English = "Angle", Chinese = "角" };
    public MultiLanguageData CantBeMovedText { get; } = new() { English = "Unmovable", Chinese = "无法移动" };
    public MultiLanguageData CircleText { get; } = new() { English = "Circle", Chinese = "圆" };
    public MultiLanguageData CsGrafeqText { get; } = new() { English = "CsGrafeq", Chinese = "CsGrafeq 几何画板" };
    public MultiLanguageData DeleteText { get; } = new() { English = "Delete", Chinese = "删除" };
    public MultiLanguageData DeselectAllText { get; } = new() { English = "Deselect All", Chinese = "取消全选" };
    public MultiLanguageData FunctionText { get; } = new() { English = "Function", Chinese = "函数" };
    public MultiLanguageData HalfLineText { get; } = new() { English = "Half", Chinese = "射线" };
    public MultiLanguageData LanguageText { get; } = new() { English = "Language", Chinese = "语言" };
    public MultiLanguageData PointText { get; } = new() { English = "Point", Chinese = "点" };
    public MultiLanguageData PolygonText { get; } = new() { English = "Polygon", Chinese = "多边形" };
    public MultiLanguageData RedoText { get; } = new() { English = "Redo", Chinese = "取消撤销" };
    public MultiLanguageData SegmentText { get; } = new() { English = "Segment", Chinese = "线段" };
    public MultiLanguageData SelectAllText { get; } = new() { English = "Select All", Chinese = "全选" };
    public MultiLanguageData SettingText { get; } = new() { English = "Setting", Chinese = "设置" };
    public MultiLanguageData StraightText { get; } = new() { English = "Straight", Chinese = "直线" };
    public MultiLanguageData ThemeText { get; } = new() { English = "Theme", Chinese = "主题" };
    public MultiLanguageData UndoText { get; } = new() { English = "Undo", Chinese = "撤销" };
    public MultiLanguageData ZoomInText { get; } = new() { English = "Zoom In", Chinese = "放大" };
    public MultiLanguageData ZoomOutText { get; } = new() { English = "Zoom Out", Chinese = "缩小" };
    public MultiLanguageData ControlledText { get; } = new() { English = "Controlled", Chinese = "控制" };
    public MultiLanguageData MovingOpText { get; } = new() { English = "Moving Optimization", Chinese = "移动优化" };

    public MultiLanguageData ZoomingOpText { get; } = new()
        { English = "Zooming Optimization (Sometimes may let performance worse)", Chinese = "缩放优化 (有时可能会导致性能下降)" };

    // Present only in en-us.resx — zh-hans.resx has no corresponding key. Chinese side left empty.
    public MultiLanguageData OnText { get; } = new() { English = "On", Chinese = "位于" };

    public IReadOnlyDictionary<string, MultiLanguageData> All { get; init; }
}