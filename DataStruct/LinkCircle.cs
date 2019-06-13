using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit.DataStruct
{
    /// <summary>
    /// LinkList的修改版，Tail和Head首尾相连。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkCircle<T> : IList<T>, IEnumerator<T>
    {
        private LinkList<T> list;
        // 为了防止首尾相连的link list的foreach出现死循环，加个值来判断。
        private int foreachCount;

        /// <summary>
        /// block count
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }

        /// <summary>
        /// get block at pos i base 0, start from head ro tail.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get { return list[i]; }
        }

        /// <summary>
        /// construct
        /// </summary>
        public LinkCircle()
        {
            list = new LinkList<T>();
            foreachCount = 0;
        }

        /// <summary>
        /// append to the end
        /// </summary>
        /// <param name="t"></param>
        public void PushBack(T t)
        {
            list.PushBack(t);
            list.Tail.Next = list.Head;
        }
        /// <summary>
        /// remove from end
        /// </summary>
        public void PopBack()
        {
            list.PopBack();
            list.Head.Front = list.HiddenRoot;
        }

        /// <summary>
        /// 在位置i，加入对象，如果要加到队列最后，使用PushBack
        /// </summary>
        public void InsertAt(T t, int i)
        {
            list.InsertAt(t, i);
        }

        /// <summary>
        /// remove from pos i base 0, tart from head ro tail.
        /// </summary>
        /// <param name="i"></param>
        public void RemoveAt(int i)
        {
            list.RemoveAt(i);
            if(i == Count - 1)
            {
                list.Head.Front = list.HiddenRoot;
            }
        }

        #region 实现foreach的方法
        /// <summary>
        /// method to support foreach
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this;
        }
        /// <summary>
        /// 放回当前遍历对象
        /// </summary>
        public T Current
        {
            get
            {
                return (T)list.Current;
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            list.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return list.Current; }
        }
        /// <summary>
        /// 移动到下一个对象
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (foreachCount >= Count)
            {
                return false;
            }
            ++foreachCount;
            return list.MoveNext();
        }
        /// <summary>
        /// 重置遍历
        /// </summary>
        public void Reset()
        {
            foreachCount = 0;
            list.Reset();
        }
        #endregion
    }
}
