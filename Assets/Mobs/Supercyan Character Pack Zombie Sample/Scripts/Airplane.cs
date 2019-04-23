using UnityEngine;

public class Airplane : MonoBehaviour
{
	public float moveSpeed = 20;
	public float rotationSpeed = 60;
	private Vector3 startPosition;
	private Quaternion startRotation;
    // Start is called before the first frame update
    private void Start()
    {
		startPosition = transform.position;
		startRotation = transform.rotation;
	}

    // Update is called once per frame
    private void Update()
    {
		transform.position += moveSpeed * -transform.forward * Time.deltaTime;
		transform.Rotate(rotationSpeed * Vector3.right * Time.deltaTime);
	}

	public void ResetPosition()
	{
		transform.position = startPosition;
		transform.rotation = startRotation;
	}
}
