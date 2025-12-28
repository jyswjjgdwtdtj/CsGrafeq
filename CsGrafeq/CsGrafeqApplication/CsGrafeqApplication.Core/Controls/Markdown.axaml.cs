using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using LiveMarkdown.Avalonia;

namespace CsGrafeqApplication.Core.Controls;

public partial class Markdown : Border
{
    public static readonly DirectProperty<Markdown, string?> MarkdownContentProperty =
        AvaloniaProperty.RegisterDirect<Markdown, string?>(
            nameof(MarkdownContent), o => o.MarkdownContent, (o, v) => o.MarkdownContent = v);

    private readonly ObservableStringBuilder MarkdownStringBuilder = new();

    public Markdown()
    {
        InitializeComponent();
        MdRender.MarkdownBuilder = MarkdownStringBuilder;
        PropertyChanged += (_, e) =>
        {
            if (e.Property == MarkdownContentProperty)
            {
                MarkdownStringBuilder.Clear();
                MarkdownStringBuilder.Append(MarkdownContent);
            }
        };
    }

    [Content]
    public string? MarkdownContent
    {
        get => field;
        set => SetAndRaise(MarkdownContentProperty, ref field, value);
    }
}