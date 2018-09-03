using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPuff : MonoBehaviour {

    float _time;
    float _size = 1f;
    Vector3 _velocity;

    public float baseGravity = -0.3f;
    public float baseTime = 3f;
    public float baseScale = 6f;
    public AnimationCurve scale;
    public AnimationCurve warmth;
    public AnimationCurve alpha;

    public void Setup(float size)
    {
        gameObject.SetActive(true);
        GetComponent<Renderer>().enabled = true;
        _size = size;
        _time = 0f;
        _velocity = Vector3.zero;
        transform.localPosition = Vector3.zero;
    }

	void FixedUpdate () {
        _time += Time.fixedDeltaTime;
        float completion = _time / (baseTime * _size);

        if (completion > 1f)
        {
            //gameObject.SetActive(false);
            //GetComponent<Renderer>().enabled = false;
            return;
        }

        transform.position += PhysicsTools.GetMovementUpdateVelocity(ref _velocity, _size * baseGravity * Vector3.down, Time.fixedDeltaTime);

        transform.localScale = baseScale * _size * scale.Evaluate(completion) * Vector3.one;
        GetComponent<Renderer>().material.SetFloat("_Heat", warmth.Evaluate(completion));
        GetComponent<Renderer>().material.SetFloat("_Alpha", alpha.Evaluate(completion));
    }
}
