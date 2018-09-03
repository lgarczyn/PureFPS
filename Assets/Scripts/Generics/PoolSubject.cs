using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolSubject : MonoBehaviour {

    public IPool parent;
    public float killtime;

    public void ReturnItem(GameObject item)
    {
        parent.ReturnItem(item);
    }

    public void ReturnItem(GameObject item, float time)
    {
        parent.ReturnItem(item, time);
    }
}
