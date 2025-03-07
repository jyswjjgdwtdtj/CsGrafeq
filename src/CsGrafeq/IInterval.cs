using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    internal interface IInterval
    {
        double Length {  get; }
        bool Contains(double n);
        bool ContainsEqual(double n);
    }
}
