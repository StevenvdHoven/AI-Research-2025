using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChiefoUtilities
{
    public class ChiefMath
    {
        public static T[] SortArrayBasedOfField<T>(T[] _array, ValueType valueType, string _fieldName)
        {
            return ChiefSorter.SortArrayBasedOfField(_array, valueType, _fieldName);
        }

        public static bool ContainsPoint(Vector2[] _points, Vector2 _pos)
        {
            int j = _points.Length - 1;
            bool _Contains = false;
            for (int i = 0; i < _points.Length; j = i++)
            {
                Vector2 pi = _points[i];
                Vector2 pj = _points[j];
                if (((pi.y <= _pos.y && _pos.y < pj.y) || (pj.y <= _pos.y && _pos.y < pi.y)) &&
                    (_pos.x < (pj.x - pi.x) * (_pos.y - pi.y) / (pj.y - pi.y) + pi.x))
                {
                    _Contains = !_Contains;
                }
            }
            return _Contains;
        }

        public static bool ContainsPoint(Vector3[] _points, Vector3 _pos)
        {
            int j = _points.Length - 1;
            bool _contains = false;
            for (int i = 0; i < _points.Length; j = i++)
            {
                Vector3 pi = _points[i];
                Vector3 pj = _points[j];
                if (((pi.z <= _pos.z && _pos.z < pj.z) || (pj.z <= _pos.z && _pos.y < pi.z)) &&
                    (_pos.x < (pj.x - pi.x) * (_pos.z - pi.z) / (pj.z - pi.z) + pi.x))
                {
                    _contains = !_contains;
                }
            }
            return _contains;
        }

        public static bool IsWithinPoints(Vector3[] _points, Vector3 _pos)
        {
            if (_points.Length < 1)
                return false;

            Vector3 _centerPoint = GetAveragePoint(_points);
            _points = ChiefSorter.SortOnClosest(_points, _pos);
            Vector3 _closestPointFrom_P = FindNearestPointOnLine(_points[0], _points[1], _pos);

            if (_points.Length > 1)
            {
                Vector3 _seconndPointFrom_p = FindNearestPointOnLine(_points[0], _points[2], _pos);
                float _distance_AtoP = Vector3.Distance(_closestPointFrom_P, _pos);
                float _distance_BtoP = Vector3.Distance(_seconndPointFrom_p, _pos);

                if (_distance_AtoP > _distance_BtoP)
                {
                    _closestPointFrom_P = _seconndPointFrom_p;
                }
            }

            float _C_distance = Vector3.Distance(_closestPointFrom_P, _centerPoint);
            float _P_distance = Vector3.Distance(_centerPoint, _pos);
            return _C_distance > _P_distance;
        }

        public static Vector3 GetAveragePoint(Vector3[] _points)
        {
            Vector3 _avg = Vector3.zero;
            foreach (Vector3 _point in _points)
            {
                _avg += _point;
            }
            return _avg / _points.Length;
        }

        public static Vector3 FindNearestPointOnLine(Vector3 _a, Vector3 _b, Vector3 _p)
        {
            //Get heading
            Vector3 heading = (_b - _a);
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            //Do projection from the point but clamp it
            Vector3 lhs = _p - _a;
            float dotP = Vector3.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return _a + heading * dotP;
        }

        /// <summary>
        /// Get the points of a circle around the position you give
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_Radius"></param>
        /// <param name="numberofPoints"></param>
        /// <returns></returns>
        public static Vector3[] GetCirclePoints(Vector3 _pos, float _Radius = 1f, int numberofPoints = 10)
        {
            // Caculate TAU wish is just Pi times 2
            float TAU = Mathf.PI * 2f;
            // Create a Vector3 List to save the points
            List<Vector3> vector3s = new List<Vector3>();
            // Go over the number of points
            for (int i = 0; i < numberofPoints; i++)
            {
                // Caculate next angle based of the number of points given
                float _angle = i * TAU / numberofPoints;
                // Caculate the x and z coordiants
                float x = Mathf.Sin(_angle) * _Radius / 2f;
                float z = Mathf.Cos(_angle) * _Radius / 2f;
                // And add it to the list
                vector3s.Add(new Vector3(x, 0f, z) + _pos);
            }

            return vector3s.ToArray();
        }

        /// <summary>
        /// Get the points of a Sphere around the position you give
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_Radius"></param>
        /// <param name="numberofPoints"></param>
        /// <returns></returns>
        public static Vector3[] GetSpherePoints(Vector3 _pos, float _Radius, int numberofPoints = 10)
        {
            float TAU = Mathf.PI * 2f;
            List<Vector3> vector3s = new List<Vector3>();
            for (int j = 0; j < numberofPoints; j++)
            {
                float _angle1 = (j + 1) * Mathf.PI / (numberofPoints + 1);
                for (int i = 0; i <= numberofPoints; i++)
                {
                    float _angle2 = i * TAU / numberofPoints;
                    float x = (Mathf.Sin(_angle1) * Mathf.Cos(_angle2)) * _Radius / 2f;
                    float y = Mathf.Cos(_angle1) * _Radius / 2f;
                    float z = (Mathf.Sin(_angle1) * Mathf.Sin(_angle2)) * _Radius / 2f;

                    vector3s.Add(new Vector3(x, y, z) + _pos);
                }
            }
            return vector3s.ToArray();
        }

        public static Quaternion GetRotationValue(Vector3 _Pos, Vector3 _TargetPos)
        {
            Vector3 dir = _TargetPos - _Pos;
            return Quaternion.LookRotation(dir);
        }

        public static Quaternion GetRotationValue(Vector2 _Pos, Vector2 _TargetPos)
        {
            Vector3 dir = _TargetPos - _Pos;
            return Quaternion.LookRotation(dir);
        }

        public static Vector3 GetRandomOffset3D(float _Range)
        {
            float x = Random.Range(-_Range, _Range);
            float y = Random.Range(-_Range, _Range);
            float z = Random.Range(-_Range, _Range);
            return new Vector3(x, y, z);
        }

        public static Vector2 GetRandomOffset2D(float _Range)
        {
            float x = Random.Range(-_Range, _Range);
            float y = Random.Range(-_Range, _Range);
            return new Vector2(x, y);
        }
    }

    public struct TempDistanceHolder
    {
        public int Index;
        public Vector3 Position;
        public float Distance;
    }
}