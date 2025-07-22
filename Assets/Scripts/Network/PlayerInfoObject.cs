using Cysharp.Threading.Tasks;
using Fusion;
using R3;
public class PlayerInfoObject : NetworkBehaviour
{
    [Networked]       // Notifies the ILWeaver to extend this property
    [Capacity(16)]    // allocates memory for 16 characters
    [UnityMultiline]  // Optional attribute to force multi-line in inspector.
    public string PlayerName { get => default; set { } }
    [Networked] public int PlayerId { get; set; }
    [Networked] public PlayerRef PlayerRef { get; set; }

    // まぁ、たぶん使わないけど、一応
    [Networked] public PlayerEquipmentSetInfoStruct PlayerEquipment { get; set; }
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

        PlayerName = playerName;
        PlayerRef = playerRef;
        PlayerId = playerRef.PlayerId;

        return Unit.Default;
    }
    private async UniTask<Unit> RegisterRootAsync()
    {
        await UniTask.WaitUntil(() => PlayerId > 0, cancellationToken: this.destroyCancellationToken);
        PlayerRootObject.Instance.OnPlayerInfoSpawnedAndRegister(this);
        return Unit.Default;
    }
}
