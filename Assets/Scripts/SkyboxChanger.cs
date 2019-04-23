using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
	private static SkyboxChanger instance;

	public Light light;
	public Material skybox;

	public Color fogDayColor;
	public Color eveningColor;
	public Color fogNightColor;

	public Color nightSkyColor;
	public Color daySkyColor;

	private int gameState = 0;
	private int prevState = -1;

	private float dayNightShiftDuration = 1;
	private float timeCount = 0;
	private int state = 0;

	void Start()
    {
		instance = this;
		light.intensity = 1.08f;
		daySkyColor = skybox.GetColor("_Tint");
		fogDayColor = RenderSettings.fogColor;
	}

    void Update()
    {
		timeCount += Time.deltaTime;
		float t = timeCount / dayNightShiftDuration;
		if (state > 0)
		{
			if (t <= 1)
			{
				light.intensity = Mathf.Lerp(1.08f, 0.2f, t);
				RenderSettings.fogColor = Color.Lerp(fogDayColor, eveningColor, t);
			}
			else if (t <= 2)
			{
				t -= 1;
				RenderSettings.fogColor = Color.Lerp(eveningColor, fogNightColor, t);
			}
			else state = 0;
		}
		if (state < 0)
		{
			if (t <= 1)
			{
				RenderSettings.fogColor = Color.Lerp(fogNightColor, eveningColor, t);
			}
			else if (t <= 2)
			{
				t -= 1;
				light.intensity = Mathf.Lerp(0.2f, 1.08f, t);
				RenderSettings.fogColor = Color.Lerp(eveningColor, fogDayColor, t);
			}
			else state = 0;
		}
    }

	public static void DayToNight()
	{
		instance.state = 1;
		instance.timeCount = 0;
	}

	public static void NightToDay()
	{
		instance.state = -1;
		instance.timeCount = 0;
	}
}
