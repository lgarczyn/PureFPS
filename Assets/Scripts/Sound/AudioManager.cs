using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : Pool<AudioManager> {

    Dictionary<AudioClip, double> delays = new Dictionary<AudioClip, double>();
    Dictionary<AudioClip, double> delaysID = new Dictionary<AudioClip, double>();

    public GameObject PlaySoundAt(AudioClip clip, Vector3 position, int id = 0, float volume = 0.3f)
    {
        GameObject instance = GetItem(transform, position);
        AudioSource source = instance.GetComponent<AudioSource>();
        PlayData playData = GetPlayData(clip, id);


        source.clip = clip;
        source.pitch = (float)playData.Pitch;
        source.volume = volume;
        source.PlayScheduled(playData.Time);

        return instance;
    }

    public void PlayWeaponSoundAt(AudioClip clip, Vector3 position, float rps, float volume = 0.1f)
    {
        GameObject instance = GetItem(transform, position);
        AudioSource source = instance.GetComponent<AudioSource>();

        PoolSubject subject = instance.GetComponent<PoolSubject>();
        

        source.clip = clip;
        source.pitch = Mathf.Max(rps / 30, 1f);
        source.volume = volume / 10f;
        source.Play();

        subject.killtime = source.clip.length / source.pitch;
    }

    public override void ReturnItem(GameObject instance)
    {
        AudioSource source = instance.GetComponent<AudioSource>();

        source.pitch = 1;
        source.volume = 1;
        source.clip = null;

        base.ReturnItem(instance);
    }

    public float minDeltaTime = 0.01f;
    public float maxDeltaTime = 0.02f;
    public float maxDelayTime = 0.5f;
    public float durationFactor = 1f;

    public PlayData GetPlayData(AudioClip clip, int id = 0)
    {
        var delays = id != 0 ? delaysID : this.delays;

        double currentTime = AudioSettings.dspTime;
        double previousTime = 0;
        delays.TryGetValue(clip, out previousTime);


        double timeDiff = currentTime - previousTime;
        double delay = Random.Range(minDeltaTime, maxDeltaTime);

        double playTime;

        if (timeDiff < delay)
            playTime = previousTime + delay;
        else
            playTime = currentTime;

        //previousTime %= maxDelayTime;
        double pitch = 1f;// / (1f + previousTime * durationFactor);

        return new PlayData(playTime, pitch);
    }

    public class PlayData
    {
        public double Time { get; private set; }
        public double Pitch { get; private set; }

        public PlayData(double time, double pitch)//double time!
        {
            this.Time = time;
            this.Pitch = pitch;
            //Debug.Log("Playing clip with delay: " + Delay + " and time modifier: " + Duration);
        }
    }
}
