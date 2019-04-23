using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Score : MonoBehaviour
{
	private Text scoreObject;

	public int scorePerSecond = 1;

	public static int score = 0;
	public static float fscore = 0;

    void Start()
    {
		scoreObject = gameObject.GetComponent<Text>();
    }
	
    void Update()
    {
		fscore += Time.deltaTime * 16;
		score = (int)fscore;
		scoreObject.text = score.ToString();
	}
}
