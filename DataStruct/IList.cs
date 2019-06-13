using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit.DataStruct
{
    interface IList<T> : IEnumerable<T>
    {
        int Count { get; }
        T this[int i] { get; }
        void PushBack(T t);
        void PopBack();
        void InsertAt(T t, int i);
        void RemoveAt(int i);
    }
}
