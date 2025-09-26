using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Events;

public class PoolableObject : MonoBehaviour
{
    private bool m_CanUse;

    public UnityEvent OnPool = new UnityEvent();
    public UnityEvent OnSpawn = new UnityEvent();

    public Transform Parent;

    public virtual void Load()
    {

    }

    public void HideOjbect()
    {
        gameObject.SetActive(false);
        m_CanUse = true;
    }

    public void PoolObject()
    {
        OnPool.Invoke();
        m_CanUse = true;
        transform.SetParent(Parent);
        gameObject.SetActive(false);
    }

    public virtual void SpawnObject(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        m_CanUse = false;
        OnSpawn.Invoke();
        ResetObject();
    }

    public virtual void SpawnObject(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);
        m_CanUse = false;
        OnSpawn.Invoke();
        ResetObject();
    }

    public void SetLifeTimeTimer(float time)
    {
        StartCoroutine(LifeTimeTimer(time));
    }

    public IEnumerator LifeTimeTimer(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        PoolObject();
    }

    public void LookAtTraget(Transform target)
    {
        transform.LookAt(target);
    }

    public bool CanUse() => m_CanUse;

    protected virtual void ResetObject()
    {
    }
}