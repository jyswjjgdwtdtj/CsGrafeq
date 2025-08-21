using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Numeric
{
    public interface IHasOperatorNumber<T> where T:IHasOperatorNumber<T>
    {
        static T Add(T left, T right) => left + right;
        static T Subtract(T left, T right) => left - right;
        static T Multiply(T left, T right) => left * right;
        static T Divide(T left, T right) => left / right;
        static T Modulo(T left, T right) => left % right;
        static T Neg(T num) => -num;
        static abstract T operator +(T left, T right);
        static abstract T operator -(T left, T right);
        static abstract T operator *(T left, T right);
        static abstract T operator /(T left, T right);
        static abstract T operator %(T left, T right);
        static abstract T operator -(T num);
    }
}
