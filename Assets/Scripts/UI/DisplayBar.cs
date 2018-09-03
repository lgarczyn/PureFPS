using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ADisplayRatio:MonoBehaviour
{
    public abstract void SetRatio(float ratio);
}

public class DisplayBar : ADisplayRatio {

    public GameObject bar;
    public bool zeroOnLeft;

    override public void SetRatio(float ratio)
    {
        if (zeroOnLeft)
        {
            bar.GetComponent<RectTransform>().anchorMax = new Vector2(ratio, 1f);
            bar.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
        }
        else
        {
            bar.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            bar.GetComponent<RectTransform>().anchorMin = new Vector2(1f - ratio, 0f);
        }
    }
}
