namespace CsGrafeq.Shapes;

public abstract class FilledShape : GeometryShape
{
    public FilledShape()
    {
    }
    public bool Filled { get; set; } = false;
}