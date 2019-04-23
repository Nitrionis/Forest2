using UnityEngine;

namespace Map
{
	public struct LevelGeneratorParameters
	{
		public Transform character;
		public Transform characterCamera;

		public GameObject treePrefab;
		public GameObject roadPrefab;
		public GameObject carPrefab;
		public GameObject zombiePrefab;
		public GameObject planePrefab;

		public Vector3 startForesPposition;

		public float treeRadius;
		public float halfFOV;
		public float treesSpacing;

		public float roadOffsetByY;
	}
}