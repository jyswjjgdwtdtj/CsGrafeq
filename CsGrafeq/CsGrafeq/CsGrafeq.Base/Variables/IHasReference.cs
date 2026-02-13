namespace CsGrafeq.Variables;

public interface IHasReference
{
    public VariablesEnum References { get; }
    public bool IsActive { get; }
}