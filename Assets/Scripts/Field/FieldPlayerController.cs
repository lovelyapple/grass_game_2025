using UnityEngine;
using Fusion;
using R3;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;
public class SpecialPoint
{
    private const float MaxPoint = 200;
    public float CurrentPoint;
    public bool IsMax => CurrentPoint >= MaxPoint;
    public float Rate => CurrentPoint / MaxPoint;
    public void AddPoint(float point)
    {
        CurrentPoint = Mathf.Min(CurrentPoint + point, MaxPoint);
    }
    public void Reset()
    {
        CurrentPoint = 0;
    }
}
public class HealthPoint
{
    private const float MaxPoint = 300;
    public float TotalPoint = MaxPoint;
    public float CurrentPoint = MaxPoint;
    public bool IsMax => CurrentPoint >= TotalPoint;
    public float Rate => CurrentPoint / TotalPoint;
    public void Init(float appendHP)
    {
        TotalPoint = MaxPoint + appendHP;
        CurrentPoint = TotalPoint;
    }
    public void Decrease(float point)
    {
        CurrentPoint = Mathf.Max(CurrentPoint - point, 0);
    }
    public void AddPoint(float point)
    {
        CurrentPoint = Mathf.Min(CurrentPoint + point, TotalPoint);
    }
}

public class FieldPlayerController : NetworkBehaviour
{
    [SerializeField] Transform CharaPoint;
    [SerializeField] Transform LandingTransform;
    [SerializeField] SpriteRenderer SaddleImage;
    [SerializeField] StatusEffectView StatusEffectView;
    [SerializeField] Transform DownPoint;

    private NetworkTransform _networkTransform;
    private PlayerBase _playerBase;
    public int PlayerId { get; private set; }
    public bool IsReady = false;
    public bool IsFinished = false;
    private Vector3 _initPos;
    private VehicleBase _vehicle;
    private float _saddleHeatRate;
    private float _appendHp;
    private SpecialPoint _specialPoint = new SpecialPoint();
    public HealthPoint HealthPoint = new HealthPoint();
    public SkillBase SkillBase{ get; private set; }
    private bool _isPlayerDriving = false;
    private bool _forceDriving = false;
    private bool _recovering = false;
    private Subject<FieldPlayerController> _onZPosUpdated = new Subject<FieldPlayerController>();
    public Observable<FieldPlayerController> OnZPosUpdatedObservable() => _onZPosUpdated;
    private Subject<IStatusEffect> _onStatusEffectExecute = new Subject<IStatusEffect>();
    public Observable<IStatusEffect> OnStatusEffectExecuteObservable() => _onStatusEffectExecute;
    private IStatusEffect _iCurrentStatueEffect = null;
    private CompositeDisposable _inputDisposables = new();
    private const float HPRECOVER_SELF_RATE = 15f;
    private const float HP_RECOVER_RATE_FROM_EMPTY = 50;
    private SaddleType _saddleType;
    private AudioSource _saddleSeCache = null;
    private AudioListener _audioListener = null;
    private void Awake()
    {
        _networkTransform = GetComponent<NetworkTransform>();
        MatchModel.GetInstance().OnMatchFinishedObservable()
        .Where(_ => _saddleSeCache != null)
        .Subscribe(_ => {
            _saddleSeCache.gameObject.SetActive(false);
         })
        .AddTo(this);
    }
    public override void Spawned()
    {
        base.Spawned();
        PlayerId = Object.StateAuthority.PlayerId;
        Debug.Log($"player {PlayerId} FieldPlayerController Spawned");

        var obj = PlayerRootObject.Instance.GetPlayerInfoObject(PlayerId);
        _saddleType = (SaddleType)obj.PlayerEquipment.SaddleType;
        var chara = obj.PlayerEquipment.Character;

        var driverPrefab = ResourceContainer.Instance.GetCharacterPrefab((Characters)chara);
        _playerBase = Instantiate(driverPrefab, CharaPoint).GetComponent<PlayerBase>();

        SkillBase = _playerBase.GetComponent<SkillBase>();
        SkillBase.Init(this);

        SaddleImage.sprite = ResourceContainer.Instance.GetSaddleImage(_saddleType, true);
        _vehicle = GetComponent<VehicleBase>();
        IsReady = true;

        _saddleHeatRate = ParameterHolder.Instance.SaddleParameters.FirstOrDefault(x => x.Type == _saddleType).HeatRate;
        _appendHp = ParameterHolder.Instance.CharaParameters.FirstOrDefault(x => x.Type == (Characters)chara).AppendHP;
        HealthPoint.Init(_appendHp);

        MatchModel.GetInstance().OnFieldPlayerControllerSpawned(this);
        _saddleSeCache = SoundManager.GetSaddleAudio(_saddleType);
        _saddleSeCache.transform.SetParent(this.transform);

        if(PlayerId != RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            var source = _saddleSeCache.GetComponent<AudioSource>();
            source.spatialBlend = 0.92f;
            source.volume = 1.0f;
        }
    }
    public void SetupInitPos(Vector3 pos)
    {
        _initPos = pos;
        // transform.position = pos;
        _networkTransform.Teleport(pos);
    }
    public void ResetPosition()
    {
        if (Object.HasStateAuthority)
        {
            // transform.position = _initPos;
            _networkTransform.Teleport(_initPos);
        }
    }
    public void RegistInput()
    {
        _vehicle.OnPositionUpdated = () =>
        {
            if(!IsFinished)
            {
                _onZPosUpdated.OnNext(this);
            }
        };

        _inputDisposables = new CompositeDisposable();
        var inputController  = GameInputController.Instance;

        inputController.IsAcceleratingObservable()
        .Where(_ => _vehicle != null && !_recovering)
        .Subscribe(x => PlayerChangeDrive(x))
        .AddTo(_inputDisposables);

        inputController.HorizontalMovingObservable()
        .Where(_ => _vehicle != null && !_recovering)
        .Subscribe(x => PlayerSetHorizontal(x))
        .AddTo(_inputDisposables);

        inputController.UseSkillObservable()
        .Where(_ => !_recovering && _specialPoint.IsMax)
        .Subscribe(x => PlayerOnInputUseSkill())
        .AddTo(_inputDisposables);

        MatchCameraController.Instance.SwitchPlayerLisitener(true);
        _audioListener = gameObject.AddComponent<AudioListener>();
    }
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || !IsReady || RoomStateController.Instance == null || RoomStateController.Instance.CurrentRoomPhase != (int)RoomPhase.Playing)
        {
            return;
        }

        if (_isPlayerDriving && !_forceDriving && HealthPoint.CurrentPoint > 0 && !_recovering)
        {
            HealthPoint.Decrease(_saddleHeatRate * Runner.DeltaTime);
            _specialPoint.AddPoint(_saddleHeatRate * Runner.DeltaTime);
            MatchModel.GetInstance().UpdateHeatAndSepcialPoint(_specialPoint, HealthPoint);

            if(HealthPoint.CurrentPoint <= 0)
            {
                ForceBreak();
                RecoverHealthAll().Forget();
            }
        }
        else if (!_recovering)
        {
            HealthPoint.AddPoint(HPRECOVER_SELF_RATE * Runner.DeltaTime);
            MatchModel.GetInstance().UpdateHeatAndSepcialPoint(_specialPoint, HealthPoint);
        }
    }

    #region  player_input
    private void PlayerChangeDrive(bool accelaring)
    {
        if (_isPlayerDriving == accelaring)
        {
            return;
        }

        _isPlayerDriving = accelaring;
        _vehicle.SetAccelerate(accelaring || _forceDriving);
        RpcConnector.Instance.Rpc_OnPlayerJumpInOut(this.PlayerId, _isPlayerDriving || _forceDriving);
    }
    private void ForceBreak()
    {
        _isPlayerDriving = false;
        _forceDriving = false;
        _vehicle.SetAccelerate(_isPlayerDriving || _forceDriving);
        RpcConnector.Instance.Rpc_OnPlayerJumpInOut(this.PlayerId, _isPlayerDriving || _forceDriving);
    }
    private void PlayerSetFixDriving(bool forceDriving)
    {
        _forceDriving = forceDriving;
        _vehicle.SetAccelerate(_forceDriving);
        RpcConnector.Instance.Rpc_OnPlayerJumpInOut(this.PlayerId, _forceDriving);
    }
    private void PlayerSetHorizontal(HorizontalMoveDir horizontalMoveDir)
    {
        _vehicle.SetHorizontalMove(horizontalMoveDir);
    }
    private void PlayerOnInputUseSkill()
    {
        PlayerSkillLocalAsync().Forget();
    }
    private async UniTask<Unit> PlayerSkillLocalAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        _specialPoint.Reset();
        MatchModel.GetInstance().UpdateHeatAndSepcialPoint(_specialPoint, HealthPoint);

        try
        {
            RpcConnector.Instance.Rpc_BroadcastOnPlayerUseSkill(PlayerId);
            await UniTask.WaitUntil(() => SkillBase.PlayingSkill, cancellationToken: token);

            if(SkillBase is SkillOfficeWorker)
            {
                _vehicle.SetSkillSpeed(5);
                PlayerSetFixDriving(true);
                var task1 = UniTask.WaitForSeconds(SkillBase.SkillDuration());
                var task2 = UniTask.WaitUntil(() => _iCurrentStatueEffect != null);
                await UniTask.WhenAny(task1, task2);
                PlayerSetFixDriving(false);
            }
            else if(SkillBase is SkillJK)
            {
                await UniTask.WaitForSeconds(SkillBase.SkillDuration());

                MatchModel.GetInstance().OnSelfUseStatusEffectSkill((int)StatusEffectType.DirectionRevert);
            }
            else if (SkillBase is SkillSumo)
            {
                await UniTask.WaitForSeconds(SkillBase.SkillDuration());

                MatchModel.GetInstance().OnSelfUseStatusEffectSkill((int)StatusEffectType.Stun);
            }

            RpcConnector.Instance.Rpc_BroadcastOnPlayerFinishSkill(PlayerId);
        }
        catch(Exception e)
        {
            throw e;
        }
        finally
        {
            _vehicle.SetSkillSpeed(0);
        }

        return Unit.Default;
    }


    private async UniTask<Unit> RecoverHealthAll()
    {
        _recovering = true;
        var token = new CancellationTokenSource().Token;
        try
        {
            while (!HealthPoint.IsMax)
            {
                token.ThrowIfCancellationRequested();

                HealthPoint.AddPoint(HP_RECOVER_RATE_FROM_EMPTY * Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, token); // 毎フレーム待つ
                MatchModel.GetInstance().UpdateHeatAndSepcialPoint(_specialPoint, HealthPoint);
            }
        }
        finally
        {
            _recovering = false;
        }

        return Unit.Default;
    }
    #endregion
    public void ReleaseController()
    {
        _vehicle.OnPositionUpdated = null;
        _vehicle.UnRegistry();
        _inputDisposables?.Dispose();
        _inputDisposables = null;

        if (_audioListener != null)
        {
            _audioListener.enabled = false;
            Destroy(_audioListener);
        }
    }

    public void OnReceivedJumpInOut(bool jumdIn) 
    {
        if (jumdIn)
        {
            _playerBase.transform.SetParent(CharaPoint);
            _playerBase.transform.localPosition = Vector3.zero;
            _saddleSeCache?.gameObject.SetActive(true);
        }
        else
        {
            _playerBase.transform.SetParent(LandingTransform);
            _playerBase.transform.localPosition = Vector3.zero;
            _saddleSeCache?.gameObject.SetActive(false);
        }

        _vehicle.IsPushing = !jumdIn;
    }
    public void OnReceivedUseSkill()
    {
        Debug.Log("Use skill");
        SkillBase.UseSkill();
    }
    public void OnReceivedFinishSkill()
    {
        SkillBase.FinishPlaySkill();

        if (_saddleSeCache != null)
        {
            _saddleSeCache?.Stop();
            _saddleSeCache?.gameObject.SetActive(false);
        }
    }
    public void OnReceivedStatusEffect(int statusEffectType)
    {
        if(_iCurrentStatueEffect != null || !_isPlayerDriving)
        {
            Debug.Log($"すでにStatusEffectがかかっているか、運転していないため、スキップ");
            return;
        }

        switch((StatusEffectType)statusEffectType)
        {
            case StatusEffectType.DirectionRevert:
                _iCurrentStatueEffect = new StatusEffectMoveRevert();
                _vehicle.SetRevert(true);
                StatusEffectView.SetImage(StatusEffectType.DirectionRevert);
                _iCurrentStatueEffect.OnExecute(this.GetCancellationTokenOnDestroy(), () =>
                {
                    _vehicle.SetRevert(false);
                    StatusEffectView.TurnOff();
                    _iCurrentStatueEffect = null;
                });
                break;

            case StatusEffectType.Stun:
                _iCurrentStatueEffect = new StatusEffectStun();
                _vehicle.SetStun(true);
                StatusEffectView.SetImage(StatusEffectType.Stun);
                _playerBase.transform.SetParent(DownPoint);
                _playerBase.transform.localPosition = Vector3.zero;
                _playerBase.transform.localEulerAngles = Vector3.zero;
                MatchCameraController.Instance.ShakeCamera();
                _iCurrentStatueEffect.OnExecute(this.GetCancellationTokenOnDestroy(), () =>
                {
                    _vehicle.SetStun(false);
                    _iCurrentStatueEffect = null;
                    StatusEffectView.TurnOff();

                    if (_isPlayerDriving)
                    {
                        _playerBase.transform.SetParent(CharaPoint);
                        _playerBase.transform.localEulerAngles = Vector3.zero;
                        _playerBase.transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        _playerBase.transform.SetParent(LandingTransform);
                        _playerBase.transform.localEulerAngles = Vector3.zero;
                        _playerBase.transform.localPosition = Vector3.zero;
                    }
                });
                break;
        }

        _onStatusEffectExecute.OnNext(_iCurrentStatueEffect);
    }
    public GameObject GetCharaObj()
    {
        return _playerBase.gameObject;
    }
}
