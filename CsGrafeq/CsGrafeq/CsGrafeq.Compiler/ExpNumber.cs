using CsGrafeq;
using CsGrafeq.Compiler;
using CsGrafeq.Numeric;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsGrafeq
{
    /// <summary>
    /// 充满了别扭 没办法……
    /// 面向用户编程就是这样子 要拗出很奇怪恶心的东西
    /// </summary>
    public class ExpNumber:ReactiveObject
    {
        private HasReferenceFunction0<DoubleNumber> Func;
        public readonly object Owner;
        public bool IsExpression { get; private set; } = false;
        private DoubleNumber Number;
        public event Action NumberChanged;
        public event Action UserSetValueStr;
        private string ShownText = "0";
        private int NumberChangedSuspended = 0;
        private void CallNumberChanged()
        {
            if (NumberChangedSuspended!=0) return;
            NumberChanged?.Invoke();
        }
        public void SuspendNumberChanged()
        {
            NumberChangedSuspended++;
        }
        public void ResumeNumberChanged(bool call=false)
        {
            NumberChangedSuspended--;
            if (call)
            {
                NumberChanged?.Invoke();
            }
        }
        public ExpNumber(double initialNumber=0,object owner=null)
        {
            Owner = owner;
            Direct = new HasReferenceFunction0<DoubleNumber>(DirectFunc, EnglishCharEnum.None);
            Func = Direct;
            EnglishChar.Instance.CharValueChanged += CharValueChanged;
            PropertyChanged += (s, e) =>
            {
            };
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
            return;
        }
        private void SetExpression(string expression)
        {
            if (double.TryParse(expression, out double result))
            {
                IsExpression = false;
                Func.Dispose();
                Number = new DoubleNumber(result);
                Func = Direct;
                //改这里就会出bug 不敢动了
                SuspendNumberChanged();
                SetValue(Func.Function().Value);
                ResumeNumberChanged(false);
                UserSetValueStr?.Invoke();
                CallNumberChanged();
                IsError = false;
                return;
            }
            Func.Dispose();
            IsExpression = true;
            if (Compiler.Compiler.TryCompile0<DoubleNumber>(expression, out var expfunc, out _))
            {
                Func = expfunc;
                IsError = false;
                SetValue(Func.Function().Value);
                UserSetValueStr?.Invoke();
                return;
            }
            Func= None;
            SetValue(double.NaN);
            IsError = true;
            UserSetValueStr?.Invoke();
        }
        private double _Value=0;
        public double Value
        {
            get => _Value;
        }
        private void SetValue(double value)
        {
            if ((!Extension.CompareDoubleIfBothNaNThenEqual(value, _Value))||IsExpression)
            {
                _Value=value;
                this.RaisePropertyChanged(nameof(Value));
                CallNumberChanged();
            }
            if (!IsExpression)
            {
                ShownText = double.IsNaN(value) ? "" : value.ToString();
                this.RaisePropertyChanged(nameof(ValueStr));
            }
        }
        /// <summary>
        /// 只能由用户触发
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
        public bool IsError { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = false;

        private void CharValueChanged(EnglishCharEnum c) {
            if (Func.Reference.HasFlag(c))
            {
                SetValue(Func.Function().Value);
            }
        }
        private DoubleNumber DirectFunc() => Number;
        private readonly HasReferenceFunction0<DoubleNumber> Direct;
        private static DoubleNumber NoneFunc() => new DoubleNumber(double.NaN);
        private readonly HasReferenceFunction0<DoubleNumber> None=new HasReferenceFunction0<DoubleNumber>(NoneFunc,EnglishCharEnum.None);
    }
}
