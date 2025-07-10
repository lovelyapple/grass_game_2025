using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBase<T> where T : new()
{
    private static T _instance;
    public void SetInstance(T newInstance)
    {
        _instance = newInstance;
    }
    public static T GetInstance()
    {
        return _instance;
    }
}
