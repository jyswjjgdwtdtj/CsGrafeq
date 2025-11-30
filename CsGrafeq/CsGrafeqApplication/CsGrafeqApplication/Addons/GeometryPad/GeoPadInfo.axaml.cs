using Avalonia.Controls;
using Avalonia.Media;

namespace CsGrafeqApplication.Addons.GeometryPad;

public partial class GeoPadInfo : UserControl
{
    public GeoPadInfo()
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
    }
}