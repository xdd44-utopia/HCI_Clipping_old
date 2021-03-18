using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ClientController : MonoBehaviour {

	public GameObject obj;
	public Camera renderCamera;

	private Color disconnectColor = new Color(0.8156f, 0.3529f, 0.4313f);
	//private Color connectColor = new Color(0.5254f, 0.7568f, 0.4f);
	private Color connectColor = new Color(0f, 0f, 0f);

	private Vector3 pos = new Vector3(0, 0, 0);
	private Vector3 view = new Vector3(0, 0, 0);
	private bool isRefreshed = false;

	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	
	void Start () {
		socketConnection = null;
		//ConnectToTcpServer("1.1.1.1");
	}
	
	void Update () {
		renderCamera.backgroundColor = (socketConnection == null ? disconnectColor : connectColor);
		if (isRefreshed) {
			obj.GetComponent<ModelController>().receivePos(pos);
			obj.GetComponent<ModelController>().receiveView(view);
			isRefreshed = false;
		}
	}
	
	public void ConnectToTcpServer (string ipText) {
		try {
			clientReceiveThread = new Thread(() => ListenForData(ipText));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e) {
			Debug.Log("On client connect exception " + e);
		}
	}
	
	private void ListenForData(string ipText) {
		socketConnection = null;
		try {
			socketConnection = new TcpClient(ipText, 8052);
			Byte[] bytes = new Byte[1024];
			while (true) {
				using (NetworkStream stream = socketConnection.GetStream()) {
					int length;
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						string serverMessage = Encoding.ASCII.GetString(incommingData);
						getVector(serverMessage);
						isRefreshed = true;
					}
				}
			}
		}
		catch (SocketException socketException) {
			Debug.Log("Socket exception: " + socketException);
		}
	}
	
	public void sendMessage(Vector3 pos) {
		if (socketConnection == null) {
			return;
		}
		try {		
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite) {
				string clientMessage = pos.x + "," + pos.y + "," + pos.z + ",";
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException) {
			Debug.Log("Socket exception: " + socketException);
		}
	}

	private void getVector(string str) {
		string[] temp = str.Split(',');
		pos = new Vector3(System.Convert.ToSingle(temp[0]), System.Convert.ToSingle(temp[1]), System.Convert.ToSingle(temp[2]));
		view = new Vector3(System.Convert.ToSingle(temp[3]), System.Convert.ToSingle(temp[4]), System.Convert.ToSingle(temp[5]));
	}
}