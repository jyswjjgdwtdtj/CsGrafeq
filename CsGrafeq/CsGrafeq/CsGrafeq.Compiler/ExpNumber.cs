using CsGrafeq.Compiler;
using CsGrafeq.MVVM;
using CsGrafeq.Numeric;
using CsGrafeq.Variables;
using CsGrafeq.Setting;
using ReactiveUI;
using static CsGrafeq.Utilities.DoubleCompareHelper;

namespace CsGrafeq.Compiler;

/// <summary>
///     充满了别扭 没办法……
///     面向用户编程就是这样子 要拗出很奇怪恶心的东西
///     不敢动 不敢动。。。
/// </summary>
public class ExpNumber : ObservableObject
{
    public bool IsActive
    {
        get;
        set
        {
            Func.IsActive = value;
            field = value;
        }
    } = true;
    private HasReferenceFunction<Func<DoubleNumber>> Direct { get;}
    private HasReferenceFunction<Func<DoubleNumber>> None { get; } = new(NoneFunc, VariablesEnum.None);
    public readonly object? Owner;
    private HasReferenceFunction<Func<DoubleNumber>> Func { get; set; }
    private DoubleNumber Number { get; set; }
    private int NumberChangedSuspended { get; set; }
    private string _shownText = "0";

    public ExpNumber(double initialNumber = 0, object? owner = null)
    {
        Owner = owner;
        Direct = new HasReferenceFunction<Func<DoubleNumber>>(DirectFunc, VariablesEnum.None);
        Func = Direct;
        VarRecorder.Instance.CharValueChanged += CharValueChanged;
        PropertyChanged += (s, e) => { };
    }

    public bool IsExpression { get; private set; }
    public double Value { get; private set; }

    /// <summary>
    ///     只能由用户触发
    /// </summary>
    public string ValueStr
    {
        get => _shownText;
        set
        {
            SetExpression(value);
            this.RaiseAndSetIfChanged(ref _shownText, value);
        }
    }

    public bool IsError
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public Exception Error
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = new();

    public event Action? NumberChanged;
    public event Action? UserSetValueStr;

    private void CallNumberChanged()
    {
        if (NumberChangedSuspended != 0) return;
        NumberChanged?.Invoke();
    }

    public void SuspendNumberChanged()
    {
        NumberChangedSuspended++;
    }

    public void ResumeNumberChanged(bool call = false)
    {
        NumberChangedSuspended--;
        if (call) NumberChanged?.Invoke();
    }

    ~ExpNumber()
    {
        VarRecorder.Instance.CharValueChanged -= CharValueChanged;
    }

    public void SetNumber(double number)
    {
        IsExpression = false;
        Func.Dispose();
        Number = new DoubleNumber(number);
        Func = Direct;
        SetValue(Func.Function().Value);
        IsError = false;
    }

    private void SetExpression(string expression)
    {
        if (double.TryParse(expression, out var result))
        {
            IsExpression = false;
            Func.Dispose();
            Number = new DoubleNumber(result);
            Func = Direct;
            //改这里就会出bug 不敢动了
            SuspendNumberChanged();
            SetValue(Func.Function().Value);
            ResumeNumberChanged();
            UserSetValueStr?.Invoke();
            CallNumberChanged();
            IsError = false;
            return;
        }

        Func.Dispose();
        IsExpression = true;
        Compiler.TryCompile<DoubleNumber>(expression, 0, Setting.Setting.Instance.EnableExpressionSimplification)
            .Match(funcTuple =>
            {
                Func = new HasReferenceFunction<Func<DoubleNumber>>((Func<DoubleNumber>)funcTuple.func,
                    funcTuple.usedVars);
                Func.IsActive=IsActive;
                IsError = false;
                SetValue(Func.Function().Value);
                UserSetValueStr?.Invoke();
            }, ex =>
            {
                Func = None;
                SetValue(double.NaN);
                IsError = true;
                Error = ex;
                UserSetValueStr?.Invoke();
            });
    }

    private void SetValue(double value)
    {
        if (!CompareDoubleIfBothNaNThenEqual(value, Value) || IsExpression)
        {
            Value = value;
            this.RaisePropertyChanged(nameof(Value));
            CallNumberChanged();
        }

        if (!IsExpression)
        {
            _shownText = double.IsNaN(value) ? "" : value.CustomToString(8, 1e-8);
            this.RaisePropertyChanged(nameof(ValueStr));
        }
    }

    private void CharValueChanged(VariablesEnum c)
    {
        if (Func.References.HasFlag(c)) SetValue(Func.Function().Value);
    }

    private DoubleNumber DirectFunc()
    {
        return Number;
    }

    private static DoubleNumber NoneFunc()
    {
        return new DoubleNumber(double.NaN);
    }
}