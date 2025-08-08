using System;
using System.Threading;
using UnityEngine;

public interface IStatusEffect
{
    public Action OnFinished { get; set; }
    public float Duation { get; }
    public void OnExecute(CancellationToken token, Action onFinished);
    public void OnRelease();
}
