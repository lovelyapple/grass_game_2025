using System;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommonDialogController : MonoBehaviour
{
    [SerializeField] Canvas InstanceCanvas;
    [SerializeField] TextMeshProUGUI MessageText;
    [SerializeField] Button ConfirmButton;
    private static CommonDialogController _instance;
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        gameObject.SetActive(false);
        _instance = this;
    }
    public static void Show(string msg, Action onConfirm)
    {
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
            await ConfirmButton.OnClickAsObservable().FirstAsync(this.destroyCancellationToken);
        }
        finally
        {
            gameObject.SetActive(false);
            onConfirm.Invoke();
        }

        return Unit.Default;
    }
}
