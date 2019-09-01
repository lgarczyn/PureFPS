using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ADisplayRatio : MonoBehaviour
{
    public abstract void SetRatio(float ratio);
}

public class DisplayBar : ADisplayRatio
{

    public GameObject bar;

    override public void SetRatio(float ratio)
    {
        bar.GetComponent<RectTransform>().localScale = new Vector2(ratio, 1f);
    }
}
