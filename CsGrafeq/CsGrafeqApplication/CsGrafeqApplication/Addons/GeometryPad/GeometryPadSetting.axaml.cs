using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CsGrafeqApplication.ViewModels;

namespace CsGrafeqApplication.Addons.GeometryPad;

public partial class GeometryPadSetting : UserControl
{
    private GeometryPad Pad;
    public GeometryPadSetting(GeometryPad pad)
    {
        DataContext = this;
        InitializeComponent();
        Pad = pad;
    }
}