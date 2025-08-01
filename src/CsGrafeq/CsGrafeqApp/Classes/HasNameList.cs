using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
namespace CsGrafeqApp.Classes
{
    internal class HasNameList<T>:AvaloniaList<T>
    {
        public string Name { get; init; }
        public HasNameList(string Name)
        {
            this.Name = Name;
        }
    }

    internal class HasNameStrList : HasNameList<string>
    {
        public HasNameStrList(string Name) : base(Name){}
    }
}
