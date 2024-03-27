using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Object pooling design pattern for generic type. 
public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour 
{
    [SerializeField] protected T prefab;

    private List<T> pooledObjects;
    private bool isReady;
    private int amount;

    public void PoolObjects(int amount = 0)
    {
        if (amount < 0)
        {
            Debug.LogError("Amount must be a non-negative integer.");
        }

        pooledObjects = new List<T>(amount);
        GameObject newObject;

        this.amount = amount;

        for (int i = 0; i < amount; i++)
        {
            newObject = Instantiate(prefab.gameObject, transform);
            newObject.SetActive(false);
            pooledObjects.Add(newObject.GetComponent<T>());
        }

        isReady = true;
    }

    //If the pool is not ready, create pool with size of 1
    //If pool ready, controlling the disabled objects and return them in for loop
    //After that, instantiate a matchable prefab and activate it.
    public T GetPooledObject()
    {
        if (!isReady)
        {
            PoolObjects(1);
        }

        for (int i = 0; i < pooledObjects.Count;i++)
        {
            if (!pooledObjects[i].isActiveAndEnabled)
            {
                return pooledObjects[i];
            }
        }

        GameObject newObject = Instantiate(prefab.gameObject, transform);
        newObject.SetActive(true);
        pooledObjects.Add(newObject.GetComponent<T>());
        ++amount;
        return newObject.GetComponent<T>();
    }

    public void ReturnObjectToPool(T toBeReturned)
    {
        if (toBeReturned == null)
            return;
        
        if (!isReady)
        {
            PoolObjects();
            pooledObjects.Add(toBeReturned);
        }

        toBeReturned.gameObject.SetActive(false);
    }
}
        