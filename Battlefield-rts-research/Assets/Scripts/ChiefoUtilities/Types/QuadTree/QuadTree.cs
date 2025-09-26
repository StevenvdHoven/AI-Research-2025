using System.Collections.Generic;
using UnityEngine;

namespace ChiefoUtilities
{
    namespace Types
    {
        public class QuadTree<T>
        {
            public float Size { get; private set; }             //Size for main quad
            public Vector3 position { get; private set; }       //World Position Of QuadTree

            public int GridSize { get; private set; }           //Total grid Size
            public int MaxQuadCapicity { get; private set; }    //Capicity per quad
            private Quad<T> MainQuad;                              //Main/Start Quad
            private List<IQuadTreeObject<T>> Points;               //All Points in world

            public QuadTree(int TotalSize, int MaxCapicity, Vector3 _pos)
            {
                GridSize = TotalSize;
                MaxQuadCapicity = MaxCapicity;
                position = _pos;

                Size = GridSize / 2f;                           //caculate the correct size for the mainQuad
            }

            public void SpawnPoints(List<T> _Points)
            {
                MainQuad = new Quad<T>(position, Size, 10);          //Create MainQuad
                Points = new List<IQuadTreeObject<T>>();
                foreach (T item in _Points)
                {
                    IQuadTreeObject<T> _object = (IQuadTreeObject<T>)item;
                    if (_object != null)
                    {
                        Points.Add(_object);
                    }
                }

                CheckPoints();
            }

            private void CheckPoints()
            {
                foreach (IQuadTreeObject<T> i in Points)
                {
                    CheckWhereIn(i);
                }
            }

            public void CheckWhereIn(IQuadTreeObject<T> point)
            {
                MainQuad.AddPoint(point);
            }

            public List<T> GetPoints(Vector3 pos, float _range)
            {
                List<IQuadTreeObject<T>> _List = MainQuad.GetPoints(pos, _range);

                List<T> _newlist = new List<T>();
                if (_List != null)
                {
                    foreach (IQuadTreeObject<T> item in _List)
                    {
                        _newlist.Add((T)item);
                    }
                }
                return _newlist;
            }

            public void OnDrawGizmos()
            {
#if UNITY_EDITOR
                List<Quad<T>> quads = new List<Quad<T>>();
                quads.Add(MainQuad);
                for (int i = 0; i < quads.Count; i++)
                {
                    List<Quad<T>> ChildQuads = quads[i].GetChilderen();
                    if (ChildQuads != null)
                    {
                        foreach (Quad<T> item in ChildQuads)
                        {
                            quads.Add(item);
                        }
                    }
                }

                foreach (Quad<T> quad in quads)
                {
                    Vector3[] _points = new Vector3[]
                    {
                new Vector3(quad.Size,0,quad.Size),
                new Vector3(quad.Size,0,-quad.Size),
                new Vector3(-quad.Size,0,-quad.Size),
                new Vector3(-quad.Size,0,quad.Size),
                    };
                    for (int i = 0; i < _points.Length; i++)
                    {
                        Gizmos.color = Color.blue;
                        Vector3 pos_1 = Vector3.zero;
                        Vector3 pos_2 = Vector3.zero;
                        if (i == _points.Length - 1)
                        {
                            pos_1 = _points[i] + quad.CenterPosition;
                            pos_2 = _points[0] + quad.CenterPosition;
                        }
                        else
                        {
                            pos_1 = _points[i] + quad.CenterPosition;
                            pos_2 = _points[i + 1] + quad.CenterPosition;
                        }

                        Gizmos.DrawLine(pos_1, pos_2);
                    }
                    Vector3 _size = new Vector3(0.1f, 0.1f, 0.1f);

                    Gizmos.DrawCube(quad.CenterPosition, _size);
                }

                List<IQuadTreeObject<T>> _LocalDrawingQuadPoints = MainQuad.GetListPoints();

                if (_LocalDrawingQuadPoints != null)
                {
                    foreach (IQuadTreeObject<T> point in _LocalDrawingQuadPoints)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(point.Position, 0.5f);
                    }
                }

                if (Points != null)
                {
                    foreach (IQuadTreeObject<T> point in Points)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(point.Position, 0.4f);
                    }
                }
#endif
            }
        }

        public struct Quad<T>
        {
            public Vector3 CenterPosition;
            public float Size;
            public List<IQuadTreeObject<T>> Points;
            public List<Quad<T>> ChildQuads;
            public int Capicity;

            public Vector3 BoundsMin;
            public Vector3 BoundsMax;

            public bool ExpandSearch;

            public Quad(Vector3 _pos, float _size, int _cap)
            {
                CenterPosition = _pos;
                Size = _size;
                Points = new List<IQuadTreeObject<T>>();
                ChildQuads = new List<Quad<T>>();
                Capicity = _cap;
                BoundsMin = new Vector3(-Size, 0, -Size) + CenterPosition;
                BoundsMax = new Vector3(Size, 0, Size) + CenterPosition;
                ExpandSearch = false;
            }

            public List<IQuadTreeObject<T>> GetPoints(Vector3 _pos, float _Range)
            {
                if (ChildQuads == null)
                {
                    return Points;
                }

                if (OverLap(_pos, _Range))
                    ExpandSearch = true;

                List<IQuadTreeObject<T>> _points = new List<IQuadTreeObject<T>>();
                if (ChildQuads.Count == 0)
                {
                    foreach (IQuadTreeObject<T> item in Points)
                    {
                        float _distance = Vector3.Distance(_pos, item.Position);
                        if (_distance <= _Range)
                        {
                            _points.Add(item);
                        }
                    }
                    return _points;
                }
                else
                {
                    bool expand = ExpandSearch;
                    if (_pos.x == CenterPosition.x || _pos.y == CenterPosition.y || _pos == CenterPosition)
                        expand = true;

                    if (ChildQuads != null)
                    {
                        foreach (Quad<T> _quad in ChildQuads)
                        {
                            if (_quad.IsWithinBounds(_pos) || expand)
                            {
                                List<IQuadTreeObject<T>> _localPoints = _quad.GetPoints(_pos, _Range);
                                foreach (IQuadTreeObject<T> LocalP in _localPoints)
                                {
                                    _points.Add(LocalP);
                                }
                            }

                            if (_quad.ExpandSearch)
                            {
                                expand = true;
                                ExpandSearch = true;
                            }
                        }
                    }
                    return _points;
                }
            }

            private bool OverLap(Vector3 _pos, float _Range)
            {
                if (Vector3.Distance(_pos, CenterPosition) <= _Range)
                {
                    return true;
                }

                Vector3[] _Position = new Vector3[]
                {
            new Vector3(_Range,0,0),
            new Vector3(_Range,0,_Range),
            new Vector3(0,0,_Range),
            new Vector3(-_Range,0,_Range),
            new Vector3(-_Range,0,0),
            new Vector3(-_Range,0,-_Range),
            new Vector3(0,0,-_Range),
            new Vector3(_Range,0,-_Range),
                };

                for (int i = 1; i < 4; i++)
                {
                    foreach (Vector3 _offset in _Position)
                    {
                        Vector3 _currentOffest = Vector3.ClampMagnitude(_offset, _Range / i);
                        Debug.DrawLine(_pos, _pos + _currentOffest, Color.red);
                        if (IsWithinBounds(_pos + _currentOffest) == false)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public void AddPoint(IQuadTreeObject<T> _point)
            {
                if (ChildQuads.Count == 0)
                {
                    Points.Add(_point);
                    if (Points.Count > Capicity)
                    {
                        DevidePoints();
                    }
                }
                else
                {
                    for (int i = 0; i < ChildQuads.Count; i++)
                    {
                        if (ChildQuads[i].IsWithinBounds(_point.Position))
                        {
                            ChildQuads[i].AddPoint(_point);
                            break;
                        }
                    }
                }
            }

            public void RemovePoint(IQuadTreeObject<T> _object)
            {
                Points.Remove(_object);
            }

            public bool IsWithinBounds(Vector3 _point)
            {
                if (BoundsMax.x >= _point.x && BoundsMin.x <= _point.x)
                {
                    if (BoundsMax.z >= _point.z && BoundsMin.z <= _point.z)
                        return true;
                }
                return false;
            }

            private void DevidePoints()
            {
                Quad<T>[] newquads = new Quad<T>[4];
                float _size = Size / 2f;

                Vector3[] _Offsets = new Vector3[]
                {
            new Vector3(_size, 0, _size),
            new Vector3(_size, 0, -_size),
            new Vector3(-_size, 0, -_size),
            new Vector3(-_size, 0, _size),
                };

                for (int i = 0; i < newquads.Length; i++)
                {
                    Quad<T> _currentQuad = new Quad<T>(CenterPosition + _Offsets[i], _size, Capicity);
                    List<IQuadTreeObject<T>> _HaveTOBeRemoved = new List<IQuadTreeObject<T>>();

                    for (int p = 0; p < Points.Count; p++)
                    {
                        if (_currentQuad.IsWithinBounds(Points[p].Position))
                        {
                            _currentQuad.AddPoint(Points[p]);
                            _HaveTOBeRemoved.Add(Points[p]);
                        }
                    }

                    newquads[i] = _currentQuad;
                }
                Points.Clear();

                foreach (Quad<T> item in newquads)
                {
                    ChildQuads.Add(item);
                }
            }

            public List<Quad<T>> GetChilderen()
            {
                return ChildQuads;
            }

            public List<IQuadTreeObject<T>> GetListPoints()
            {
                if (ChildQuads == null)
                    return Points;

                if (ChildQuads.Count == 0)
                {
                    return Points;
                }
                else
                {
                    List<IQuadTreeObject<T>> _Points = new List<IQuadTreeObject<T>>();
                    foreach (Quad<T> child in ChildQuads)
                    {
                        List<IQuadTreeObject<T>> _localPoints = child.GetListPoints();
                        foreach (IQuadTreeObject<T> _p in _localPoints)
                        {
                            _Points.Add(_p);
                        }
                    }
                    return _Points;
                }
            }
        }

        public struct Point : IQuadTreeObject<Point>
        {
            public Vector3 Position { get; set; }
            public float Speed;
        }
    }
}