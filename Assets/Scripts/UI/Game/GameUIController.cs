using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] GameObject LoadUIRoot;
    [SerializeField] Image LoadUI;
    [SerializeField] private UIButtonPressHandler AccelerateButtonHandler;
    public Observable<bool> IsPressingAccelerateButtonObservable() => AccelerateButtonHandler.IsPressingObservable();

    private void Awake()
    {
        var col = LoadUI.color;
        col.a = 1;
        LoadUI.color = col;
        LoadUIRoot.gameObject.SetActive(true);
    }
}
