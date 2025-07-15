using System;
using R3;
using StarMessage.Models;
using UnityEngine;

public class GameInputController : MonoBehaviour
{
    [SerializeField] GameUIController GameUIController;
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

    private readonly ReactiveProperty<bool> _isAccelerating = new ReactiveProperty<bool>(false);
    public Observable<bool> IsAcceleratingObservable() => _isAccelerating;

    public HorizontalMoveDir MoveDir;
    private bool _isPressingAccelerateUI = false;
    private void Awake()
    {
        _instance = this;
        GameUIController.IsPressingAccelerateButtonObservable()
        .Subscribe(isPressing => _isPressingAccelerateUI = isPressing)
        .AddTo(this);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) || _isPressingAccelerateUI)
        {
            _isAccelerating.Value = true;
        }
        else
        {
            _isAccelerating.Value = false;
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
