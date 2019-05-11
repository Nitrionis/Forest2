using UnityEngine;

namespace Map
{
	public class PlanesLevel : Level
	{
		private const float planeSpeed = 12;
		private const float trackDistance = 80;

		private abstract class Strategy
		{
			protected Level level;
			public Strategy(Level level)
			{
				this.level = level;
			}
			public virtual void Start() { }
			public virtual void FixedUpdate() { }
			public abstract float CalculateAllLaunchesTime();
		}

		private class WallStrategy : Strategy
		{
			private const int dynamicPlanesCount = 3;
			private int wavesCount;

			public WallStrategy(Level level) : base(level)
			{
				// TODO
				wavesCount = 2 + (int)(Random.value * 2.99f);
			}
			public override float CalculateAllLaunchesTime()
			{
				return trackDistance * wavesCount / (CameraMover.instance.moveSpeed + planeSpeed);
			}
			public override void Start()
			{
				planesSystem.dynamicPlanesCount = dynamicPlanesCount;
				for (int i = 0; i < 9; i++)
					planesSystem.AddPlaneToQueue(
						Quaternion.identity,
						(i % 3 - 1) *  18 * Vector3.right);
			}
		}

		private Strategy strategy;

		public PlanesLevel(bool isEmpty = false) : base(isEmpty)
		{
			levelType = LevelType.Planes;
			if (!isEmpty)
			{
				
			}
		}

		public override Level CreateNew()
		{
			return new PlanesLevel();
		}

		public override void InitializeLevelRect()
		{
			strategy = GetRandomStrategy();

			float levelDistance = (3 + strategy.CalculateAllLaunchesTime())
				* CameraMover.instance.moveSpeed;
			float levelStartPos = lastlLevel.levelStartEnd.y;
			levelStartEnd = new Vector2(levelStartPos, levelStartPos + levelDistance);
		}

		public override void FixedUpdate()
		{
			strategy.FixedUpdate();
		}

		public override void Start()
		{
			strategy.Start();
		}
		public override void Finish()
		{
			planesSystem.DeactivateAllPlanes();
		}

		private Strategy GetRandomStrategy()
		{
			return new WallStrategy(this);
		}
	}
}