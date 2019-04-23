using UnityEngine;
using UnityEngine.UI;

public class CameraMover : MonoBehaviour
{
	public static CameraMover instance;

	public Transform movebleObject;
	public Transform antirotationObject;
	public Slider slider;
	public float moveSpeed = 10;
	public float rotationSpeed = 45;
	public float moveRotationSpeed = 100;
	public float maxAngle = 60;
	private float minAngle;
	private float currRotation = 0;
	public Vector3 mapDir;
	public Vector3 moveDir;
	public Vector3 upAxis;
	private Quaternion startRotation;

	public Color crashScreenColor;
	public enum Shake
	{
		Non, Fatal, Nonfatal
	}
	public Shake shakeState = Shake.Non;
	private const float shakeDuration = 0.4f;
	private const float shakeFrequency = 1;
	private int shakeCount = 0;
	private Quaternion minShakeRotation;
	private Quaternion maxShakeRotation;
	private float timeCount = 0;
	public Transform plane;
	private Quaternion planeStartRotation;
	public GameObject quad;
	private int colorId;
	private Material quadMaterial;
	private bool isSpeedLock = false;

	void Start()
    {
		instance = this;

		planeStartRotation = plane.transform.rotation;

		minAngle = -maxAngle;
		mapDir = Quaternion.Euler(10, 0, 0) * Vector3.forward;
		upAxis = Vector3.Cross(mapDir, Vector3.right);
		startRotation = movebleObject.rotation;

		minShakeRotation = Quaternion.Euler(-10, 0,-5);
		maxShakeRotation = Quaternion.Euler(30, 0, 5);

		quadMaterial = quad.GetComponent<Renderer>().material;
		colorId = Shader.PropertyToID("_Color");
		quadMaterial.SetColor(colorId, new Color(1, 1, 1, 0));
		//rigidbody = movebleObject.GetComponent<Rigidbody>();
	}

	private Color lastColor;

	void Update()
    {
		if (Input.GetKey(KeyCode.A))
		{
			currRotation = Mathf.Max(currRotation - rotationSpeed * Time.deltaTime, minAngle);
		}
		if (Input.GetKey(KeyCode.D))
		{
			currRotation = Mathf.Min(currRotation + rotationSpeed * Time.deltaTime, maxAngle);
		}
		currRotation = transform.rotation.eulerAngles.y;
		if (currRotation > 180)
			currRotation = currRotation - 360;
		currRotation = Mathf.Clamp(currRotation, minAngle, maxAngle);

		slider.value = (currRotation + maxAngle) / (maxAngle * 2);

		Quaternion rotation = Quaternion.AngleAxis(currRotation, upAxis);
		moveDir = Vector3.Lerp(moveDir, rotation * mapDir, Time.deltaTime * moveRotationSpeed);

		movebleObject.rotation = Quaternion.AngleAxis(currRotation, Vector3.up) * startRotation;

		movebleObject.position += Time.deltaTime * moveSpeed * moveDir;
		if (Input.GetKey(KeyCode.A))
			movebleObject.position += Time.deltaTime * moveSpeed * Vector3.left;
		if (Input.GetKey(KeyCode.D))
			movebleObject.position += Time.deltaTime * moveSpeed * Vector3.right;
		if (Input.GetKey(KeyCode.W))
			movebleObject.position += Time.deltaTime * moveSpeed * mapDir;

		if (shakeState == Shake.Fatal)
		{
			timeCount += Time.deltaTime;
			float t = timeCount;
			if (timeCount <= shakeDuration)
			{
				quadMaterial.SetColor(colorId, new Color(1, 0.5f, 0.5f, Mathf.Clamp01(2*t)));
				t = t / shakeDuration;
				antirotationObject.rotation = Quaternion.Lerp(Quaternion.identity, minShakeRotation, t);
			}
			else if (timeCount <= 2 * shakeDuration)
			{
				lastColor = new Color(1, 0.5f, 0.5f, 1 - t);
				quadMaterial.SetColor(colorId, lastColor);
				t = (t - shakeDuration) / shakeDuration;
				antirotationObject.rotation = Quaternion.Lerp(minShakeRotation, maxShakeRotation, t);
			}
			else if (timeCount <= 3 * shakeDuration)
			{
				quadMaterial.SetColor(colorId, new Color(1, 0.5f, 0.5f, Mathf.Lerp(lastColor.a, 0, t)));
				t = (t - 2 * shakeDuration) / shakeDuration;
				antirotationObject.rotation = Quaternion.Lerp(maxShakeRotation, Quaternion.identity, t);
			}
			else
			{
				quadMaterial.SetColor(colorId, new Color(1, 0.5f, 0.5f, 0));
				timeCount = 0;
				shakeState = Shake.Non;
			}
		}
		else
		{
			antirotationObject.rotation = Quaternion.identity;
		}
		plane.transform.rotation = planeStartRotation;
	}

	public static void FatalСollision()
	{
		instance.shakeState = Shake.Fatal;
	}

	public static void NonfatalСollision()
	{
		//instance.shakeState = Shake.Nonfatal;
	}

	public static void LockSpeed(float speed)
	{
		instance.isSpeedLock = true;
		instance.moveSpeed = speed;
	}

	public static void UnlockSpeed()
	{
		instance.isSpeedLock = false;
	}

	public static void SetSpeed(float speed)
	{
		if (!instance.isSpeedLock)
			instance.moveSpeed = speed;
	}
}
