using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public int buttonNum;
	public GameObject obj;
	void Start() {
		
	}

	// Update is called once per frame
	void Update() {
		
	}
	public void OnPointerDown(PointerEventData eventData){
		switch (buttonNum) {
			case 0: obj.GetComponent<ModelController>().increaseX = true; break;
			case 1: obj.GetComponent<ModelController>().decreaseX = true; break;
			case 2: obj.GetComponent<ModelController>().increaseY = true; break;
			case 3: obj.GetComponent<ModelController>().decreaseY = true; break;
			case 4: obj.GetComponent<ModelController>().increaseZ = true; break;
			case 5: obj.GetComponent<ModelController>().decreaseZ = true; break;
		}
	}
	
	public void OnPointerUp(PointerEventData eventData){
		switch (buttonNum) {
			case 0: obj.GetComponent<ModelController>().increaseX = false; break;
			case 1: obj.GetComponent<ModelController>().decreaseX = false; break;
			case 2: obj.GetComponent<ModelController>().increaseY = false; break;
			case 3: obj.GetComponent<ModelController>().decreaseY = false; break;
			case 4: obj.GetComponent<ModelController>().increaseZ = false; break;
			case 5: obj.GetComponent<ModelController>().decreaseZ = false; break;
		}
	}
}
