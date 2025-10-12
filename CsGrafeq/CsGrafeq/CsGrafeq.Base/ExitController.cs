using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public class ExitController(Action exit) : IDisposable
    {
        public void Dispose()
        {
            exit?.Invoke();
        }
    }
}
