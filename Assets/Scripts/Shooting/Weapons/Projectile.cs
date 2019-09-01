using UnityEngine;
using Data;

public class Projectile : MonoBehaviour
{

    static long _currentID = 0;

    public float precision = 1f;

    ProjectileData _data;
    EffectData _effect;

    long _id;
    IWeapon _parent;
    float _deathTime;
    float _lastUpdateTime;
    int _bounces;
    bool _exploded;
    bool[] _subweaponCreated;
    int _activeSubweaponCount;

    public Vector3 velocity;

    public void Init(Transform parent, IWeapon shooter, Vector3 position, Vector3 velocity, float shotTime, ProjectileData data, EffectData effect)
    {
        enabled = true;
        transform.localPosition = position;
        GetComponent<TrailRenderer>().enabled = true;

        if (velocity.sqrMagnitude != 0f)
            transform.rotation = Quaternion.LookRotation(velocity);

        _data = data;
        _effect = effect;
        _parent = shooter;
        _id = _currentID++;

        this.velocity = velocity;

        transform.SetParent(parent, true);
        _bounces = _data.bulletTrajectory.bounceCount;
        _exploded = false;

        _lastUpdateTime = shotTime;
        _deathTime = shotTime + data.lifetime + Random.Range(0, data.lifetimeVariation);

        if (_deathTime <= shotTime)
            Kill(DeathCause.Timeout, shotTime);

        _activeSubweaponCount = 0;
        _subweaponCreated = new bool[data.subweapons.Count];

        for (int i = 0; i < data.subweapons.Count; i++)
        {
            SubweaponData subweapon = data.subweapons[i];
            if (subweapon.activation.onStartup)
            {
                AddSubweapon(subweapon, shotTime, i);
            }
        }
    }

    void AddSubweapon(Data.SubweaponData data, float time, int index)
    {
        _activeSubweaponCount++;
        _subweaponCreated[index] = true;
        SubweaponManager.instance.GetItem(transform, data, time + data.activation.setupTime);
    }

    public void NotifyTrigger(float time)
    {
        foreach (Subweapon subweapon in GetComponentsInChildren<Subweapon>())
            subweapon.NotifyTrigger(time);
        for (int i = 0; i < _data.subweapons.Count; i++)
        {
            SubweaponData subweapon = _data.subweapons[i];
            if (subweapon.activation.onTrigger && _subweaponCreated[i] == false)
            {
                AddSubweapon(subweapon, time, i);
            }
        }
    }

    public void NotifyDeadWeapon()
    {
        _parent = null;
        foreach (Subweapon subweapon in GetComponentsInChildren<Subweapon>())
            subweapon.NotifyDeadWeapon();
        //TODO: check retire possible, then retire
    }

    public void NotifyDeadSubweapon()
    {
        _activeSubweaponCount--;
        if (_activeSubweaponCount == 0)
            Retire();
    }
    public long GetID()
    {
        return _id;
    }
    void Move(float stopTime)
    {
        float startTime = _lastUpdateTime;
        _lastUpdateTime = stopTime;

        float movementTime = stopTime - startTime;

        PhysicsTools.ParabolicCastHit hit;
        float gravity = Physics.gravity.y * _data.bulletTrajectory.gravity;

        bool didHit = PhysicsTools.ParabolicCast(
            transform.position,
            velocity,
            gravity,
            movementTime,
            out hit,
            Physics.AllLayers,//TODO add layers 
            precision);

        //ParabolicCastHit always returns the flight duration, end point and end velocity
        //if nothing was hit, those are just the points at which the search was stopped

        transform.position = hit.end;
        velocity = hit.velocity;
        float hitTime = startTime + hit.time;
        _lastUpdateTime = hitTime;

        if (didHit)
        {
            GameObject target = hit.ray.collider.gameObject;
            IAffectable affectable = target.GetComponent<IAffectable>();

            if (affectable != null)
            {
                affectable.Apply(_effect, hitTime, hit.velocity, hit.end);
                Kill(DeathCause.Contact, hitTime);
            }
            else if (_bounces > 0)
            {
                velocity = Vector3.Reflect(velocity, hit.ray.normal);
                transform.position += velocity.normalized / 10f;
                _bounces--;
                if (hit.time > movementTime)
                {
                    Debug.LogError("Hit happened after max time", this);
                }
                Move(stopTime);
            }
            else
                Kill(DeathCause.Contact, hitTime);
        }
    }

    void Update()
    {
        //Move projectile, and if necessary, kill it
        //Note that Move can call Kill. Projectile will ignore any further calls to Kill after the first one.
        float stopTime = Mathf.Min(Time.time, _deathTime);
        Move(stopTime);

        if (Time.time > _deathTime)
            Kill(DeathCause.Timeout, _deathTime);

        if (transform.position.sqrMagnitude > 10000 * 10000)
            Kill(DeathCause.Timeout, Time.fixedTime);
    }

    public enum DeathCause
    {
        Contact,
        Timeout,
    }

    public void Kill(DeathCause cause, float time)
    {
        if (_exploded)
            return;

        _exploded = true;
        enabled = false;

        for (int i = 0; i < _data.subweapons.Count; i++)
        {
            SubweaponData subweapon = _data.subweapons[i];
            if (_subweaponCreated[i] == false && (
                (subweapon.activation.onContact && cause == DeathCause.Contact)
                || (subweapon.activation.onTimeout && cause == DeathCause.Timeout)))
            {
                AddSubweapon(subweapon, time, i);
            }
        }
        if (_activeSubweaponCount == 0)
            Retire();
    }

    public void Retire()
    {
        _parent.NotifyDeadProjectile(this);
        ProjectileManager.instance.ReturnItem(gameObject, 1f);
    }
}
