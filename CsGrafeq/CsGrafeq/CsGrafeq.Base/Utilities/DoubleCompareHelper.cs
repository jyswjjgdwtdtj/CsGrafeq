using System.Runtime.CompilerServices;
using ReactiveUI;

namespace CsGrafeq.Utilities;

public static class DoubleCompareHelper
{
    public static bool CompareDoubleIfBothNaNThenEqual(double a, double b)
    {
        if (a == b) return true;
        if (double.IsNaN(a) && double.IsNaN(b)) return true;
        return false;
    }

    public static double RaiseAndSetIfChangedDouble<TObj>(this TObj reactiveObject, ref double backingField,
        double newValue, [CallerMemberName] string? propertyName = null) where TObj : IReactiveObject
    {
        if (CompareDoubleIfBothNaNThenEqual(backingField, newValue)) return newValue;
        reactiveObject.RaisePropertyChanging(propertyName);
        backingField = newValue;
        reactiveObject.RaisePropertyChanged(propertyName);
        return newValue;
    }
}