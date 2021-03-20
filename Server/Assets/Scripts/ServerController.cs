using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ServerController : MonoBehaviour {

	public Text ipText;
	public Text rcvText;
	public GameObject obj;
	public GameObject touchProcessor;
	public GameObject faceTracker;
	public Camera renderCamera;

	private Color disconnectColor = new Color(0.8156f, 0.3529f, 0.4313f);
	//private Color connectColor = new Color(0.5254f, 0.7568f, 0.4f);
	private Color connectColor = new Color(0f, 0f, 0f);

	private Vector3 pos = new Vector3(0, 0, 0);

	private TcpListener tcpListener;
	private Thread tcpListenerThread;
	private TcpClient connectedTcpClient;
	private string rcvMsg = "";
	private bool refreshed = false;

	private bool noConnection = true;
	
	void Start () {
		tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests));
		tcpListenerThread.IsBackground = true;
		tcpListenerThread.Start();
	}
	
	void Update () {
		ipText.text = getIPAddress();
		rcvText.text = rcvMsg;
		renderCamera.backgroundColor = (connectedTcpClient == null ? disconnectColor : connectColor);
		if (connectedTcpClient != null && noConnection) {
			sendMessage();
			noConnection = false;
		}
		if (refreshed) {
			getVector();
			refreshed = false;
		}
	}
	
	private void ListenForIncommingRequests () {
		try {
			tcpListener = new TcpListener(IPAddress.Any, 8052);
			tcpListener.Start();
			Debug.Log("Server is listening");
			Byte[] bytes = new Byte[1024];
			while (true) {
				using (connectedTcpClient = tcpListener.AcceptTcpClient()) {
					using (NetworkStream stream = connectedTcpClient.GetStream()) {
						int length;
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							rcvMsg = Encoding.ASCII.GetString(incommingData);
							refreshed = true;
						}
					}
				}
			}
		}
		catch (SocketException socketException) {
			Debug.Log("SocketException " + socketException.ToString());
		}
	}
	
	public void sendMessage() {
		Vector3 tp = touchProcessor.GetComponent<TouchProcessor>().pos;
		Vector3 tv = faceTracker.GetComponent<FaceTracker>().currentObserve;
		if (connectedTcpClient == null) {
			return;
		}
		
		try {			
			NetworkStream stream = connectedTcpClient.GetStream();
			if (stream.CanWrite) {
				string serverMessage =
					tp.x + "," +
					tp.y + "," +
					tp.z + "," +
					tv.x + "," +
					tv.y + "," +
					tv.z + ","
					;
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
				Debug.Log("Server sent his message - should be received by client");
			}
		}
		catch (SocketException socketException) {
			Debug.Log("Socket exception: " + socketException);
		}
	}
	
	private string getIPAddress() {
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new System.Exception("No network adapters with an IPv4 address in the system!");
	}

	private void getVector() {
		string[] temp = rcvMsg.Split(',');
		if (!touchProcessor.GetComponent<TouchProcessor>().isPanning) {
			float x = System.Convert.ToSingle(temp[0]);
			float y = System.Convert.ToSingle(temp[1]);
			float z = System.Convert.ToSingle(temp[2]);
			Vector3 v = new Vector3(x, y, z);
			touchProcessor.GetComponent<TouchProcessor>().pos = v;
		}
		touchProcessor.GetComponent<TouchProcessor>().angle = System.Convert.ToSingle(temp[3]);
	}

}