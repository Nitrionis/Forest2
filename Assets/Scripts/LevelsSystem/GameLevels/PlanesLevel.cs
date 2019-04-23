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

		private class DiveStrategy : Strategy
		{
			static Quaternion rotateLeft, rotateRight;
			static Quaternion startRotationOne, startRotationTwo;

			static DiveStrategy()
			{
				rotateLeft = Quaternion.Euler(0.0f, 15.0f, 0.0f);
				rotateRight = Quaternion.Euler(0.0f,-15.0f, 0.0f);
				startRotationOne = Quaternion.Euler(0.0f, 45.0f, 0.0f);
				startRotationTwo = Quaternion.Euler(0.0f,-45.0f, 0.0f);
			}

			private const int dynamicPlanesCount = 3;
			private float timeBetweenLaunches;
			private float timeCount;
			private int launchesCount;
			private int launchCounter;
			private Quaternion lastRotation;
			
			public DiveStrategy(Level level) : base(level)
			{
				launchesCount = 5 + (int)(Random.value * 4.99f);
				timeBetweenLaunches = 2.0f;
				launchCounter = 0;
				timeCount = 0;
				lastRotation = startRotationOne;
			}
			public override float CalculateAllLaunchesTime()
			{
				return (launchesCount - 1) * timeBetweenLaunches + trackDistance / (CameraMover.instance.moveSpeed + planeSpeed);
			}
			public override void Start()
			{
				planesSystem.dynamicPlanesCount = dynamicPlanesCount;
			}
			public override void FixedUpdate()
			{
				if (level.IsLevelActive() && launchCounter < launchesCount)
				{
					timeCount += Time.fixedDeltaTime;
					if (timeCount >= timeBetweenLaunches)
					{
						timeCount -= timeBetweenLaunches;
						//Vector3 offset = new Vector3(0,0,0);
						planesSystem.AddPlaneToQueue(lastRotation, Vector3.zero);
						lastRotation *= rotateRight;
					}
				}
			}
		}

		private Strategy strategy;

		public PlanesLevel(bool isEmpty = false) : base(isEmpty)
		{
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

			float levelDistance = strategy.CalculateAllLaunchesTime()
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
			//if (Random.value < 0.5f)
			//	return new WallStrategy();
			//else
			//	return new DiveStrategy();

			//return new DiveStrategy(this);
			return new WallStrategy(this);
		}
	}
}