using CsGrafeq;
using CsGrafeq.Interval;
using CsGrafeq.Interval.Compiler;

Compiler.Compile<IntervalSet>("y=(x+1)%x");