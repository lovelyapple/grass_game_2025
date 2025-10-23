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
    public const float MaxPoint = 300;
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
    [SerializeField] ParticleSystem SaddleFX;

    private NetworkTransform _networkTransform;
    private PlayerBase _playerBase;
    private VehicleBase _vehicle;
    public int PlayerId { get; private set; }
    public bool IsReady = false;
    public bool IsFinished = false;
    private Vector3 _initPos;
    private float _maxSpeed;
    private float _acceleration;
    private float _saddleHeatRate;
    private float _appendHp;
    private float _hpSecoverSpeed;
    private SpecialPoint _specialPoint = new SpecialPoint();
    public HealthPoint HealthPoint = new HealthPoint();
    public SkillBase SkillBase { get; private set; }
    private bool _isPlayerDriving = false;
    private bool _isBacking = false;
    private bool _forceDriving = false;
    private bool _recovering = false;
    private Subject<FieldPlayerController> _onZPosUpdated = new Subject<FieldPlayerController>();
    public Observable<FieldPlayerController> OnZPosUpdatedObservable() => _onZPosUpdated;
    private Subject<IStatusEffect> _onStatusEffectExecute = new Subject<IStatusEffect>();
    public Observable<IStatusEffect> OnStatusEffectExecuteObservable() => _onStatusEffectExecute;
    private IStatusEffect _iCurrentStatueEffect = null;
    private bool _isStuning => _iCurrentStatueEffect != null && _iCurrentStatueEffect is StatusEffectStun;
    private CompositeDisposable _inputDisposables = new();
    private const float HPRECOVER_SELF_RATE = 25f;
    private const float HP_RECOVER_RATE_FROM_EMPTY = 50;
    private Characters _characterType;
    private SaddleType _saddleType;
    private Vehicles _vehicleType;
    private AudioSource _saddleSeCache = null;
    private AudioListener _audioListener = null;
    public float SkillSheildTimeLeft = 0f;
    private const float Skill_Shield_Time = 0.5f;
    [SerializeField] private Animator DodgeAnim;
    private void Awake()
    {
        _networkTransform = GetComponent<NetworkTransform>();
        MatchModel.GetInstance().OnMatchFinishedObservable()
        .Where(_ => _saddleSeCache != null)
        .Subscribe(_ =>
        {
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
        _characterType = (Characters)obj.PlayerEquipment.Character;
        _vehicleType = (Vehicles)obj.PlayerEquipment.Vehicle;

        var driverPrefab = ResourceContainer.Instance.GetCharacterPrefab(_characterType);
        _playerBase = Instantiate(driverPrefab, CharaPoint).GetComponent<PlayerBase>();

        SkillBase = _playerBase.GetComponent<SkillBase>();
        SkillBase.Init(this);

        SaddleImage.sprite = ResourceContainer.Instance.GetSaddleImage(_saddleType, true);
        _vehicle = GetComponent<VehicleBase>();
        IsReady = true;

        var vechielParameter = ParameterHolder.Instance.VehcileParameters.FirstOrDefault(x => x.Type == (Vehicles)obj.PlayerEquipment.Vehicle);
        _maxSpeed = vechielParameter.MaxSpeed;
        _acceleration = vechielParameter.Acceleration;
        _saddleHeatRate = ParameterHolder.Instance.SaddleParameters.FirstOrDefault(x => x.Type == _saddleType).HeatRate;
        _appendHp = ParameterHolder.Instance.CharaParameters.FirstOrDefault(x => x.Type == _characterType).AppendHP;
        _hpSecoverSpeed = ParameterHolder.Instance.CharaParameters.FirstOrDefault(x => x.Type == _characterType).RecoverSpeed;

        _vehicle.MaxSpeed = _maxSpeed;
        _vehicle.Acceleration = _acceleration;

        HealthPoint.Init(_appendHp);

        MatchModel.GetInstance().OnFieldPlayerControllerSpawned(this);
        _saddleSeCache = SoundManager.GetSaddleAudio(_saddleType);
        _saddleSeCache.transform.SetParent(this.transform);

        if (PlayerId != RoomModel.GetInstance().SelfPlayerRef.PlayerId)
        {
            var source = _saddleSeCache.GetComponent<AudioSource>();
            source.spatialBlend = 0.92f;
            source.volume = 1.0f;
        }

        SetSaddleFXActive(false);
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
            if (!IsFinished)
            {
                _onZPosUpdated.OnNext(this);
            }
        };

        _inputDisposables = new CompositeDisposable();
        var inputController = GameInputController.Instance;

        var canBack = _vehicleType == Vehicles.Unicycle;
        inputController.SetCanBack(canBack);

        if(canBack)
        {
            inputController.IsAcceleratingObservable()
            .Where(_ => _vehicle != null && !_recovering && !_isStuning && MatchModel.GetInstance().CountDownFinished == true)
            .Subscribe(x => PlayerChangeDrive(x))
            .AddTo(_inputDisposables);
        }
        else
        {
            inputController.IsAcceleratingObservable()
            .Where(_ => _vehicle != null && !_recovering && !_isStuning && MatchModel.GetInstance().CountDownFinished == true)
            .Where(dir => dir >= 0)
            .Subscribe(x => PlayerChangeDrive(x))
            .AddTo(_inputDisposables);
        }

        inputController.HorizontalMovingObservable()
        .Where(_ => _vehicle != null && !_recovering && !_isStuning)
        .Subscribe(x => PlayerSetHorizontal(x))
        .AddTo(_inputDisposables);

        inputController.UseSkillObservable()
        .Where(_ => !_recovering && _specialPoint.IsMax && !_isStuning)
        .Subscribe(x => PlayerOnInputUseSkill())
        .AddTo(_inputDisposables);

        MatchCameraController.Instance.SwitchPlayerLisitener(true);
        _audioListener = gameObject.AddComponent<AudioListener>();
    }
    private bool _isHittingPrev = false;
    public override void FixedUpdateNetwork()
    {
        if (SkillSheildTimeLeft > 0)
        {
            SkillSheildTimeLeft -= Runner.DeltaTime;
        }

        if (!Object.HasStateAuthority ||
        !IsReady ||
        RoomStateController.Instance == null ||
        RoomStateController.Instance.CurrentRoomPhase != (int)RoomPhase.Playing)
        {
            return;
        }

        if (_isPlayerDriving && !_forceDriving && HealthPoint.CurrentPoint > 0 && !_recovering)
        {
            var healthDecrease = _saddleHeatRate * Runner.DeltaTime;

            if (!_isHittingPrev)
            {
                healthDecrease *= 20f;

                MatchModel.GetInstance().RequestPlayVoice(_characterType, false);
            }

            HealthPoint.Decrease(healthDecrease);
            _specialPoint.AddPoint(_saddleHeatRate * Runner.DeltaTime);
            MatchModel.GetInstance().UpdateHeatAndSepcialPoint(_specialPoint, HealthPoint);
            _isHittingPrev = true;

            if (HealthPoint.CurrentPoint <= 0)
            {
                ForceBreak();
                RecoverHealthAll().Forget();
                _isHittingPrev = false;
            }
        }
        else if (!_recovering)
        {
            if (_isHittingPrev)
            {
                SkillSheildTimeLeft = Skill_Shield_Time;
            }

            _isHittingPrev = false;
            HealthPoint.AddPoint(HPRECOVER_SELF_RATE * _hpSecoverSpeed * Runner.DeltaTime);
            MatchModel.GetInstance().UpdateHeatAndSepcialPoint(_specialPoint, HealthPoint);
        }
        else
        {
            _isHittingPrev = false;
        }
    }

    #region  player_input
    private void PlayerChangeDrive(int accelaringDir)
    {
        var accelaring = accelaringDir != 0;
        if (_isPlayerDriving == accelaring)
        {
            return;
        }

        var dir = 0;

        if (_forceDriving || accelaringDir > 0)
        {
            dir = 1;
        }
        else if(accelaringDir < 0)
        {
            dir = -1;
        }
        else
        {
            dir = 0;
        }

        _isPlayerDriving = accelaring;
        _vehicle.SetAccelerate(dir);
        _playerBase.SetDriving(accelaring);
        SetSaddleFXActive(accelaring);
        RpcConnector.Instance.Rpc_OnPlayerJumpInOut(this.PlayerId, _isPlayerDriving || _forceDriving);
    }
    private void ForceBreak()
    {
        _forceDriving = false;
        _isPlayerDriving = false;
        SetSaddleFXActive(false);
        _vehicle.SetAccelerate(0);
        _playerBase.SetDriving(false);
        _playerBase.transform.SetParent(LandingTransform);
        _playerBase.transform.localEulerAngles = Vector3.zero;
        _playerBase.transform.localPosition = Vector3.zero;
        RpcConnector.Instance.Rpc_OnPlayerJumpInOut(this.PlayerId, false);
        OnReceivedJumpInOut(false);
    }
    private void PlayerSetFixDriving(bool forceDriving)
    {
        _forceDriving = forceDriving;
        _vehicle.SetAccelerate(1);
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

            if (SkillBase is SkillOfficeWorker)
            {
                itemSpeedBuffTokenSource?.Cancel();
                _vehicle.SetSkillSpeed(5);
                PlayerSetFixDriving(true);
                var task1 = UniTask.WaitForSeconds(SkillBase.SkillDuration());
                var task2 = UniTask.WaitUntil(() => _iCurrentStatueEffect != null);
                await UniTask.WhenAny(task1, task2);
                PlayerSetFixDriving(false);
            }
            else if (SkillBase is SkillJK)
            {
                await UniTask.WaitForSeconds(SkillBase.SkillDuration());

                MatchModel.GetInstance().OnSelfUseStatusEffectSkill((int)StatusEffectType.DirectionRevert);
            }
            else if (SkillBase is SkillSumo)
            {
                await UniTask.WaitForSeconds(SkillBase.SkillDuration());

                MatchModel.GetInstance().OnSelfUseStatusEffectSkill((int)StatusEffectType.Stun);
            }

            SoundManager.PlayerDrivingSPVoice(_characterType);

            RpcConnector.Instance.Rpc_BroadcastOnPlayerFinishSkill(PlayerId);
        }
        catch (Exception e)
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
            MatchModel.GetInstance().PlayHPAlert(true);
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
            MatchModel.GetInstance().PlayHPAlert(false);
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
    private void SetSaddleFXActive(bool active)
    {
        if (SaddleFX != null)
        {
            var mainF = SaddleFX.main;
            mainF.startColor = active ? Color.yellow : Color.black;
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
    public void OnReceivedStatusEffect(int statusEffectType, bool ignoreNotDrive, bool isUserSkill)
    {
        if (_iCurrentStatueEffect != null)
        {
            Debug.Log("すでにStatusEffectがかかっているかスキップ");
            return;
        }

        if (!_isPlayerDriving)
        {
            if (!ignoreNotDrive)
            {
                if (isUserSkill && SkillSheildTimeLeft > 0)
                {
                    Debug.Log("運転していないため、スキップ");
                    PlayDodgeAnim();
                    return;
                }
            }
        }

        switch ((StatusEffectType)statusEffectType)
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
                SetSaddleFXActive(false);
                _vehicle.SetStun(true);
                _vehicle.SetAccelerate(0);
                _playerBase.SetDriving(false);
                StatusEffectView.SetImage(StatusEffectType.Stun);

                _isPlayerDriving = false;
                _playerBase.transform.SetParent(LandingTransform);
                _playerBase.transform.localEulerAngles = Vector3.zero;
                _playerBase.transform.localPosition = Vector3.zero;

                MatchCameraController.Instance.ShakeCamera();

                if (Object.HasStateAuthority)
                {
                    MatchModel.GetInstance().RequestPlayVoice(_characterType, true);
                }

                _iCurrentStatueEffect.OnExecute(this.GetCancellationTokenOnDestroy(), () =>
                {
                    _vehicle.SetStun(false);
                    _iCurrentStatueEffect = null;
                    StatusEffectView.TurnOff();

                    _playerBase.transform.SetParent(LandingTransform);
                    _playerBase.transform.localEulerAngles = Vector3.zero;
                    _playerBase.transform.localPosition = Vector3.zero;
                });
                break;
        }

        _onStatusEffectExecute.OnNext(_iCurrentStatueEffect);
    }
    public void OnReceivedItemEffect(int itemEfffectType)
    {
        switch ((ItemEffectType)itemEfffectType)
        {
            case ItemEffectType.Heal:
                ApplyHeal(200);
                break;
            case ItemEffectType.SpeedUp:
                {
                    if (SkillBase is not SkillOfficeWorker)
                    {
                        if (!SkillBase.PlayingSkill)
                        {
                            ItemSpeedUpAsync().Forget();
                        }
                    }
                    else
                    {
                        ItemSpeedUpAsync().Forget();
                    }

                    MatchModel.GetInstance().UpdateHeatAndSepcialPoint(_specialPoint, HealthPoint);
                    var prefab = ResourceContainer.Instance.GetItemEffectIcon(ItemEffectType.SpeedUp);
                    var instance = GameObject.Instantiate(prefab, this.transform);
                    instance.gameObject.SetActive(true);
                }
                break;

            case ItemEffectType.Jummer:
                SoundManager.PlayOneShot(seType: SeType.Se_Barrer_broke);
                OnReceivedStatusEffect((int)StatusEffectType.Stun, true, false);
                break;
        }
    }

    private void ApplyHeal(int healAmount)
    {
        HealthPoint.AddPoint(healAmount);
        SoundManager.PlayerActionVoice(_characterType);
        MatchModel.GetInstance().UpdateHeatAndSepcialPoint(_specialPoint, HealthPoint);
        var prefab = ResourceContainer.Instance.GetItemEffectIcon(ItemEffectType.Heal);
        var instance = GameObject.Instantiate(prefab, this.transform);
        instance.gameObject.SetActive(true);
    }
    CancellationTokenSource itemSpeedBuffTokenSource;
    private async UniTask<Unit> ItemSpeedUpAsync()
    {
        SoundManager.PlayerActionVoice(_characterType);
        itemSpeedBuffTokenSource?.Cancel();

        try
        {
            itemSpeedBuffTokenSource = new CancellationTokenSource();
            var token = CancellationTokenSource.CreateLinkedTokenSource(itemSpeedBuffTokenSource.Token, destroyCancellationToken).Token;
            _vehicle.SetSkillSpeed(4);//サラリーマンは5        
            await UniTask.WaitForSeconds(5, cancellationToken: token);// 適当に5秒
            _vehicle?.SetSkillSpeed(0);
        }
        finally
        {
            itemSpeedBuffTokenSource = null;
        }

        return Unit.Default;
    }
    public GameObject GetCharaObj()
    {
        return _playerBase.gameObject;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "ItemBox")
        {
            var itemBox = collision.gameObject.GetComponent<FieldItemBase>();
            RpcConnector.Instance.Rpc_BroadcastTouchItemBox(PlayerId, itemBox.ItemId, 0);
        }
    }

    private void PlayDodgeAnim()
    {
        ApplyHeal(30);
        DodgeAnim.Play("Play");
    }
}
