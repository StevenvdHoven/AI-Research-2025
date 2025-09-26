using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    public GameObject m_Prefab;
    public List<PoolableObject> m_Pool;
    public Transform m_Parent;

    public ObjectPool(GameObject _prefab, int _beginLenght, Transform parent)
    {
        m_Prefab = _prefab;
        m_Parent = parent;
        MakePool(_beginLenght, parent);
    }

    private void MakePool(int length, Transform parent)
    {
        m_Pool = new List<PoolableObject>();
        for (int i = 0; i < length; i++)
        {
            PoolableObject _poolableObject = null;
            if (parent == null)
            {
                _poolableObject = Object.Instantiate(m_Prefab).GetComponent<PoolableObject>();    //Getcompent poolableobject;
            }
            else
            {
                _poolableObject = Object.Instantiate(m_Prefab, parent).GetComponent<PoolableObject>();
            }
            _poolableObject.Parent = parent;
            _poolableObject.Load();
            _poolableObject.HideOjbect();                                                            //Pool the object;
            m_Pool.Add(_poolableObject);                                                              //Add to the list;
        }
    }

    public PoolableObject GetObject()
    {
        for (int i = 0; i < m_Pool.Count; i++)
        {
            if (m_Pool[i].CanUse())
            {
                return m_Pool[i];
            }
        }
        if (m_Parent == null)
        {
            PoolableObject _poolableObject = Object.Instantiate(m_Prefab).GetComponent<PoolableObject>();    //Getcompent poolableobject;
            _poolableObject.Load();
            _poolableObject.PoolObject();                                                             //Pool the object;
            m_Pool.Add(_poolableObject);
            return _poolableObject;
        }
        else
        {
            PoolableObject _poolableObject = Object.Instantiate(m_Prefab, m_Parent).GetComponent<PoolableObject>();    //Getcompent poolableobject;
            _poolableObject.Parent = m_Parent;
            _poolableObject.Load();
            _poolableObject.PoolObject();                                                             //Pool the object;
            m_Pool.Add(_poolableObject);
            return _poolableObject;
        }
    }

    public void PoolAll()
    {
        foreach (PoolableObject poolableObject in m_Pool)
        {
            poolableObject.PoolObject();
        }
    }
}