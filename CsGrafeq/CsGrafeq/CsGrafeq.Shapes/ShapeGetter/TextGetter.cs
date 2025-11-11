namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class TextGetter : Getter
{
    public abstract string GetText();
}

public class TextGetter_Static : TextGetter
{
    public TextGetter_Static(string text)
    {
        Text = text;
    }
    protected string Text { get; init; }
    public override string GetText()
    {
        return Text;
    }
}