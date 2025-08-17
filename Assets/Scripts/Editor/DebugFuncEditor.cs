using System;
using R3;
using UnityEngine;
using UnityEditor;

public class DebugFuncEditor : EditorWindow
{
    private DateTime GameStartAtTime;
    private IDisposable _countdownSubscription;
    public double RemainSeconds { get; private set; }
    private int _seTestTimes = 1;
    private SeType _seType;
    [MenuItem("Debug/DebugFunc")]
    public static void Open()
    {
        GetWindow<DebugFuncEditor>();
    }

    public void OnGUI()
    {
        if (GUILayout.Button("StartCountDown"))
        {
            StartCountDown();
        }

        if (GUILayout.Button("StopCountDown"))
        {
            OnCountDownFinished();
        }

        if (_countdownSubscription != null)
        {
            GUILayout.Label($"Count Down Running RemainingSeconds: {RemainSeconds}");
        }
        else
        {
            GUILayout.Label($"Count Down Idle Last Updated {RemainSeconds}");
        }

        if(GUILayout.Button("SetPosition"))
        {
            MatchModel.GetInstance().SelfPlayer.FieldPlayerController.ResetPosition();
        }
        _seTestTimes = EditorGUILayout.IntField(_seTestTimes);

        if (GUILayout.Button("TestSound"))
        {
            for(int i = 0; i < _seTestTimes; i++)
            SoundManager.Test();
        }

        _seType = (SeType)EditorGUILayout.EnumPopup(_seType);
        if(GUILayout.Button("PlaySe"))
        {
            SoundManager.PlayOneShot(_seType);
        }
    }

    private void Update()
    {
        Repaint();
    }

    private void StartCountDown()
    {
        var endTime = DateTime.UtcNow.AddSeconds(GameConstant.CountDownSec);
        double unixTimeMs = new DateTimeOffset(endTime).ToUnixTimeMilliseconds();
        
        GameStartAtTime = DateTimeOffset.FromUnixTimeMilliseconds((long)unixTimeMs).UtcDateTime;
        RemainSeconds = (GameStartAtTime - DateTime.UtcNow).TotalSeconds;
        UnityEngine.Debug.LogWarning($"Start CountDown Client {RemainSeconds}");
        _countdownSubscription = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .TakeWhile(_ => DateTime.UtcNow < GameStartAtTime)
            .Subscribe(
                onNext: _ =>
                {
                    RemainSeconds = (GameStartAtTime - DateTime.UtcNow).TotalSeconds;
                    Debug.Log($"Count Down Operating{RemainSeconds}");
                },
                onCompleted: _ =>
                {
                    OnCountDownFinished();
                }
            );
    }

    private void OnCountDownFinished()
    {
        _countdownSubscription?.Dispose();
        _countdownSubscription = null;
    }
}
