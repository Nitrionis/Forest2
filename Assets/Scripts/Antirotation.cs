using UnityEngine;

public class Antirotation : MonoBehaviour
{
	public static Antirotation instance;

	public bool isShaking = false;

	private float duration = 1;
	private float quarter;
	private float timeOver = 0;

	private Quaternion mainRotation = Quaternion.identity;
	private Quaternion positiveDeviation = Quaternion.AngleAxis( 15, Vector3.up);
	private Quaternion negativeDeviation = Quaternion.AngleAxis(-15, Vector3.up);

	void Start()
	{
		instance = this;
		quarter = duration / 4;
		//startRotation = Quaternion.identity;
	}

	void Update()
    {
		timeOver += Time.deltaTime;

	}

	public void Shake()
	{
		timeOver = 0;
		isShaking = true;
	}
}
