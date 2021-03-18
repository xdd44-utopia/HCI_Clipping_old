using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFieldController : MonoBehaviour
{
	public GameObject client;
	// Start is called before the first frame update
	void Start() {
		var input = gameObject.GetComponent<InputField>();
		var se= new InputField.SubmitEvent();
		se.AddListener(SubmitName);
		input.onEndEdit = se;
	}

	// Update is called once per frame
	void Update() {
		
	}
	private void SubmitName(string arg) {
		Debug.Log(arg);
		client.GetComponent<ClientController>().ConnectToTcpServer(arg);
 	}
}
