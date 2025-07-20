using System;
using R3;
using UnityEngine;
[Serializable]
public class DriverParameter
{
    public string DriverName;
}
public class PlayerBase : MonoBehaviour
{
    [SerializeField] DriverParameter Parameter;
    private VehicleBase _myVehicle;
    private Transform _vehicleChairTransform;
    private bool _driving;
    private Subject<PlayerBase> _onPlayerDrivingMovingSubject = new Subject<PlayerBase>();
    public Observable<PlayerBase> OnPlayerDrivingMovingAsObservable() => _onPlayerDrivingMovingSubject;
    public void DebugSetVehicle(VehicleBase vehicleBase)
    {
        _driving = true;
        _myVehicle = vehicleBase;
        _vehicleChairTransform = _myVehicle.SaddleTransform;
    }
    // Update is called once per frame
    void Update()
    {
        if (_driving && _vehicleChairTransform != null)
        {
            if (transform.position != _vehicleChairTransform.position)
            {
                transform.position = _vehicleChairTransform.position;
                _onPlayerDrivingMovingSubject.OnNext(this);
            }
        }
    }
}
