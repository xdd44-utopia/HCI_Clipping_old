using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
	public GameObject sender;
	public GameObject otherPoint;
	public LineRenderer otherFrame;
	public Text text;
	private bool useFaceTrack = false;

	private bool increaseX = false;
	private bool decreaseX = false;
	private bool increaseY = false;
	private bool decreaseY = false;
	private bool increaseZ = false;
	private bool decreaseZ = false;

	private Vector3 posOpposite = new Vector3(0, 0, 0);
	private Vector3 posSelf = new Vector3(0, 0, 0);
	private float delayTimer = 0;
	private float delayTolerance = 25f;
	private MeshRenderer meshRenderer;
	private LineRenderer lineRenderer;

	private Vector3 observe = new Vector3(0, 0, -200f);
	private Vector3 defaultObserve = new Vector3(0, 0, -200f);
	private float observationScale = 150f;
	private float observeMoveSensitive = 0.025f;
	private float lineWidth = 0.25f;
	
	void Start() {
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.enabled = false;
		lineRenderer = this.gameObject.AddComponent<LineRenderer>();
		lineRenderer.alignment = LineAlignment.TransformZ;
		lineRenderer.startWidth = lineWidth;
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		lineRenderer.positionCount = 2;
		lineRenderer.startColor = new Color(1, 1, 1, 1);
		lineRenderer.endColor = new Color(1, 1, 1, 1);
		lineRenderer.enabled = false;
		otherPoint.SetActive(false);
	}

	void Update() {
		if (delayTimer > 0) {
			delayTimer -= delayTolerance * Time.deltaTime;
		}
		//Debug.Log(delayTimer);
		updateObservation();
		updatePoint();
		updateLine();
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
			) * observationScale;
			text.text = "Face pos: " + observe;
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
	}

	void updatePoint() {
		if (Input.touchCount > 0) {
			Touch touch = Input.GetTouch(0);
			processTouchPoint(new Vector2(touch.position.x, touch.position.y));
		}
		else if (Input.GetMouseButton(0)){
			processTouchPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		}
		else {
			meshRenderer.enabled = false;
		}
	}

	void processTouchPoint(Vector2 v) {
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(v);
		if (Physics.Raycast(ray, out hit)) {
			transform.position = hit.point;
		}
		//sender.GetComponent<ServerController>().sendMessage(new Vector2(transform.position.x, transform.position.y));
		meshRenderer.enabled = true;
	}

	void updateLine() {
		updateProjection();
		lineRenderer.enabled = (meshRenderer.enabled && delayTimer > 0);
		otherPoint.SetActive(delayTimer > 0);
	}

	void updateProjection() {
		lineRenderer.SetPosition(0, transform.position);
		lineRenderer.SetPosition(1, getProjection(posOpposite));
		lineRenderer.endWidth = lineWidth / (observe.z - posOpposite.z) * observe.z;

		otherPoint.transform.position = getProjection(posOpposite);
		otherPoint.transform.localScale = new Vector3(observe.z / (observe.z - posOpposite.z), observe.z / (observe.z - posOpposite.z), observe.z / (observe.z - posOpposite.z));

		otherFrame.SetPosition(1, getProjection(new Vector3(0f, -11f, 8.66f)));
		otherFrame.SetPosition(2, getProjection(new Vector3(0f, 11f, 8.66f)));
	}

	Vector3 getProjection(Vector3 v) {
		v -= new Vector3(observe.x, observe.y, 0);
		v.x = v.x / (observe.z - v.z) * observe.z;
		v.y = v.y / (observe.z - v.z) * observe.z;
		v += new Vector3(observe.x, observe.y, 0);
		return v;
	}

	public void receivePos(Vector2 p) {
		posOpposite = new Vector3(2.5f - p.x / 2, p.y, Mathf.Sqrt(3f) * (p.x + 5f) / 2);
		delayTimer = 1f;
		Debug.Log("receive opposite pos: " + p);

	}

	private void observeXIncrease() { observe.x += observeMoveSensitive; }
	private void observeXDecrease() { observe.x -= observeMoveSensitive; }
	private void observeYIncrease() { observe.y += observeMoveSensitive; }
	private void observeYDecrease() { observe.y -= observeMoveSensitive; }
	private void observeZIncrease() { observe.z += observeMoveSensitive; }
	private void observeZDecrease() { observe.z -= observeMoveSensitive; }

	public void startIncreaseX() { increaseX = true; }
	public void stopIncreaseX() { increaseX = false; }
	public void startDecreaseX() { decreaseX = true; }
	public void stopDecreaseX() { decreaseX = false; }
	public void startIncreaseY() { increaseY = true; }
	public void stopIncreaseY() { increaseY = false; }
	public void startDecreaseY() { decreaseY = true; }
	public void stopDecreaseY() { decreaseY = false; }
	public void startIncreaseZ() { increaseZ = true; }
	public void stopIncreaseZ() { increaseZ = false; }
	public void startDecreaseZ() { decreaseZ = true; }
	public void stopDecreaseZ() { decreaseZ = false; }

	public void switchObservationMode() {
		if (useFaceTrack) {
			useFaceTrack = false;
			observe = defaultObserve;
		} else {
			useFaceTrack = true;
		}
	}

}
