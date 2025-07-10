using UnityEngine;

public class RacingModel : SingletonBase<RacingModel>
{
    private VehicleBase _debugVehicle;
    private PlayerBase _debugPlayer;
    public PlayerBase DebugInitPlayerAndVehicle()
    {
        // ビークル　と　Player　のPrefabを渡して作る

        if (_debugVehicle == null)
        {
            _debugVehicle = GameObject.FindAnyObjectByType<VehicleBase>();
        }

        if (_debugPlayer == null)
        {
            _debugPlayer = GameObject.FindAnyObjectByType<PlayerBase>();
        }

        _debugPlayer.DebugSetVehicle(_debugVehicle);

        return _debugPlayer;
    }
}
