namespace CsGrafeq;

public struct CgRectangle : IEquatable<CgRectangle>
{
    public Vec Location;
    public Vec Size;

    public CgRectangle(Vec location, Vec size)
    {
        Location = location;
        Size = size;
    }

    public bool Equals(CgRectangle other)
    {
        return Location.Equals(other.Location) && Size.Equals(other.Size);
    }
}