using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// Pool to contain object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IDisposable
    {
        private static ObjectPool<T> instance;
        private static object syncObj = new object();
        /// <summary>
        /// The method to create T and generate the key.
        /// </summary>
        /// <param name="param">Parameter for create.</param>
        /// <param name="obj">The real object to use.</param>
        /// <param name="key">The key to match the object.This should be unique in ObjectPool.</param>
        /// <returns></returns>
        public delegate bool CreateNewObject(object param, out T obj, out string key);
        /// <summary>
        /// In some case you may need to match the key mistiness.
        /// For example, you may want to match any key start with "AType".
        /// You should implement the match method.
        /// </summary>
        /// <param name="param">A param for use. </param>
        /// <param name="compareObject">The object which is released in free list to match. </param>
        /// <returns></returns>
        public delegate bool MatchBoxKey(object param, T compareObject);
        // All operation for freeList must be done in lock(syncObj), since there is no lock in freeList inside.
        private List<string> freeList;
        // Dictionary<key, object>, stored real objects for use.
        private Dictionary<string, T> datas;
        /// <summary>
        /// Capacity of PoolBox.
        /// </summary>
        public uint Capacity { get; set; }
        /// <summary>
        /// Count of PoolBox.
        /// </summary>
        public uint BoxCount { get { return (uint)datas.Count; } }

        /// <summary>
        /// Constructor of the ObjectPool. Sub class must call this method.
        /// </summary>
        protected ObjectPool()
        {
            freeList = new List<string>();
            datas = new Dictionary<string, T>();
            Capacity = 100;
        }

        /// <summary>
        /// Get the ObjectPool instance(singleton).
        /// </summary>
        /// <returns></returns>
        public static ObjectPool<T> GetPoolInstance()
        {
            if (instance == null)
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new ObjectPool<T>();
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// Get or create object data, if these has free data, take the first one.
        /// </summary>
        /// <param name="createNewObjectMethod">The method to generate new data.</param>
        /// <param name="param">Parameter for create.</param>
        /// <param name="key">Key to realse T, put data to free list.</param>
        /// <returns></returns>
        public T GetObjectData(CreateNewObject createNewObjectMethod, object param, out string key)
        {
            return GetObjectData(createNewObjectMethod, param, out key, null, string.Empty);
        }

        /// <summary>
        /// Get or create object data, search the free list and match by matchMethod. If match return the exist object.
        /// </summary>
        /// <param name="createNewObjectMethod">The method to generate new data.</param>
        /// <param name="paramObj">Parameter for create.</param>
        /// <param name="key">Key to realse T, put data to free list.</param>
        /// <param name="matchMethod">The method to match key. If null get the first object in the free list.</param>
        /// <param name="param">Parameter for matchMethod.</param>
        /// <returns></returns>
        public T GetObjectData(CreateNewObject createNewObjectMethod, object paramObj, out string key, MatchBoxKey matchMethod, object param)
        {
            T result = default(T);
            key = string.Empty;
            lock (syncObj)
            {
                // check the free list first. Use free PoolBox first.
                if (freeList.Count > 0)
                {
                    result = CheckAndGetObjectData(param, matchMethod);
                }
                // then try to new a PoolBox
                if (result == null)
                {
                    T data;
                    if (BoxCount < Capacity)
                    {
                        if (createNewObjectMethod(paramObj, out data, out key))
                        {
                            // matchMethod != null and datas has the match key value.
                            if (!datas.ContainsKey(key))
                            {
                                datas.Add(key, data);
                                result = data;
                            }
                            else
                            {
                                // datas has the match key value. Cannot add to datas wait this object release to free list.
                                WaitToGetObjectFromFreeList(createNewObjectMethod, paramObj, out key, matchMethod, param);
                            }
                        }
                        else
                        {
                            var msg = string.Format("Fail to create object.");
                            throw new Exception(msg);
                        }
                    }
                    else
                    {
                        var msg = string.Format("The buffer is full, the object with key \"{0}\" cannot be added.", key);
                        throw new Exception(msg);
                    }
                }

                if (result == null && matchMethod == null)
                {
                    // wait for one obj free.
                    // No object in free list and datas is full. Just wait some object relase to free list.
                    WaitToGetObjectFromFreeList(createNewObjectMethod, paramObj, out key, matchMethod, param);
                }
            }
            return result;
        }

        private T WaitToGetObjectFromFreeList(CreateNewObject createNewObjectMethod, object paramObj, out string key, MatchBoxKey matchMethod, object param)
        {
            Monitor.Wait(syncObj);
            return GetObjectData(createNewObjectMethod, paramObj, out key, matchMethod, param);
        }

        /// <summary>
        /// Release an object, so this object can be used by others.
        /// </summary>
        /// <param name="key">Key of the object.</param>
        public void RelaseObjectData(string key)
        {
            lock (syncObj)
            {
                if (datas.ContainsKey(key))
                {
                    AddToFreeList(key);
                }
                Monitor.Pulse(syncObj);
            }
        }

        private T CheckAndGetObjectData(object param, MatchBoxKey matchMethod)
        {
            T result = default(T);
            string key;
            bool getData = false;
            try
            {
                if (matchMethod != null)
                {
                    if (TryGetFreeList(out key, matchMethod, param))
                    {
                        getData = true;
                    }
                }
                else
                {
                    if (TryGetFreeList(out key))
                    {
                        getData = true;
                    }
                }


                if (getData && datas.ContainsKey(key))
                {
                    result = datas[key];
                    RemoveFromFreeList(key);
                }
            }
            catch
            {
                // TODO: add exception message.
                throw;
            }
            return result;
        }

        #region freeList operation
        private bool TryGetFreeList(out string value)
        {
            value = string.Empty;
            if (freeList.Count > 0)
            {
                value = freeList[0];
                return true;
            }
            return false;
        }

        private bool TryGetFreeList(out string value, MatchBoxKey matchMethod, object matchParam)
        {
            value = string.Empty;
            if (freeList.Count > 0)
            {
                foreach (var key in freeList)
                {
                    if (matchMethod(matchParam, datas[key]))
                    {
                        value = key;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool RemoveFromFreeList(string key)
        {
            if (freeList.Contains(key))
            {
                return freeList.Remove(key);
            }
            return false;
        }

        private void AddToFreeList(string key)
        {
            freeList.Add(key);
        }
        #endregion

        /// <summary>
        /// Dispose all objects
        /// </summary>
        public void Dispose()
        {
            if (typeof(T) is IDisposable)
            {
                foreach (var data in datas)
                {
                    if (data.Value != null)
                    {
                        (data.Value as IDisposable).Dispose();
                    }
                }
                datas.Clear();
            }
            instance = null;
        }
    }
}
