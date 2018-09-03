using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitTree {

    SortedList<double, LimitTree> children;
    int capacity;
    GameObject value;

    public LimitTree(GameObject value)
    {
        this.value = value;
        //Signal to value that you exist, to be ignored or not
    }

    //If not ignored, then target can init you
    public void Init(int childrenLimit)
    {
        children = new SortedList<double, LimitTree>(childrenLimit);
    }

	public void AddChild(double id, GameObject value)
    {
        if (children.Count == children.Capacity)
            children.RemoveAt(0);
        children.Add(id, new LimitTree(value));
    }
}
