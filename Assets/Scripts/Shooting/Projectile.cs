using UnityEngine;
using Data;

public class Projectile : MonoBehaviour {

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

    public Vector3 velocity;

    public void Init(Transform parent, IWeapon shooter, Vector3 position, Vector3 velocity, float shotTime, ProjectileData data, EffectData effect)
    {
        enabled = true;
        GetComponent<TrailRenderer>().enabled = true;

        transform.localPosition = position;

        if (velocity.sqrMagnitude != 0f)
            transform.rotation = Quaternion.LookRotation(velocity);

        _data = data;
        _effect = effect;
        _parent = shooter;
        _id = _currentID++;
        
        this.velocity = velocity;
        
        transform.SetParent(parent, true);
        _bounces = _data.trajectory.bounceCount;
        _exploded = false;

        _lastUpdateTime = shotTime;
        _deathTime = shotTime + data.trajectory.lifetime + Random.Range(0, data.trajectory.lifetimeVariation);

        if (_deathTime <= shotTime)
            Kill(DeathCause.Timeout, shotTime);

        if (data.subweapon != null)//TODO subweapon life cycle
            if (data.subweapon.activation.onStartup)
                GetComponent<SubWeapon>().Activate(data.subweapon, shotTime);

        //Move(shotTime, );
        //spinAngle = Random.Range(0, 360);
    }

    public void NotifyTrigger(float time)
    {
        var sub = GetComponent<SubWeapon>();
        if (sub)
        {
            GetComponent<SubWeapon>().NotifyTrigger(time);

            if (_data.subweapon.activation.onTrigger)
                sub.Activate(_data.subweapon, time);
        }
    }

    public void NotifyDeadWeapon()
    {
        _parent = null;
        if (GetComponent<SubWeapon>())
            GetComponent<SubWeapon>().NotifyDeadWeapon();
    }

    public long GetID()
    {
        return _id;
    }

    //float spinAngle = 0f;
    void Move(float stopTime)
    {
        float startTime = _lastUpdateTime;
        float movementTime = stopTime - startTime;
        _lastUpdateTime = stopTime;


        PhysicsTools.ParabolicCastHit hit;
        float gravity = Physics.gravity.y * _data.trajectory.gravity;

        bool didHit = PhysicsTools.ParabolicCast(transform.position, velocity, gravity, movementTime, out hit, Physics.AllLayers, precision);//TODO add layers 

        //ParabolicCastHit always returns the flight duration, end point and end velocity
        //if nothing was hit, those are just the points at which the search was stopped

        transform.position = hit.end;
        velocity = hit.velocity;

        if (didHit)
        {
            GameObject target = hit.ray.collider.gameObject;
            Affectable affectable = target.GetComponent<Affectable>();

            float hitTime = startTime + hit.time;

            if (affectable)
            {
                affectable.Apply(_effect, hitTime, hit.velocity, hit.end);
                Kill(DeathCause.Contact, hitTime);
                return;
            }

            if (_bounces > 0)
            {
                velocity = Vector3.Reflect(velocity, hit.ray.normal);
                transform.position += velocity.normalized / 10f;
                _bounces--;
                if (hit.time > movementTime)
                {
                    Debug.LogError("Hit happened after max time", this);
                    return;
                }
                Move(stopTime);
            }
            else
                Kill(DeathCause.Contact, hitTime);

            return;
        }

        //apply spin
        /*spinAngle = (spinAngle + _data.trajectory.spin_angle_increment * Time.fixedDeltaTime) % 360f;

        Vector3 dir = velocity;
        float dist = dir.magnitude;
        if (dist > 0 && _data.trajectory.spin_force > 0f)
        {
            Quaternion dirq = Quaternion.LookRotation(dir);
            Vector3 forceDir = new Vector3(Mathf.Cos(spinAngle), Mathf.Sin(spinAngle));
            forceDir = dirq * forceDir;
            if (_time >= _data.trajectory.spin_starting_time)
                forceDir = forceDir.normalized * dist * _data.trajectory.spin_force;
            else
                forceDir = forceDir.normalized * dist * _data.trajectory.spin_force * _time / _data.trajectory.spin_starting_time;

            _rb.AddForce(forceDir, ForceMode.Acceleration);
        }*/
    }

    void FixedUpdate()
    {
        //Note that Move can call Kill. Projectile will ignore any further calls to Kill after the first one.

        //fixedTime > _lastUpdateTime

        //_deathTime > fixedTime -> normal move
        //_deathTIme < fixedTime && deathTime > lastUpdate -> kill move
        //_deahtTime < _lastUpdate -> what?
        if (Time.fixedTime < _deathTime)
        {
            Move(Time.fixedTime);
        }
        else if (_deathTime > _lastUpdateTime)
        {
            Move(_deathTime);
            //If move hasn't already killed it, then it has arrived to _deathTime without collisions
            Kill(DeathCause.Timeout, _deathTime);
        }
        else
        {
            Debug.LogError("Projectile should already be dead", this);
            Kill(DeathCause.Timeout, _deathTime);
        }
        
        if (transform.position.magnitude > 10000)
        {
            //Timing doesn't really matter, out of bounds
            Kill(DeathCause.Contact, Time.fixedTime);
        }
    }

    public enum DeathCause
    {
        Contact,
        Timeout,
        Trigger,
    }

    public void Kill(DeathCause cause, float time)
    {
        if (_exploded)
            return;

        _parent.NotifyDeadProjectile(this);
        


        _exploded = true;
        //GetComponent<TrailRenderer>().autodestruct = true;
        
        enabled = false;
        //GetComponent<SubWeapon>().enabled = false;
        //GetComponent<MeshRenderer>().enabled = false;

        //Destroy(gameObject, GetComponent<TrailRenderer>().time);
        //var tr = GetComponent<TrailRenderer>();
        //if (tr && tr.time > 0)
        //    GetComponent<PoolSubject>().ReturnItem(gameObject, tr.time);
        //else
        //    GetComponent<PoolSubject>().ReturnItem(gameObject);

        if (_data.subweapon != null)
        {
            if (cause == DeathCause.Contact && _data.subweapon.activation.onContact)
                GetComponent<SubWeapon>().Activate(_data.subweapon, time);

            else if (cause == DeathCause.Timeout && _data.subweapon.activation.onTimeout)
                GetComponent<SubWeapon>().Activate(_data.subweapon, time);

            else if (cause == DeathCause.Trigger && _data.subweapon.activation.onTrigger)
                GetComponent<SubWeapon>().Activate(_data.subweapon, time);
            else
                Retire();
        }
        else
            Retire();
    }

    public void Retire()
    {
        ProjectileManager.instance.ReturnItem(gameObject, 1f);
    }
    

}
