using UnityEngine;

namespace Map
{
	public class LevelGenerator : MonoBehaviour
	{
		public static LevelGenerator instance;

		public enum SceneMode
		{
			Static,
			Dynamic
		}
		public static SceneMode sceneMode;

		static LevelGenerator()
		{
			sceneMode = SceneMode.Static;
		}

		[Space]
		[Header("Character position")]
		public Transform character;
		public Transform characterCamera;
		[Space]
		[Header("Prefabs")]
		public GameObject treePrefab;
		public GameObject roadPrefab;
		public GameObject carPrefab;
		public GameObject planePrefab;
		public GameObject zombiePrefab;
		[Space]
		[Header("Forest settings")]
		public float treeRadius = 1;
		public float halfFOV = 45;
		public float treesSpacing = 10.0f;
		[Space]
		[Header("Roads settings")]
		public float roadOffsetByY = 1;
		[Space]
		[Header("Speed settings")]
		public ParticleSystem speedParticleSystem;
		public float easyMoveSpeed = 11;
		public float hardMoveSpeed = 16;

		private LevelGeneratorBase levelGenerator;

		void Start()
		{
			instance = this;

			Level.difficulty = 0.0f;
			Level.character = character;
			Level.characterSpeed = 10.0f;

			var parameters = new LevelGeneratorParameters();

			parameters.character = character;
			parameters.characterCamera = characterCamera;

			parameters.treePrefab = treePrefab;
			parameters.roadPrefab = roadPrefab;
			parameters.carPrefab = carPrefab;
			parameters.zombiePrefab = zombiePrefab;
			parameters.planePrefab = planePrefab;

			parameters.treeRadius = treeRadius;
			parameters.halfFOV = halfFOV;
			parameters.treesSpacing = treesSpacing;

			parameters.startForesPposition = transform.position;

			parameters.roadOffsetByY = roadOffsetByY;

			parameters.easyMoveSpeed = easyMoveSpeed;
			parameters.hardMoveSpeed = hardMoveSpeed;

			SpeedLevel.speedParticleSystem = speedParticleSystem;
			StaticSpeedLevel.speedParticleSystem = speedParticleSystem;

			if (sceneMode == SceneMode.Dynamic)
				levelGenerator = new DynamicLevelGenerator(parameters);
			else
				levelGenerator = new StaticLevelGenerator(parameters);
		}

		void Update()
		{
			levelGenerator.Update();
		}

		void FixedUpdate()
		{
			levelGenerator.FixedUpdate();
		}
	}
}