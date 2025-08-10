using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class StatusEffectStun : IStatusEffect
{
    public StatusEffectType EffectType => StatusEffectType.Stun;
    public Action OnFinished { get; set; }
    public float Duation => 5f;
    public void OnExecute(CancellationToken token, Action onFinished) 
    {
        RunAsync(token).Forget();
        OnFinished = onFinished;
    }
    public void OnRelease() 
    {
        OnFinished?.Invoke();
    }
    private async UniTask<Unit> RunAsync(CancellationToken token)
    {
        try
        {
            await UniTask.WaitForSeconds(Duation, cancellationToken: token);
        }
        finally
        {
            OnRelease();
        }

        return Unit.Default;
    }
}
