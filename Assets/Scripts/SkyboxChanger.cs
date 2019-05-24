using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
	private static SkyboxChanger instance;

	public Light light;
	public Material skybox;

	public float fogDayDistance;
	public float fogNightDistance;

	public Color fogDayColor;
	public Color fogEveningColor;
	public Color fogNightColor;

	public Color nightSkyColor;
	public Color daySkyColor;

	public Material zombieBodyMaterial;
	public Material zombieHeadMaterial;
	private int colorParamId;

	private int gameState = 0;
	private int prevState = -1;

	private float dayNightShiftDuration = 1;
	private float timeCount = 0;
	private int state = 0;

	public Color[] fogNightColors;
	public Color[] zombieColors;
	private Color targetNightColor;

	private Color transitionStartColor;
	private float transitionStartLightIntensity;
	private float transitionFogEnd;

	void Start()
    {
		instance = this;
		light.intensity = 1.08f;
		transitionStartLightIntensity = 1.08f;
		daySkyColor = skybox.GetColor("_Tint");
		fogDayColor = RenderSettings.fogColor;
		colorParamId = Shader.PropertyToID("_Color");

		//Time.timeScale = 16;
	}

    void Update()
    {
		//if (Score.score > 5300 && Time.timeScale != 1)
		//	Time.timeScale = 1;

		timeCount += Time.deltaTime;
		float t = timeCount / dayNightShiftDuration;
		if (state > 0)
		{
			if (t <= 1)
			{
				light.intensity = Mathf.Lerp(transitionStartLightIntensity, 0.2f, t);
				RenderSettings.fogColor = Color.Lerp(transitionStartColor, fogEveningColor, t);
			}
			else if (t <= 2)
			{
				t -= 1;
				RenderSettings.fogColor = Color.Lerp(fogEveningColor, targetNightColor, t);
				RenderSettings.fogEndDistance = Mathf.Lerp(transitionFogEnd, fogNightDistance, t);
			}
			else state = 0;
		}
		if (state < 0)
		{
			if (t <= 1)
			{
				RenderSettings.fogColor = Color.Lerp(targetNightColor, fogEveningColor, t);
				RenderSettings.fogEndDistance = Mathf.Lerp(fogNightDistance, fogDayDistance, t);
			}
			else if (t <= 2)
			{
				t -= 1;
				light.intensity = Mathf.Lerp(0.2f, 1.08f, t);
				RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fogDayColor, t);
			}
			else state = 0;
		}
    }

	public static void DayToNight(bool isLastLevelZombie = false)
	{
		instance.transitionStartColor = RenderSettings.fogColor;
		instance.transitionStartLightIntensity = instance.light.intensity;
		instance.transitionFogEnd = RenderSettings.fogEndDistance;

		int colorThemeId = (int)(Random.value * instance.fogNightColors.Length - 0.001f);
		instance.targetNightColor = instance.fogNightColors[colorThemeId];
		instance.zombieBodyMaterial.SetColor(instance.colorParamId, instance.zombieColors[colorThemeId]);
		instance.zombieHeadMaterial.SetColor(instance.colorParamId, instance.zombieColors[colorThemeId]);
		instance.state = 1;
		instance.timeCount = 0;
	}

	public static void NightToDay(bool isNextLevelZombie = false)
	{
		instance.state = -1;
		instance.timeCount = 0;
	}
}
