using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Data;
using UnityEditor;

public class WeaponSerializable : Weapon, ISerializationCallbackReceiver {
    
    [SerializeField]
    protected List<SubWeaponData> subWeaponsData = new List<SubWeaponData>();

    public void OnBeforeSerialize()
    {
        subWeaponsData.Clear();

        SubWeaponData sub = weaponData.projectile.subweapon;

        while (sub != null)
        {
            subWeaponsData.Add(sub);
            sub = sub.projectile.subweapon;
        }
    }

    public void OnAfterDeserialize()
    {
        for (int i = 0; i < subWeaponsData.Count; i++)
            if (subWeaponsData[i] == null)
                subWeaponsData[i] = new SubWeaponData();

        SubWeaponData node = weaponData.projectile.subweapon = subWeaponsData.FirstOrDefault();

        for (int i = 1; node != null && i < subWeaponsData.Count; i++)
        {
            node.projectile.subweapon = subWeaponsData[i];
            node = node.projectile.subweapon;
        }
    }
}

/*
[CustomEditor(typeof(WeaponSerializable))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WeaponSerializable myScript = (WeaponSerializable)target;
        if (GUILayout.Button("Load1"))
        {

        }
    }
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
        position = DispChildProperty(position, property, "effect");

        switch ((WeaponType)property.FindPropertyRelative("type").enumValueIndex) {
            case WeaponType.Contact: 
            break;
            case WeaponType.Explosion: 
            position = DispChildProperty(position, property, "explosion");
            break;
            case WeaponType.Gun: 
            position = DispChildProperty(position, property, "shot");
            position = DispChildProperty(position, property, "recoil");
            position = DispChildProperty(position, property, "projectile");
            break;
            case WeaponType.Shield: 
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
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("effect"));

        switch ((WeaponType)property.FindPropertyRelative("type").enumValueIndex) {
            case WeaponType.Contact: 
            break;
            case WeaponType.Explosion: 
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("explosion"));
            break;
            case WeaponType.Gun: 
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



[CustomPropertyDrawer(typeof(SubWeaponData))]
public class SubWeaponDataDrawer : PropertyDrawer
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
            break;
            case WeaponType.Shield: 
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