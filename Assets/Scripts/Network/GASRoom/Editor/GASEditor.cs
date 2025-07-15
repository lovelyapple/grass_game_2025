using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class GASEditor : EditorWindow
{
    [MenuItem("Debug/GAS")]
    public static void Open()
    {
        GetWindow<GASEditor>();
    }
    public void OnGUI()
    {
        if(GUILayout.Button("Request LIST "))
        {
            RoomService.FetchRoomList(new System.Threading.CancellationToken()).Forget();
        }

        if(GUILayout.Button("Update"))
        {
            RoomService.UpdateRoom(new RoomInfo() { room_name = "room1", player_count = 3, status = "playing" }, new System.Threading.CancellationToken()).Forget();
        }
    }
}
