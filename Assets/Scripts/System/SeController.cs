using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Fusion;
using R3;
using UnityEngine;
using UnityEngine.Audio;

public class SeController : MonoBehaviour
{
    private const float _startTimeOutSec = 3f;
        private const float _endTimeOutSec = 5f;
    [SerializeField] AudioSource Source;
    private AudioMixerGroup _mixerGroup;
    public bool IsEmpty => !Source.isPlaying;
    public void PlaySe(AudioClip audioClip)
    {
        if(_mixerGroup == null)
        {
            _mixerGroup = SoundManager.SeMixerGroup();
            Source.outputAudioMixerGroup = _mixerGroup;
        }

        PlayAsync(audioClip).Forget();
    }
    private async UniTask<Unit> PlayAsync(AudioClip audioClip)
    {
        gameObject.SetActive(true);

        try
        {
            Source.PlayOneShot(audioClip);
            var destroyToken = this.destroyCancellationToken;
            var timeOutTask1 = UniTask.WaitForSeconds(_startTimeOutSec, cancellationToken: destroyToken);
            var waitStart = UniTask.WaitUntil(() => Source.isPlaying, cancellationToken: destroyToken);
            await UniTask.WhenAny(timeOutTask1, waitStart);

            var timeOutTask2 = UniTask.WaitForSeconds(_endTimeOutSec, cancellationToken: destroyToken);
            var waitEnd = UniTask.WaitUntil(() => !Source.isPlaying, cancellationToken: destroyToken);
            await UniTask.WhenAny(timeOutTask1, waitStart);
        }
        catch (Exception ex)
        {
            Log.Error($"se play error: {ex}");
        }
        finally
        {
            Source.Stop();
            Source.clip = null;
            gameObject.SetActive(false);
        }
        return Unit.Default;
    }
}
