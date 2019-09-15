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
    int _bounces;
    bool _exploded;
    bool[] _subweaponCreated;
    int _activeSubweaponCount;

    //State and of time of last received state/normal update
    public Vector3 lastVelocity;
    public Vector3 lastPosition;
    public float lastTime;
    // Parabolic trajectory info
    public float lastRatio;
    // Orbit info
    public Quaternion firedDirection;
    public float firedTime;
    // Components
    private Rigidbody _rigidbody;
    private MeshRenderer _renderer;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<MeshRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    public void Init(IWeapon shooter, Vector3 position, Quaternion rotation, float shotTime, ProjectileData data, EffectData effect)
    {
        BaseInit(shooter, shotTime, data, effect);
        switch (data.trajectoryType)
        {
            case TrajectoryType.Orbital: InitOrbital(position, rotation, shotTime); break;
            case TrajectoryType.Parabolic: InitParabolic(position, rotation, shotTime); break;
        }
    }

    void BaseInit(IWeapon shooter, float shotTime, ProjectileData data, EffectData effect)
    {
        enabled = true;

        _data = data;
        _effect = effect;
        _parent = shooter;
        _id = _currentID++;
        _exploded = false;
        _deathTime = shotTime + data.lifetime + Random.Range(0, data.lifetimeVariation);
        _bounces = 0;

        _activeSubweaponCount = 0;
        _subweaponCreated = new bool[data.subweapons.Count];

        if (_deathTime <= shotTime)
        {
            Kill(DeathCause.Timeout, shotTime);
            return;
        }

        //TODO: add subweapons after pos is set?
        for (int i = 0; i < data.subweapons.Count; i++)
        {
            SubweaponData subweapon = data.subweapons[i];
            if (subweapon.activation.onStartup)
            {
                AddSubweapon(subweapon, shotTime, i);
            }
        }

        _rigidbody.detectCollisions = true;
        _trailRenderer.enabled = true;
        _renderer.enabled = true;
    }

    void InitParabolic(Vector3 position, Quaternion rotation, float shotTime)
    {
        _bounces = _data.parabolicTrajectory.bounceCount;
        SetBouncyness(_bounces > 0);

        Vector3 forward = rotation * PhysicsTools.RandomVectorInCone(_data.parabolicTrajectory.cone);

        //TODO: fix so that forward is already normalized
        forward = forward.normalized;

        Vector3 velocity = forward * _data.velocity;

        SetPhysicsState(position, velocity, shotTime);

        //Accelerate projectiles by the time lost to frame imprecision
        lastRatio = (Time.fixedTime - lastTime) / Time.fixedDeltaTime;
        _rigidbody.velocity *= lastRatio;

        //Multiply gravity by the square of that factor, to account for the short time
        _rigidbody.AddForce(Physics.gravity
            * _data.parabolicTrajectory.gravity
            * lastRatio
            * lastRatio,
            ForceMode.Acceleration);

        //TODO calculate the actual position by iteration,
        // given the frame count, and therefore the number of times gravity was applied
    }

    void InitOrbital(Vector3 position, Quaternion direction, float shotTime)
    {
        SetBouncyness(false);
        //Apply tilt to the start rotation
        float tilt = _data.orbitalTrajectory.tiltVariation;
        tilt = Random.Range(-tilt, tilt);
        tilt += _data.orbitalTrajectory.tilt;
        direction *= Quaternion.AngleAxis(tilt, Vector3.forward);
        //Store the data to calculate the orbit
        firedTime = shotTime;
        firedDirection = direction.normalized;//TODO check if normalization necessary (probably should be kept to be safe)

        //Get the starting state
        Vector3 startPosition;
        GetLocalOrbitPos(shotTime, out startPosition);
        startPosition += position;

        SetPhysicsState(startPosition, Vector3.zero, shotTime);
        //TODO set bounciness to 0
        PrepareForNextStateOrbit(shotTime, Time.fixedTime + Time.fixedDeltaTime, true);
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

    public void NotifyDeadParent()
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

    public enum DeathCause
    {
        Contact,
        Timeout,
    }

    public void Kill(DeathCause cause, float time)
    {
        if (_exploded)
            return;

        AddPositionToRenderer(_rigidbody.position);

        _exploded = true;
        enabled = false;
        _trailRenderer.enabled = false;
        _renderer.enabled = false;

        _rigidbody.detectCollisions = false;
        _rigidbody.velocity = Vector3.zero;

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
        //TODO Check if not already retired (spooky)
        //Could happen if subweapons both try to retire it in NotifyDeadWeapon
        _trailRenderer.Clear();
        _parent.NotifyDeadProjectile(this);
        ProjectileManager.instance.ReturnItem(gameObject, 1f);
    }

    private void SetPhysicsState(Vector3 position, Vector3 velocity, float timeOfState)
    {
        lastVelocity = velocity;
        lastPosition = position;
        lastTime = timeOfState;
        _rigidbody.position = position;
        _rigidbody.velocity = velocity;
        if (velocity.sqrMagnitude != 0)
            _rigidbody.rotation = Quaternion.LookRotation(velocity, Vector3.up);
        AddPositionToRenderer(position);
    }

    private void SetBouncyness(bool isBouncy)
    {
        GetComponent<Collider>().material = PhysicMaterialSelector.instance.GetPhysicMaterial(isBouncy);
    }

    private void CalculatePhysicsStateParabolic()
    {
        // if the item should be slowed down
        if (lastRatio != 1f)
        {
            _rigidbody.velocity /= lastRatio;
            lastRatio = 1f;
        }
        // store the state and time for collision
        SetPhysicsState(_rigidbody.position, _rigidbody.velocity, Time.fixedTime);
        // apply gravity
        if (_data.parabolicTrajectory.gravity != 0f)
        {
            _rigidbody.AddForce(Physics.gravity
                * _data.parabolicTrajectory.gravity,
                ForceMode.Acceleration);
        }
    }

    private Quaternion GetOrbitRotation(float time)
    {
        //convert velocity to deg per s using range
        float degPerSecond = _data.velocity / (2 * Mathf.PI / 360f * _data.orbitalTrajectory.range);
        //deduce total path orbited
        float timeSpent = (time - firedTime);
        //calculate position for time - half of lifetime as the starting position
        //that way, the path is always symmetrical relative to the aimed direction
        float degRotation = degPerSecond * (timeSpent - _data.lifetime / 2);

        //Get start orbit
        Quaternion rotation = Quaternion.AngleAxis(degRotation, Vector3.right);

        return (firedDirection * rotation).normalized;//TODO check if normalization necessary
    }

    private void CalculatePhysicsStateOrbit()
    {
        PrepareForNextStateOrbit(Time.fixedTime, Time.fixedTime + Time.fixedDeltaTime, false);
    }

    private void GetLocalOrbitPos(float time, out Vector3 position)
    {
        Quaternion rotation = GetOrbitRotation(time);
        position = rotation * Vector3.forward * _data.orbitalTrajectory.range;
    }

    private Vector3 predictedNextFramePos;

    private void PrepareForNextStateOrbit(float time, float nextTime, bool firstFrame)
    {
        //Get next position
        Vector3 nextPosition;
        GetLocalOrbitPos(nextTime, out nextPosition);
        nextPosition += _parent.GetPosition();

        //Find velocity to reach next position by next frame
        Vector3 deltaPosition = nextPosition - _rigidbody.position;
        Vector3 velocity = deltaPosition / Time.fixedDeltaTime;
        SetPhysicsState(_rigidbody.position, velocity, time);
    }

    private void FixedUpdate()
    {
        if (_deathTime < Time.fixedTime)
        {
            Kill(DeathCause.Timeout, _deathTime);
            return;
        }

        if (transform.position.sqrMagnitude > 10000 * 10000)
        {
            Kill(DeathCause.Timeout, Time.fixedTime);
            return;
        }

        switch (_data.trajectoryType)
        {
            case TrajectoryType.Orbital: CalculatePhysicsStateOrbit(); break;
            case TrajectoryType.Parabolic: CalculatePhysicsStateParabolic(); break;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Vector3 contactPoint = other.GetContact(0).point;
        // Approximate the time of the collision,
        // taking in account the possibility of currently being faster due to catching up
        float timeOfCollision = lastTime +
            (lastPosition - _rigidbody.position).magnitude / lastVelocity.magnitude;

        if (timeOfCollision > _deathTime)
        {
            Kill(DeathCause.Timeout, _deathTime);
            return;
        }

        IAffectable affectable = other.collider.GetComponent<IAffectable>();

        if (affectable != null)
        {
            affectable.Apply(_effect, timeOfCollision, -other.relativeVelocity, contactPoint);
            Kill(DeathCause.Contact, timeOfCollision);
        }
        else if (_bounces > 0)
        {
            _bounces--;
            if (_bounces == 0)
                SetBouncyness(false);
            AddPositionToRenderer(contactPoint);
        }
        else
            Kill(DeathCause.Contact, timeOfCollision);
    }

    void AddPositionToRenderer(Vector3 position)
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null || trail.enabled == false)
            return;
        trail.AddPosition(position);
    }
}
