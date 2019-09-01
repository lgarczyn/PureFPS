using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class XRayable : MonoBehaviour {

	static Material materialOn;
	static Material materialOff;

	private void Start() {
		if (materialOff == null) {
			materialOn = GetComponent<MeshRenderer>().sharedMaterial;
			materialOff = GetComponent<MeshRenderer>().sharedMaterial;
			materialOff = new Material(materialOff);
			materialOff.SetShaderPassEnabled("Always", false);
		}
	}
	public void SetOutlineEnabled(bool value) {
		Start();
		Debug.Log("setting shader pass as :" + value);
        //GetComponent<MeshRenderer>().sharedMaterial.SetShaderPassEnabled("Always", value);
        //GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_AlphaStrength", value ? 8f : 0f);
        //GetComponent<MeshRenderer>().SetPropertyBlock(value ? on : off, 0);
        //GetComponent<MeshRenderer>().sharedMaterial.SetPropertyBlock(value ? on : off, 0);
		GetComponent<MeshRenderer>().sharedMaterial = value ? materialOn : materialOff;
	}
}



[CustomEditor(typeof(XRayable))]
public class OutlineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        XRayable xray = (XRayable)target;

		if(GUILayout.Button("On"))
			xray.SetOutlineEnabled(true);

		if(GUILayout.Button("Off"))
			xray.SetOutlineEnabled(false);
    }
}
