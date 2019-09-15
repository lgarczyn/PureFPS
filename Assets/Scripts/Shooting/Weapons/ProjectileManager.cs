using UnityEngine;
using System.Collections;
using System;

public class ProjectileManager : Pool<ProjectileManager>
{

    public Projectile InitProjectile(IWeapon parent, Vector3 position, Quaternion rotation, float timeOfShot, Data.ProjectileData data, Data.EffectData effect)
    {
        GameObject go = base.GetItem();

        Projectile proj = go.GetComponent<Projectile>();

        proj.Init(parent, position, rotation, timeOfShot, data, effect);

        return proj;
    }

    public override void ReturnItem(GameObject item)
    {
        item.GetComponent<TrailRenderer>().Clear();
        item.GetComponent<TrailRenderer>().enabled = false;
        base.ReturnItem(item);
    }
}
