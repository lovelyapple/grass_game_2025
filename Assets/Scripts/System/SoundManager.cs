using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Audio;
[Serializable]
public class VoiceContainer
{
    public Characters CharacterType;
    public List<AudioClip> DrivingVoice;
    public AudioClip SpVoice;
}
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

    [SerializeField] AudioSource TitleBGM;
    [SerializeField] AudioSource GameBGM;
    [SerializeField] AudioSource ResultBGM;

    [SerializeField] SeDataHolder SeHolder;
    [SerializeField] List<VoiceContainer> VoiceContainers;

    private const float DEFAULT_BGM_VOLUM = 0.1f;
    private const float LOW_BGM_VOLUM = 0.05f;
    private AudioSource _currentBgmSource;

    [SerializeField] List<SeController> SeControllers = new List<SeController>();
    [SerializeField] List<SeController> VoiceControllers = new List<SeController>();
    public static AudioMixerGroup SeMixerGroup() => _instance.GameAudioMixer.FindMatchingGroups("SE")[0];
    public static AudioMixerGroup VoiceMixerGroup() => _instance.GameAudioMixer.FindMatchingGroups("Voice")[0];
    public static void PlaySE(AudioClip clip)
    {
        _instance.seSource.outputAudioMixerGroup = _instance.GameAudioMixer.FindMatchingGroups("SE")[0];
        _instance.seSource.PlayOneShot(clip);
    }
    public static void PlayOneShot(SeType seType)
    {
        _instance.PlayerOneShot(seType);
    }
    private void PlayerOneShot(SeType seType)
    {
        var seClip = SeHolder.GetSeData(seType);

        var emptyCtrl = SeControllers.FirstOrDefault(x => x.IsEmpty);

        if(emptyCtrl == null)
        {
            Debug.LogWarning($"there is no empty source");
            return;
        }

        emptyCtrl.PlaySe(seClip);
    }
    public static SeController PlayerDrivingVoiceRandom(Characters characters)
    {
        return _instance.PlayerVoiceOneShot(characters);
    }
    private SeController PlayerVoiceOneShot(Characters characters)
    {
        var container = VoiceContainers.FirstOrDefault(x => x.CharacterType == characters);
        var voiceCnt = container.DrivingVoice.Count;
        var clip = container.DrivingVoice[UnityEngine.Random.Range(0, voiceCnt)];

        var emptyCtrl = VoiceControllers.FirstOrDefault(x => x.IsEmpty);

        if (emptyCtrl == null)
        {
            Debug.LogWarning($"there is no empty source");
            return null;
        }

        emptyCtrl.PlayVoice(clip);
        return emptyCtrl;
    }
    public static SeController PlayerDrivingSPVoice(Characters characters)
    {
        return _instance.PlayerSPVoiceOneShot(characters);
    }
    private SeController PlayerSPVoiceOneShot(Characters characters)
    {
        var container = VoiceContainers.FirstOrDefault(x => x.CharacterType == characters);

        var emptyCtrl = VoiceControllers.FirstOrDefault(x => x.IsEmpty);

        if (emptyCtrl == null)
        {
            Debug.LogWarning($"there is no empty source");
            return null;
        }

        emptyCtrl.PlayVoice(container.SpVoice);
        return emptyCtrl;
    }
    public static AudioSource GetSaddleAudio(SaddleType saddleType)
    {
        AudioSource source = null;
        switch(saddleType)
        {
            case SaddleType.Ice:
                source =  _instance.SaddleIce;
                break;

            case SaddleType.Leather:
                source = _instance.SaddleLeather;
                break;
            default:
            case SaddleType.Iron:
                source = _instance.SaddleTeppan;
                break;
        }

        var instance = Instantiate(source);
        return instance;
    }
    public static void PlayBgm(BgmType bgmType)
    {
        _instance.PlayBGMInner(bgmType);
    }
    private void PlayBGMInner(BgmType bgmType)
    {
        if(_currentBgmSource != null)
        {
            _currentBgmSource.gameObject.SetActive(false);
        }

        switch (bgmType)
        {
            case BgmType.Title:
                _currentBgmSource = _instance.TitleBGM;
                break;
            case BgmType.Game:
                _currentBgmSource = _instance.GameBGM;
                break;
            case BgmType.Result:
                _currentBgmSource = _instance.ResultBGM;
                break;

            case BgmType.None:
                _currentBgmSource = null;
                break;
        }

        if (_currentBgmSource != null)
        {
            _currentBgmSource.gameObject.SetActive(true);
            _currentBgmSource.volume = DEFAULT_BGM_VOLUM;
        }
    }
    public static void RequestLowSmoothPGM(CancellationToken token)
    {
        _instance.LowSmoothBgmAsync(token).Forget();
    }
    private async UniTask<Unit> LowSmoothBgmAsync(CancellationToken token)
    {
        if(_currentBgmSource == null)
        {
            return Unit.Default;
        }

        while(_currentBgmSource != null && _currentBgmSource.volume < LOW_BGM_VOLUM && !token.IsCancellationRequested)
        {
            _currentBgmSource.volume -= Time.deltaTime;

            await UniTask.Yield();
        }

        _currentBgmSource.volume = LOW_BGM_VOLUM;

        return Unit.Default;
    }

    [SerializeField] AudioClip test;
    [ContextMenu("Test")]
    public static void Test()
    {
        PlaySE(_instance.test);
    }
}
