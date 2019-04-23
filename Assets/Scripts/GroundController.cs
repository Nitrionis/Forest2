using UnityEngine;

public class GroundController : MonoBehaviour
{
	public Transform character;
	private Transform[] planes;
	private Transform supportingPlane;

	private const float PlaneSize = 250;

	private Vector3[][] offsets = new Vector3[][]
	{
		new Vector3[]{ new Vector3(-PlaneSize, 0, 0), new Vector3(-PlaneSize, 0, PlaneSize), new Vector3( 0, 0, PlaneSize) },
		new Vector3[]{ new Vector3( PlaneSize, 0, 0), new Vector3( PlaneSize, 0, PlaneSize), new Vector3( 0, 0, PlaneSize) },
		new Vector3[]{ new Vector3(-PlaneSize, 0, 0), new Vector3(-PlaneSize, 0,-PlaneSize), new Vector3( 0, 0,-PlaneSize) },
		new Vector3[]{ new Vector3( PlaneSize, 0, 0), new Vector3( PlaneSize, 0,-PlaneSize), new Vector3( 0, 0,-PlaneSize) }
	};

	// Start is called before the first frame update
	void Start()
	{
		planes = new Transform[transform.childCount];
		for (int i = 0; i < planes.Length; i++)
			planes[i] = transform.GetChild(i);

		supportingPlane = GetSupportingPlane();
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Vector3 forward = character.forward;
		supportingPlane = GetSupportingPlane();
		if (character.position.x < supportingPlane.position.x)
			if (forward.z > 0)
				ApplyOffset(0);
			else
				ApplyOffset(2);
		else
			if (forward.z > 0)
				ApplyOffset(1);
			else
				ApplyOffset(3);
	}

	private void ApplyOffset(int offsetId)
	{
		for (int i = 0, j = 0; i < planes.Length; i++)
		{
			if (planes[i] != supportingPlane)
			{
				planes[i].localPosition = offsets[offsetId][j] + supportingPlane.localPosition;
				j++;
			}
		}
	}

	private Transform GetSupportingPlane()
	{
		Vector3 charPos = character.position;
		for (int i = 0; i < planes.Length; i++)
		{
			if ((Mathf.Abs(planes[i].position.x - charPos.x) < PlaneSize / 2)
				&& (Mathf.Abs(planes[i].position.z - charPos.z) < PlaneSize / 2))
				return planes[i];
		}
		return null;
	}
}
