using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelController : MonoBehaviour
{
	public GameObject sender;
	public GameObject renderCam;
	private bool useFaceTrack = false;

	private string address = "172.20.10.6";
	//Macbook local connecting to iPhone hotspot: 172.20.10.2
	//Samsung connecting to iPhone hotspot: 172.20.10.6
	//Samsung connecting to xdd44's wifi: 192.168.0.106
	//Macbook local connecting to xdd44's wifi: 192.168.0.101
	//iPhone connecting to iPhone hotspot: 10.150.153.190

	private float camWidth;
	private float camHeight;
	private float angle = 2 * Mathf.PI / 3;

	private Vector3 pos = new Vector3(0, 0, 0.6f);
	private Vector3 defaultPos = new Vector3(0, 0, 0.6f);
	private Vector3 prevTouch;
	private float moveSensitive = 0.01f;

	private Vector3 observe = new Vector3(0, 0, -5f);
	private Vector3 defaultObserve = new Vector3(0, 0, -5f);
	
	void Start() {
		Camera cam = Camera.main;
		camHeight = 2f * cam.orthographicSize;
		camWidth = camHeight * cam.aspect;
		transform.rotation = Quaternion.Euler(0, 180 * angle / Mathf.PI - 180, 0);
	}

	void Update() {
		updateFov();
		updatePos();
	}

	void updateFov() {
		Camera cam = renderCam.GetComponent<Camera>();
		float fovHorizontal = Mathf.Atan(-(Mathf.Abs(renderCam.transform.position.x) + camWidth / 2) / renderCam.transform.position.z) * 2;
		fovHorizontal = fovHorizontal * 180 / Mathf.PI;
		fovHorizontal = Camera.HorizontalToVerticalFieldOfView(fovHorizontal, cam.aspect);
		float fovVertical = Mathf.Atan(-(Mathf.Abs(renderCam.transform.position.y) + camHeight / 2) / renderCam.transform.position.z) * 2;
		fovVertical = fovVertical * 180 / Mathf.PI;
		cam.fieldOfView = (fovVertical > fovHorizontal ? fovVertical : fovHorizontal);
	}

	void updatePos() {
		if (Input.touchCount > 0) {
			Touch touch = Input.GetTouch(0);
			if (touch.position.y < 1400f && touch.position.y > 150f) {
				Vector2 tp = new Vector2(touch.position.x, touch.position.y);
				switch (touch.phase) {
					case TouchPhase.Began:
						prevTouch = tp;
						break;
					case TouchPhase.Moved:
						pos += moveSensitive * (new Vector3(tp.x, tp.y, 0) - new Vector3(prevTouch.x, prevTouch.y, 0));
						prevTouch = tp;
						sender.GetComponent<ClientController>().sendMessage(convertToServer(pos));
						break;
				}
			}
		}
		if (Input.GetMouseButtonDown(0) && Input.mousePosition.y < 1400f && Input.mousePosition.y > 150f) {
			Vector2 tp = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			prevTouch = tp;
		}
		else if (Input.GetMouseButton(0) && Input.mousePosition.y < 1400f && Input.mousePosition.y > 150f) {
			Vector2 tp = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			pos += moveSensitive * (new Vector3(tp.x, tp.y, 0) - new Vector3(prevTouch.x, prevTouch.y, 0));
			Debug.Log(convertToServer(new Vector3(tp.x, tp.y, 0)) + " " + convertToServer(new Vector3(prevTouch.x, prevTouch.y, 0)));
			prevTouch = tp;
			sender.GetComponent<ClientController>().sendMessage(convertToServer(pos));
		}
		transform.position = pos;
	}

	private Vector3 convertFromServer(Vector3 v) {
		Vector3 origin = new Vector3(camWidth / 2 + camWidth * Mathf.Cos(Mathf.PI - angle) / 2, 0, - camWidth * Mathf.Sin(Mathf.PI - angle) / 2);
		Vector3 x = new Vector3(Mathf.Cos(Mathf.PI - angle), 0, - Mathf.Sin(Mathf.PI - angle));
		Vector3 z = new Vector3(Mathf.Cos(angle - Mathf.PI / 2), 0, Mathf.Sin(angle - Mathf.PI / 2));
		v -= origin;
		return new Vector3(multXZ(v, x), v.y, multXZ(v, z));
	}

	private Vector3 convertToServer(Vector3 v) {
		Vector3 origin = new Vector3(- camWidth / 2 - camWidth * Mathf.Cos(Mathf.PI - angle) / 2, 0, - camWidth * Mathf.Sin(Mathf.PI - angle) / 2);
		Vector3 x = new Vector3(Mathf.Cos(Mathf.PI - angle), 0, Mathf.Sin(Mathf.PI - angle));
		Vector3 z = new Vector3(-Mathf.Cos(angle - Mathf.PI / 2), 0, Mathf.Sin(angle - Mathf.PI / 2));
		v -= origin;
		return new Vector3(multXZ(v, x), v.y, multXZ(v, z));
	}

	private float multXZ(Vector3 from, Vector3 to) {
		return from.x * to.x + from.z * to.z;
	}

	public void receivePos(Vector3 p) {
		pos = convertFromServer(p);
		// Debug.Log("receive opposite pos: " + p);
		// Debug.Log("converted: " + pos);
	}

	public void receiveView(Vector3 p) {
		observe = convertFromServer(p);
	}

	public void switchObservationMode() {
		if (useFaceTrack) {
			useFaceTrack = false;
			observe = defaultObserve;
		} else {
			useFaceTrack = true;
		}
	}

	public void resetAll() {
		pos = defaultPos;
		sender.GetComponent<ClientController>().ConnectToTcpServer(address);
	}

}
