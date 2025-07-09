using System;
using R3;
using UnityEngine;

public class GameInputController : MonoBehaviour
{
    // private readonly Subject<bool> _onPushAccelerate = new Subject<bool>();
    // public Observable<bool> OnPushAccelerateAsObservable() => _onPushAccelerate;
    private static GameInputController _instance;

    public static GameInputController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameInputController>(findObjectsInactive: FindObjectsInactive.Include);
            }

            return _instance;
        }
    }

    public bool IsPushingAccelerate;

    public HorizontalMoveDir MoveDir;
    private void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            IsPushingAccelerate = true;
        }
        else
        {
            IsPushingAccelerate = false;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveDir = HorizontalMoveDir.Right;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveDir = HorizontalMoveDir.Left;
        }
        else
        {
            MoveDir = HorizontalMoveDir.None;
        }
    }
}
