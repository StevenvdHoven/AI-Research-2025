using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChiefoUtilities
{
    namespace PathFinding
    {
        public class ChiefPathFinder
        {
            public const float MOVE_DIRECT_COST = 10;
            public const float MOVE_DIAGNOL_COST = 15;

            public static List<PathNode> FindPath(ChiefGrid _grid, Vector3 _startPos, Vector3 _targetPos)
            {
                PathNode _startNode = _grid.PathNodeFromWorldPoint(_startPos);
                PathNode _targetNode = _grid.PathNodeFromWorldPoint(_targetPos);

                Heap<PathNode> _OpenList = new Heap<PathNode>(_grid.MaxSize);
                HashSet<PathNode> _ClosedList = new HashSet<PathNode>();

                _OpenList.Add(_startNode);

                while (_OpenList.Count > 0)
                {
                    PathNode _currentNode = _OpenList.RemoveFirst();

                    if (_currentNode == _targetNode)
                    {
                        return RetracePath(_startNode, _targetNode);
                    }

                    List<PathNode> _Neighbours = _grid.GetNeighbours(_currentNode);
                    foreach (PathNode n in _Neighbours)
                    {
                        if (!n.IsWalkable || _ClosedList.Contains(n))
                            continue;
                        int newCost = _currentNode.gCost + GetDistance(_currentNode, n);
                        if (newCost < n.gCost || !_OpenList.Contains(n))
                        {
                            n.gCost = newCost;
                            n.hCost = newCost + GetDistance(n, _targetNode);

                            n.ParentNode = _currentNode;

                            if (!_OpenList.Contains(n))
                            {
                                _OpenList.Add(n);
                            }
                        }
                    }
                    _ClosedList.Add(_currentNode);
                }
                return null;
            }

            public static List<PathNode> RetracePath(PathNode _StartNode, PathNode _Targetnode)
            {
                List<PathNode> _path = new List<PathNode>();
                PathNode _currentNode = _Targetnode;
                while (_currentNode != _StartNode)
                {
                    _path.Add(_currentNode);
                    _currentNode = _currentNode.ParentNode;
                }
                _path.Reverse();
                return _path;
            }

            public static int GetDistance(PathNode A, PathNode B)
            {
                int distanceX = Mathf.Abs(A.Xindex - B.Xindex);
                int distanceY = Mathf.Abs(A.Yindex - B.Yindex);

                if (distanceX > distanceY)
                    return Mathf.RoundToInt(MOVE_DIAGNOL_COST * distanceY + MOVE_DIRECT_COST * (distanceX - distanceY));
                else
                    return Mathf.RoundToInt(MOVE_DIAGNOL_COST * distanceX + MOVE_DIRECT_COST * (distanceY - distanceX));
            }
        }

        public class ChiefGrid
        {
            private int m_SizeX;
            private int m_SizeY;

            private Vector3 m_WorldPosition;
            private Vector3 m_GridWorldSize;

            public PathNode[,] PathNodes { get; set; }

            public float PathNodeRadius { get; set; }

            public LayerMask LayerMask { get; }

            public int MaxSize
            {
                get
                {
                    return m_SizeX * m_SizeY;
                }
            }

            private float m_PathNodeDiamator;

            public ChiefGrid(Vector3 _worldPosition, Vector2 _size, float _NodeRadius, LayerMask _layerMask)
            {
                m_GridWorldSize = new Vector3(_size.x, 0f, _size.y);

                PathNodeRadius = _NodeRadius;

                LayerMask = _layerMask;

                m_WorldPosition = _worldPosition;

                m_PathNodeDiamator = PathNodeRadius * 2;

                m_SizeX = Mathf.RoundToInt(_size.x / m_PathNodeDiamator);
                m_SizeY = Mathf.RoundToInt(_size.y / m_PathNodeDiamator);
                CreateGrid();
            }

            private void CreateGrid()
            {
                PathNodes = new PathNode[m_SizeX, m_SizeY];
                Vector3 _bottomLeft = m_WorldPosition - Vector3.right * m_GridWorldSize.x / 2 - Vector3.forward * m_GridWorldSize.z / 2;

                for (int x = 0; x < m_SizeX; x++)
                {
                    for (int y = 0; y < m_SizeY; y++)
                    {
                        Vector3 _worldPoint = new Vector3(x * m_PathNodeDiamator + PathNodeRadius, 0f, y * m_PathNodeDiamator + PathNodeRadius) + _bottomLeft;
                        bool _walkable = !Physics.CheckSphere(_worldPoint, PathNodeRadius, LayerMask);
                        PathNodes[x, y] = new PathNode(new Vector2Int(x, y), _worldPoint, _walkable);
                    }
                }
            }

            public List<PathNode> GetNeighbours(PathNode _node)
            {
                List<PathNode> _neighbours = new List<PathNode>();

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        int xPointer = _node.Xindex + x;
                        int yPointer = _node.Yindex + y;
                        if (xPointer >= 0 && xPointer < m_SizeX)
                        {
                            if (yPointer >= 0 && yPointer < m_SizeY)
                            {
                                _neighbours.Add(PathNodes[xPointer, yPointer]);
                            }
                        }
                    }
                }
                return _neighbours;
            }

            public PathNode PathNodeFromWorldPoint(Vector3 _worldPoint)
            {
                float _Xpercent = (_worldPoint.x + m_GridWorldSize.x / 2) / m_GridWorldSize.x;
                float _Ypercent = (_worldPoint.z + m_GridWorldSize.z / 2) / m_GridWorldSize.z;
                _Xpercent = Mathf.Clamp01(_Xpercent);
                _Ypercent = Mathf.Clamp01(_Ypercent);

                int _x = Mathf.RoundToInt((m_SizeX - 1) * _Xpercent);
                int _y = Mathf.RoundToInt((m_SizeY - 1) * _Ypercent);
                return PathNodes[_x, _y];
            }

            public void DrawGizmos()
            {
                Gizmos.DrawWireCube(m_WorldPosition, new Vector3(m_GridWorldSize.x, 1f, m_GridWorldSize.z));
                if (PathNodes != null)
                {
                    foreach (PathNode item in PathNodes)
                    {
                        Gizmos.color = (item.IsWalkable) ? Color.gray : Color.red;
                        Gizmos.DrawCube(item.WorldPosition, Vector3.one * (m_PathNodeDiamator - .1f));
                    }
                }
            }
        }

        public class PathNode : IHeapItem<PathNode>
        {
            public Vector3 WorldPosition;

            public int Xindex;
            public int Yindex;

            public int gCost;
            public int hCost;

            private int heapindex;

            public int fCost
            {
                get { return gCost + hCost; }
            }

            public int HeapIndex
            {
                get
                {
                    return heapindex;
                }
                set
                {
                    heapindex = value;
                }
            }

            public bool IsWalkable;
            public PathNode ParentNode;

            public PathNode(Vector2Int _indexes, Vector3 _worldPos, bool _IsWalkable)
            {
                Xindex = _indexes.x;
                Yindex = _indexes.y;

                WorldPosition = _worldPos;
                IsWalkable = _IsWalkable;
            }

            public int CompareTo(PathNode other)
            {
                int compare = fCost.CompareTo(other.fCost);
                if (compare == 0)
                {
                    compare = hCost.CompareTo(other.hCost);
                }
                return -compare;
            }
        }

        public class Heap<T> where T : IHeapItem<T>
        {
            private T[] items;
            private int currentItemCount;

            public Heap(int maxHeapSize)
            {
                items = new T[maxHeapSize];
            }

            public void Add(T item)
            {
                item.HeapIndex = currentItemCount;
                items[currentItemCount] = item;
                SortUp(item);
                currentItemCount++;
            }

            public T RemoveFirst()
            {
                T firstItem = items[0];
                currentItemCount--;
                items[0] = items[currentItemCount];
                items[0].HeapIndex = 0;
                SortDown(items[0]);
                return firstItem;
            }

            public void UpdateItem(T item)
            {
                SortUp(item);
            }

            public int Count
            {
                get
                {
                    return currentItemCount;
                }
            }

            public bool Contains(T item)
            {
                return Equals(items[item.HeapIndex], item);
            }

            private void SortDown(T item)
            {
                while (true)
                {
                    int childIndexLeft = item.HeapIndex * 2 + 1;
                    int childIndexRight = item.HeapIndex * 2 + 2;
                    int swapIndex = 0;

                    if (childIndexLeft < currentItemCount)
                    {
                        swapIndex = childIndexLeft;

                        if (childIndexRight < currentItemCount)
                        {
                            if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                            {
                                swapIndex = childIndexRight;
                            }
                        }

                        if (item.CompareTo(items[swapIndex]) < 0)
                        {
                            Swap(item, items[swapIndex]);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            private void SortUp(T item)
            {
                int parentIndex = (item.HeapIndex - 1) / 2;

                while (true)
                {
                    T parentItem = items[parentIndex];
                    if (item.CompareTo(parentItem) > 0)
                    {
                        Swap(item, parentItem);
                    }
                    else
                    {
                        break;
                    }

                    parentIndex = (item.HeapIndex - 1) / 2;
                }
            }

            private void Swap(T itemA, T itemB)
            {
                items[itemA.HeapIndex] = itemB;
                items[itemB.HeapIndex] = itemA;
                int itemAIndex = itemA.HeapIndex;
                itemA.HeapIndex = itemB.HeapIndex;
                itemB.HeapIndex = itemAIndex;
            }
        }

        public interface IHeapItem<T> : IComparable<T>
        {
            int HeapIndex
            {
                get;
                set;
            }
        }
    }
}