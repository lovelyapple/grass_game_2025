using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using System;

public static class RxExtension
{
    public static UniTask<T> ToUniTask<T>(this IObservable<T> observable)
    {
        // Observable を UniTask に変換して最初の値を取得
        return observable.ToUniTask();
    }
}
