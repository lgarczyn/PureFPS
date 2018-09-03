using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPool
{
    GameObject GetItem(Transform parent, Vector3 position);
    void ReturnItem(GameObject item);
    void ReturnItem(GameObject item, float time);
    void Reserve(uint amount);
}

//The argument to pool has to inherit from pool. Recursion FTW
public class Pool<T>: Singleton<T>, IPool where T:Pool<T>
{
    Stack<GameObject>items = new Stack<GameObject>();

    public GameObject item;
    public uint initializationCount;
    public uint Count { get { return (uint)items.Count; } }

    private void Start()
    {
        Reserve(initializationCount);
    }

    public void Reserve(uint amount)
    {
        for (int i = 0; i < initializationCount; i++)
            ReturnItem(CreateItem());
    }

    private GameObject CreateItem()
    {
        GameObject instance = Instantiate(item);
        if (instance.GetComponent<PoolSubject>())
            instance.GetComponent<PoolSubject>().parent = this;
        InitItem(instance);
        return instance;
    }

    protected virtual void InitItem(GameObject item) { }

    public virtual GameObject GetItem(Transform parent, Vector3 position)
    {
        GameObject instance;
        if (items.Count > 0)
            instance = items.Pop();
        else
            instance = CreateItem();


        PoolSubject subject = instance.GetComponent<PoolSubject>();
        if (subject && subject.killtime > 0f)
            ReturnItem(instance, subject.killtime);

        instance.SetActive(true);
        instance.transform.SetParent(parent);
        instance.transform.position = position;

        AudioSource source = instance.GetComponent<AudioSource>();
        if (source) { source.enabled = true; source.Play(); }

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
