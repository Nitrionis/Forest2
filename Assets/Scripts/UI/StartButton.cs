using UnityEngine;

public class StartButton : MonoBehaviour, IButton
{
	public ButtonAction GetActionCode()
	{
		return ButtonAction.Start;
	}
}
