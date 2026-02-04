using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using CsGrafeqApplication.Dialogs.InfoDialog;

namespace CsGrafeqApplication.Addons.GeometricPad;

public partial class ActionsView : UserControl
{
    public static readonly DirectProperty<ActionsView, GeometricPad> OwnerProperty =
        AvaloniaProperty.RegisterDirect<ActionsView, GeometricPad>(
            nameof(Owner), o => o.Owner, (o, v) => o.Owner = v);

    public ActionsView()
    {
        InitializeComponent();
    }

    public GeometricPad Owner
    {
        get => field;
        set => SetAndRaise(OwnerProperty, ref field, value);
    }

    private void Expander_TemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is Expander expander)
            expander.TemplateApplied += (_, te) =>
            {
                var togglebtn = te.NameScope.Find<ToggleButton>("PART_ToggleButton");
                togglebtn.TemplateApplied += (_, tte) =>
                {
                    var path = tte.NameScope.Find<Path>("PART_ExpandIcon");
                    path.Bind(Path.FillProperty, Resources.GetResourceObservable("CgForegroundBrush"));
                };
            };
    }

    /// <summary>
    ///     几何操作RadioButton被选择
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void RadioButtonChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
            if (rb.IsChecked == true && rb.Tag is ActionData ad)
            {
                Owner.SetAction(ad);
                this.Info(new TextBlock { Text = Owner.CurrentAction.Description.Data }, InfoType.Information);
            }
    }
}