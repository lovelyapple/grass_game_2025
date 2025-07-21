using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using Unity.Mathematics;
using UnityEngine;
public struct PlayerBaseInfoStruct : INetworkStruct
{
    [Networked]       // Notifies the ILWeaver to extend this property
    [Capacity(16)]    // allocates memory for 16 characters
    [UnityMultiline]  // Optional attribute to force multi-line in inspector.
    public string PlayerName { get => default; set { } }
    public int PlayerId;
}
public class PlayerInfoObject : NetworkBehaviour
{
    [Networked] public PlayerBaseInfoStruct BaseInfoStruct{ get; set; }
    public NetworkString<_16> Name;
    [Networked] public PlayerRef PlayerRef { get; set; }

    // まぁ、たぶん使わないけど、一応
    [Networked] public PlayerEquipmentSetInfoStruct PlayerEquipment { get; set; }

    private NetworkString<_32> _requestName = "";
    private bool _isSpawned = false;
    public override void Spawned()
    {
        base.Spawned();
        _isSpawned = true;
        RegisterRootAsync().Forget();
    }
    public async UniTask<Unit> InitializeAsync(string playerName, PlayerRef playerRef)
    {
        await UniTask.WaitUntil(() => _isSpawned, cancellationToken: this.destroyCancellationToken);

        _requestName = new NetworkString<_32>(playerName);

        BaseInfoStruct = new PlayerBaseInfoStruct()
        {
            PlayerName = playerName,
            PlayerId = playerRef.PlayerId,
        };

         PlayerRef = playerRef;

        return Unit.Default;
    }
    public void UpdateEquipment(EquipmentSetInfo setInfo)
    {
        PlayerEquipment = setInfo.ToStruct();
    }

    private async UniTask<Unit> RegisterRootAsync()
    {
        await UniTask.WaitUntil(() => BaseInfoStruct.PlayerId > 0, cancellationToken: this.destroyCancellationToken);
        PlayerRootObject.Instance.OnPlayerInfoSpawned(this);
        return Unit.Default;
    }
}
