using System;
using R3;
using UnityEngine;
public class PlayerBase : MonoBehaviour
{
    [SerializeField] SpriteRenderer StandingSprite;
    [SerializeField] SpriteRenderer DrivingSprite;

    public void SetDriving(bool isDriving)
    {
        StandingSprite.enabled = !isDriving;
        DrivingSprite.enabled = isDriving;
    }
}
