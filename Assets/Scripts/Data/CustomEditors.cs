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


[CustomEditor(typeof(Affectable))]
public class AffectableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Add effect"))
        {
            ((Affectable)target).Apply(((Affectable)target).toAdd, Time.time + 1f, Vector3.zero, Vector3.zero);
        }
    }
}

[CustomEditor(typeof(PlayerControllerAffectable))]
public class PlayerControllerAffectableEditor : AffectableEditor { }

[CustomEditor(typeof(RigidBodyAffectable))]
public class RigidBodyAffectableEditor : AffectableEditor { }
