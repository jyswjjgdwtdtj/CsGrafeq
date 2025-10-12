using Avalonia.Controls;

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