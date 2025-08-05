namespace CsGrafeq.TupperInterval;
public delegate (bool,bool) TupperIntervalHandler<in T>(T x,T y) where T:IInterval;