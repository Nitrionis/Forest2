using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DataSender : MonoBehaviour
{
	public string serverIpAddress;

	void Start()
	{
		StartCoroutine(Upload());
	}

	private IEnumerator Upload()
	{
		byte[] myData = System.Text.Encoding.UTF8.GetBytes("Unity space");
		UnityWebRequest www = UnityWebRequest.Put("http://10.193.10.119:8080/", myData);
		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			Debug.Log(www.error);
		}
		else
		{
			Debug.Log("Form upload complete!");
		}
	}
}
