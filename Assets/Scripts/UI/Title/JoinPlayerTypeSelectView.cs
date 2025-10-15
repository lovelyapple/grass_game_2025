using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Sytem.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinPlayerTypeSelectView : MonoBehaviour
{
    [SerializeField] ControllerSelectButtonsReceiver ControllerReceiver;
    [SerializeField] Button JoinAdminButton;
    [SerializeField] Button JoinPlayerButton;
    [SerializeField] TMP_InputField AdminRoomInputField;
    [SerializeField] TMP_InputField PlayerNameInputField;

    private int _clickResult = 0;
    private void Awake()
    {
#if !ADMIN_BUILD
        JoinAdminButton.gameObject.SetActive(false);
        AdminRoomInputField.gameObject.SetActive(false);
#endif

#if !PLAYER_BUILD
        JoinPlayerButton.gameObject.SetActive(false);
        PlayerNameInputField.gameObject.SetActive(false);
#endif


        var adminBtnObserable = ControllerReceiver.OnTapButtonObservable()
        .Where(btn => btn == JoinAdminButton).Select(_ => Unit.Default);

        Observable.Merge(adminBtnObserable, JoinAdminButton.OnClickAsObservable())
        .Subscribe(_ => _clickResult = (int)PlayerRole.Admin)
        .AddTo(this);

        var playerBtnObservable = ControllerReceiver.OnTapButtonObservable()
        .Where(btn => btn == JoinPlayerButton).Select(_ => Unit.Default);

        Observable.Merge(playerBtnObservable, JoinPlayerButton.OnClickAsObservable())
        .Where(_ => !string.IsNullOrEmpty(PlayerNameInputField.text))
        .Subscribe(_ =>
        {
            GamePlayerInfoModel.GetInstance().SelfName = PlayerNameInputField.text;
            _clickResult = (int)PlayerRole.Player;
        })
        .AddTo(this);

        PlayerNameInputField.text = GamePlayerInfoModel.GetInstance().SelfName;
    }
    public async UniTask<(PlayerRole, string)> OpenViewAsync(CancellationToken cancellation)
    {
        _clickResult = 0;
        gameObject.SetActive(true);
        await UniTask.WaitUntil(() => _clickResult != 0, cancellationToken: cancellation);
        gameObject.SetActive(false);
        return ((PlayerRole)_clickResult, AdminRoomInputField.text);
    }

}
