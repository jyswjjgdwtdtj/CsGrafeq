using System;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace CsGrafeqApplication.Views;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
        DataContext = Setting.Instance;
    }
}