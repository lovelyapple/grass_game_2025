using System;
using Fusion;
using R3;
using UnityEngine;

[Serializable]
public class VehicleParameter
{
    public string Name;
    public float MaxSpeed;
    public float Acceleration;
}
public class VehicleBase : NetworkBehaviour
{
    public Transform SaddleTransform;
    [SerializeField] VehicleParameter Parameter;

    public float _currentSpeed;
    public bool _accelerating;
    public HorizontalMoveDir _horizontalMoveDir;
    private bool _breaking;
    private const float FRICTION = 0.95f;
    private const float BREAK = 0.90f;
    private const float MIN_SPEED = 0.001f;
    private const float HOR_MOVE_SPEED = 2f;
    private const float ROAD_WIDTH = 9f;
    public void Registry(GameInputController inputController)
    {
        inputController.IsAcceleratingObservable()
        .Subscribe(x => _accelerating = x)
        .AddTo(this);
    }
    public override void FixedUpdateNetwork()
    {
        // Share Modeでは、HasInputAuthorityはTrueになる
        if (!Object.HasStateAuthority)
        {
            return;
        }

        // _accelerating = GameInputController.Instance._isAccelerating;
        _horizontalMoveDir = GameInputController.Instance.MoveDir;

        if (_accelerating)
        {
            _currentSpeed += Runner.DeltaTime * Parameter.Acceleration;

            _currentSpeed = Mathf.Clamp(_currentSpeed, 0, Parameter.MaxSpeed);
        }
        else if (_breaking)
        {
            _currentSpeed *= FRICTION;
            _currentSpeed *= BREAK;

            if (_currentSpeed <= MIN_SPEED)
            {
                _currentSpeed = 0f;
            }
        }
        else if (_currentSpeed > 0)
        {
            _currentSpeed *= FRICTION;

            if (_currentSpeed <= MIN_SPEED)
            {
                _currentSpeed = 0f;
            }
        }

        if (_horizontalMoveDir == HorizontalMoveDir.Left)
        {
            var position = transform.position + Vector3.left * HOR_MOVE_SPEED * Runner.DeltaTime;
            position.x = Mathf.Clamp(position.x, -ROAD_WIDTH, ROAD_WIDTH);
            transform.position = position;
        }
        else if (_horizontalMoveDir == HorizontalMoveDir.Right)
        {
            var position = transform.position + Vector3.right * HOR_MOVE_SPEED * Runner.DeltaTime;
            position.x = Mathf.Clamp(position.x, -ROAD_WIDTH, ROAD_WIDTH);
            transform.position = position;
        }

        if (_currentSpeed > 0)
        {
            transform.position = transform.position + Vector3.forward * _currentSpeed * Runner.DeltaTime;
        }
    }
}
