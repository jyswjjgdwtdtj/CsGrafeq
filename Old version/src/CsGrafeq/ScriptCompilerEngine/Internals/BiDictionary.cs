using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.Internals
{
    internal class BiDictionary<T1,T2>
    {
        private readonly Dictionary<T1, T2> _Forward=new Dictionary<T1, T2>();
        private readonly Dictionary<T2, T1> _Backward=new Dictionary<T2, T1>();
        public readonly DictionaryValueGetter<T1,T2> Forward;
        public readonly DictionaryValueGetter<T2,T1> Backward;
        public BiDictionary()
        {
            Forward = new DictionaryValueGetter<T1, T2>(_Forward,_Backward);
            Backward=new DictionaryValueGetter<T2, T1>(_Backward,_Forward);
        }
        public BiDictionary(Dictionary<T1, T2> source):this()
        {
            foreach(var i in source)
            {
                _Forward.Add(i.Key, i.Value);
                _Backward.Add(i.Value,i.Key);
            }
        }
        public void Add(T1 value1, T2 value2)
        {
            _Forward.Add(value1, value2);
            _Backward.Add(value2, value1);
        }
        public bool Contains_ForwardKey(T1 key)
        {
            return _Forward.ContainsKey(key);
        }
        public bool Contains_BackwardKey(T2 key)
        {
            return _Backward.ContainsKey(key);
        }
        public void Remove_ForwardKey(T1 key)
        {
            _Backward.Remove(_Forward[key]);
            _Forward.Remove(key);
        }
        public void Remove_BackwardKey(T2 key)
        {
            _Forward.Remove(_Backward[key]);
            _Backward.Remove(key);
        }
        public int Count
        {
            get { return _Forward.Count; }
        }
        public class DictionaryValueGetter<T3,T4>
        {
            private readonly Dictionary<T3, T4> Forward;
            private readonly Dictionary<T4, T3> Backward;
            internal DictionaryValueGetter(Dictionary<T3, T4> forward, Dictionary<T4, T3> backward)
            {
                Forward=forward;
                Backward=backward;
            }
            public T4 this[T3 key]
            {
                get
                {
                    return Forward[key];
                }
                set
                {
                    Backward.Remove(Forward[key]);
                    Forward[key]=value;
                    Backward.Add(value,key);
                }
            }
        }

    }
}
