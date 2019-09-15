using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Data;

public class WeaponSerializable : Weapon, ISerializationCallbackReceiver
{

    [System.Serializable]
    public struct TreeItem
    {
        public int depth;
        public SubweaponData data;

        public TreeItem(int depth, SubweaponData data)
        {
            this.depth = depth;
            this.data = data;
        }
    }
    [SerializeField]
    protected List<TreeItem> subweaponsData = new List<TreeItem>();

    void UnfoldTree(ref List<SubweaponData> subweapons, int depth)
    {
        if (subweapons != null)
        {
            foreach (SubweaponData sub in subweapons)
            {
                //TODO: remove once serialization is safe enough
                if (subweaponsData.Any(a => a.data == sub))
                {
                    Debug.LogError("Recursive subweapons detected, skipping");
                    continue;
                }

                subweaponsData.Add(new TreeItem(depth, sub));
                UnfoldTree(ref sub.projectile.subweapons, depth + 1);
            }
        }
    }

    int FoldTree(ref List<SubweaponData> subweapons, int depth, int index)
    {
        while (index < subweaponsData.Count)
        {
            SubweaponData subdata = subweaponsData[index].data;
            if (subdata == null)
            {
                subdata = new SubweaponData();
                subweaponsData[index] = new TreeItem(subweaponsData[index].depth, subdata);
            }
            subdata.projectile.subweapons = new List<SubweaponData>();

            subweapons.Add(subdata);
            index++;

            //TODO clean loop
            if (index >= subweaponsData.Count)
                break;

            int newDepth = subweaponsData[index].depth;

            if (newDepth == depth)
                continue;
            if (newDepth < depth)
                break;
            if (newDepth > depth)
                index = FoldTree(ref subdata.projectile.subweapons, depth + 1, index);
        }
        return (index);
    }

    public void OnBeforeSerialize()
    {
        subweaponsData.Clear();
        UnfoldTree(ref weaponData.projectile.subweapons, 0);
    }

    public void OnAfterDeserialize()
    {
        weaponData.projectile.subweapons = new List<SubweaponData>();
        try
        {
            FoldTree(ref weaponData.projectile.subweapons, 0, 0);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}

/*
[CustomPropertyDrawer(typeof(WeaponData))]
public class WeaponDataPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUILayout.PropertyField(property, true);

        if (property.FindPropertyRelative("type").intValue == (int)Data.WeaponType.Gun)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("projectile").FindPropertyRelative("subweapons"), true);
        }
        // foreach (SerializedProperty children in property)
        //     position.height = EditorGUI.GetPropertyHeight(property, false);
        //     EditorGUI.indentLevel = property.depth;
        //     bool isExpanded = property.isExpanded;
        //     property.isExpanded = false;
        //     EditorGUI.PropertyField(position, property, false);
        //     property.isExpanded = isExpanded;
        //     position.y += position.height;

        //     position.height = 20f;
        //     EditorGUI.LabelField(position, "[[" + property.name + "]]");
        //     position.y += position.height;
        // } while(property.NextVisible(property.isExpanded));
        foreach (SerializedProperty a in property)
        {
            if (a.name == "subweapon" || a.name == "subweapons" || a.name == "projectile")
                continue;
            position.y += EditorGUI.GetPropertyHeight(a, true);
            EditorGUI.PropertyField(position, a);
        }


        //EditorGUI.PropertyField(position, property.FindPropertyRelative("projectile").FindPropertyRelative("subweapons"), GUIContent.none);
    }

    // public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    // {
    //     float height = 0;

    //     foreach (SerializedProperty a in property)
    //     {
    //         height += EditorGUI.GetPropertyHeight(a, true);
    //     }
    //     return height;
    // }
}*/

/*[ExecuteInEditMode]
[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Debug.Log("on inspector gui");
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("effect"));

        switch ((WeaponType)serializedObject.FindProperty("type").enumValueIndex) {
            case WeaponType.Contact: 
            break;
            case WeaponType.Explosion: 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosion"));
            break;
            case WeaponType.Gun: 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shot"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("recoil"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectile"));
            break;
            case WeaponType.Shield: 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shield"));
            break;
            
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resource"));

        serializedObject.ApplyModifiedProperties();
        Debug.Log("on inspector gui");
    }
}*/

/*
[CustomPropertyDrawer(typeof(WeaponData))]
public class WeaponDataDrawer : PropertyDrawer
{
    Rect DispChildProperty(Rect position, SerializedProperty property, string name)
    {
        var childProperty = property.FindPropertyRelative(name);
        EditorGUI.PropertyField(position, childProperty, true);
        position.y += EditorGUI.GetPropertyHeight(childProperty);
        return position;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, null, property);
        //serializedObject.Update();

        position = DispChildProperty(position, property, "name");
        position = DispChildProperty(position, property, "type");
        position = DispChildProperty(position, property, "activation");
        position = DispChildProperty(position, property, "effect");

        switch ((WeaponType)property.FindPropertyRelative("type").enumValueIndex) {
            case WeaponType.Contact: 
            break;
            case WeaponType.Explosion: 
            position = DispChildProperty(position, property, "explosion");
            break;
            case WeaponType.Gun:             
            position = DispChildProperty(position, property, "aim");
            position = DispChildProperty(position, property, "shot");
            position = DispChildProperty(position, property, "recoil");
            position = DispChildProperty(position, property, "projectile");
            position = DispChildProperty(position, property, "shield");
            break;
        }
        position = DispChildProperty(position, property, "resource");

        //property.ApplRelativeyModifiedProperties();
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var height = 0f;

        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("name"));
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("type"));
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("activation"));
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("effect"));

        switch ((WeaponType)property.FindPropertyRelative("type").enumValueIndex) {
            case WeaponType.Contact: 
            break;
            case WeaponType.Explosion: 
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("explosion"));
            break;
            case WeaponType.Gun: 
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("aim"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("shot"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("recoil"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("projectile"));
            break;
            case WeaponType.Shield: 
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("shield"));
            break;
            
        }
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("resource"));
        return height;
    }
}
*/
