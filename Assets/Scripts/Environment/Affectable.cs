using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Affectable : MonoBehaviour {

    public float health;
    public float sight;
    public float speed;

    float frameEnd;
    float frameStart;

    public List<Effect> healthEffects = new List<Effect>();
    public List<Effect> speedEffects = new List<Effect>();
    public List<Effect> sightEffects = new List<Effect>();

    public void Apply(Data.EffectData data, float time, Vector3 direction, Vector3 position) {
        
        if (data.damage != 0f)
        {
            if (data.damageDuration <= 0f)
                health -= data.damage;
            else
            {
                Effect e = new Effect(time, data.damageDuration, data.damage);
                healthEffects.Add(e);
                health -= GetValue(e, 0, frameEnd);
            }
        }

        if (data.freeze != 0f)
        {
            speedEffects.Add(new Effect(time, data.freezeDuration, data.freeze));
        }

        if (data.blindness != 0f)
        {
            sightEffects.Add(new Effect(time, data.blindnessDuration, data.blindness));
        }


        if (data.knockback != 0f && GetComponent<Rigidbody>() && direction.sqrMagnitude != 0f)
        {
            GetComponent<Rigidbody>().AddForceAtPosition(direction.normalized * data.knockback, position, ForceMode.VelocityChange);
        }
    }

    float GetValue(Effect effect, float frameStart, float frameEnd)
    {
        float start = Mathf.Max(frameStart, effect.startTime);
        float end = Mathf.Min(frameEnd, effect.endTime);
        float len = end - start;

        return effect.amount * (len / effect.duration);
    }


    float GetValue(ref List<Effect> effects, float frameStart, float frameEnd)
    {
        float value = 0f;

        foreach (Effect e in effects)
        {
            value += GetValue(e, frameStart, frameEnd);
        }

        effects.RemoveAll(x => x.endTime <= frameStart);//DO I RISK DELETING THINGS?
        return value;
    }

    void LateUpdate()
    {
        frameStart = frameEnd;
        frameEnd = Time.time;

        float hf = GetValue(ref healthEffects, frameStart, frameEnd);

        if (hf > 0)
        {
            health -= GetValue(ref healthEffects, frameStart, frameEnd);
        }

        sight = 100 - GetValue(ref sightEffects, frameStart, frameEnd);
        speed = 100 - GetValue(ref speedEffects, frameStart, frameEnd);
    }

    [System.Serializable]
    public struct Effect
    {
        public float startTime;
        public float endTime;
        public float amount;
        public float duration
        {
            get
            {
                return endTime - startTime;
            }
        }

        public Effect (float start, float duration, float amount)
        {
            this.startTime = start;
            this.endTime = start + duration;
            this.amount = amount;
        }
    }
}
