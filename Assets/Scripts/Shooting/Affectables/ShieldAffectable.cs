using UnityEngine;
using System.Collections.Generic;
using Data;

public class ShieldAffectable : MonoBehaviour, IAffectable
{
    List<Affectable.AppliedEffect> healthEffects = new List<Affectable.AppliedEffect>();
    bool isShieldActive;

    public void Apply(Data.EffectData data, float time, Vector3 direction, Vector3 position)
    {
        if (data.damage.value != 0f)
        {
            healthEffects.Add(new Affectable.AppliedEffect(time, data.damage));
        }
        //Accumulate damage for a frame, and break should it be too much (implementation in subweapon)
        //framerate affecting this is bad
        //It could be an easy source of difference between server and client
        //  mostly due to differing framerates, and projectiles not being executed in the same order
        //  The shield can't break mid-update due to projectiles arriving at possibly different times in a single frame
        //question is, will it bring more desynchronization than differing framerate and user input 
        //let's admit I wanted to do this right
        //  if the shield breaks, it needs to respawn all the projectiles that hit it, from the exact moment it broke
        //  this is a pain, but doable
        //  Simply reactivating them and calling Update should do the trick
        //      Maybe
        //          But doesn't work for explosions, and other weird stuff
        //OR
        //  shield explodes for accumulated damage minus some supposed to take in account change of missing
        //  would make you want to avoid having your shield break, and deactivating it instead
        //  this reduces the chances of shield breaking in advanced matches, otherwise it kinda sucks

        //There will always be differences between server and client
        //framerates, user control, some more minor stuff
        //This is an advantage for users with poor framerates, but they are disadvanteged otherwise
    }

    IWeapon weapon;

    float frameStart;
    float frameEnd;

    public void Init(IWeapon weapon, float size)
    {
        if (this.isShieldActive == false)
        {
            this.isShieldActive = true;
            this.gameObject.SetActive(true);
            this.weapon = weapon;
            this.transform.localScale = Vector3.one * size;
            frameEnd = Time.time;
            frameStart = frameEnd - Time.deltaTime;
        }
    }

    public void Deactivate()
    {
        if (this.isShieldActive == true)
        {
            this.isShieldActive = false;
            this.gameObject.SetActive(false);
            healthEffects.Clear();
        }
    }

    public void Update()
    {
        frameStart = frameEnd;
        frameEnd = Time.time;

        float hf = Affectable.GetValue(ref healthEffects, frameStart, frameEnd);
        healthEffects.RemoveAll(effect => effect.endTime < frameEnd);

        if (hf > 0f)
            weapon.RemoveResource(hf, frameStart);
    }
}
