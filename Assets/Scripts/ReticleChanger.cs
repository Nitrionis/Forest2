using UnityEngine;

public class ReticleChanger : MonoBehaviour
{
	public static ReticleChanger instance;

	public GameObject reticle;
	private Material material;
	private int sizeId;
	private int colorId;
	public Color disabledColor;
	public Color activeColor;
	private Color currColor;
	private bool isActive;
	private float timeCount;
	private readonly float timeOffset = 0.5f;
	public GameObject[] deactivatableObjectsList;
	//public Text recordScoreText;
	// Start is called before the first frame update
	void Start()
    {
		instance = this;
		timeCount = 0;
		isActive = false;
		currColor = disabledColor;
		material = reticle.GetComponent<Renderer>().material;
		sizeId = Shader.PropertyToID("_InnerCoef");
		colorId = Shader.PropertyToID("_RetColor");
		//recordScoreText.text = PlayerPrefs.GetInt("recordScore", 0).ToString();
	}

    // Update is called once per frame
    void Update()
    {
        if (isActive)
		{
			timeCount += Time.deltaTime;
			float t = Mathf.Clamp01(0.7f * timeCount - timeOffset);
			material.SetFloat(sizeId, Mathf.Lerp(1.0f, 0.01f, t));
			material.SetColor(colorId, Color.Lerp(disabledColor, activeColor, t));
			if (t >= 1)
			{
				Run();
			}
		}
    }

	public void Activate()
	{
		isActive = true;
	}

	public void Deactivate()
	{
		isActive = false;
		currColor = disabledColor;
		timeCount = 0;
		material.SetFloat(sizeId, 1.0f);
		material.SetColor(colorId, disabledColor);
	}

	public static void Run()
	{
		Score.isGameStarted = true;
		foreach (var o in instance.deactivatableObjectsList)
			o.SetActive(false);
	}
}
