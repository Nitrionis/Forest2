using UnityEngine;

public class Reticle : MonoBehaviour
{
	public LayerMask layerMask;

	private void FixedUpdate()
	{
		RaycastHit hit;
		// Does the ray intersect any objects excluding the player layer
		if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20, layerMask))
		{
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
			Debug.Log("Did Hit");
		}
		else
		{
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.cyan);
			Debug.Log("Did not Hit");
		}
	}
}
