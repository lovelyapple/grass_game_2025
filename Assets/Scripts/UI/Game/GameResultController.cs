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
    private void OnDestory()
    {
        _countdownSubscription?.Dispose();
        _countdownSubscription = null;
    }
    public async UniTask<Unit> PerformAsync(int winner, CancellationToken token)
    {
        gameObject.SetActive(true);
        TimeLeft.text = "--";

        SetupWinner(winner);
        RemainSeconds = 30f;


        _countdownSubscription = CommonHelper.PerformCountDownAsync(GameConstant.CountDownSec, 
        (sec) => {
            RemainSeconds = sec;
            TimeLeft.text = RemainSeconds.ToString();
        }, () => 
        {
            TimeLeft.text = "--";
        });

        await UniTask.WaitWhile(() => RemainSeconds > 0, cancellationToken: token);

        gameObject.SetActive(false);
        return Unit.Default;
    }
    private void SetupWinner(int playerId)
    {
        var playerInfo = PlayerRootObject.Instance.GetPlayerInfoObject(playerId);

        WinnerView.UpdateEquipmentInfo(new EquipmentSetInfo(playerInfo.PlayerEquipment));
    }
}
