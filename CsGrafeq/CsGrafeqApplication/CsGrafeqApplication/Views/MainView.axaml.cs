using Avalonia.Controls;
using System;

namespace CsGrafeqApplication.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        this.KeyDown += Global.CallKeyDown;
        this.KeyUp += Global.CallKeyUp;
    }

}