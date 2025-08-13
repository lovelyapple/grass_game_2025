using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class SaddleParameter
{
    public SaddleType Type;
    public float HeatRate;
}
[Serializable]
public class CharaParameter
{
    public Characters Type;
    public float AppendHP = 1.0f;
}
public class ParameterHolder : MonoBehaviour
{
    private static ParameterHolder _instance;
    public static ParameterHolder Instance
    {
        get{
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<ParameterHolder>();
            }

            return _instance;
        }
    }
    public List<SaddleParameter> SaddleParameters;
    public List<CharaParameter> CharaParameters;
}
