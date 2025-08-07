using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public static class CommonHelper
{
    public static IDisposable PerformCountDownAsync(double addSec, Action<int> onRemainUpdate, Action onFinished)
    {
        var endTime = DateTime.UtcNow.AddSeconds(addSec);
        double endTimeUnixMilliseconds = new DateTimeOffset(endTime).ToUnixTimeMilliseconds();

        var startAtTime = DateTimeOffset.FromUnixTimeMilliseconds((long)endTimeUnixMilliseconds).UtcDateTime;
        var RemainSeconds = (startAtTime - DateTime.UtcNow).TotalSeconds;
        Debug.LogWarning($"Start CountDown Client {RemainSeconds}");
        IDisposable countdownSubscription = null;

        countdownSubscription = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .TakeWhile(_ => DateTime.UtcNow < startAtTime)
            .Subscribe(
                onNext: _ =>
                {
                    RemainSeconds = (startAtTime - DateTime.UtcNow).TotalSeconds;
                    onRemainUpdate?.Invoke((int)RemainSeconds);
                },
                onCompleted: _ =>
                {
                    onFinished?.Invoke();
                    countdownSubscription?.Dispose();
                    countdownSubscription = null;
                }
            );

        return countdownSubscription;
    }
}
public static class ReadyObservableExtensions
{
    // Await可能なDo
    public static async UniTask DoAsync<T>(
        this Observable<T> source,
        Action<T> action
    )
    {
        var value = await source.FirstAsync(); // 最初の値を待つ
        action(value);
    }
}