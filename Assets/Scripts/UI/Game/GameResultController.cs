using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;

public class GameResultController : MonoBehaviour
{
    [SerializeField] PlayerEquipmentSetView WinnerView;
    [SerializeField] TextMeshProUGUI TimeLeft;
    private double RemainSeconds;
    private IDisposable _countdownSubscription;
    public async UniTask<Unit> PerformAsync(int winner, CancellationToken token)
    {
        gameObject.SetActive(true);
        TimeLeft.text = "--";

        SetupWinner(winner);

        var endTime = DateTime.UtcNow.AddSeconds(GameConstant.CountDownSec);
        double unixTimeMs = new DateTimeOffset(endTime).ToUnixTimeMilliseconds();
        PerformCountDownAsync(unixTimeMs);

        await UniTask.WaitWhile(() => RemainSeconds > 0, cancellationToken: token);

        gameObject.SetActive(false);
        return Unit.Default;
    }
    private void PerformCountDownAsync(double endTimeUnixMilliseconds)
    {
        var startAtTime = DateTimeOffset.FromUnixTimeMilliseconds((long)endTimeUnixMilliseconds).UtcDateTime;
        RemainSeconds = (startAtTime - DateTime.UtcNow).TotalSeconds;
        Debug.LogWarning($"Start CountDown Client {RemainSeconds}");

        _countdownSubscription = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .TakeWhile(_ => DateTime.UtcNow < startAtTime)
            .Subscribe(
                onNext: _ =>
                {
                    RemainSeconds = (startAtTime - DateTime.UtcNow).TotalSeconds;
                    TimeLeft.text = ((int)RemainSeconds).ToString();
                },
                onCompleted: _ =>
                {
                    FinishCountDown();
                }
            );
    }
    private void FinishCountDown()
    {
        _countdownSubscription?.Dispose();
        _countdownSubscription = null;
        RemainSeconds = 0;
    }
    private void SetupWinner(int playerId)
    {
        var playerInfo = PlayerRootObject.Instance.GetPlayerInfoObject(playerId);

        WinnerView.UpdateEquipmentInfo(new EquipmentSetInfo(playerInfo.PlayerEquipment));
    }
}
