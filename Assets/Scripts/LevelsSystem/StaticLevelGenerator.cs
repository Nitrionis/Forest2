using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class StaticLevelGenerator : LevelGeneratorBase
	{
		//protected struct LevelWrapper
		//{
		//	public Level Level;
		//	public float difficulty;
		//}

		protected const int levelsCount = 4;
		protected Queue<Level> levels;
		protected const int uniqueLevelsTypesCount = 5;

		protected Queue<Level> unusedLevels;

		public StaticLevelGenerator(LevelGeneratorParameters parameters) : base(parameters)
		{
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
			//if (levels.Count < levelsCount)
			//{
			//	Level.lastlLevel = GetNextLevel();
			//	levels.Enqueue(Level.lastlLevel);
			//}
		}

		public override void FixedUpdate()
		{
			foreach (Level l in levels)
				l.FixedUpdate();
			Level.roadSystem.FixedUpdate();
			Level.zombieMover.FixedUpdate();
			Level.planesSystem.FixedUpdate();
		}
	}
}