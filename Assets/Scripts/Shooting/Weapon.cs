using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IWeapon
{
    void NotifyDeadProjectile(Projectile projectile);
}

public class Weapon : MultiUpdateObject, IWeapon {

    public PlayerController controller;
    public ADisplayRatio ammoDisplay;
    WeaponAudio weaponAudio;
    
    public Data.WeaponData weaponData;

    bool intendToFire;
    bool intendToReload;
    float resource_level;

    Vector3 nextPosition;
    Vector3 prevPosition;
    float nextVelocity;
    float prevVelocity;

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
            child.Value.NotifyDeadWeapon();
        }
    }

    void NotifyTrigger(float time)
    {
        foreach (var child in _children)
        {
            child.Value.NotifyTrigger(time);
        }
    }

    enum State
    {
        None,
        Firing,
        Reloading,
        ReloadingAmmo,
    }

    override protected void RealOnEnable()
    {
        weaponAudio = GetComponent<WeaponAudio>();
        resource_level = weaponData.resource.resourceCap;
        prevPosition = nextPosition = transform.position;
        prevVelocity = nextVelocity = 0f;
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
        nextVelocity = 0;// GetComponent<Rigidbody>().velocity.magnitude; TODO get velocity

        prevPosition = nextPosition;
        nextPosition = transform.position;

        intendToFire = intentions.fire;
        intendToReload = intentions.reload;
    }

    protected override void AfterUpdates()
    {
        if (ammoDisplay != null)
            ammoDisplay.SetRatio(weaponData.resource.resourceCap == 0 ? 1f : resource_level / weaponData.resource.resourceCap);
    }

    protected override Wait MultiUpdate (float deltaTime) {

        if ((state == State.None || state == State.ReloadingAmmo) && intendToFire)
            state = State.Firing;

        if (state == State.Firing && intendToFire == false)
            state = State.None;

        if ((state == State.None || state == State.Firing) && intendToReload)
            if (weaponData.resource.resourceCap > 0)//if magazine system
                state = State.Reloading;

        if (prevState == State.Firing && state != State.Firing)
            weaponAudio.EndFire();

        Wait ret;

        if (state == State.None)
            ret = Rest(deltaTime);
        else if (state == State.Firing)
            ret = Fire(deltaTime);
        else if (state == State.Reloading)
            ret = Reload();
        else if (state == State.ReloadingAmmo)
            ret = ReloadAmmo();
        else
            ret = Wait.ForFrame();

        prevState = state;

        return ret;
    }
    #endregion
    #region Recoil
    
    private Vector2 GetRecoilValues() {
        Vector2 recoil = weaponData.recoil.constant;

        recoil.x += Random.Range(-weaponData.recoil.random.x, weaponData.recoil.random.x);
        recoil.y += Random.Range(-weaponData.recoil.random.y, weaponData.recoil.random.y);

        return recoil;
    }
    
    #endregion
    #region States
    
    private void Shoot(float deltaTime)
    {
        Vector3 position = Vector3.Lerp(prevPosition, nextPosition, frameRatio);
        float velocity = Mathf.Lerp(prevVelocity, nextVelocity, frameRatio);
        //Quaternion rotation = Quaternion.Lerp(prevRotation, nextRotation, frameRatio);
        Quaternion rotation = controller.GetRotation(currentTime);

        Vector2 recoil = GetRecoilValues();
        controller.HandleRecoil(recoil);

        for (int i = 0; i < weaponData.shot.count; i++)
        {
            Vector3 forward = PhysicsTools.RandomVectorInCone(weaponData.shot.cone);

            forward *= weaponData.shot.velocity + velocity * weaponData.shot.inheritedVelocity;
                
            Projectile p = ProjectileManager.instance.GetItem(this, position, rotation * forward, currentTime, weaponData.projectile, weaponData.effect);
            _children.Add(p.GetID(), p);
        }
    }

    private Wait Fire(float deltaTime)
    {
        if (weaponData.shot.rate < 0.01f) //check if some asshole broke the limit in the editor (the closer to 0, the longer the fire mode is stuck)
            weaponData.shot.rate = 0.01f;

        if (weaponAudio)
            weaponAudio.StartFire(weaponData.shot.rate);

        //calculate time between each shot
        float triggerTime = 1f / weaponData.shot.rate;

        //if there is an ammo system
        if (weaponData.resource.resourceCap > 0)
        {
            if (resource_level < 0)
            {
                Debug.LogError("mag count < 0, setting to 0", this);
                resource_level = 0;
                return Wait.For(triggerTime);
            }

            //if no bullet left, simply wait again, who knows, recharge might have worked?
            if (resource_level < 1f)
            {
                state = State.None;
                return Wait.For(triggerTime);
            }

            //Fire

            Shoot(deltaTime);
            resource_level--;

            //if bullet reach 0, and penalty is on, set penalty_time, and ask for reload once over
            if (resource_level < 1f && weaponData.resource.timePerEmptyClip > 0f)
            {

                state = State.Reloading;
                return Wait.For(weaponData.resource.timePerEmptyClip);
            }
        }
        else
            Shoot(deltaTime);

        return Wait.For(triggerTime);
    }

    private Wait Reload()
    {

        if (weaponData.resource.timePerBullet == 0f || resource_level == weaponData.resource.resourceCap)
        {
            state = State.None;
            resource_level = weaponData.resource.resourceCap;

            return Wait.ForFrame();
        }
        else
        {
            state = State.ReloadingAmmo;
            return Wait.For(weaponData.resource.timePerReload);
        }
    }

    private Wait ReloadAmmo()
    {
        resource_level++;
        if (resource_level >= weaponData.resource.resourceCap)
            state = State.None;

        return Wait.ForFrame();
    }

    private Wait Rest(float deltaTime)
    {
        if (weaponData.resource.resourcePerSec > 0f)
        {
            resource_level += deltaTime * weaponData.resource.resourcePerSec;
            if (resource_level > weaponData.resource.resourceCap)
                resource_level = weaponData.resource.resourceCap;
        }
        return Wait.ForFrame();
    }
    #endregion
}
