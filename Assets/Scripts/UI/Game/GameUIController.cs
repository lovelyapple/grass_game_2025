using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private UIButtonPressHandler AccelerateButtonHandler;
    public Observable<bool> IsPressingAccelerateButtonObservable() => AccelerateButtonHandler.IsPressingObservable();

}
