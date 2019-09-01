using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Explosion : MonoBehaviour
{
    public float explosionForce = 4;
    public float explosionSpeed = 1;

    public float destructionSize = 13;
    public float destructionTime = 0.7f;
    public float destructionDepthMult = 0.3f;

    float explosionSize = 1;

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
        //Approximate killtime
        GetComponent<PoolSubject>().killtime = 50;
        GetComponent<AudioSource>().pitch = 1f;
        puffs = GetComponentsInChildren<ExplosionPuff>();
        children = GetComponentsInChildren<ParticleSystem>();
    }

    public void UpdateSize(float size)
    {
        if (explosionSize != size)
        {
            MultiplySize(size / explosionSize);
            explosionSize = size;
            GetComponent<PoolSubject>().killtime = 50 * size * explosionSpeed;
            GetComponent<AudioSource>().pitch = size * explosionSpeed;
        }
    }

    public void MultiplySize(float size)
    {

        foreach (ParticleSystem system in children)
        {
            var main = system.main;

            main.startSize = MultiplyCurve(main.startSize, size);
            main.startSpeed = MultiplyCurve(main.startSpeed, Mathf.Sqrt(size) * explosionSpeed);
            main.startLifetime = MultiplyCurve(main.startLifetime, size * explosionSpeed);
            main.startDelay = MultiplyCurve(main.startDelay, size * explosionSpeed);
            main.gravityModifier = MultiplyCurve(main.gravityModifier, Mathf.Sqrt(size) * explosionSpeed);

            var shape = system.shape;
            shape.radius = size;
            //system.shape.length *= size;
            system.transform.localPosition *= size;
        }
    }

    public void Explode()
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

        float r = 10 * explosionSize;
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
            rb.AddExplosionForce(explosionForce * explosionSize, transform.position, r, 1 * explosionSize, ForceMode.Impulse);
        }
    }
}

[CustomEditor(typeof(Explosion))]
public class ObjectBuilderEditor : Editor
{
    float factor = 1f;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Explosion exp = (Explosion)target;
        factor = EditorGUILayout.FloatField("Factor", factor);
        if (GUILayout.Button("Invert"))
        {
            factor = 1 / factor;
        }
        if (GUILayout.Button("Scale"))
        {
            exp.Init();
            exp.MultiplySize(factor);
        }
    }
}
