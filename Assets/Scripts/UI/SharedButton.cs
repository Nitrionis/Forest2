using UnityEngine;

public class SharedButton : MonoBehaviour, IButton
{
	public ButtonAction action;
	public ButtonAction GetActionCode()
	{
		return action;
	}
}
