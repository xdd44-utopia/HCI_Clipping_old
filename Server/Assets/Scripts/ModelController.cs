using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelController : MonoBehaviour
{
	public GameObject touchProcessor;
	public Text text;
	
	void Start() {
	}

	void Update() {
		transform.position = touchProcessor.GetComponent<TouchProcessor>().pos;
	}

}
