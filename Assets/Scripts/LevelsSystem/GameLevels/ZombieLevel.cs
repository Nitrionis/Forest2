using UnityEngine;

namespace Map
{
	public class ZombieLevel : Level
	{
		private const float baseLevelDistance = 150;
		private const float maxLevelDistance = 300;

		private class ZombieCreationStrategy
		{
			private Vector2 easyRange, hardRange;

			public ZombieCreationStrategy(float minEasyAngle, float maxEasyAngle, float minHardAngle, float maxHardAngle)
			{
				easyRange = new Vector2(minEasyAngle, maxEasyAngle);
				hardRange = new Vector2(minHardAngle, maxHardAngle);
			}

			public Vector2 GetZombieCreationAngleRange()
			{
				return Vector2.Lerp(easyRange, hardRange, difficulty);
			}
		}
		private static ZombieCreationStrategy[] strategies;

		static ZombieLevel()
		{
			strategies = new ZombieCreationStrategy[3]
			{
				new ZombieCreationStrategy(-45, 45,-30, 30),
				new ZombieCreationStrategy( 15, 45, 10, 30),
				new ZombieCreationStrategy(-45, 15,-30, 10)
			};
		}

		private int strategyIndex;

		public ZombieLevel(bool isEmpty = false) : base(isEmpty)
		{
			if (!isEmpty)
			{
				strategyIndex = 0;//(int)(strategies.Length * 0.999f * Random.value)*/;
				zombieMover.zombieCreationAngleRange = strategies[strategyIndex].GetZombieCreationAngleRange();
			}
		}

		public override Level CreateNew()
		{
			return new ZombieLevel();
		}

		public override void InitializeLevelRect()
		{
			levelStartEnd = new Vector2(
				lastlLevel.levelStartEnd.y,
				lastlLevel.levelStartEnd.y + Mathf.Lerp(
					baseLevelDistance, 
					Mathf.Lerp(baseLevelDistance, maxLevelDistance, difficulty), 
					Random.value)
			);
		}

		public override void Start()
		{
			zombieMover.isActive = true;
			SkyboxChanger.DayToNight();
		}

		public override void Finish()
		{
			zombieMover.isActive = false;
			SkyboxChanger.NightToDay();
		}
	}
}
