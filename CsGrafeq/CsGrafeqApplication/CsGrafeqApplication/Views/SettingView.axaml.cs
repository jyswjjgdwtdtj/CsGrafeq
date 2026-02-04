using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace CsGrafeqApplication.Views;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
        DataContext = Setting.Instance;
    }

    private void TemplateAppliedHandler(object? s, TemplateAppliedEventArgs e)
    {
        var tb = s as TextBox;
        var borderelement = e.NameScope.Find<Border>("PART_BorderElement");
        borderelement.CornerRadius = new CornerRadius(0);
        borderelement.BorderThickness = new Thickness(0, 0, 0, 2);
        borderelement.Background = Brushes.Transparent;
        borderelement.IsVisible = true;
        tb.LostFocus += (s, e) =>
        {
            borderelement.IsVisible = true;
            borderelement.Background = Brushes.Transparent;
        };
        tb.GotFocus += (s, e) =>
        {
            borderelement.IsVisible = true;
            borderelement.Background = Brushes.Transparent;
        };
    }
}