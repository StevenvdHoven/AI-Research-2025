using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Reflection;
using System.Collections;
using ChiefoUtilities;

namespace ChiefoUtilities.Types
{
    public delegate void OnChiefoAsyncTask(object _output);
    /// <summary>
    /// A list that sorts itself in the background.
    /// Make sure your Type contains the IComparable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChiefoList<T> : IList<T>, ICollection<T>
    {
        public T this[int index] { get => TList[index]; set => TList[index] = value; }

        public int Count => TList.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        private List<T> TList;

        public ChiefoList()
        {
            Type _type = typeof(T);
            Type[] _interfaces = _type.GetInterfaces();
            foreach (Type _interface in _interfaces)
            {
                if (_interface == typeof(IComparable))
                {
                    TList = new List<T>();
                    return;
                }
            }
            throw new Exception($"{typeof(T)} doesn't contain the interface IComparable!!");

        }

        #region ChiefoList Methods

        public void ScaledSort()
        {
            ChiefSorter.SortMethod _method = ChiefSorter.SortMethod.BubbleSort;
            if (TList.Count >= 10)
                _method = ChiefSorter.SortMethod.QuickSort;

            TList = ChiefSorter.Sort(TList.ToArray(), _method).ToList();

        }
        #endregion

        #region List Methods

        public void Add(T item)
        {
            TList.Add(item);
            ScaledSort();
        }

        public void Clear()
        {
            TList.Clear();
        }

        public bool Contains(T item)
        {
            return TList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            TList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return TList.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return TList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            TList.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return TList.Remove(item);
        }

        public void RemoveAt(int index)
        {
            TList.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TList.GetEnumerator();
        }
        #endregion
    }
}
