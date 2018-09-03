using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	void Start () {
		StartCoroutine(Coroutine());
	}
	
	IEnumerator Coroutine () {
		yield return new WaitForSeconds(10);
		GetComponent<Explosion>().Init();
		GetComponent<Explosion>().Setup();
	}
}
