using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Data;


static public class CustomPropertyTools
{
    static public void DisplayWarning(ref Rect pos, string warning)
    {
        EditorGUI.HelpBox(new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight), warning, MessageType.Warning);
        //EditorGUI.LabelField(new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight), warning);

        pos.y += EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(ProjectileData))]
public class ProjectileDataEditor : PropertyDrawer
{
    const int curveWidth = 50;
    const float min = 0;
    const float max = 1;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
       if (checkLifetime(prop))
            CustomPropertyTools.DisplayWarning(ref pos, "Lifetime is not positive");

        EditorGUI.PropertyField(pos, prop, true);
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        int warningSpace = 0;

        if (checkLifetime(prop))
            warningSpace++;

        return EditorGUI.GetPropertyHeight(prop, label) + warningSpace * EditorGUIUtility.singleLineHeight;
    }

    bool checkLifetime(SerializedProperty prop)
    {
        float lifetime = prop
           .FindPropertyRelative("trajectory")
           .FindPropertyRelative("lifetime")
           .floatValue;

        return lifetime <= 0f;
    }

    bool checkWidth(SerializedProperty prop)
    {
        float lifetime = prop
           .FindPropertyRelative("trajectory")
           .FindPropertyRelative("lifetime")
           .floatValue;

        return lifetime <= 0f;
    }
}