using CsGrafeq.Compiler;
using CsGrafeq.MVVM;
using CsGrafeq.Numeric;
using ReactiveUI;
using static CsGrafeq.Utilities.DoubleCompareHelper;

namespace CsGrafeq;

/// <summary>
///     充满了别扭 没办法……
///     面向用户编程就是这样子 要拗出很奇怪恶心的东西
/// </summary>
public class ExpNumber : ObservableObject
{
    private readonly HasReferenceFunction<Func<DoubleNumber>> Direct;

    private readonly HasReferenceFunction<Func<DoubleNumber>> None = new(NoneFunc, EnglishCharEnum.None);
    public readonly object? Owner;
    private HasReferenceFunction<Func<DoubleNumber>> Func;
    private DoubleNumber Number;
    private int NumberChangedSuspended;
    private string ShownText = "0";

    public ExpNumber(double initialNumber = 0, object? owner = null)
    {
        Owner = owner;
        Direct = new HasReferenceFunction<Func<DoubleNumber>>(DirectFunc, EnglishCharEnum.None);
        Func = Direct;
        EnglishChar.Instance.CharValueChanged += CharValueChanged;
        PropertyChanged += (s, e) => { };
    }

    public bool IsExpression { get; private set; }
    public double Value { get; private set; }

    /// <summary>
    ///     只能由用户触发
    /// </summary>
    public string ValueStr
    {
        get => ShownText;
        set
        {
            SetExpression(value);
            this.RaiseAndSetIfChanged(ref ShownText, value);
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
        EnglishChar.Instance.CharValueChanged -= CharValueChanged;
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
        Compiler.Compiler.TryCompile<DoubleNumber>(expression, 0, Setting.Instance.EnableExpressionSimplification)
            .Match(funcTuple =>
            {
                Func = new HasReferenceFunction<Func<DoubleNumber>>((Func<DoubleNumber>)funcTuple.func,
                    funcTuple.usedVars);
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
            ShownText = double.IsNaN(value) ? "" : value.CustomToString(8, 1e-8);
            this.RaisePropertyChanged(nameof(ValueStr));
        }
    }

    private void CharValueChanged(EnglishCharEnum c)
    {
        if (Func.Reference.HasFlag(c)) SetValue(Func.Function().Value);
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