using UnityEngine;
using Fusion;
using StarMessage.Models;

public class FieldPlayerController : NetworkBehaviour
{
    [SerializeField] Transform CharaPoint;
    [SerializeField] SpriteRenderer SaddleImage;
    private PlayerBase _playerBase;
    public int PlayerId { get; private set; }
    public bool IsReady = false;

    public override void Spawned()
    {
        base.Spawned();
        PlayerId = Object.InputAuthority.PlayerId;
        Debug.Log($"player {PlayerId} FieldPlayerController Spawned");

        var obj = PlayerRootObject.Instance.GetPlayerInfoObject(PlayerId);
        var saddle = obj.PlayerEquipment.SaddleType;
        var chara = obj.PlayerEquipment.Character;

        var driverPrefab = ResourceContainer.Instance.GetCharacterPrefab((Characters)chara);
        _playerBase = Instantiate(driverPrefab, CharaPoint).GetComponent<PlayerBase>();

        SaddleImage.sprite = ResourceContainer.Instance.GetSaddleImage((SaddleType)saddle, true);
        IsReady = true;

        MatchModel.GetInstance().OnFieldPlayerControllerSpawned(this);
    }
}
