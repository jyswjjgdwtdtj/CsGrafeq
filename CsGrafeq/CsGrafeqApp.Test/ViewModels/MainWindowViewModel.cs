using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApp.Test.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public Shape Shape { get; set; }=new Point(new PointGetter_FromLocation((0,0)));
}