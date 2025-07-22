using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinPlayerTypeSelectView : MonoBehaviour
{
    [SerializeField] Button JoinAdminButton;
    [SerializeField] Button JoinPlayerButton;
    [SerializeField] TMP_InputField AdminRoomInputField;
    [SerializeField] TMP_InputField PlayerNameInputField;

    private int _clickResult = 0;
    private void Awake()
    {
        JoinAdminButton.OnClickAsObservable()
        .Subscribe(_ => _clickResult = (int)PlayerRole.Admin)
        .AddTo(this);

        JoinPlayerButton.OnClickAsObservable()
        .Where(_ => !string.IsNullOrEmpty(PlayerNameInputField.text))
        .Subscribe(_ =>
        {
            GamePlayerInfoModel.GetInstance().SelfName = PlayerNameInputField.text;
            _clickResult = (int)PlayerRole.Player;
        })
        .AddTo(this);

        PlayerNameInputField.text = GamePlayerInfoModel.GetInstance().GetDefaultRandomName();
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
