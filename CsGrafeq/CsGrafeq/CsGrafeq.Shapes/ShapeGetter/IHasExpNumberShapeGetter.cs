namespace CsGrafeq.Shapes.ShapeGetter;

public interface IHasExpNumberShapeGetter
{
    IReadOnlyList<ExpNumberData> ExpNumbers { get; }
}

public struct ExpNumberData
{
    public required string Description;
    public required ExpNumber Number;
}