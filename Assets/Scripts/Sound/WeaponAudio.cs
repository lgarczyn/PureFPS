using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAudio : MonoBehaviour {
    
    public float volumeMultiplier = 0.2f;
    bool isPlaying;

    public void StartFire(float rps)
    {
        //uniDebug.Log("Starting fire fps:" + rps);
        AudioSource source = GetComponent<AudioSource>();
        
        Range range = WeaponAudioManager.instance.GetClip(rps);
        float pitch = rps * source.clip.length / range.shotCount;
        source.volume = volumeMultiplier;

        if (source.clip != range.clip)
        {
            source.clip = range.clip;
            //if (isPlaying)
            //    source.Play();//might be a problem
        }

        if (pitch < 1f && range.shotCount == 1)
        {
            source.loop = false;
            source.pitch = 1f;
            isPlaying = false;
        }
        else
        {
            source.loop = true;
            source.pitch = pitch;
        }

        if (isPlaying == false)
        {
            isPlaying = true;
            source.Play();
        }
    }

    public void EndFire()
    {
        isPlaying = false;
        GetComponent<AudioSource>().Stop();
    }
}
