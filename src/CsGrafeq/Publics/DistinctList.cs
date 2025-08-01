using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publics
{
    public class DistinctList<T>:IEnumerable<T>,ICollection<T>,IList<T>
    {
        private List<T> innerList=new List<T>();
        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
        public void Add(T shape)
        {
            if(!innerList.Contains(shape))
                innerList.Add(shape);
        }
        public void Clear() { innerList.Clear(); }
        public bool Contains(T shape) { return innerList.Contains(shape); }
        public void CopyTo(T[] array, int arrayIndex) { innerList.CopyTo(array, arrayIndex); }
        public bool Remove(T shape) { return innerList.Remove(shape); }
        public int Count { get { return innerList.Count; } }
        public bool IsReadOnly =>false;
        public int IndexOf(T shape) { return innerList.IndexOf(shape); }
        public T this[int index] { 
            get { return innerList[index]; }
            set { innerList[index] = value; }
        }
        public void Insert(int index, T shape) { innerList.Insert(index, shape); }
        public void RemoveAt(int index) { innerList.RemoveAt(index); }
        public T[] ToArray()
        {
            return innerList.ToArray();
        }
    }
}
