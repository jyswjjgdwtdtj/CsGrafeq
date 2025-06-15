using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public interface IInterval
    {
        double GetLength();
        bool Contains(double n);
        bool ContainsEqual(double n);
        IInterval SetDef((bool, bool) def);
        IInterval SetCont(bool cont);
    }
}
