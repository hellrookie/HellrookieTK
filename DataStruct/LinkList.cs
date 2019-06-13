using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit.DataStruct
{
    /// <summary>
    /// 链表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkList<T> : IList<T>, IEnumerator<T>
    {
        private LinkBlock<T> current;
        /// <summary>
        /// List的数量
        /// </summary>
        public int Count { get; private set; }
        internal LinkBlock<T> HiddenRoot { get; private set; }
        internal LinkBlock<T> Head { get; private set; }
        internal LinkBlock<T> Tail { get; private set; }
        /// <summary>
        /// 下标以0为基数，获取第i个位置的数据
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                return GetBlockAt(i).Value;
            }
        }

        /// <summary>
        /// 创建LinkList实例
        /// </summary>
        public LinkList()
        {
            HiddenRoot = new LinkBlock<T>(default(T));
            current = HiddenRoot;
            Count = 0;
        }

        /// <summary>
        /// 在List尾部加入数据t
        /// </summary>
        /// <param name="t">数据</param>
        public void PushBack(T t)
        {
            if (Head == null)
            {
                Head = Tail = new LinkBlock<T>(t);
                Head.Front = HiddenRoot;
                HiddenRoot.Next = Head;
            }
            else
            {
                var newBlock = new LinkBlock<T>(t);
                newBlock.Front = Tail;
                Tail.Next = newBlock;
                Tail = newBlock;
            }
            ++Count;
        }

        /// <summary>
        /// 删除尾部的数据
        /// </summary>
        public void PopBack()
        {
            if (Tail != null)
            {
                Tail.Delete();
                --Count;
            }
        }

        /// <summary>
        /// 以0为基数，在位置i，加入对象，如果要加到队列最后，使用PushBack
        /// </summary>
        public void InsertAt(T t, int i)
        {
            var block = GetBlockAt(i);
            var newBlock = new LinkBlock<T>(t);
            newBlock.Front = block.Front;
            newBlock.Next = block;
            block.Front = newBlock;
            ++Count;
        }

        /// <summary>
        /// 以0为基数，删除第i个位置的数据
        /// </summary>
        /// <param name="i">位置</param>
        public void RemoveAt(int i)
        {
            GetBlockAt(i).Delete();
            --Count;
        }

        private LinkBlock<T> GetBlockAt(int i)
        {
            if (i >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            LinkBlock<T> result = null;
            for (int j = 0; j <= i; ++j)
            {
                result = HiddenRoot.Next;
            }
            return result;
        }

        #region Enumeratable impelement
        /// <summary>
        /// 放回当前遍历对象
        /// </summary>
        public object Current
        {
            get
            {
                return current.Value;
            }
        }
        /// <summary>
        /// 移动到下一个对象
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (current.Next == null)
            {
                return false;
            }
            current = current.Next;
            return true;
        }

        /// <summary>
        /// 重置遍历
        /// </summary>
        public void Reset()
        {
            current = HiddenRoot;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        T IEnumerator<T>.Current
        {
            get
            {
                return current.Value;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            //
        }
        #endregion
    }
}
