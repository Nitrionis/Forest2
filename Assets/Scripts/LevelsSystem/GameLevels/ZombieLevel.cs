using UnityEngine;

namespace Map
{
	public class ZombieLevel : Level
	{
		private const float baseLevelDistance = 320;
		private const float maxLevelDistance = 640;

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

		private float levelDuration;
		private float timeCount;
		private int strategyIndex;

		public ZombieLevel(bool isEmpty = false) : base(isEmpty)
		{
			levelType = LevelType.Zombies;
			if (!isEmpty)
			{
				timeCount = 0;
				strategyIndex = 0;
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
					Mathf.Lerp(
						baseLevelDistance, 
						maxLevelDistance, 
						difficulty), 
					(1.0f + Random.value) * 0.5f));
			levelDuration = (levelStartEnd.y - levelStartEnd.x) / CameraMover.instance.moveSpeed;
		}

		public override void Update()
		{
			if (IsLevelActive())
			{
				timeCount += Time.deltaTime;
				if (timeCount > 3)
					zombieMover.isSpawnActive = true;
				if (timeCount > levelDuration)
					zombieMover.isSpawnActive = false;
			}
		}

		public override void Start()
		{
			zombieMover.isMoveActive = true;
			SkyboxChanger.DayToNight();
		}

		public override void Finish()
		{
			zombieMover.isSpawnActive = false;
			zombieMover.isMoveActive = false;
			SkyboxChanger.NightToDay();
		}
	}
}
