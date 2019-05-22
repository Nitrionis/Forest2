using UnityEngine;

public class Reticle : MonoBehaviour
{
	public static Reticle instance;

	public LayerMask layerMask;
	public GameObject reticlePointer;
	private Material material;

	public GameObject defaultReticle;

	private float timeOver = 0;
	private bool isButton = false;

	private int rangePropId;
	private int colorPropId;

	private bool isReticleActive;

	private void Start()
	{
		instance = this;
		isReticleActive = true;
		reticlePointer.SetActive(true);
		material = reticlePointer.GetComponent<Renderer>().material;
		rangePropId = Shader.PropertyToID("_ColorRampOffset");
		colorPropId = Shader.PropertyToID("_Color");
		reticlePointer.SetActive(false);
		defaultReticle.SetActive(true);
	}

	private void Update()
	{
		timeOver = Mathf.Clamp01(timeOver + Time.deltaTime);
		material.SetFloat(rangePropId, 1 - timeOver);
		if (timeOver == 1)
		{
			ReticleChanger.Run();
			reticlePointer.SetActive(false);
			defaultReticle.SetActive(false);
			isReticleActive = false;
		}
	}

	private void FixedUpdate()
	{
		if (isReticleActive)
		{
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20, layerMask))
			{
				//Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
				isButton = true;
				reticlePointer.SetActive(true);
				defaultReticle.SetActive(false);
			}
			else
			{
				//Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.cyan);
				timeOver = 0;
				isButton = false;
				reticlePointer.SetActive(false);
				defaultReticle.SetActive(true);
			}
		}
	}
}
