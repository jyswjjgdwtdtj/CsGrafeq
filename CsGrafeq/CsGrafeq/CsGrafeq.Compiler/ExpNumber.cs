using CsGrafeq.Compiler;
using CsGrafeq.MVVM;
using CsGrafeq.Numeric;
using CsGrafeq.Variables;
using CsGrafeq.Setting;
using ReactiveUI;
using CsGrafeq.Numeric.Exact;
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
    private HasReferenceFunction<Func<ExactNumber>> Direct { get;}
    private HasReferenceFunction<Func<ExactNumber>> None { get; } = new(NoneFunc, VariablesEnum.None);
    public readonly object? Owner;
    private HasReferenceFunction<Func<ExactNumber>> Func { get; set; }
    private ExactNumber Number { get; set; }
    private int NumberChangedSuspended { get; set; }
    private string _shownText = "0";

    public ExpNumber(double initialNumber = 0, object? owner = null)
    {
        Owner = owner;
        Direct = new HasReferenceFunction<Func<ExactNumber>>(DirectFunc, VariablesEnum.None);
        Func = Direct;
        VarRecorder.Instance.CharValueChanged += CharValueChanged;
        PropertyChanged += (s, e) => { };
    }

    public bool IsExpression { get; private set; }
    public ExactNumber Value { get; private set; }

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

    public void SetNumber(ExactNumber number)
    {
        IsExpression = false;
        Func.Dispose();
        Number = number;
        Func = Direct;
        SetValue(Number);
        IsError = false;
    }

    private void SetExpression(string expression)
    {
        if (double.TryParse(expression, out var result))
        {
            IsExpression = false;
            Func.Dispose();
            Number = ExactNumber.CreateFloat(result);
            Func = Direct;
            //改这里就会出bug 不敢动了
            SuspendNumberChanged();
            SetValue(Func.Function());
            ResumeNumberChanged();
            UserSetValueStr?.Invoke();
            CallNumberChanged();
            IsError = false;
            return;
        }

        Func.Dispose();
        IsExpression = true;
        Compiler.TryCompile<ExactNumber>(expression, 0, Setting.Setting.Instance.EnableExpressionSimplification)
            .Match(funcTuple =>
            {
                Func = new HasReferenceFunction<Func<ExactNumber>>((Func<ExactNumber>)funcTuple.func,
                    funcTuple.usedVars);
                Func.IsActive=IsActive;
                IsError = false;
                SetValue(Func.Function());
                UserSetValueStr?.Invoke();
            }, ex =>
            {
                Func = None;
                SetValue(ExactNumber.NaN);
                IsError = true;
                Error = ex;
                UserSetValueStr?.Invoke();
            });
    }

    private void SetValue(ExactNumber value)
    {
        if (!value.Equals(Value) || IsExpression)
        {
            Value = value;
            this.RaisePropertyChanged(nameof(Value));
            CallNumberChanged();
        }

        if (!IsExpression)
        {
            _shownText = double.IsNaN(value.ToFloat()) ? "" : value.ToFloat().CustomToString(8, 1e-8);
            this.RaisePropertyChanged(nameof(ValueStr));
        }
    }

    private void CharValueChanged(VariablesEnum c)
    {
        if (Func.References.HasFlag(c)) SetValue(Func.Function());
    }

    private ExactNumber DirectFunc()
    {
        return Number;
    }

    private static ExactNumber NoneFunc()
    {
        return ExactNumber.NaN;
    }
}