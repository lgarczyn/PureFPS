using UnityEngine;

public class MultiUpdateObject : MonoBehaviour
{

    protected float prevFrameTime;
    protected float nextFrameTime;
    protected float timeToNextUpdate;
    protected float rofMultiplier;

    protected float currentTime
    {
        get
        {
            return prevFrameTime + timeToNextUpdate;
        }
    }

    protected void OnEnable()
    {
        prevFrameTime = nextFrameTime = Time.time;
        timeToNextUpdate = 0;
        rofMultiplier = 1f;
        RealOnEnable();
    }

    protected void Reset(float time)
    {
        prevFrameTime = nextFrameTime = time;
        timeToNextUpdate = 0;
        rofMultiplier = 1f;
    }

    [System.Serializable]
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

    void CallMultiUpdates()
    {
        float remainingTime = nextFrameTime - prevFrameTime;
        remainingTime *= rofMultiplier;

        int safety = 0;

        while (timeToNextUpdate <= remainingTime)
        {
            remainingTime -= timeToNextUpdate;

            float frameRatio = 1f - (remainingTime) / (nextFrameTime - prevFrameTime);//?

            Wait wait = MultiUpdate(timeToNextUpdate, frameRatio);

            switch (wait.waitingType)
            {
                case Wait.WaitType.Frame:
                    timeToNextUpdate = 0;
                    return;
                case Wait.WaitType.Ever:
                    timeToNextUpdate = float.MaxValue;
                    this.enabled = false;
                    return;
                case Wait.WaitType.Time:
                    timeToNextUpdate = wait.waitingTime;
                    break;
            }

            safety++;
            if (safety > 10000)
            {
                enabled = false;
                Debug.LogError("Too many frames. Endless loop probable.");
                break;
            }
        }
        timeToNextUpdate -= remainingTime * rofMultiplier;
    }

    void Update()
    {
        prevFrameTime = nextFrameTime;
        nextFrameTime = Time.time;

        BeforeUpdates();
        CallMultiUpdates();
        AfterUpdates();
    }

    protected virtual Wait MultiUpdate(float deltaTime, float frameRatio)
    {
        return Wait.ForFrame();
    }

    protected void SetRofMultiplier(float multiplier)
    {
        rofMultiplier = multiplier;
    }

    protected virtual void BeforeUpdates() { }
    protected virtual void AfterUpdates() { }

    protected virtual void RealOnEnable() { }
}
