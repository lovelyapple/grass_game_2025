using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
public class GamePlayerVoiceController : MonoBehaviour
{
    private SeController _currentVoiceController;
    private void Awake()
    {
        MatchModel.GetInstance().PlayDrivingVoiceObservable()
        .Subscribe(PlayVoice)
        .AddTo(this);
    }
    public void PlayVoice((Characters characters, bool playForce) item)
    {
        if(_currentVoiceController != null && !_currentVoiceController.IsEmpty && !item.playForce)
        {
            return;
        }

        _currentVoiceController = SoundManager.PlayerDrivingVoiceRandom(item.characters);
    }
}
