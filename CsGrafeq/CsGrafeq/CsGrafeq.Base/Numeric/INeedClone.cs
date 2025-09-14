using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Numeric
{
    public interface INeedClone<T> where T : INeedClone<T>
    {
        static abstract T Clone(T source);
    }
}
