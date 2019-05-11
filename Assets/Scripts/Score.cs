using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Score : MonoBehaviour
{
	private static Score instance;

	private Text scoreObject;

	public int scorePerSecond = 1;

	public static int score = 0;
	public static float fscore = 0;

	public static bool isGameStarted = false;

	public static int hp = 3;
	private static float notСollisionTimeCounter;

	public GameObject[] hpStrips;

	public GameObject congratulationComment;
	public GameObject wastedComment;
	public Text currentScoreText;
	public Text recordScoreText;

	private static int prevScore;
	private static bool isFirstPlay;
	private static bool isWasted;

	public static float scoreCoef = 1f;

	static Score()
	{
		isFirstPlay = true;
		isWasted = true;
	}

	void Start()
    {
		instance = this;
		notСollisionTimeCounter = 0;
		scoreObject = gameObject.GetComponent<Text>();
		recordScoreText.text = "Best score " + PlayerPrefs.GetInt("recordScore", 0).ToString();
		congratulationComment.SetActive(false);
		wastedComment.SetActive(false);
		if (isFirstPlay)
		{
			currentScoreText.gameObject.SetActive(false);
		}
		else
		{
			currentScoreText.gameObject.SetActive(true);
			currentScoreText.text = "Current score " + prevScore;
			if (isWasted)
				wastedComment.SetActive(true);
			else
				congratulationComment.SetActive(true);
		}
		isFirstPlay = false;
    }
	
    void Update()
    {
		if (isGameStarted)
		{
			fscore += Time.deltaTime * 16 * scoreCoef;
			score = (int)fscore;
			scoreObject.text = score.ToString();
		}
		notСollisionTimeCounter += Time.deltaTime;
		if (notСollisionTimeCounter >= 30)
		{
			notСollisionTimeCounter -= 30;
			hp = Mathf.Clamp(hp + 1, 1, 3);
			hpStrips[hp - 1].SetActive(true);
		}
	}

	public static void Сollision()
	{
		if (notСollisionTimeCounter > 2 && hp > 0)
		{
			notСollisionTimeCounter = 0;
			instance.hpStrips[hp - 1].SetActive(false);
			hp = Mathf.Max(0, hp - 1);
			if (hp == 0/* && Input.GetKey(KeyCode.R)*/)
			{
				int maxScore = PlayerPrefs.GetInt("recordScore", 0);
				if (score > maxScore)
				{
					isWasted = false;
					PlayerPrefs.SetInt("recordScore", score);
				}
				else isWasted = true;
				isGameStarted = false;
				hp = 3;
				prevScore = score;
				score = 0;
				fscore = 0;
				SceneManager.LoadScene("MainScene");
			}
		}
	}
}
