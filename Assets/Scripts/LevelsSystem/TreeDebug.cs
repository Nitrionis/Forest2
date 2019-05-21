using UnityEngine;

public class TreeDebug : MonoBehaviour
{
	public static GameObject treeDebugObject;
	public GameObject debugObject;
	// Start is called before the first frame update
	void Start()
    {
		treeDebugObject = debugObject;
	}
}
