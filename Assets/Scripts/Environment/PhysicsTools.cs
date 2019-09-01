using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsTools
{

    public static Vector3 GetMovement(Vector3 velocity, Vector3 force, float time)
    {
        return (velocity * time) + 0.5f * force * (time * time);
    }

    public static Vector3 GetMovementUpdateVelocity(ref Vector3 velocity, Vector3 force, float time)
    {
        Vector3 ret = (velocity * time) + 0.5f * force * (time * time);
        velocity += force * time;

        return ret;
    }

    public static Vector3 GetVelocity(Vector3 velocity, Vector3 force, float time)
    {
        return velocity + force * time;
    }

    public static Vector3 GetPosition(Vector3 position, Vector3 velocity, Vector3 force, float time)
    {
        return position + GetMovement(velocity, force, time);
    }

    public static Vector3 GetMovement(Vector3 velocity, float gravity, float time)
    {
        return (velocity * time) + new Vector3(0, 0.5f * gravity * (time * time));
    }

    public static Vector3 GetMovementUpdateVelocity(ref Vector3 velocity, float gravity, float time)
    {
        Vector3 ret = (velocity * time) + new Vector3(0, 0.5f * gravity * (time * time), 0);
        velocity += new Vector3(0, gravity * time, 0);

        return ret;
    }

    public static Vector3 GetVelocity(Vector3 velocity, float gravity, float time)
    {
        return velocity + new Vector3(0, gravity * time, 0);
    }

    public static Vector3 GetPosition(Vector3 position, Vector3 velocity, float gravity, float time)
    {
        return position + GetMovement(velocity, gravity, time);
    }

    public static Vector3 RandomVectorInCone(float radius)
    {
        //http://math.stackexchange.com/questions/56784/generate-a-random-direction-within-a-cone
        //The 2 - sphere is unique in that slices of equal height have equal surface area
        //That is, to sample points on the unit sphere uniformly, you can sample z uniformly on[−1, 1] and ϕ uniformly on[0, 2π).
        //If your cone were centred around the north pole, the angle θ would define a minimal z coordinate cosθ,
        //and you could sample z uniformly on[cosθ, 1] and ϕ uniformly on[0, 2π) to obtain the vector
        //(sqrt(1 - z^2) * cosϕ, sqrt(1 - z^2) * sinϕ, z)
        float radradius = radius * Mathf.PI / 360;
        float z = Random.Range(Mathf.Cos(radradius), 1);
        float t = Random.Range(0, Mathf.PI * 2);
        return new Vector3(Mathf.Sqrt(1 - z * z) * Mathf.Cos(t), Mathf.Sqrt(1 - z * z) * Mathf.Sin(t), z);
    }

    public struct ParabolicCastHit
    {
        public RaycastHit ray;
        public float time;
        public Vector3 velocity;
        public Vector3 end;
    }

    //WARNING: ParabolicCastHit.ray.distance is NOT the arc length of the entire trajectory, or even related to that. Do not use it.
    //For better precision, increase sphere cast area by width of parabola segment
    public static bool ParabolicCast(Vector3 start, Vector3 velocity, float gravity, float duration, out ParabolicCastHit hit, int layers = Physics.AllLayers, float precision = 1f, float width = 0f, int recursions = 3)
    {
        int steps;
        float timeStep;

        //If the trajectory is a line (or point), or the precision is null
        if (gravity == 0f || /*(velocity.x == 0f && velocity.z == 0f)  ||*/ precision <= 0f || duration == 0f)
        {
            steps = 1;
            timeStep = duration;
            recursions = 0;
        }
        else
        {
            steps = 1 + Mathf.FloorToInt(precision * duration * (gravity * gravity));
            timeStep = duration / steps;
        }

        float t = 0f;
        Vector3 pos = start;
        for (int i = 1; i <= steps; i++)
        {
            float prevT = t;
            t = timeStep * i;

            Vector3 prev = pos;
            pos = GetPosition(start, velocity, gravity, t);

            bool didHit;
            if (width == 0f)
                didHit = Physics.Linecast(prev, pos, out hit.ray, layers, QueryTriggerInteraction.Ignore);
            else
                didHit = Physics.SphereCast(new Ray(prev, pos - prev), width, out hit.ray, Vector3.Distance(prev, pos), layers, QueryTriggerInteraction.Ignore);//TODO SphereLineCast ?

            if (didHit)
            {
                //Debug.DrawLine(prev, hit.ray.point, Color.Lerp(Color.yellow, Color.red, 0.35f), 5f);

                //Check the last step again, at a higher precision
                /*if (recursions > 0 && false)//TODO check that more recursion will actually fix the problem
                {
                    Vector3 velocityAtT = GetVelocity(velocity, gravity, prevT);
                    didHit = ParabolicCast(prev, velocityAtT, gravity, timeStep, out hit, layers, precision * 10f, recursions - 1);
                    hit.time += prevT;
                    return didHit;
                }
                else*/
                {
                    //Debug.DrawLine(prev, pos, Color.Lerp(Color.yellow, Color.red, 0.35f), 10f);
                    float fraction = hit.ray.distance / Vector3.Distance(prev, pos);
                    hit.time = prevT + fraction * timeStep;
                    hit.velocity = GetVelocity(velocity, gravity, hit.time);
                    hit.end = hit.ray.point;
                    return true;
                }
            }
            //else
            //    Debug.DrawLine(prev, pos, Color.Lerp(Color.yellow, Color.red, 0.35f), 10f);
        }
        hit.ray = new RaycastHit();
        hit.time = duration;
        hit.velocity = GetVelocity(velocity, gravity, duration);
        hit.end = hit.ray.point = GetPosition(start, velocity, gravity, duration);
        return false;
    }
}
