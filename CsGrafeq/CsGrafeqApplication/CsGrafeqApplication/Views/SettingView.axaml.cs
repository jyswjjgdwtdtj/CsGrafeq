using Avalonia.Controls;

namespace CsGrafeqApplication.Views;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
        DataContext = Setting.Instance;
    }
}