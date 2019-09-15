using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicMaterialSelector : Singleton<PhysicMaterialSelector>
{
    public PhysicMaterial bouncy;
    public PhysicMaterial unbouncy;

    public PhysicMaterial GetPhysicMaterial(bool isBouncy)
    {
        return isBouncy ? bouncy : unbouncy;
    }
}
