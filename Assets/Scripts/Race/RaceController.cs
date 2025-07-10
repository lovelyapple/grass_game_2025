using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework.Constraints;
using R3;
using StarMessage.Models;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    [SerializeField] private Transform EndLineTransform;
    private CompositeDisposable _racingDisposable;
    private void Awake()
    {
        DebugInit();
    }

    private void OnDestroy()
    {
        _racingDisposable.Dispose();
        _racingDisposable = null;
    }

    private void DebugInit()
    {
        if (_racingDisposable == null)
        {
            _racingDisposable = new CompositeDisposable();
        }

        var player = RacingModel.GetInstance().DebugInitPlayerAndVehicle();
        player.OnPlayerDrivingMovingAsObservable()
            .Subscribe(CheckPlayerFinished)
            .AddTo(_racingDisposable);


    }

    private void CheckPlayerFinished(PlayerBase playerBase)
    {
        if (playerBase.transform.position.z > EndLineTransform.position.z)
        {
            Debug.Log("Player Finished Line");
        }
    }
}
