using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPool
{
    void ReturnItem(GameObject item);
    void ReturnItem(GameObject item, float time);
    void Reserve(uint amount);
}

//The argument to pool has to inherit from pool. Recursion FTW
public class Pool<T> : Singleton<T>, IPool where T : Pool<T>
{
    Stack<GameObject> items = new Stack<GameObject>();

    public GameObject item;
    public uint initializationCount;
    public uint Count { get { return (uint)items.Count; } }

    private void Start()
    {
        Reserve(initializationCount);
    }

    public void Reserve(uint amount)
    {
        for (int i = 0; i < amount; i++)
            ReturnItem(CreateItem());
    }

    private GameObject CreateItem()
    {
        GameObject instance = Instantiate(item);
        if (instance.GetComponent<PoolSubject>())
            instance.GetComponent<PoolSubject>().parent = this;
        instance.transform.SetParent(transform);
        InitItem(instance);
        return instance;
    }

    protected virtual void InitItem(GameObject item) { }

    protected virtual GameObject GetItem()
    {
        GameObject instance;
        do
        {
            if (items.Count > 0)
                instance = items.Pop();
            else
                instance = CreateItem();
        } while (instance == null); // Take in account possibility that gameobject was destroyed

        PoolSubject subject = instance.GetComponent<PoolSubject>();
        if (subject && subject.killtime > 0f)
            ReturnItem(instance, subject.killtime);

        //AudioSource source = instance.GetComponent<AudioSource>();
        //if (source) { source.enabled = true; source.Play(); }

        instance.SetActive(true);

        return (instance);
    }

    protected virtual GameObject GetItem(Transform parent, Vector3 position)
    {
        GameObject instance = GetItem();
        instance.transform.SetParent(parent);
        instance.transform.localPosition = position;

        return instance;
    }

    public virtual void ReturnItem(GameObject item)
    {
        item.SetActive(false);
        item.transform.SetParent(transform);
        item.transform.position = Vector3.zero;
        items.Push(item);
    }

    public virtual void ReturnItem(GameObject item, float time)
    {
        StartCoroutine(ReturnItemCoroutine(item, time));
    }

    protected virtual IEnumerator ReturnItemCoroutine(GameObject item, float time)
    {
        yield return new WaitForSeconds(time);
        ReturnItem(item);//Should call final overrider
    }
}
