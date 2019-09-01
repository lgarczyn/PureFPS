using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAffectable
{
    void Apply(Data.EffectData data, float time, Vector3 direction, Vector3 position);
}

public class Affectable : MonoBehaviour, IAffectable
{

    public float health;
    public float shield;
    public float sight;
    public float speedMult;
    public float rofMult;

    //Only used by editor
    public Data.EffectData toAdd;

    float frameEnd;
    float frameStart;

    List<AppliedEffect> healthEffects = new List<AppliedEffect>();
    List<AppliedEffect> shieldEffects = new List<AppliedEffect>();
    List<AppliedEffect> speedEffects = new List<AppliedEffect>();
    List<AppliedEffect> sightEffects = new List<AppliedEffect>();
    List<AppliedEffect> rofEffects = new List<AppliedEffect>();

    public virtual void Apply(Data.EffectData data, float time, Vector3 direction, Vector3 position)
    {

        if (data.damage.value != 0f)
        {
            healthEffects.Add(new AppliedEffect(time, data.damage));
        }

        if (data.shield.value != 0f)
        {
            shieldEffects.Add(new AppliedEffect(time, data.shield));
        }

        if (data.blindness.value != 0f)
        {
            sightEffects.Add(new AppliedEffect(time, data.blindness));
        }

        if (data.speed.value != 1f)
        {
            speedEffects.Add(new AppliedEffect(time, data.speed));
        }

        if (data.rof.value != 1f)
        {
            rofEffects.Add(new AppliedEffect(time, data.rof));
        }
    }

    static float GetUnshieldedDamage(ref List<AppliedEffect> shieldEffects, float frameTime, float damage)
    {
        //TODO make list sorted by default
        shieldEffects.Sort((x, y) => x.endTime.CompareTo(y.endTime));

        for (int i = 0; i < shieldEffects.Count; i++)
        {
            AppliedEffect effect = shieldEffects[i];
            if (frameTime < effect.startTime || frameTime > effect.endTime)
                continue;
            if (effect.amount > damage)
            {
                effect.amount -= damage;
                shieldEffects[i] = effect;
                return (0f);
            }
            damage -= effect.amount;
            effect.amount = 0;
            shieldEffects[i] = effect;
        }
        return (damage);
    }

    static float GetShieldValue(ref List<AppliedEffect> shieldEffects, float frameTime)
    {
        float current_shield = 0f;
        foreach (var effect in shieldEffects)
        {
            if (frameTime < effect.startTime || frameTime > effect.endTime)
                continue;
            current_shield += effect.amount;
        }
        return current_shield;
    }

    static float GetEffectValue(AppliedEffect effect, float frameStart, float frameEnd)
    {
        //TODO figure out how to handle range. Probably doesn't matter, but >= effect.startime
        if (effect.endTime < frameStart || effect.startTime > frameEnd)
            return (0f);
        if (effect.duration == 0)
            return (effect.amount);

        float start = Mathf.Max(frameStart, effect.startTime);
        float end = Mathf.Min(frameEnd, effect.endTime);
        float len = end - start;
        //TODO actually test this
        //at least doesn't work for an effect len of 0
        return effect.amount * (len / effect.duration);
    }

    public static float GetValue(ref List<AppliedEffect> effects, float frameStart, float frameEnd)
    {
        float value = 0f;

        foreach (AppliedEffect e in effects)
        {
            value += GetEffectValue(e, frameStart, frameEnd);
        }
        return value;
    }

    static float GetEffectValueMult(AppliedEffect effect, float frameStart, float frameEnd)
    {
        if (effect.endTime < frameStart || effect.startTime > frameEnd)
        {
            return (1f);
        }
        float power = effect.amount;
        if (effect.endTime < frameEnd || effect.startTime > frameStart)
        {
            float start = Mathf.Max(frameStart, effect.startTime);
            float end = Mathf.Min(frameEnd, effect.endTime);
            float len = end - start;
            float frameLen = frameEnd - frameStart;
            power = Mathf.Pow(power, len / frameLen);
        }

        return power;
    }

    public static float GetValueMult(ref List<AppliedEffect> effects, float frameStart, float frameEnd)
    {
        float value = 1f;

        foreach (AppliedEffect e in effects)
        {
            value *= GetEffectValueMult(e, frameStart, frameEnd);
        }
        return value;
    }

    public void Update()
    {
        frameStart = frameEnd;
        frameEnd = Time.time;

        float hf = GetValue(ref healthEffects, frameStart, frameEnd);

        if (hf > 0)
        {
            //TODO find way to have subframe shields maybe ?
            //Need to use GetUnshieldedDamage for each effect, yikes
            //Currently all damage is applied start of next frame, meaning missed shields
            health -= GetUnshieldedDamage(ref shieldEffects, frameStart, hf);
        }
        shield = GetShieldValue(ref shieldEffects, frameStart);

        sight = 100 - GetValue(ref sightEffects, frameStart, frameEnd);

        rofMult = GetValueMult(ref rofEffects, frameStart, frameEnd);
        speedMult = GetValueMult(ref speedEffects, frameStart, frameEnd);

        healthEffects.RemoveAll(x => x.endTime <= frameEnd);
        sightEffects.RemoveAll(x => x.endTime <= frameEnd);
        rofEffects.RemoveAll(x => x.endTime <= frameEnd);
        speedEffects.RemoveAll(x => x.endTime <= frameEnd);

        shieldEffects.RemoveAll(x => x.endTime <= frameEnd || x.amount == 0);
    }

    [System.Serializable]
    public struct AppliedEffect
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

        public AppliedEffect(float start, Data.Effect effect)
        {
            this.startTime = start;
            this.endTime = start + effect.duration;
            this.amount = effect.value;
        }
    }
}
