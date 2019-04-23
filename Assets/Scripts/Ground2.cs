using UnityEngine;

public class Ground2 : MonoBehaviour
{
	public Transform character;

	private Material material;
	private int id;
	private Vector3 prevCharPos;
	public Vector2 offset = Vector2.zero;

	void Start()
    {
		id = Shader.PropertyToID("_UvOffset");
		material = GetComponent<MeshRenderer>().material;
		prevCharPos = character.transform.position;

	}

    // Update is called once per frame
    void Update()
    {
		Vector3 dpos = (character.position - prevCharPos) / 16;
		offset.x += -dpos.x;
		offset.y += -dpos.z;
		offset.x %= 1.0f;
		offset.y %= 1.0f;
		material.SetVector(id, offset);
		prevCharPos = character.position;
	}
}
