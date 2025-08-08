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
    private float _addSpeed = 0f;
    private bool _isRevertMoving = false;
    public bool IsPushing;
    public Action OnPositionUpdated = null;
    private void OnDestroy()
    {
        UnRegistry();
    }
    public void SetRevert(bool isRevertMoving)
    {
        _isRevertMoving = isRevertMoving;
    }
    public void SetSkillSpeed(float speed)
    {
        _addSpeed = speed;
    }
    public void SetAccelerate(bool accelaring)
    {
        _accelerating = accelaring;
    }
    public void SetHorizontalMove(HorizontalMoveDir horizontalMoveDir)
    {
        _horizontalMoveDir = horizontalMoveDir;
    }
    public void UnRegistry()
    {
        _accelerating = false;
        _breaking = false;
        _horizontalMoveDir = HorizontalMoveDir.None;
    }
    public override void FixedUpdateNetwork()
    {
        if(!Object.HasStateAuthority)
        {
            return;
        }

        if (_accelerating && !IsPushing)
        { 
            _currentSpeed += Runner.DeltaTime * Parameter.Acceleration;

            _currentSpeed = Mathf.Clamp(_currentSpeed, 0, Parameter.MaxSpeed + _addSpeed);
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
            var dir = _isRevertMoving ? Vector3.right : Vector3.left;
            var position = transform.position + dir * HOR_MOVE_SPEED * Runner.DeltaTime;
            position.x = Mathf.Clamp(position.x, -ROAD_WIDTH, ROAD_WIDTH);
            transform.position = position;
        }
        else if (_horizontalMoveDir == HorizontalMoveDir.Right)
        {
            var dir = _isRevertMoving ? Vector3.left : Vector3.right;
            var position = transform.position + dir * HOR_MOVE_SPEED * Runner.DeltaTime;
            position.x = Mathf.Clamp(position.x, -ROAD_WIDTH, ROAD_WIDTH);
            transform.position = position;
        }

        if (_currentSpeed > 0)
        {
            var dir = _isRevertMoving ? Vector3.back : Vector3.forward;
            transform.position = transform.position + dir * _currentSpeed * Runner.DeltaTime;

            if(OnPositionUpdated != null)
            {
                OnPositionUpdated();
            }
        }
    }
}
