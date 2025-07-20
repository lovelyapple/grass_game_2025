using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Networking;
[Serializable]
public class RoomListInfo
{
    public List<RoomInfo> rooms;
}
[Serializable]
public class RoomInfo
{
    public string room_name;
    public int player_count;
    public string status;
    public RoomInfo ( string roomName, int playerCount, string st)
    {
        room_name = roomName;
        player_count = playerCount;
        status = st;
    }
}
public class RoomService
{
    private const string APP_TOKEN = "5e0b7d4ef7fc31e6c691a92acf5806b742df3ded";
    private const string WEB_API_URL = "https://script.google.com/macros/s/AKfycbwkAQnqofSp6f9oi9vp33lBYgQvH5yjdLbKcpRt2swXBbqEab2AtaAzljMoK5xgsOIS3Q/exec";
    public static async UniTask<List<RoomInfo>> FetchRoomList(CancellationToken token)
    {
        string url = $"{WEB_API_URL}?token={APP_TOKEN}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        var operation = await request.SendWebRequest().ToUniTask(cancellationToken: token);

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"FetchRoomList error {request.error}");
            return null;
        }

        var json = request.downloadHandler.text;
        var roomsInfo = JsonUtility.FromJson<RoomListInfo>(json);
      
        return roomsInfo.rooms;
    }
    public static async UniTask<Unit> UpdateRoom(RoomInfo roomInfo, CancellationToken token)
    {
        string url = $"{WEB_API_URL}?token={APP_TOKEN}";
        var json = JsonUtility.ToJson(roomInfo);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = await request.SendWebRequest().ToUniTask(cancellationToken: token);

        if (operation.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"FetchRoomList error {request.error}");
            return default;
        }

        return Unit.Default;
    }
}
