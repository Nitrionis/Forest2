using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Map
{
	public class LevelGenerator : MonoBehaviour
	{
		private const int levelsCount = 4;
		private Queue<Level> levels;

		private const int uniqueLevelsTypesCount = 4;

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

		private struct LevelChance
		{
			private float
				easyStart, easyEnd,
				hardStart, hardEnd,
				currStart, currEnd;
			private Level level;

			public LevelChance(
				float easyStart, float easyEnd,
				float hardStart, float hardEnd,
				Level level)
			{
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
		}
		private LevelChance[] probabilities;


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
				new LevelChance(0.0f, 0.5f, 0.0f, 0.0f, new SimpleLevel(true)),
				new LevelChance(0.5f, 1.0f, 0.0f, 1.0f, new MotorwayLevel(true)),
				// TODO
				new LevelChance(1.0f, 1.0f, 1.0f, 1.0f, new PlanesLevel(true)),
				new LevelChance(1.0f, 1.0f, 1.0f, 1.0f, new ZombieLevel(true))
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
				Level.lastlLevel = levels.Peek();
			}
		}

		void Update()
		{
			Level.difficulty = Mathf.Clamp01(0.0001f * Score.fscore);
			foreach (var p in probabilities)
				p.UpdateChance(Level.difficulty);
			CameraMover.SetSpeed(Mathf.Lerp(easyMoveSpeed, hardMoveSpeed, Level.difficulty));

			Level.forest.Update();
			Level.roadSystem.Update();
			
			foreach (Level l in levels)
			{
				l.Update();
				if (l.firstStartcall && l.IsLevelActive())
				{
					l.Start();
					l.firstStartcall = false;
				}
			}
			if (levels.Count > 0 && levels.Peek().IsLevelСompleted())
			{
				levels.Dequeue().Finish();
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

			//float f = UnityEngine.Random.value;
			//foreach (var l in probabilities)
			//{
			//	if (l.Check(f))
			//		return l.CreateNewLevel();
			//}
			//return new SimpleLevel();

			m2++;
			if (m2 % 3 == 0)
				return new ZombieLevel();
			else if (m2 % 3 == 1)
				return new SpeedLevel();
			else
				return new PlanesLevel();

			//return new MotorwayLevel();
		}
	}
}