using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CarBuilder : MonoBehaviour
{
	public GameObject objectToBeDeactivated;
	public GameObject[] gameObjects;

	public bool[] ignoreradeSubmeshes;
	public float[] glowSubmeshes;
	public Color[] submeshColors;

	private const int defaultArraySize = 4000; 

	void Awake()
	{
		Mesh newMesh = new Mesh();
		List<Vector3> notVertices = new List<Vector3>(1);
		notVertices.Add(Vector3.zero);
		newMesh.SetVertices(notVertices);
		newMesh.subMeshCount = 1;

		List<Color> newColors = new List<Color>(defaultArraySize);
		List<Vector3> newVertices = new List<Vector3>(defaultArraySize);
		List<Vector3> newNormals = new List<Vector3>(defaultArraySize);
		List<int> indices = new List<int>(defaultArraySize);

		int nextIndex = 0;
		for (int i = 0; i < gameObjects.Length; i++)
		{
			MeshRenderer meshRenderer = gameObjects[i].GetComponent<MeshRenderer>();
			MeshFilter meshFilter = gameObjects[i].GetComponent<MeshFilter>();

			Material[] materials = meshRenderer.materials;
			Mesh oldMesh = meshFilter.mesh;

			int oldVertexCount = oldMesh.vertexCount;

			List<Vector3> oldVertices = new List<Vector3>(oldVertexCount);
			oldMesh.GetVertices(oldVertices);
			List<Vector3> oldNormals = new List<Vector3>(oldVertexCount);
			oldMesh.GetNormals(oldNormals);

			Matrix4x4 rotate = Matrix4x4.Rotate(gameObjects[i].transform.localRotation);
			Matrix4x4 translate = Matrix4x4.Translate(gameObjects[i].transform.localPosition);
			Matrix4x4 scale = Matrix4x4.Scale(gameObjects[i].transform.localScale);
			Matrix4x4 matrix = translate * rotate * scale;

			for (int j = 0; j < oldVertices.Count; j++)
			{
				oldVertices[j] = matrix.MultiplyPoint(oldVertices[j]);
				oldNormals[j] = rotate * oldNormals[j];
			}

			int[] vertexOffsets = new int[oldVertexCount];

			for (int j = 0; j < materials.Length; j++)
			{
				if (ignoreradeSubmeshes.Length > j && ignoreradeSubmeshes[j])
					continue;
				// Get submesh color
				Color color;
				if (submeshColors.Length > j && submeshColors[j] != Color.black)
					color = submeshColors[j];
				else
					color = materials[j].GetColor("_Color");
				int[] submeshIndices = oldMesh.GetIndices(j);

				//Debug.Log("Submesh color " + color);

				Vector3 localScale = gameObjects[i].transform.localScale;
				if (localScale.x < 0 && localScale.y < 0 && localScale.z < 0)
				{
					//Debug.Log("Invers " + submeshIndices.Length);
					for (int k = 0; k < submeshIndices.Length; k += 3)
					{
						int index = submeshIndices[k + 1];
						submeshIndices[k + 1] = submeshIndices[k + 2];
						submeshIndices[k + 2] = index;
					}
					for (int k = 0; k < submeshIndices.Length; k++)
					{
						// REVERS NORMALS
					}
				}
				// Set default values
				for (int k = 0; k < vertexOffsets.Length; k++)
					vertexOffsets[k] = -1;
				//Debug.Log(submeshIndices.Length);
				for (int k = 0; k < submeshIndices.Length; k++)
				{
					int vertexIndex = submeshIndices[k];

					if (vertexOffsets[vertexIndex] == -1)
					{
						newVertices.Add(oldVertices[vertexIndex]);
						if (localScale.x < 0 && localScale.y < 0 && localScale.z < 0)
							newNormals.Add(-oldNormals[vertexIndex]);
						else
							newNormals.Add(oldNormals[vertexIndex]);
						newColors.Add(color);
						vertexOffsets[vertexIndex] = nextIndex;
						indices.Add(nextIndex);
						nextIndex++;
					}
					else
					{
						indices.Add(vertexOffsets[vertexIndex]);
					}
				}
			}
		}

		newMesh.SetVertices(newVertices);
		newMesh.SetNormals(newNormals);
		newMesh.SetColors(newColors);
		newMesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

		Debug.Log("Full vertex count " + newVertices.Count);
		Debug.Log("Full indices count " + indices.Count);

		GetComponent<MeshFilter>().mesh = newMesh;

		if (objectToBeDeactivated != null)
			objectToBeDeactivated.SetActive(false);

		//Material material = GetComponent<MeshRenderer>().material;
		//Color[] data = new Color[] { Color.blue, Color.red, Color.green };
		//material.SetColor("_Color", data[(int)Random.Range(0, 2.99f)]);
	}


	private Vector3 rotation = Vector3.zero;

	void Update()
	{
		rotation.x += Time.deltaTime * 2 * 360;
		rotation.x %= 360;
		Quaternion quaternion = Quaternion.Euler(rotation);
		Matrix4x4 matrix = Matrix4x4.Rotate(quaternion);
		Shader.SetGlobalMatrix("_Rotation", matrix);
	}
}
