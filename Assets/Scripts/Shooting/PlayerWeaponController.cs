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
