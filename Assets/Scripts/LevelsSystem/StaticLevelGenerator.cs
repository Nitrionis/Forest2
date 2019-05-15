using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class StaticLevelGenerator : LevelGeneratorBase, ITreesSpacingController
	{
		private Level[] levelsKinds;

		private const int uniqueLevelsTypesCount = 5;
		private const int levelsCountPerMap = 1000;

		private struct AwaitingLevel
		{
			public Level.LevelType levelType;
			public float startPosition;
			public AwaitingLevel(Level.LevelType levelType, float startPosition)
			{
				this.levelType = levelType;
				this.startPosition = startPosition;
			}
		}
		private Queue<AwaitingLevel> awaitingLevels;
		private float[] spacings;

		protected const int levelsCount = 4;
		protected Queue<Level> levels;

		private float ednOfPrevLevel = -10.7f + 120f;

		private int mode2 = 0;

		public StaticLevelGenerator(LevelGeneratorParameters parameters) : base(parameters)
		{
			StaticPlanesLevel.counter = 0;

			Level.forest = new ForestStatic(parameters, this);
			Level.roadSystem = new RoadSystem(parameters, Level.forest);
			Level.zombieMover = new ZombieMover(parameters);
			Level.planesSystem = new StaticPlanesSystem(parameters);

			levelsKinds = new Level[5];
			levelsKinds[0] = new StaticSimpleLevel();
			levelsKinds[1] = new StaticSpeedLevel();
			levelsKinds[2] = new StaticSimpleLevel();
			// TODO
			levelsKinds[3] = new StaticPlanesLevel();
			levelsKinds[4] = new StaticSimpleLevel();

			spacings = new float[2400];
			for (int i = 0; i < spacings.Length; i++)
				spacings[i] = 10f;

			awaitingLevels = new Queue<AwaitingLevel>(levelsCountPerMap);
			int nextLineUniqueId = 0;

			awaitingLevels.Enqueue(new AwaitingLevel(Level.LevelType.Simple, ednOfPrevLevel));
			AddLevelToSpacings(Level.LevelType.Simple, 0, ref nextLineUniqueId);

			awaitingLevels.Enqueue(new AwaitingLevel(Level.LevelType.Speed, ednOfPrevLevel));
			AddLevelToSpacings(Level.LevelType.Speed, 0, ref nextLineUniqueId);

			for (int i = 2; i < levelsCountPerMap; i++)
			{
				mode2 = (mode2 + 1) % 2;
				if (mode2 == 1)
				{
					awaitingLevels.Enqueue(new AwaitingLevel(Level.LevelType.Planes, ednOfPrevLevel));
					if (nextLineUniqueId < spacings.Length)
						AddLevelToSpacings(Level.LevelType.Planes, 0 /* TODO */, ref nextLineUniqueId);
				}
				else
				{
					awaitingLevels.Enqueue(new AwaitingLevel(Level.LevelType.Simple, ednOfPrevLevel));
					if (nextLineUniqueId < spacings.Length)
						AddLevelToSpacings(Level.LevelType.Simple, 0 /* TODO */, ref nextLineUniqueId);
				}
				//Level.LevelType levelType = (Level.LevelType)(uint)(3.99f * CustomRandom.Get((uint)i) + 1);
				//awaitingLevels.Enqueue(new AwaitingLevel(levelType, ednOfPrevLevel));
				//if (nextLineUniqueId < spacings.Length)
				//	AddLevelToSpacings(levelType, 0 /* TODO */, ref nextLineUniqueId);
			}

			levels = new Queue<Level>(levelsCount);
			Level.levels = levels;

			Level.lastlLevel = new EmptyLevel();

			for (int i = 1; i < levelsCount; i++)
			{
				AwaitingLevel awaitingLevel = awaitingLevels.Dequeue();
				Level.lastlLevel = ((StaticLevel)levelsKinds[(int)awaitingLevel.levelType]).CreateNew(0, awaitingLevel.startPosition);
				levels.Enqueue(Level.lastlLevel);
			}
		}

		private void AddLevelToSpacings(Level.LevelType levelType, float difficulty, ref int nextLineUniqueId)
		{
			StaticLevel level = (StaticLevel)levelsKinds[(int)levelType];
			int linesCount = level.GetTreesLinesCount(difficulty);
			float spacing = level.GetTreeSpacing(difficulty);
			for (int i = 0; i < linesCount && nextLineUniqueId < spacings.Length; i++)
			{
				spacings[nextLineUniqueId] = spacing;
				nextLineUniqueId++;
			}
			ednOfPrevLevel += spacing * linesCount;
		}

		private Level CreateLevel(Level.LevelType levelType)
		{
			return levelsKinds[(uint)levelType].CreateNew();
		}

		public override void Update()
		{
			if (Score.isGameStarted)
			{
				Level.difficulty = Mathf.Clamp01(0.0001f * Score.fscore);
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
				AwaitingLevel awaitingLevel = awaitingLevels.Dequeue();
				Level.lastlLevel = ((StaticLevel)levelsKinds[(int)awaitingLevel.levelType]).CreateNew(0, awaitingLevel.startPosition);
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

		public float GetSpacingByUniqueTreesLineId(int uniqueLineId)
		{
			if (uniqueLineId < 0)
				return 10f;
			return spacings[uniqueLineId];
		}
	}
}