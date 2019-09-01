using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WeaponIntentions
{
    public WeaponIntentions(bool fire, bool reload)
    {
        this.fire = fire;
        this.reload = reload;
    }

    public bool fire;
    public bool reload;
}

public abstract class AWeaponController : MonoBehaviour
{
    public abstract WeaponIntentions GetIntentions();
    public abstract void HandleRecoil(Vector2 recoil);
    public abstract Quaternion GetRotation(float time);
}
/*
//TODO: cleanup
public class PlayerWeaponController : AWeaponController {
    
    public Transform head;
    public Transform headRecoil;
    public bool constantFire;

    public override WeaponIntentions GetIntentions()
    {
        return new WeaponIntentions(Input.GetMouseButton(0) || constantFire, Input.GetMouseButton(1));
    }


    public override Quaternion GetRotation()
    {
        return head.rotation;
    }

    public override void HandleRecoil(Vector2 recoil)
    {

    }
}*/
