using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExplosionManager : Pool<ExplosionManager>
{
    protected override void InitItem(GameObject item)
    {
        //Force init some important elements
        item.GetComponent<Explosion>().Init();
    }

    public GameObject GetItem(Vector3 position, float range)
    {
        GameObject explosion = base.GetItem(transform, position);

        Explosion com = explosion.GetComponent<Explosion>();

        com.UpdateSize(range);
        com.Explode();

        //AudioSource source = explosion.GetComponent<AudioSource>();
        //AudioManager.PlayData playData = AudioManager.instance.GetPlayData(source.clip);

        //source.pitch = (float)playData.Pitch;
        //source.PlayScheduled(playData.Time);

        return explosion;
    }
    
    public override GameObject GetItem(Transform parent, Vector3 position)
    {
        throw new Exception("ExplosionManager.GetItem(Transform, Position) is deprecated");
    }

}
