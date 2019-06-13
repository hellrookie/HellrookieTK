using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit.DataStruct
{
    internal class LinkBlock<T>
    {
        public LinkBlock<T> Front { get; set; }
        public LinkBlock<T> Next { get; set; }
        public T Value { get; private set; }

        public LinkBlock(T t)
        {
            Value = t;
        }

        public void Delete()
        {
            if(Front != null)
            {
                Front.Next = Next;
            }
            if(Next != null)
            {
                Next.Front = Front;
            }
        }
    }
}
