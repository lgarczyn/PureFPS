using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Data;
using System;

public class SubWeapon : MultiUpdateObject, IWeapon {

    SubWeaponData data;

    int shot_count;

    Vector3 prevVelocity;
    Vector3 nextVelocity;

    Vector3 nextPosition;
    Vector3 prevPosition;

    Projectile proj;

    Transform[] targets;
    State state;

    enum State
    {
        Setup,
        Contact,
        DumbFire,
        SmartFire,
    }

    public void Awake()
    {
        proj = GetComponent<Projectile>();
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
            child.Value.NotifyDeadWeapon();
        }
        _children.Clear();
    }
    
    public void NotifyTrigger(float time)
    {
        foreach (var child in _children)
        {
            child.Value.NotifyTrigger(time);
        }
    }

    public void Activate(SubWeaponData data, float time)
    {
        this.data = data;

        enabled = true;

        prevPosition = nextPosition = transform.position;
        prevVelocity = nextVelocity = proj.velocity;

        shot_count = data.resource.resourceCap;

        state = State.Setup;
    }

    protected override void BeforeUpdates()
    {
        prevVelocity = nextVelocity;
        nextVelocity = proj.velocity;

        prevPosition = nextPosition;
        nextPosition = transform.position;
    }

    Wait Setup() {

        //TODO calculate death time
        //TODO
        switch (data.type) {
            case WeaponType.Contact: break;//Start checking for contact in range=size, stop on timeout
            case WeaponType.Explosion: {
                //Spawn correct explosion graphics
                Explode();
                GetComponent<Projectile>().Retire();
                break;
            }
            case WeaponType.Gun: break;//Start usual gun thing
            case WeaponType.Shield: break;//Spawn shield
        }

        return Wait.ForEver();
    }

    void Explode() {
        Vector3 gravity = data.projectile.trajectory.gravity * Physics.gravity;
        Vector3 position = PhysicsTools.GetPosition(prevPosition, prevVelocity, gravity, timeSinceLastFrame);
        
        ExplosionManager.instance.GetItem(position, data.explosion.range);
    }

    void FireAt(Quaternion direction, float speed)
    {
        //calculate real position in this frame
        Vector3 gravity = data.projectile.trajectory.gravity * Physics.gravity;
        Vector3 position = PhysicsTools.GetPosition(prevPosition, prevVelocity, gravity, timeSinceLastFrame);
        Vector3 velocity = prevVelocity + gravity * timeSinceLastFrame;

        for (int i = 0; i < data.shot.rate; i++)
        {
            Vector3 forward = PhysicsTools.RandomVectorInCone(data.shot.cone);

            Vector3 projVelocity = velocity * data.shot.inheritedVelocity + forward * data.shot.velocity;
            ProjectileManager.instance.GetItem(this, position, projVelocity, currentTime, data.projectile, data.effect);
        }
    }

    Wait Fire()
    {
        float trigger_time = 1f / data.shot.rate;

        shot_count -= data.resource.resourceCap;

        if (shot_count <= 0)
        {
            GetComponent<Projectile>().Kill(Projectile.DeathCause.Timeout, currentTime);
            return Wait.ForEver();
        }

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
                      where t.GetComponent<Affectable>()
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

    void Kill(float time)
    {
        NotifyDeadWeapon();
        //Return to pool

    }
}
