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
        private Function0<DoubleNumber> Func;
        private DoubleNumber Number;
        private EnglishCharEnum Reference = EnglishCharEnum.None;
        public bool IsExpression { get; private set; } = false;
        public event Action NumberChanged;
        private string ShownText = "0";
        public ExpNumber(double initialNumber=0) {
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
            EnglishChar.Instance.RemoveReference(Reference);
            Reference = EnglishCharEnum.None;
            Number = new DoubleNumber(number);
            Func = Direct;
            Value = Func().Value;
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
            EnglishChar.Instance.RemoveReference(Reference);
            Reference = EnglishCharEnum.None;
            Func = None;
            if (Compiler.Compiler.TryCompile0<DoubleNumber>(expression, out var usedVars, out var expfunc, out _))
            {
                Console.WriteLine(usedVars.ToString());
                Reference = usedVars;
                EnglishChar.Instance.AddReference(Reference);
                Func = expfunc;
                IsError = false;
                Value = Func().Value;
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
            if (Reference.HasFlag(c))
            {
                Value = Func().Value;
            }
        }
        private DoubleNumber Direct() => Number;
        private static DoubleNumber None() =>new DoubleNumber(double.NaN);
    }
}
