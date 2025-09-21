using UnityEngine;

public class ParticleOneShot : MonoBehaviour
{
    [SerializeField] private ParticleSystem particle;
    private void Awake()
    {
        var main = particle.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
    }
}
