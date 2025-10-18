using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

namespace CsGrafeqApplication
{
    public class MultiLanguageData:ReactiveObject
    {
        public string English { get; init=>this.RaiseAndSetIfChanged(ref field,value); }
        public string Chinese { get; init=>this.RaiseAndSetIfChanged(ref field,value); }

        public string Data { get;private set=>this.RaiseAndSetIfChanged(ref field,value); }

        public MultiLanguageData()
        {
            Languages.LanguageChanged += () => { Data = Languages.CurrentLanguage == "en-us" ? English : Chinese;};
            this.PropertyChanged+=(s,e)=> Data = Languages.CurrentLanguage == "en-us" ? English : Chinese;
            
        }
    }
}
