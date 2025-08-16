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

        SaddleTeppan.gameObject.SetActive(false);
        SaddleIce.gameObject.SetActive(false);
        SaddleLeather.gameObject.SetActive(false);
        SaddleOthers.gameObject.SetActive(false);
    }
    [SerializeField] AudioMixer GameAudioMixer;
    [SerializeField] AudioSource SaddleTeppan;
    [SerializeField] AudioSource SaddleIce;
    [SerializeField] AudioSource SaddleLeather;
    [SerializeField] AudioSource SaddleOthers;
    public static void PlaySE(AudioClip clip)
    {
        _instance.seSource.outputAudioMixerGroup = _instance.GameAudioMixer.FindMatchingGroups("SE")[0];
        _instance.seSource.PlayOneShot(clip);
    }
    public static AudioSource GetSaddleAudio(SaddleType saddleType)
    {
        switch(saddleType)
        {
            case SaddleType.Ice:
                return _instance.SaddleIce;

            case SaddleType.Leather:
                return _instance.SaddleLeather;

            default:
            case SaddleType.Iron:
                return _instance.SaddleTeppan;
        }
    }

    [SerializeField] AudioClip test;
    [ContextMenu("Test")]
    public static void Test()
    {
        PlaySE(_instance.test);
    }
}
