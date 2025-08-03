using UnityEngine;
using Fusion;
using R3;

public class FieldPlayerController : NetworkBehaviour
{
    public class SpecialPoint
    {
        private const float MaxPoint = 1000;
        public float CurrentPoint;
        public bool IsMax => CurrentPoint >= MaxPoint;
    }
    public class HeatPoint
    {
        private const float MaxPoint = 1000;
        public float CurrentPoint;
        public bool IsMax => CurrentPoint >= MaxPoint;
    }
    [SerializeField] Transform CharaPoint;
    [SerializeField] Transform LandingTransform;
    [SerializeField] SpriteRenderer SaddleImage;
    private NetworkTransform _networkTransform;
    private PlayerBase _playerBase;
    public int PlayerId { get; private set; }
    public bool IsReady = false;
    public bool IsFinished = false;
    private Vector3 _initPos;
    private VehicleBase _vehicle;
    private SpecialPoint _specialPoint = new SpecialPoint();
    private HeatPoint _heatPoint = new HeatPoint();
    private bool _isDriving = true;
    private Subject<FieldPlayerController> _onZPosUpdated = new Subject<FieldPlayerController>();
    public Observable<FieldPlayerController> OnZPosUpdatedObservable() => _onZPosUpdated;
    private Subject<SpecialPoint> _specialPointChangeSubject = new Subject<SpecialPoint>();
    public Observable<SpecialPoint> SpecialPointChangeObservable() => _specialPointChangeSubject;
    private Subject<HeatPoint> _heatPointChangeSubject = new Subject<HeatPoint>();
    public Observable<HeatPoint> HeatPointChangeObservable() => _heatPointChangeSubject;
    private CompositeDisposable _inputDisposables = new();
    private void Awake()
    {
        _networkTransform = GetComponent<NetworkTransform>();
    }
    public override void Spawned()
    {
        base.Spawned();
        PlayerId = Object.StateAuthority.PlayerId;
        Debug.Log($"player {PlayerId} FieldPlayerController Spawned");

        var obj = PlayerRootObject.Instance.GetPlayerInfoObject(PlayerId);
        var saddle = obj.PlayerEquipment.SaddleType;
        var chara = obj.PlayerEquipment.Character;

        var driverPrefab = ResourceContainer.Instance.GetCharacterPrefab((Characters)chara);
        _playerBase = Instantiate(driverPrefab, CharaPoint).GetComponent<PlayerBase>();

        SaddleImage.sprite = ResourceContainer.Instance.GetSaddleImage((SaddleType)saddle, true);
        _vehicle = GetComponent<VehicleBase>();
        IsReady = true;

        MatchModel.GetInstance().OnFieldPlayerControllerSpawned(this);
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
        _vehicle.Registry();
        _vehicle.OnPositionUpdated = () =>
        {
            if(!IsFinished)
            {
                _onZPosUpdated.OnNext(this);
            }
        };

        _inputDisposables = new CompositeDisposable();
        GameInputController.Instance.OnJumpInOutObservable()
        .Subscribe(_ => ChangeDrive())
        .AddTo(_inputDisposables);
    }
    private void ChangeDrive()
    {
        _isDriving = !_isDriving;
        RpcConnector.Instance.Rpc_OnPlayerJumpInOut(this.PlayerId, _isDriving);
    }
    public void ReleaseController()
    {
        _vehicle.OnPositionUpdated = null;
        _vehicle.UnRegistry();
        _inputDisposables?.Dispose();
        _inputDisposables = null;
    }

    public void OnReceivedJumpInOut(bool jumpDown)
    {
        if (jumpDown)
        {
            _playerBase.transform.SetParent(CharaPoint);
            _playerBase.transform.localPosition = Vector3.zero;
        }
        else
        {
            _playerBase.transform.SetParent(LandingTransform);
            _playerBase.transform.localPosition = Vector3.zero;
        }

        _vehicle.IsPushing = !jumpDown;
    }
}
