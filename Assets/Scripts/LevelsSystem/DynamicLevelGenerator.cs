using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class DynamicLevelGenerator : LevelGeneratorBase
	{
		protected const int levelsCount = 4;
		protected Queue<Level> levels;
		protected const int uniqueLevelsTypesCount = 5;

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
				this.easyEnd = easyEnd;
				this.hardStart = hardStart;
				this.hardEnd = hardEnd;
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

		public DynamicLevelGenerator(LevelGeneratorParameters parameters) : base(parameters)
		{
			Level.forest = new Forest(parameters);
			Level.roadSystem = new RoadSystem(parameters, Level.forest);
			Level.zombieMover = new ZombieMover(parameters);
			Level.planesSystem = new PlanesSystem(parameters);

			probabilities = new LevelChance[uniqueLevelsTypesCount] {
				new LevelChance(0.0f, 0.2f, 0.0f, 0.0f, new SimpleLevel(true),   0),
				new LevelChance(0.2f, 0.7f, 0.0f, 0.1f, new SpeedLevel(true),    1),
				new LevelChance(0.7f, 1.0f, 0.1f, 0.4f, new MotorwayLevel(true), 2),
				new LevelChance(0.0f, 0.0f, 0.4f, 0.7f, new PlanesLevel(true),   3),
				new LevelChance(0.0f, 0.0f, 0.7f, 1.0f, new ZombieLevel(true),   4)
			};

			levels = new Queue<Level>(levelsCount);
			Level.levels = levels;

			Level.lastlLevel = new EmptyLevel();
			levels.Enqueue(Level.lastlLevel);

			for (int i = 1; i < levelsCount; i++)
			{
				Level.lastlLevel = new SimpleLevel();
				levels.Enqueue(Level.lastlLevel);
			}
		}

		public override void Update()
		{
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

		public override void FixedUpdate()
		{
			foreach (Level l in levels)
				l.FixedUpdate();
			Level.roadSystem.FixedUpdate();
			Level.zombieMover.FixedUpdate();
			Level.planesSystem.FixedUpdate();
		}

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
		}
	}
}