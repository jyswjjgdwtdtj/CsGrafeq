using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using CsGrafeqApp.Classes;
using CsGrafeqApp.Shapes;
using System;
using System.Collections.Generic;

namespace CsGrafeqApp.Addons.GeometryPad;

public partial class OpControl : UserControl
{
    private Action<string> SetAction { get; init; }
    public ShapeList Shapes { get; init; }
    internal static HasNameStrList EditAction { get; } = new("Edit") { "Move", "Select" };
    internal static HasNameStrList LineAction { get; } = new("Line") { "Straight", "Half", "Segment", "Vertical", "Parallel", "Perpendicular Bisector", "Fitted" };
    internal static HasNameStrList PointAction { get; } = new("Point") { "Put", "Middle", "Median Center", "Out Center", "In Center", "Ortho Center", "Axial Symmetry", "Nearest" };
    internal static HasNameStrList PolygonAction { get; }= new ("Polygon") {"Polygon"};
    internal static HasNameStrList CircleAction { get; }= new ("Circle") {"Three Points","Center and Point"};

    internal readonly static DirectProperty<OpControl, AvaloniaList<HasNameStrList>> NameStrListProperty =
        AvaloniaProperty.RegisterDirect<OpControl,AvaloniaList<HasNameStrList> >(nameof(Operations), o => Operations);
    internal static AvaloniaList<HasNameStrList> Operations { get;}=new() { EditAction ,LineAction, PointAction, PolygonAction, CircleAction};
    public OpControl(ShapeList shapes,Action<string> setAction)
    {
        Shapes = shapes;
        InitializeComponent();
        DataContext = this;
        SetAction = setAction;
    }
    public void TextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox box)
        {
            if (!double.TryParse(box.Text, out _))
                box.Text = "0";
        }
    }
    public void TextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox box)
        {
            var parent = box.Parent;
            if (parent != null)
            {
                if (e.Key == Key.Left)
                {
                    List<TextBox> ls = new List<TextBox>();
                    foreach (var i in parent.GetLogicalChildren())
                        if (i is TextBox tb)
                            ls.Add(tb);
                    int index = ls.IndexOf(box);
                    if (index > 0)
                        ls[index - 1].Focus();
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    List<TextBox> ls = new List<TextBox>();
                    foreach (var i in parent.GetLogicalChildren())
                        if (i is TextBox tb)
                            ls.Add(tb);
                    int index = ls.IndexOf(box);
                    if (index < ls.Count - 1)
                        ls[index + 1].Focus();
                    e.Handled = true;
                }
            }
        }

    }

    public void RadioButtonChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
        {
            if(rb.IsChecked == true)
                SetAction(rb.Name);
        }
    }

    public void DeleteBtnClicked(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
    }
}