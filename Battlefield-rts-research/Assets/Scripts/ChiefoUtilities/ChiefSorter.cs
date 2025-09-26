using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;

namespace ChiefoUtilities
{
    public enum ValueType
    {
        Int,
        Float,
        Double,
    }

    public static class ChiefSorter
    {
        #region BubbleSort
        public static T[] SortArrayBasedOfField<T>(T[] _array, ValueType valueType, string _fieldName)
        {
            switch (valueType)
            {
                case ValueType.Int:
                    return SortArrayBasedOfFieldInt<T>(_array, _fieldName);

                case ValueType.Float:
                    return SortArrayBasedOfFieldFloat<T>(_array, _fieldName);

                case ValueType.Double:
                    return SortArrayBasedOfFieldDouble<T>(_array, _fieldName);
            }
            return null;
        }

        private static T[] SortArrayBasedOfFieldInt<T>(T[] _array, string _fieldName)
        {
            for (int i = 0; i < _array.Length; i++)
            {
                for (int j = 0; j < _array.Length; j++)
                {
                    int _value1 = GetFieldValue<int, T>(_array[i], _fieldName);
                    int _value2 = GetFieldValue<int, T>(_array[j], _fieldName);
                    if (_value1 < _value2)
                    {
                        T _temp = _array[i];
                        _array[i] = _array[j];
                        _array[j] = _temp;
                    }
                }
            }
            return _array;
        }

        private static T[] SortArrayBasedOfFieldFloat<T>(T[] _array, string _fieldName)
        {
            for (int i = 0; i < _array.Length; i++)
            {
                for (int j = 0; j < _array.Length; j++)
                {
                    float _value1 = GetFieldValue<float, T>(_array[i], _fieldName);
                    float _value2 = GetFieldValue<float, T>(_array[j], _fieldName);
                    if (_value1 < _value2)
                    {
                        T _temp = _array[i];
                        _array[i] = _array[j];
                        _array[j] = _temp;
                    }
                }
            }
            return _array;
        }

        private static T[] SortArrayBasedOfFieldDouble<T>(T[] _array, string _fieldName)
        {
            for (int i = 0; i < _array.Length; i++)
            {
                for (int j = 0; j < _array.Length; j++)
                {
                    double _value1 = GetFieldValue<double, T>(_array[i], _fieldName);
                    double _value2 = GetFieldValue<double, T>(_array[j], _fieldName);
                    if (_value1 < _value2)
                    {
                        T _temp = _array[i];
                        _array[i] = _array[j];
                        _array[j] = _temp;
                    }
                }
            }
            return _array;
        }

        public static Vector3[] SortOnClosest(Vector3[] _points, Vector3 _target)
        {
            Vector3 _temp = Vector3.zero;
            for (int i = 0; i < _points.Length; i++)
            {
                for (int j = 0; j < _points.Length; j++)
                {
                    float _Prdis = Vector3.Distance(_points[i], _target);
                    float _NeDis = Vector3.Distance(_points[j], _target);
                    if (_Prdis < _NeDis)
                    {
                        _temp = _points[j];
                        _points[j] = _points[i];
                        _points[i] = _temp;
                    }
                }
            }
            return _points;
        }


        public static int[] SortArray(int[] _array)
        {
            for (int i = 0; i < _array.Length; i++)
            {
                for (int j = 0; j < _array.Length; j++)
                {
                    if (_array[i] < _array[j])
                    {
                        int _temp = _array[i];
                        _array[i] = _array[j];
                        _array[j] = _temp;
                    }
                }
            }
            return _array;
        }

        public static float[] SortArray(float[] _array)
        {
            for (int i = 0; i < _array.Length; i++)
            {
                for (int j = 0; j < _array.Length; j++)
                {
                    if (_array[i] < _array[j])
                    {
                        float _temp = _array[i];
                        _array[i] = _array[j];
                        _array[j] = _temp;
                    }
                }
            }
            return _array;
        }
        public static double[] SortArray(double[] _array)
        {
            for (int i = 0; i < _array.Length; i++)
            {
                for (int j = 0; j < _array.Length; j++)
                {
                    if (_array[i] < _array[j])
                    {
                        double _temp = _array[i];
                        _array[i] = _array[j];
                        _array[j] = _temp;
                    }
                }
            }
            return _array;
        }
        #endregion BubbleSort

        #region QuickSort
        public static void QuickSortArrayBasedOfField<T>(ref T[] _array, ValueType valueType, string _fieldName)
        {
            switch (valueType)
            {
                case ValueType.Int:
                    QuickSortArrayBasedOfFieldInt<T>(ref _array,0,_array.Length -1, _fieldName);
                    break;

                case ValueType.Float:
                    QuickSortArrayBasedOfFieldFloat<T>(ref _array, 0, _array.Length-1, _fieldName);
                    break;

                case ValueType.Double:
                    QuickSortArrayBasedOfFieldDouble<T>(ref _array, 0, _array.Length-1 , _fieldName);
                    break;
            }
        }
        private static void QuickSortArrayBasedOfFieldInt<T>(ref T[] _array,int _startIndex,int _endIndex, string _fieldName)
        {
            if (_startIndex < _endIndex)
            {
                int _pi = PartitionInt(ref _array, _startIndex, _endIndex,_fieldName);

                QuickSortArrayBasedOfFieldInt(ref _array, _startIndex, _pi - 1,_fieldName);
                QuickSortArrayBasedOfFieldInt(ref _array, _pi + 1, _endIndex,_fieldName);
            }
        }
        private static void QuickSortArrayBasedOfFieldFloat<T>(ref T[] _array, int _startIndex, int _endIndex, string _fieldName)
        {
            if (_startIndex < _endIndex)
            {
                int _pi = PartitionFloat(ref _array, _startIndex, _endIndex, _fieldName);

                QuickSortArrayBasedOfFieldFloat(ref _array, _startIndex, _pi - 1, _fieldName);
                QuickSortArrayBasedOfFieldFloat(ref _array, _pi + 1, _endIndex, _fieldName);
            }
        }
        private static void QuickSortArrayBasedOfFieldDouble<T>(ref T[] _array, int _startIndex, int _endIndex, string _fieldName)
        {
            if (_startIndex < _endIndex)
            {
                int _pi = PartitionDouble(ref _array, _startIndex, _endIndex, _fieldName);

                QuickSortArrayBasedOfFieldFloat(ref _array, _startIndex, _pi - 1, _fieldName);
                QuickSortArrayBasedOfFieldFloat(ref _array, _pi + 1, _endIndex, _fieldName);
            }
        }

        public static void QuickSortArray(ref int[] _array, int _startIndex, int _endIndex)
        {
            if (_startIndex < _endIndex)
            {
                int _pi = Partition(ref _array, _startIndex, _endIndex);

                QuickSortArray(ref _array, _startIndex, _pi - 1);
                QuickSortArray(ref _array, _pi + 1, _endIndex);
            }
        }
        public static void QuickSortArray(ref double[] _array, int _startIndex, int _endIndex)
        {
            if (_startIndex < _endIndex)
            {
                int _pi = Partition(ref _array, _startIndex, _endIndex);

                QuickSortArray(ref _array, _startIndex, _pi - 1);
                QuickSortArray(ref _array, _pi + 1, _endIndex);
            }
        }
        public static void QuickSortArray(ref float[] _array, int _startIndex, int _endIndex)
        {
            if (_startIndex < _endIndex)
            {
                int _pi = Partition(ref _array, _startIndex, _endIndex);

                QuickSortArray(ref _array, _startIndex, _pi - 1);
                QuickSortArray(ref _array, _pi + 1, _endIndex);
            }
        }

        private static int PartitionInt<T>(ref T[] _array, int _startIndex, int _endIndex,string _fieldname)
        {

            int _pivot = _array[_endIndex].GetFieldValue<int,T>(_fieldname);

            int i = (_startIndex - 1);

            for (int j = _startIndex; j < _endIndex; j++)
            {
                if (_array[j].GetFieldValue<int,T>(_fieldname) < _pivot)
                {
                    i++;
                    SwapArray(ref _array, i, j);
                }
            }
            SwapArray(ref _array, i + 1, _endIndex);
            return (i + 1);

        }
        private static int PartitionFloat<T>(ref T[] _array, int _startIndex, int _endIndex, string _fieldname)
        {

            float _pivot = _array[_endIndex].GetFieldValue<int, T>(_fieldname);

            int i = (_startIndex - 1);

            for (int j = _startIndex; j < _endIndex; j++)
            {
                if (_array[j].GetFieldValue<float, T>(_fieldname) < _pivot)
                {
                    i++;
                    SwapArray(ref _array, i, j);
                }
            }
            SwapArray(ref _array, i + 1, _endIndex);
            return (i + 1);

        }
        private static int PartitionDouble<T>(ref T[] _array, int _startIndex, int _endIndex, string _fieldname)
        {

            double _pivot = _array[_endIndex].GetFieldValue<int, T>(_fieldname);

            int i = (_startIndex - 1);

            for (int j = _startIndex; j < _endIndex; j++)
            {
                if (_array[j].GetFieldValue<double, T>(_fieldname) < _pivot)
                {
                    i++;
                    SwapArray(ref _array, i, j);
                }
            }
            SwapArray(ref _array, i + 1, _endIndex);
            return (i + 1);

        }

        private static int Partition(ref float[] _array, int _startIndex, int _endIndex)
        {

            float _pivot = _array[_endIndex];

            int i = (_startIndex - 1);

            for (int j = _startIndex; j < _endIndex; j++)
            {
                if (_array[j] < _pivot)
                {
                    i++;
                    SwapArray(ref _array, i, j);
                }
            }
            SwapArray(ref _array, i + 1, _endIndex);
            return (i + 1);

        }
        private static int Partition(ref double[] _array, int _startIndex, int _endIndex)
        {

            double _pivot = _array[_endIndex];

            int i = (_startIndex - 1);

            for (int j = _startIndex; j < _endIndex; j++)
            {
                if (_array[j] < _pivot)
                {
                    i++;
                    SwapArray(ref _array, i, j);
                }
            }
            SwapArray(ref _array, i + 1, _endIndex);
            return (i + 1);

        }
        private static int Partition(ref int[] _array, int _startIndex, int _endIndex)
        {

            int _pivot = _array[_endIndex];

            int i = (_startIndex - 1);

            for (int j = _startIndex; j < _endIndex; j++)
            {
                if (_array[j] < _pivot)
                {
                    i++;
                    SwapArray(ref _array, i, j);
                }
            }
            SwapArray(ref _array, i + 1, _endIndex);
            return (i + 1);

        }

        public static void SwapArray<T>(ref T[] _array, int i, int j)
        {
            T _temp = _array[i];
            _array[i] = _array[j];
            _array[j] = _temp;
        }
        #endregion QuickSort

        #region IComparable Sorts
        public enum SortMethod
        {
            BubbleSort,
            QuickSort,
        }

        public static T[] Sort<T>(T[] _array, SortMethod _method)
        {
            switch (_method)
            {
                case SortMethod.BubbleSort:
                    return BubbleSort(_array);
                case SortMethod.QuickSort:
                    return QuickSort(ref _array, 0, _array.Length - 1);
            }
            return null;
        }

        private static T[] BubbleSort<T>(T[] _array)
        {
            T _temp = default;
            for (int i = 0; i < _array.Length; i++)
            {
                for (int j = 0; j < _array.Length; j++)
                {
                    IComparable _a = (IComparable)_array[i];
                    if (1 == _a.CompareTo(_array[j]))
                    {
                        _temp = _array[i];
                        _array[i] = _array[j];
                        _array[j] = _temp;
                    }
                }
            }
            return _array;
        }

        public static T[] QuickSort<T>(ref T[] _array, int _startIndex, int _endIndex)
        {
            if (_startIndex < _endIndex)
            {
                int _pi = Partition(ref _array, _startIndex, _endIndex);

                QuickSort(ref _array, _startIndex, _pi - 1);
                QuickSort(ref _array, _pi + 1, _endIndex);
            }
            return _array;
        }

        private static int Partition<T>(ref T[] _array, int _startIndex, int _endIndex)
        {

            T _pivot = _array[_endIndex];

            int i = (_startIndex - 1);

            for (int j = _startIndex; j < _endIndex; j++)
            {
                IComparable _compare = (IComparable)_array[j];
                if (1 == _compare.CompareTo(_pivot))
                {
                    i++;
                    SwapArray(ref _array, i, j);
                }
            }
            SwapArray(ref _array, i + 1, _endIndex);
            return (i + 1);

        }

        #endregion
        public static TFieldType GetFieldValue<TFieldType, TObjectType>(this TObjectType obj, string fieldName)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic);
            return (TFieldType)fieldInfo.GetValue(obj);
        }
    }
}