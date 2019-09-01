using UnityEngine;

public class RigidBodyAffectable : Affectable, IAffectable
{
    public override void Apply(Data.EffectData data, float time, Vector3 direction, Vector3 position)
    {
        base.Apply(data, time, direction, position);

        if (data.knockback != 0f && GetComponent<Rigidbody>() && direction.sqrMagnitude != 0f)
        {
            GetComponent<Rigidbody>().AddForceAtPosition(direction.normalized * data.knockback, position, ForceMode.VelocityChange);
        }
    }
}
