using UnityEngine;
using System.Collections;
using System;

public class Singleton<T>:MonoBehaviour where T:MonoBehaviour 
{

    void Awake()
    {
        if (instance != null)
            throw Exception("Singleton already exists.");
        instance = GetComponent<T>();
        AwakeSingleton();
    }

    private Exception Exception(string v)
    {
        throw new NotImplementedException();
    }

    public static T instance;

    protected virtual void AwakeSingleton(){ }
}
