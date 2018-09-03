using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiUpdateObject : MonoBehaviour {

    protected float prevFrameTime;
    protected float deltaTime;
    protected float currentTime;
    protected float nextFrameTime;

    protected float frameRatio
    {
        get
        {
            if (nextFrameTime - prevFrameTime == 0f)
                return (0f);
            return (currentTime - prevFrameTime) / (nextFrameTime - prevFrameTime);
        }
    }

    protected float timeToNextFrame
    {
        get
        {
            return nextFrameTime - currentTime;
        }
    }

    protected float timeSinceLastFrame
    {
        get
        {
            return currentTime - prevFrameTime;
        }
    }

    protected void OnEnable()
    {
        prevFrameTime = currentTime = nextFrameTime = Time.time;
        deltaTime = 0;
        RealOnEnable();
    }

    //Cancel the current waiting time
    /*protected void Abort(float abortTime)
    {
        deltaTime = 0;
        enabled = false;
    }*/

    protected struct Wait
    {
        public enum WaitType
        {
            Time,
            Frame,
            Ever
        }

        public WaitType waitingType;
        public float waitingTime;

        private Wait(WaitType type, float forDelay = 0)
        {
            waitingType = type;
            waitingTime = forDelay;
        }

        public static Wait ForNothing()
        {
            return new Wait(WaitType.Time, 0f);
        }

        public static Wait For(float delay)
        {
            return new Wait(WaitType.Time, delay);
        }

        public static Wait ForFrame()
        {
            return new Wait(WaitType.Frame);
        }

        public static Wait ForEver()
        {
            return new Wait(WaitType.Ever);
        }

    }

	void Update ()
    {
        BeforeUpdates();

        prevFrameTime = nextFrameTime;
        nextFrameTime = Time.time;

        int safety = 0;

        while (currentTime < nextFrameTime)
        {
            Wait wait = MultiUpdate(deltaTime);

            float prevTime = currentTime;
            switch (wait.waitingType)
            {
                case Wait.WaitType.Frame: currentTime = nextFrameTime; break;
                case Wait.WaitType.Time: currentTime += wait.waitingTime; break;
                case Wait.WaitType.Ever: this.enabled = false; currentTime = float.MaxValue; break;
            }
            deltaTime = currentTime - prevTime;

            safety++;
            if (safety > 10000)
            {
                enabled = false;
                Debug.LogError("Too many frames. Endless loop probable.");
                return;
            }
        }

        AfterUpdates();
    }

    protected virtual Wait MultiUpdate(float deltaTime)
    {
        return Wait.ForFrame();
    }

    protected virtual void BeforeUpdates() { }
    protected virtual void AfterUpdates() { }

    protected virtual void RealOnEnable() { }
}
