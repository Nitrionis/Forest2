using UnityEngine;

public class СollisionСheck : MonoBehaviour
{
	void OnCollisionEnter()
	{
		CameraMover.FatalСollision();
	}

	void OnTriggerEnter()
	{
		CameraMover.FatalСollision();
	}
}
