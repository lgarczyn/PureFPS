using UnityEngine;
using System.Collections;
using System;

public class ProjectileManager : Pool<ProjectileManager>
{

    public Projectile GetItem(IWeapon parent, Vector3 position, Vector3 velocity, float timeOfShot, Data.ProjectileData data, Data.EffectData effect)
    {
        GameObject go = base.GetItem(transform, position);

        Projectile proj = go.GetComponent<Projectile>();

        proj.Init(transform, parent, position, velocity, timeOfShot, data, effect);

        return proj;
    }

    public override void ReturnItem(GameObject item)
    {
        item.GetComponent<TrailRenderer>().Clear();
        item.GetComponent<TrailRenderer>().enabled = false;
    }
}
