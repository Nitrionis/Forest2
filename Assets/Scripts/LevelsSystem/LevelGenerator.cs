using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class LevelGenerator : MonoBehaviour
	{
		private const int levelsCount = 4;
		private Queue<Level> levels;

		private const int uniqueLevelsTypesCount = 5;

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

		private class LevelChance
		{
			private float
				easyStart, easyEnd,
				hardStart, hardEnd,
				currStart, currEnd;
			private Level level;
			public readonly int id;

			public LevelChance(
				float easyStart, float easyEnd,
				float hardStart, float hardEnd,
				Level level, int id)
			{
				this.id = id;
				this.easyStart = easyStart;
				this.easyEnd   = easyEnd;
				this.hardStart = hardStart;
				this.hardEnd   = hardEnd;
				currStart = easyStart;
				currEnd = easyEnd;

				this.level = level;
			}

			public bool Check(float f)
			{
				return f > currStart && f < currEnd;
			}

			public Level CreateNewLevel()
			{
				return level.CreateNew();
			}

			public void UpdateChance(float f)
			{
				currStart = Mathf.Lerp(easyStart, hardStart, f);
				currEnd = Mathf.Lerp(easyEnd, hardEnd, f);
			}

			public void Print()
			{
				Debug.Log(currStart + " " + currEnd);
			}
		}
		private LevelChance[] probabilities;
		private int prevLevelId = -1;

		private class EmptyLevel : Level
		{
			public EmptyLevel() : base(true) { }

			public override Level CreateNew() { return null; }

			public override void InitializeLevelRect()
			{
				levelStartEnd = new Vector2(0, character.position.z);
			}
		}

		void Start()
		{
			probabilities = new LevelChance[uniqueLevelsTypesCount] {
				new LevelChance(0.0f, 0.2f, 0.0f, 0.0f, new SimpleLevel(true),   0),
				new LevelChance(0.2f, 0.7f, 0.0f, 0.1f, new SpeedLevel(true),    1),
				new LevelChance(0.7f, 1.0f, 0.1f, 0.4f, new MotorwayLevel(true), 2),
				new LevelChance(0.0f, 0.0f, 0.4f, 0.7f, new PlanesLevel(true),   3),
				new LevelChance(0.0f, 0.0f, 0.7f, 1.0f, new ZombieLevel(true),   4)
			};
			Level.difficulty = 0.0f;

			levels = new Queue<Level>(levelsCount);
			Level.levels = levels;
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

			Level.forest = new Forest(parameters);
			Level.roadSystem = new RoadSystem(parameters);
			Level.zombieMover = new ZombieMover(parameters);
			Level.planesSystem = new PlanesSystem(parameters);

			SpeedLevel.speedParticleSystem = speedParticleSystem;

			levels.Enqueue(new EmptyLevel());
			Level.lastlLevel = levels.Peek();

			for (int i = 1; i < levelsCount; i++)
			{
				levels.Enqueue(new SimpleLevel());
			}
			Level.lastlLevel = levels.Peek();
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.J))
				foreach (var l in probabilities)
					l.Print();

			if (Score.isGameStarted)
			{
				Level.difficulty = Mathf.Clamp01(0.0001f * Score.fscore);
				foreach (var p in probabilities)
					p.UpdateChance(Level.difficulty);
				CameraMover.SetSpeed(Mathf.Lerp(easyMoveSpeed, hardMoveSpeed, Level.difficulty));
			}
			else
			{
				Level.difficulty = 0;
				CameraMover.UnlockSpeed();
				CameraMover.SetSpeed(0);
			}

			Level.forest.Update();
			Level.roadSystem.Update();

			if (levels.Count > 0 && levels.Peek().IsLevelСompleted())
			{
				levels.Dequeue().Finish();
			}
			foreach (Level l in levels)
			{
				l.Update();
				if (l.firstStartcall && l.IsLevelActive())
				{
					l.Start();
					l.firstStartcall = false;
				}
			}
			if (levels.Count < levelsCount)
			{
				Level.lastlLevel = GetNextLevel();
				levels.Enqueue(Level.lastlLevel);
			}
		}

		void FixedUpdate()
		{
			foreach (Level l in levels)
				l.FixedUpdate();
			Level.roadSystem.FixedUpdate();
			Level.zombieMover.FixedUpdate();
			Level.planesSystem.FixedUpdate();
		}

		private int m2 = 0;

		private Level GetNextLevel()
		{
			float f = UnityEngine.Random.value;
			foreach (var l in probabilities)
			{
				if (l.Check(f))
				{
					if (prevLevelId == 4 && l.id == 4)
						return new SpeedLevel();
					prevLevelId = l.id;
					return l.CreateNewLevel();
				}
			}
			return new SimpleLevel();

			//m2++;
			//if (m2 % 4 == 0)
			//	return new ZombieLevel();
			//else if (m2 % 4 == 1)
			//	return new SpeedLevel();
			//else if (m2 % 4 == 2)
			//	return new PlanesLevel();
			//else
			//	return new MotorwayLevel();

			//if (m2 % 2 == 0)
			//return new ZombieLevel();
			//else
			//	return new SimpleLevel();
		}
	}
}