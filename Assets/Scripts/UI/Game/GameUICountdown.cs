using UnityEngine;

public class GameUICountdown : MonoBehaviour
{
    public void OnCountDownFinished()
    {
        MatchModel.GetInstance().CountDownFinished = true;
        gameObject.SetActive(false);
    }
    public void PlaySe3()
    {
        SoundManager.PlayOneShot(SeType.Se_Count_Down_3);
    }
    public void PlaySe2()
    {
        SoundManager.PlayOneShot(SeType.Se_Count_Down_2);
    }
    public void PlaySe1()
    {
        SoundManager.PlayOneShot(SeType.Se_Count_Down_1);
    }
    public void PlaySe0()
    {
        SoundManager.PlayOneShot(SeType.Se_Count_Down_0);
    }
}
