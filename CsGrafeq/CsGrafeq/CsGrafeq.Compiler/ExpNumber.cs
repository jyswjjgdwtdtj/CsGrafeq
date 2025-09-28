using CsGrafeq.Compiler;
using CsGrafeq.Numeric;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsGrafeq;

namespace CsGrafeq
{
    public class ExpNumber:ReactiveObject
    {
        private HasReferenceFunction0<DoubleNumber> Func;
        private DoubleNumber Number;
        public bool IsExpression { get; private set; } = false;
        public event Action NumberChanged;
        private string ShownText = "0";
        public ExpNumber(double initialNumber=0)
        {
            Direct = new HasReferenceFunction0<DoubleNumber>(DirectFunc, EnglishCharEnum.None);
            Func = Direct;
            SetValueNumber(initialNumber);
            EnglishChar.Instance.CharValueChanged += CharValueChanged;
        }
        ~ExpNumber()
        {
            EnglishChar.Instance.CharValueChanged -= CharValueChanged;
        }

        public void SetValueNumber(double number)
        {
            SetNumber(number);
            SetValueStr(number.ToString());
        }
        private void SetNumber(double number)
        {
            IsExpression = false;
            Func.Dispose();
            Number = new DoubleNumber(number);
            Func = Direct;
            Value = Func.Function().Value;
            IsError = false;
        }
        public bool SetExpression(string expression)
        {
            if (double.TryParse(expression, out double result))
            {
                SetNumber(result);
                return true;
            }
            IsExpression = true;
            Func.Dispose();
            Func = None;
            if (Compiler.Compiler.TryCompile0<DoubleNumber>(expression, out var expfunc, out _))
            {
                Func = expfunc;
                IsError = false;
                Value = Func.Function().Value;
                return true;
            }
            Value = double.NaN;
            IsError = true;
            return false;
        }
        public double Value
        {
            get => field; 
            private set
            {
                if (!Extension.CompareDoubleIfBothNaNThenEqual(value,field))
                {
                    this.RaiseAndSetIfChanged(ref field, value);
                    NumberChanged?.Invoke();
                }
            }
        }

        private void SetValueStr(string value)
        {
            ShownText = value;
            this.RaisePropertyChanged(nameof(ValueStr));
        }
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
                Value = Func.Function().Value;
            }
        }
        private DoubleNumber DirectFunc() => Number;
        private readonly HasReferenceFunction0<DoubleNumber> Direct;
        private static DoubleNumber NoneFunc() => new DoubleNumber(double.NaN);
        private readonly HasReferenceFunction0<DoubleNumber> None=new HasReferenceFunction0<DoubleNumber>(NoneFunc,EnglishCharEnum.None);
    }
}
