using CsGrafeq.Compiler;
using CsGrafeq.Interval.Interface;
using CsGrafeq.Numeric;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsGrafeq.Interval
{
    public class HasReferenceIntervalSetFunc<T>(IntervalHandler<T> function,EnglishCharEnum reference) : HasReferenceFunction<IntervalHandler<T>>(function,reference) where T:IInterval<T>
    {
    }
}
