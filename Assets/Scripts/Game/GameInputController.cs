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
    private readonly ReactiveProperty<HorizontalMoveDir> _moveDir = new ReactiveProperty<HorizontalMoveDir>();
    public Observable<HorizontalMoveDir> HorizontalMovingObservable() => _moveDir;
    private readonly Subject<Unit> _useClickUseSkillSubject = new Subject<Unit>();
    public Observable<Unit> OnClickUseSkillKeySujbect() => _useClickUseSkillSubject;
    private readonly Subject<Unit> _useSkillSubject = new Subject<Unit>();
    public Observable<Unit> UseSkillObservable() => _useSkillSubject;

    private bool _isPressingAccelerateUI = false;
    private bool _isPressingUpUI = false;
    private bool _isPressingDownUI = false;
    private void Awake()
    {
        _instance = this;
        GameUIController.IsPressingAccelerateButtonObservable()
        .Subscribe(isPressing => _isPressingAccelerateUI = isPressing)
        .AddTo(this);

        GameUIController.IsPressingUpButtonObservable()
        .Subscribe(isPressing => _isPressingUpUI = isPressing)
        .AddTo(this);

        GameUIController.IsPressingDownButtonObservable()
        .Subscribe(isPressing => _isPressingDownUI = isPressing)
        .AddTo(this);

        Observable.Merge(GameUIController.OnClickUseSkillButtonObservable(), OnClickUseSkillKeySujbect())
        .Subscribe(_ => _useSkillSubject.OnNext(Unit.Default))
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

        if (Input.GetKey(KeyCode.UpArrow) || _isPressingUpUI)
        {
            _moveDir.OnNext(HorizontalMoveDir.Right);
        }
        else if (Input.GetKey(KeyCode.DownArrow) || _isPressingDownUI)
        {
            _moveDir.OnNext(HorizontalMoveDir.Left);
        }
        else
        {
            _moveDir.OnNext(HorizontalMoveDir.None);
        }

        if(Input.GetKeyDown(KeyCode.Return))
        {
            _useSkillSubject.OnNext(Unit.Default);
        }
    }
}
