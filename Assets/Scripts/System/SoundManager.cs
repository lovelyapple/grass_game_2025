using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public enum SeType
    {
        Applause,
        MatchFinished,
        Max,
    }
    [Serializable]
    public class SeData
    {
        public SeType Type;
        public AudioClip Clip; 
    }
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
    
    private const float DEFAULT_BGM_VOLUM = 0.1f;
    private AudioSource _currentBgmSource;
    [SerializeField] List<SeData> SeDataList = new List<SeData>();
    [SerializeField] List<SeController> SeControllers = new List<SeController>();
    public static AudioMixerGroup SeMixerGroup() => _instance.GameAudioMixer.FindMatchingGroups("SE")[0];
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
        var seClip = SeDataList.FirstOrDefault(x => x.Type == seType).Clip;

        var emptyCtrl = SeControllers.FirstOrDefault(x => x.IsEmpty);

        if(emptyCtrl == null)
        {
            Debug.LogWarning($"there is no empty source");
            return;
        }

        emptyCtrl.PlaySe(seClip);
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
        }
    }

    [SerializeField] AudioClip test;
    [ContextMenu("Test")]
    public static void Test()
    {
        PlaySE(_instance.test);
    }
}
