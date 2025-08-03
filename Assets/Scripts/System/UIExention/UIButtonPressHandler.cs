using System;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject FrontObject;
    private ReactiveProperty<bool> _isPressingSubject = new ReactiveProperty<bool>(false);
    public Observable<bool> IsPressingObservable() => _isPressingSubject;
    private bool isPressing = false;
    // private IDisposable pressStream;

    // void Start()
    // {
    //     // 押している間の処理
    //     pressStream = Observable.EveryUpdate()
    //         .Subscribe(_ =>
    //         {
    //             Debug.Log("ボタン押されてる");
    //             // 実行したい処理
    //         })
    //         .AddTo(this);
    // }
    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log($"Point down pointerPress {eventData.pointerPress?.name}");
        // Debug.Log($"Point down pointerClick {eventData.pointerClick?.name}");
        // Debug.Log($"Point down pointerEnter {eventData.pointerEnter?.name}");
        // pointerPress が反応しない？Clickを使う
        if (eventData.pointerEnter == FrontObject)
        {
            _isPressingSubject.Value = true;
            // _isPressingSubject.ForceNotify();
            // Debug.Log("OnPointer down");
        }
        // Button.ispointdown
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerPress == FrontObject)
        {
            _isPressingSubject.Value = false;
            // _isPressingSubject.ForceNotify();
            // Debug.Log("OnPointer up");
        }
    }

    // private void OnDestroy()
    // {
    //     pressStream?.Dispose();
    // }
}
