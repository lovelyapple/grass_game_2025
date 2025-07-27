using UnityEngine;
using Fusion;
using StarMessage.Models;

public class FieldPlayerController : NetworkBehaviour
{
    [SerializeField] Transform CharaPoint;
    [SerializeField] SpriteRenderer SaddleImage;
    private NetworkTransform _networkTransform;
    private PlayerBase _playerBase;
    public int PlayerId { get; private set; }
    public bool IsReady = false;
    private Vector3 _initPos;
    private VehicleBase _vehicle;
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
    }
}
