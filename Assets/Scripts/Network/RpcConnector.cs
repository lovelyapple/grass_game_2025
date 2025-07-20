using Fusion;
using R3;
using UnityEngine;

public class RpcConnector : NetworkBehaviour
{
    private static RpcConnector _instance;
    public static RpcConnector Instance{
        get{
            if(_instance == null)
            {
                _instance = GameObject.FindFirstObjectByType<RpcConnector>();
            }
            return _instance;
        }
    }
    private readonly Subject<EquipmentSetInfo> _onBroadcastEquipmentSaveSubject = new Subject<EquipmentSetInfo>();
    public Observable<EquipmentSetInfo> BroadcastEquipmentSaveObservable() => _onBroadcastEquipmentSaveSubject;
    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_BroadcastEquipmentSave(int playerId, Characters chara, SaddleType saddle, Vehicles vehicle)
    {
        var info = new EquipmentSetInfo()
        {
            PlayerId = playerId,
            Character = chara,
            Saddle = saddle,
            Vehicle = vehicle,
        };

        _onBroadcastEquipmentSaveSubject.OnNext(info);
    }
}
