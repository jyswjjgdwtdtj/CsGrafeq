using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public struct Vector2<T>
    {
        public T X,Y;
        public Vector2(T x, T y)
        {
            X = x;
            Y = y;
        }
    }
}
