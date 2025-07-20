using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using NUnit.Framework;
using StarMessage.Models;
using UnityEngine;

public class RoomPlayerTracker : MonoBehaviour, INetworkRunnerCallbacks
{
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public void Start()
    {
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        runner.AddCallbacks(this);
    }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        var isSelf = player == runner.LocalPlayer;
        
        if (isSelf)
        {
            RoomModel.GetInstance().OnSelfJoinedRoom(runner.SessionInfo.Name, player);
        }

        RoomModel.GetInstance().OnPlayerJoined(player, isSelf);
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        RoomModel.GetInstance().OnPlayerLeaved(player);
    }
}
