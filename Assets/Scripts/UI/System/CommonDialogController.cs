using System;
using Cysharp.Threading.Tasks;
using R3;
using Sytem.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommonDialogController : MonoBehaviour
{

    [SerializeField] ControllerSelectButtonsReceiver ControllerReceiver;
    [SerializeField] Canvas InstanceCanvas;
    [SerializeField] TextMeshProUGUI MessageText;
    [SerializeField] Button ConfirmButton;
    private static CommonDialogController _instance;
    private bool _tapped = false;
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        gameObject.SetActive(false);
        _instance = this;

        ControllerReceiver.OnTapButtonObservable()
        .Where(btn => btn == ConfirmButton)
        .Subscribe(x => _tapped = true)
        .AddTo(this);
    }
    public static void Show(string msg, Action onConfirm)
    {
        _instance._tapped = false;
        _instance.ShowAsync(msg, onConfirm).Forget();
    }
    private async UniTask<Unit> ShowAsync(string msg, Action onConfirm)
    {
        if(onConfirm == null)
        {
            Debug.Log($"not result delegate");
            return Unit.Default;
        }

        try
        {
            MessageText.text = msg;
            InstanceCanvas.enabled = true;
            gameObject.SetActive(true);
            var task1 = ConfirmButton.OnClickAsObservable().FirstAsync(this.destroyCancellationToken).AsUniTask();
            var task2 = UniTask.WaitUntil(() => _tapped, cancellationToken: this.destroyCancellationToken);
            await UniTask.WhenAny(task1, task2);
        }
        finally
        {
            gameObject.SetActive(false);
            onConfirm.Invoke();
        }

        return Unit.Default;
    }
}
