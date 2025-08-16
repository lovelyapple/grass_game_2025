using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public AudioSource seSource;
    public void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [SerializeField] AudioMixer GameAudioMixer;
    [SerializeField] AudioClip test;
    [SerializeField] int Times = 1;
    [ContextMenu("Test")]
    public static void Test()
    {
        for(int i = 0; i < _instance.Times; i++)
        {
            PlaySE(_instance.test);
        }
    }
    public static void PlaySE(AudioClip clip)
    {
        _instance.seSource.outputAudioMixerGroup = _instance.GameAudioMixer.FindMatchingGroups("SE")[0];
        _instance.seSource.PlayOneShot(clip);
    }
}
