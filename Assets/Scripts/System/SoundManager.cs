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
    public static void PlayerBGM(BgmType bgmType)
    {
        switch(bgmType)
        {
            case BgmType.Title:
                _instance.TitleBGM.gameObject.SetActive(true);
                _instance.GameBGM.gameObject.SetActive(false);
                _instance.ResultBGM.gameObject.SetActive(false);
                break;
            case BgmType.Game:
                _instance.TitleBGM.gameObject.SetActive(false);
                _instance.GameBGM.gameObject.SetActive(true);
                _instance.ResultBGM.gameObject.SetActive(false);
                break;
            case BgmType.Result:
                _instance.TitleBGM.gameObject.SetActive(false);
                _instance.GameBGM.gameObject.SetActive(false);
                _instance.ResultBGM.gameObject.SetActive(true);
                break;

            case BgmType.None:
                _instance.TitleBGM.gameObject.SetActive(false);
                _instance.GameBGM.gameObject.SetActive(false);
                _instance.ResultBGM.gameObject.SetActive(false);
                break;
        }
    }

    [SerializeField] AudioClip test;
    [ContextMenu("Test")]
    public static void Test()
    {
        PlaySE(_instance.test);
    }
}
