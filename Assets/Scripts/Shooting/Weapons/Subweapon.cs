﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Data;
using System;

public class Subweapon : MultiUpdateObject, IWeapon
{
    public ShieldAffectable shield;

    WeaponData data;

    float resourceLevel;

    Vector3 prevVelocity;
    Vector3 nextVelocity;

    Vector3 nextPosition;
    Vector3 prevPosition;

    Projectile proj;

    Transform[] targets = new Transform[0];
    State state;

    enum State
    {
        Setup,
        Contact,
        DumbFire,
        SmartFire,
    }

    SortedDictionary<long, Projectile> _children = new SortedDictionary<long, Projectile>();

    public void NotifyDeadProjectile(Projectile projectile)
    {
        var id = projectile.GetID();
        _children.Remove(id);
    }

    public void NotifyDeadWeapon()
    {
        foreach (var child in _children)
        {
            child.Value.NotifyDeadParent();
        }
        _children.Clear();
        shield.Deactivate();
    }

    public void NotifyTrigger(float time)
    {
        foreach (var child in _children)
        {
            child.Value.NotifyTrigger(time);
        }
    }

    public void RemoveResource(float frameTime, float resourceDrain)
    {
        resourceLevel -= resourceDrain;
        if (data.type == WeaponType.Shield && resourceLevel <= 0)
        {
            Retire();
        }
    }

    public Vector3 GetPosition()
    {
        //TODO use projectile and time argument + transform.localposition ?
        //Probably will be required for rotation too
        return transform.position;
    }

    public void Activate(WeaponData data, float time)
    {
        this.ResetMultiUpdate(time);

        proj = transform.parent.GetComponent<Projectile>();

        this.data = data;

        enabled = true;

        prevPosition = nextPosition = transform.position;
        prevVelocity = nextVelocity = proj.GetComponent<Rigidbody>().velocity;//MAKE BETTER

        resourceLevel = data.resource.resourceCap;

        state = State.Setup;
    }

    protected override void BeforeUpdates()
    {
        prevVelocity = nextVelocity;
        nextVelocity = proj.GetComponent<Rigidbody>().velocity;//MAKE BETTER GetVelocityAt()

        prevPosition = nextPosition;
        nextPosition = transform.position;
    }

    Wait Setup()
    {

        //TODO calculate death time
        switch (data.type)
        {
            case WeaponType.Contact: break;//TODO: Start checking for contact in range=size, stop on timeout
            case WeaponType.Explosion:
                {
                    //Spawn correct explosion graphics
                    Explode(0f);
                    break;
                }
            case WeaponType.Shield:
                {
                    shield.Init(this, data.shield.size);
                    break;
                }
            case WeaponType.Gun: break;//Start usual gun thing
        }

        return Wait.ForEver();
    }

    //TODO fix all of these uses of delta time, which is usually false on the first frame anyway

    void Explode(float deltaTime)
    {
        Vector3 gravity = Physics.gravity; //TODO get actual gravity from parent projectile. better yet, implement getpos from proj
        Vector3 position = PhysicsTools.GetPosition(prevPosition, prevVelocity, gravity, deltaTime);

        ExplosionManager.instance.GetItem(position, data.explosion.range);
    }

    void FireAt(float deltaTime, Quaternion direction, float speed)
    {
        //calculate real position in this frame
        //TODO use projectile to also get _lastTime ???
        //If I want an actual replayability, this is gonna be a mess
        Vector3 gravity = Physics.gravity; //TODO get actual gravity from parent projectile
        Vector3 position = PhysicsTools.GetPosition(prevPosition, prevVelocity, gravity, deltaTime);

        for (int i = 0; i < data.shot.rate; i++)
        {
            ProjectileManager.instance.InitProjectile(this, position, transform.rotation, currentTime, data.projectile, data.effect);
        }
    }

    Wait Fire()
    {
        float trigger_time = 1f / data.shot.rate;

        if (resourceLevel < 1)
        {
            //TODO find a way to kill projectiles with multiple subweapons
            GetComponent<Projectile>().Kill(Projectile.DeathCause.Timeout, currentTime);
            return Wait.ForEver();
        }

        resourceLevel--;

        return Wait.For(trigger_time);
    }

    /*
    void Acquire()
    {
        LayerMask terrain = LayerMask.NameToLayer("Terrain");
        LayerMask enemies = LayerMask.NameToLayer("PlayerEnemies");

        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, data.autoAim.detection_range, enemies);

        float sqrRange = data.autoAim.detection_range * data.autoAim.detection_range;

        var targetsEnumerable = from t in targetsInRange
                      where t.GetComponent<IAffectable>()
                      where Vector3.SqrMagnitude(transform.position - t.transform.position) > sqrRange
                      where Physics.Raycast(transform.position, t.transform.position, float.PositiveInfinity, terrain) == false
                      orderby Vector3.SqrMagnitude(transform.position - t.transform.position) ascending
                      select t.transform;

        targets = targetsEnumerable.Take(data.autoAim.maxTargets).ToArray();
    }*/

    protected override Wait MultiUpdate(float deltaTime)
    {
        //Waits for setup time if present, waits forever if explosion type
        if (state == State.Setup)
            return Setup();

        //Simply fire and return time to next shot
        if (state == State.DumbFire)
            return Fire();

        //If searching for targets, search
        if (state == State.SmartFire)
        {
            //Acquire();

            if (targets.Any())
                return Fire();

            return Wait.ForFrame();
        }

        throw new System.Exception("Invalid State Exception");
    }

    public void Retire()
    {
        NotifyDeadWeapon();
        proj.NotifyDeadSubweapon();
        gameObject.SetActive(false);
        SubweaponManager.instance.ReturnItem(gameObject, 0f);
    }
}
