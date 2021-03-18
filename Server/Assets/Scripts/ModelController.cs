using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelController : MonoBehaviour
{
	public GameObject sender;
	public GameObject renderCam;
	public Text text;
	private bool useFaceTrack = false;

	private float camWidth;
	private float camHeight;

	public bool increaseX = false;
	public bool decreaseX = false;
	public bool increaseY = false;
	public bool decreaseY = false;
	public bool increaseZ = false;
	public bool decreaseZ = false;
	private Vector3 pos = new Vector3(0, 0, 0);
	private Vector3 defaultPos = new Vector3(0, 0, 0.6f);
	private Vector3 prevTouch;
	private float moveSensitive = 0.01f;

	private Vector3 observe = new Vector3(0, 0, -5f);
	private Vector3 defaultObserve = new Vector3(0, 0, -5f);
	private float correction = 2.5f;
	private float smoothSpeed = 5f;
	private float smoothTolerance = 1f;
	private float observationScalePlaner = 50f;
	private float observationScaleVertical = 100f;
	private float observeMoveSensitive = 0.05f;
	
	void Start() {
		Camera cam = Camera.main;
		camHeight = 2f * cam.orthographicSize;
		camWidth = camHeight * cam.aspect;
	}

	void Update() {
		updateObservation();
		updateFov();
		updatePos();
	}

	void updateObservation() {
		if (useFaceTrack) {
			GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
			GameObject testObj = new GameObject();
			Instantiate(testObj, objects[0].transform.position, Quaternion.identity);
			testObj.transform.position = objects[0].transform.position;
			objects = GameObject.FindGameObjectsWithTag("FacePosition");
			testObj.transform.RotateAround(
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				-objects[0].transform.rotation.eulerAngles.y
			);
			testObj.transform.RotateAround(
				new Vector3(0f, 0f, 0f),
				new Vector3(1f, 0f, 0f),
				-objects[0].transform.rotation.eulerAngles.x
			);
			testObj.transform.RotateAround(
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 0f, 1f),
				-objects[0].transform.rotation.eulerAngles.z
			);
			observe = new Vector3(
				testObj.transform.position.x,
				testObj.transform.position.y,
				-testObj.transform.position.z
			);
			observe.x *= observationScalePlaner;
			observe.y *= observationScalePlaner;
			observe.y += correction;
			observe.z *= observationScaleVertical;
			Destroy(testObj, 0f);
		}
		else {
			if (increaseX) { observe.x += observeMoveSensitive; }
			if (decreaseX) { observe.x -= observeMoveSensitive; }
			if (increaseY) { observe.y += observeMoveSensitive; }
			if (decreaseY) { observe.y -= observeMoveSensitive; }
			if (increaseZ) { observe.z += observeMoveSensitive; }
			if (decreaseZ) { observe.z -= observeMoveSensitive; }
			text.text = "Manual mode";
		}
		if (Vector3.Distance(renderCam.transform.position, observe) > smoothTolerance) {
			renderCam.transform.position = Vector3.Lerp(renderCam.transform.position, observe, smoothSpeed * Time.deltaTime);
			sender.GetComponent<ServerController>().sendMessage();
		}
		text.text = "Face pos: " + renderCam.transform.position;
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
			if (touch.position.y < 850f && touch.position.y > 300f) {
				Vector2 tp = new Vector2(touch.position.x, touch.position.y);
				switch (touch.phase) {
					case TouchPhase.Began:
						prevTouch = tp;
						break;
					case TouchPhase.Moved:
						pos += moveSensitive * (new Vector3(tp.x, tp.y, 0) - new Vector3(prevTouch.x, prevTouch.y, 0));
						prevTouch = tp;
						sender.GetComponent<ServerController>().sendMessage();
						break;
				}
			}
		}
		if (Input.GetMouseButtonDown(0) && Input.mousePosition.y < 2500f && Input.mousePosition.y > 300f) {
			Vector2 tp = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			prevTouch = tp;
		}
		else if (Input.GetMouseButton(0) && Input.mousePosition.y < 2500f && Input.mousePosition.y > 300f) {
			Vector2 tp = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			pos += moveSensitive * (new Vector3(tp.x, tp.y, 0) - new Vector3(prevTouch.x, prevTouch.y, 0));
			prevTouch = tp;
			sender.GetComponent<ServerController>().sendMessage();
		}
		transform.position = pos;
	}

	public void receivePos(Vector3 p) {
		//posOpposite = new Vector3(2.5f - p.x / 2, p.y, Mathf.Sqrt(3f) * (p.x + 5f) / 2);
		pos = p;
		Debug.Log("receive opposite pos: " + p);
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
		useFaceTrack = false;
		pos = defaultPos;
		observe = defaultObserve;
	}

}
