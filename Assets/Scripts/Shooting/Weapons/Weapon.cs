using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IWeapon
{
    void NotifyDeadProjectile(Projectile projectile);
    void RemoveResource(float resource, float frameTime);

    Vector3 GetPosition();
}

public class Weapon : MultiUpdateObject, IWeapon
{

    public PlayerController controller;
    public ADisplayRatio ammoDisplay;
    public Affectable playerAffectable;
    public ShieldAffectable shieldAffectable;
    WeaponAudio weaponAudio;

    public Data.WeaponData weaponData;

    bool intendToFire;
    bool intendToReload;
    float resourceLevel;

    Vector3 nextPosition;
    Vector3 prevPosition;
    Vector3 nextVelocity;
    Vector3 prevVelocity;

    State prevState;
    State state;

    SortedDictionary<long, Projectile> _children = new SortedDictionary<long, Projectile>();

    public void NotifyDeadProjectile(Projectile projectile)
    {
        var id = projectile.GetID();
        _children.Remove(id);
    }

    void NotifyDeadWeapon()
    {
        foreach (var child in _children)
        {
            child.Value.NotifyDeadParent();
        }
    }

    void NotifyTrigger(float time)
    {
        foreach (var child in _children)
        {
            child.Value.NotifyTrigger(time);
        }
    }

    public void RemoveResource(float resourceDrain, float frameTime)
    {
        this.resourceLevel -= resourceDrain;
    }

    public Vector3 GetPosition()
    {
        //TODO use player and time argument + transform.localposition ?
        return transform.position;
    }

    enum State
    {
        None,
        Firing,
        Reloading,
        ReloadingInteruptible,
    }

    override protected void RealOnEnable()
    {
        weaponAudio = GetComponent<WeaponAudio>();
        resourceLevel = weaponData.resource.resourceCap;
        prevPosition = nextPosition = transform.position;
        prevVelocity = nextVelocity = Vector3.zero;


        if (weaponData.shot.rate < 0.01f)
            weaponData.shot.rate = 0.01f;
        //TODO find way to remove this, either in serialization or checking is subweapons is allocated
        if (weaponData.projectile.subweapons == null)
            weaponData.projectile.subweapons = new List<Data.SubweaponData>();
    }

    private void OnDisable()
    {
        _children.Clear();
    }

    #region Updates

    protected override void BeforeUpdates()
    {
        var intentions = controller.GetIntentions();

        prevVelocity = nextVelocity;
        //TODO: clean this
        nextVelocity = playerAffectable.GetComponent<Rigidbody>().velocity;

        prevPosition = nextPosition;
        nextPosition = transform.position;

        intendToFire = intentions.fire;
        intendToReload = intentions.reload;

        if (playerAffectable)
        {
            SetRofMultiplier(playerAffectable.rofMult);
            //TODO: update audio rof
        }
    }

    protected override void AfterUpdates()
    {
        if (ammoDisplay != null)
            ammoDisplay.SetRatio(weaponData.resource.resourceCap == 0 ? 1f : resourceLevel / weaponData.resource.resourceCap);
    }

    protected override Wait MultiUpdate(float deltaTime)
    {

        if ((state == State.None || state == State.ReloadingInteruptible) && intendToFire)
            state = State.Firing;

        if (state == State.Firing && intendToFire == false)
            state = State.None;

        if ((state == State.None || state == State.Firing) && intendToReload)
            if (weaponData.resource.resourceCap > 0)//if magazine system
                state = State.Reloading;

        if (resourceLevel < 0)
        {
            Debug.LogError("mag count < 0, setting to 0", this);
            resourceLevel = 0;
        }

        if (state == State.Firing && weaponData.resource.resourceCap > 0f && resourceLevel < 1f)
            state = State.Reloading;

        Wait ret;

        if (prevState == State.Firing && state != State.Firing)
            StopFiring();

        if (state == State.None)
            ret = Rest();
        else if (state == State.Firing)
            ret = Fire();
        else if (state == State.Reloading)
            ret = Reload();
        else if (state == State.ReloadingInteruptible)
            ret = ReloadAmmo();
        else
            ret = Wait.ForFrame();

        prevState = state;

        return ret;
    }
    #endregion
    #region Recoil

    private Vector2 GetRecoilValues()
    {
        Vector2 recoil = weaponData.recoil.constant;

        recoil.x += Random.Range(-weaponData.recoil.random.x, weaponData.recoil.random.x);
        recoil.y += Random.Range(-weaponData.recoil.random.y, weaponData.recoil.random.y);

        return recoil;
    }

    #endregion
    #region States

    private Wait ShootGun()
    {
        float frameRatio = this.frameRatio;
        float deltaTime = this.deltaTime;

        //Vector3 position = Vector3.Lerp(prevPosition, nextPosition, frameRatio);
        Vector3 velocity = Vector3.Lerp(prevVelocity, nextVelocity, frameRatio);

        //Interpolate weapon position
        Vector3 position;
        Vector3 velocityDif = nextVelocity - prevVelocity;
        if (velocityDif.sqrMagnitude == 0)
        {
            position = Vector3.Lerp(prevPosition, nextPosition, frameRatio);
        }
        else
        {
            Vector3 force = velocityDif / deltaTime;
            position = prevPosition + velocity * deltaTime + 0.5f * force * deltaTime * deltaTime;
        }

        //Quaternion rotation = Quaternion.Lerp(prevRotation, nextRotation, frameRatio);
        Quaternion rotation = controller.GetRotation(currentTime);

        Vector2 recoil = GetRecoilValues();
        controller.HandleRecoil(recoil);

        for (int i = 0; i < weaponData.shot.count; i++)
        {
            Projectile p = ProjectileManager.instance.InitProjectile(
                this,
                position,
                rotation,
                currentTime,
                weaponData.projectile,
                weaponData.effect);
            _children.Add(p.GetID(), p);
        }
        return Wait.For(1f / weaponData.shot.rate);
    }

    private Wait ShootContact()
    {
        playerAffectable.Apply(weaponData.effect, currentTime, Vector3.up + Vector3.back, Vector3.zero);
        return Wait.For(1f / weaponData.shot.rate);
    }

    private Wait ShootShield()
    {
        shieldAffectable.Init(this, weaponData.shield.size);
        return Wait.ForFrame();
    }

    private Wait Shoot()
    {
        if (weaponAudio)
            weaponAudio.StartFire(weaponData.shot.rate * playerAffectable.rofMult);

        switch (weaponData.type)
        {
            case Data.WeaponType.Gun: return ShootGun();
            case Data.WeaponType.Contact: return ShootContact();
            case Data.WeaponType.Shield: return ShootShield();
            default: throw new System.Exception("Invalid State Exception");
        }
    }

    private void StopFiring()
    {
        weaponAudio.EndFire();
        if (weaponData.type == Data.WeaponType.Shield)
        {
            shieldAffectable.Deactivate();
        }
    }

    private Wait Fire()
    {

        //if there is an ammo system
        if (weaponData.resource.resourceCap > 0)
        {
            //TODO: maybe generalize this?
            if (weaponData.type != Data.WeaponType.Shield)
                resourceLevel--;
            return Shoot();
        }
        else
            return Shoot();
    }

    private Wait Reload()
    {
        float malus = resourceLevel < 1f ? weaponData.resource.timePerEmptyClip : 0f;

        if (weaponData.resource.timePerBullet == 0f)
        {
            state = State.None;
            resourceLevel = weaponData.resource.resourceCap;
            return Wait.For(weaponData.resource.timePerReload + malus);
        }
        else
        {
            state = State.ReloadingInteruptible;
            return Wait.For(weaponData.resource.timePerReload + malus);
        }
    }

    private Wait ReloadAmmo()
    {
        resourceLevel++;
        if (resourceLevel >= weaponData.resource.resourceCap)
            state = State.None;

        return Wait.For(weaponData.resource.timePerBullet);
    }

    private Wait Rest()
    {
        if (weaponData.resource.resourcePerSec > 0f)
        {
            resourceLevel += deltaTime * weaponData.resource.resourcePerSec;
            if (resourceLevel > weaponData.resource.resourceCap)
                resourceLevel = weaponData.resource.resourceCap;
        }
        return Wait.ForFrame();
    }
    #endregion
}
