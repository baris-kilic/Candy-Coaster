using System.Runtime.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/*
 * Singleton design pattern for generic type.  
 */
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    { 
        get 
        {
            if (instance == null)
                Debug.Log("No instance found of " + typeof(T));
            return instance;
        } 
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Instance found" + typeof(T) + "Self Destructing");
            Destroy(gameObject);
        }
        else
        {
            instance = this as T;
            Init();
        }
    }

    protected void OnDestroy()
    {
        if (this == instance)
            instance = null;
    }

    protected virtual void Init()
    {

    }
}
