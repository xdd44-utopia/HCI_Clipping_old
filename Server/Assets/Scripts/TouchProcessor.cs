using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchProcessor : MonoBehaviour
{
	public GameObject sender;
	public GameObject obj;

	[HideInInspector]
	public bool isPanning;
	private float panTimer;
	private float panDelay = 0.1f;

	private float sendTimer = -1;

	[HideInInspector]
	public Vector3 pos;
	private Vector3 defaultPos;
	[HideInInspector]
	public float angle;
 
	private const float panRatio = 1;
	private const float minPanDistance = 0;

	private Vector3 panDelta;

	void Start()
	{
		pos = new Vector3(0, 0, 0);
		defaultPos = new Vector3(0, 0, 0);
	}

	// Update is called once per frame
	void Update()
	{
		calculate();
		freemoving();

		if (sendTimer < 0) {
			sender.GetComponent<ServerController>().sendMessage();
			sendTimer = 0.05f;
		}
		else {
			sendTimer -= Time.deltaTime;
		}

	}

	private void freemoving() {
		if (panDelta.magnitude > minPanDistance) {
			pos += panDelta;
			panTimer = panDelay;
		}

		isPanning = (panTimer > 0);
		panTimer -= Time.deltaTime;
	}
 
	private void calculate () {
		panDelta = new Vector3(0, 0, 0);
 
		// if two fingers are touching the screen at the same time ...
		if (Input.touchCount == 1) {
			Touch touch1 = Input.touches[0];
			if (touch1.phase == TouchPhase.Moved) {
				panDelta = touch1.deltaPosition;

				if (panDelta.magnitude > minPanDistance) {
					panDelta *= Camera.main.orthographicSize / 772;
				}
				else {
					panDelta = new Vector3(0, 0, 0);
				}
			}
			else if (touch1.phase == TouchPhase.Began) {
				isPanning = true;
			}
			else if (touch1.phase == TouchPhase.Ended) {
				isPanning = false;
			}
		}
		
	}

	private Vector3 processTouchPoint(Vector3 v) {
		v.x -= 360;
		v.y -= 772;
		v *= Camera.main.orthographicSize / 772;
		return v;
	}

	public void resetAll() {
		pos = defaultPos;
	}

}
