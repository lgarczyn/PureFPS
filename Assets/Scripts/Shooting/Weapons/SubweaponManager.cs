using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SubweaponManager : Pool<SubweaponManager>
{
    public GameObject GetItem(Transform parent, Data.WeaponData data, float time)
    {
        GameObject subweapon = base.GetItem(parent, Vector3.zero);
        Subweapon com = subweapon.GetComponent<Subweapon>();

        com.Activate(data, time);

        return subweapon;
    }
}
