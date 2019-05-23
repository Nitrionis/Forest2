using UnityEngine;

public enum ButtonAction
{
	Start = 0,
	Age_12_14 = 1,
	Age_15_18 = 2,
	Age_19_25 = 3,
	Age_26_35 = 4,
	Age_36_more = 5,
	Female = 6,
	Male = 7
}

public interface IButton
{
	ButtonAction GetActionCode();
}

public class Reticle : MonoBehaviour
{
	public static Reticle instance;

	public LayerMask layerMask;
	public GameObject reticlePointer;
	private Material material;

	public GameObject startMenu;
	public GameObject ageMenu;
	public GameObject genderMenu;

	public GameObject defaultReticle;

	private float timeOver = 0;
	private bool isButton = false;

	private int rangePropId;
	private int colorPropId;

	private bool isReticleActive;

	private GameObject prevHitObject;
	private IButton button = null;

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

		startMenu.SetActive(false);
		ageMenu.SetActive(true);
		genderMenu.SetActive(false);
	}

	private void Update()
	{
		timeOver = Mathf.Clamp01(timeOver + Time.deltaTime);
		material.SetFloat(rangePropId, 1 - timeOver);
		if (timeOver == 1)
		{
			ButtonAction buttonAction = button.GetActionCode();
			if (buttonAction == ButtonAction.Start)
			{
				ReticleChanger.Run();
				reticlePointer.SetActive(false);
				defaultReticle.SetActive(false);
				isReticleActive = false;
			}
			else if (buttonAction == ButtonAction.Male || buttonAction == ButtonAction.Female)
			{
				StatisticsManager.SetGender(buttonAction);
				genderMenu.SetActive(false);
				startMenu.SetActive(true);
			}
			else
			{
				StatisticsManager.SetAge(buttonAction);
				ageMenu.SetActive(false);
				genderMenu.SetActive(true);
			}
		}
	}

	private void FixedUpdate()
	{
		if (isReticleActive)
		{
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20, layerMask))
			{
				if (hit.transform.gameObject != prevHitObject)
				{
					prevHitObject = hit.transform.gameObject;
					button = hit.transform.gameObject.GetComponent<IButton>();
					isButton = true;
					reticlePointer.SetActive(true);
					defaultReticle.SetActive(false);
				}
			}
			else
			{
				prevHitObject = null;
				timeOver = 0;
				isButton = false;
				reticlePointer.SetActive(false);
				defaultReticle.SetActive(true);
			}
		}
	}
}
