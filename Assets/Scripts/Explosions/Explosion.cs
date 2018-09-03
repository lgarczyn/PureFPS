using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float explosionForce = 4;
    public float explosionSize = 1;
    public float explosionSpeed = 1;

    public float destructionSize = 13;
    public float destructionTime = 0.7f;
    public float destructionDepthMult = 0.3f;

    ParticleSystem[] children;
    ExplosionPuff[] puffs;

    ParticleSystem.MinMaxCurve MultiplyCurve(ParticleSystem.MinMaxCurve curve, float value)
    {
        curve.curveMultiplier *= value;
        curve.constantMin *= value;
        curve.constantMax *= value;
        return curve;
    }

    public void Init()
    {
        puffs = GetComponentsInChildren<ExplosionPuff>();
        children = GetComponentsInChildren<ParticleSystem>();

        float time = 0f;

        foreach (ParticleSystem system in children)
        {
            var main = system.main;

            main.startSize = MultiplyCurve(main.startSize, explosionSize);
            main.startSpeed = MultiplyCurve(main.startSpeed, Mathf.Sqrt(explosionSize) * explosionSpeed);
            main.startLifetime = MultiplyCurve(main.startLifetime, explosionSize * explosionSpeed);
            main.startDelay = MultiplyCurve(main.startDelay, explosionSize * explosionSpeed);
            main.gravityModifier = MultiplyCurve(main.gravityModifier, Mathf.Sqrt(explosionSize) * explosionSpeed);
            
            float ntime = main.startSize.constantMax;
            if (ntime > time)
                time = ntime;

            var shape = system.shape;
            shape.radius = explosionSize;
            //system.shape.length *= explosionSize;
            system.transform.localPosition *= explosionSize;
        }

        GetComponent<PoolSubject>().killtime = time;
        GetComponent<AudioSource>().pitch = 1f / explosionSize;


    }

    public void Setup()
    {
        foreach (ParticleSystem system in children)
        {
            system.Clear();
            system.Play();
        }

        foreach (ExplosionPuff puff in puffs)
            puff.Setup(explosionSize);

        GetComponent<AudioSource>().Play();

        TerrainDestructor.instance.DestroyTerrain(transform.position, destructionSize * explosionSize, destructionTime * explosionSize, destructionDepthMult);

        //skip frame yield return null;
        //return;

        float r = 10*explosionSize;
        var cols = Physics.OverlapSphere(transform.position, r);
        var rigidbodies = new List<Rigidbody>();
        foreach (var col in cols)
        {
            if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
            {
                rigidbodies.Add(col.attachedRigidbody);
            }
        }
        foreach (var rb in rigidbodies)
        {
            rb.AddExplosionForce(explosionForce*explosionSize, transform.position, r, 1*explosionSize, ForceMode.Impulse);
        }
    }
}
