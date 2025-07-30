using UnityEngine;
using Fusion;
using StarMessage.Models;
using R3;

public class FieldPlayerController : NetworkBehaviour
{
    [SerializeField] Transform CharaPoint;
    [SerializeField] SpriteRenderer SaddleImage;
    private NetworkTransform _networkTransform;
    private PlayerBase _playerBase;
    public int PlayerId { get; private set; }
    public bool IsReady = false;
    public bool IsFinished = false;
    private Vector3 _initPos;
    private VehicleBase _vehicle;
    private Subject<FieldPlayerController> _onZPosUpdated = new Subject<FieldPlayerController>();
    public Observable<FieldPlayerController> OnZPosUpdatedObservable() => _onZPosUpdated;
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
    }
    public void ReleaseController()
    {
        _vehicle.OnPositionUpdated = null;
        _vehicle.UnRegistry();
    }
}
