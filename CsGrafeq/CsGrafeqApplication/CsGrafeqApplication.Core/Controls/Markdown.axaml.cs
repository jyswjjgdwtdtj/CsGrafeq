using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LiveMarkdown.Avalonia;

namespace CsGrafeqApplication.Core.Controls;

public partial class Markdown : UserControl
{
    public Markdown()
    {
        InitializeComponent();
        ContentTemplate = new FuncDataTemplate<object?>(o => true, o =>
        {
            var renderer = new MarkdownRenderer();
            var builder = new ObservableStringBuilder();
            renderer.MarkdownBuilder = builder;
            builder.Append(o as string ?? string.Empty);
            LogicalChildren.Clear();
            LogicalChildren.Add(renderer);
            return renderer;
        });
    }
}