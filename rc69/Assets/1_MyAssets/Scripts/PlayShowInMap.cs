using Raccoon;
using UnityEngine;

public class PlayShowInMap : MonoBehaviour
{
    public bool isPlay = false;

    public float minDelay = 1;
    public float maxDelay = 5;

    float currentTime = 0;
    public AudioClip currentClip;

    public float vol = 0.7f;
    public void StartPlay(AudioClip currentClip, float vol)
    {
        this.vol = vol;
        SetIsPlay(true);
        this.currentClip = currentClip;
        currentTime = GetTimeDelay();
        GameAudio.Instance.StopSoundFX("Environment");
    }

    public void SetIsPlay(bool isPlay)
    {
        this.isPlay = isPlay;
    }
    // Update is called once per frame
    void Update()
    {
        if (!isPlay) return;
        CountPlay();
    }

    void CountPlay()
    {
        if (currentClip == null) return;
        currentTime -= Time.deltaTime;
        if(currentTime <= 0)
        {
            Play();
            currentTime = currentClip.length + GetTimeDelay();
        }
    }

    void Play()
    {
        GameData.Get.PlaySFX("Environment", currentClip, vol);
    }

    float GetTimeDelay()
    {
        return UnityEngine.Random.Range(minDelay, maxDelay);
    }
}
