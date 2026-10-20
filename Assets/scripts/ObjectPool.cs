using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject Prefab;
}

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;

    private readonly Dictionary<GameObject, Stack<GameObject>> pools = new Dictionary<GameObject, Stack<GameObject>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    private static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("ObjectPool").AddComponent<ObjectPool>();
            }
            return instance;
        }
    }

    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Instance.GetInternal(prefab, position, rotation);
    }

    public static void Release(GameObject obj)
    {
        if (obj == null || !obj.activeSelf) return;

        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled == null || instance == null)
        {
            Destroy(obj);
            return;
        }

        instance.ReleaseInternal(obj, pooled.Prefab);
    }

    private GameObject GetInternal(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>();
            pools[prefab] = stack;
        }

        GameObject obj = null;
        while (stack.Count > 0 && obj == null)
        {
            obj = stack.Pop();
        }

        if (obj == null)
        {
            obj = Instantiate(prefab, position, rotation, transform);
            obj.AddComponent<PooledObject>().Prefab = prefab;
        }
        else
        {
            obj.transform.SetPositionAndRotation(position, rotation);
        }

        obj.SetActive(true);
        return obj;
    }

    private void ReleaseInternal(GameObject obj, GameObject prefab)
    {
        obj.SetActive(false);

        if (!pools.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>();
            pools[prefab] = stack;
        }
        stack.Push(obj);
    }
}
