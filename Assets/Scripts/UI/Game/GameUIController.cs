using System;
using Cysharp.Threading.Tasks;
using R3;
using StarMessage.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] GameObject LoadUIRoot;
    [SerializeField] GameObject GameHudRoot;
    [SerializeField] Image LoadUI;
    [SerializeField] private UIButtonPressHandler AccelerateButtonHandler;
    [SerializeField] private UIButtonPressHandler UpButtonHandler;
    [SerializeField] private UIButtonPressHandler DownButtonHandler;
    [SerializeField] private Button JumpInDownButton;
    [SerializeField] private GameResultController ResultController;
    public Observable<bool> IsPressingAccelerateButtonObservable() => AccelerateButtonHandler.IsPressingObservable();
    public Observable<bool> IsPressingUpButtonObservable() => UpButtonHandler.IsPressingObservable();
    public Observable<bool> IsPressingDownButtonObservable() => DownButtonHandler.IsPressingObservable();
    public Observable<Unit> OnClickJumpInDowmButtonObservable() => JumpInDownButton.OnClickAsObservable();

    private void Awake()
    {
        var col = LoadUI.color;
        col.a = 1;
        LoadUI.color = col;
        LoadUIRoot.gameObject.SetActive(true);

        MatchModel.GetInstance().ShowLoadUIObservable()
        .Subscribe(show => LoadUIRoot.gameObject.SetActive(show))
        .AddTo(this);

        MatchModel.GetInstance().OnMatchFinishedObservable()
        .Subscribe(async x => await RunResult(x))
        .AddTo(this);
    }

    private async UniTask<Unit> RunResult(int playerId)
    {
        GameHudRoot.SetActive(false);

        await ResultController.PerformAsync(playerId, this.destroyCancellationToken);

        if (GameCoreModel.Instance.IsAdminUser)
        {
            await UniTask.WaitUntil(() => RoomModel.GetInstance().IsEmpty);
            SceneChanger.GetInstance().RequestChangeSceneAsyc(SceneChanger.SceneName.Title).Forget();
        }
        else
        {
            MatchModel.GetInstance().Reset();
            RoomModel.GetInstance().ShutdownAndGotoTitle();
        }

        return Unit.Default;
    }
}
