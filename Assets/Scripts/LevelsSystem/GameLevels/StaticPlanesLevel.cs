using UnityEngine;

namespace Map
{
	public class StaticPlanesLevel : StaticLevel
	{
		private const float planeSpeed = 12;
		private const float trackDistance = 80;
		private const int dynamicPlanesCount = 3;

		public static int counter;

		private int wavesCount;
		

		public StaticPlanesLevel() : base() { }

		public StaticPlanesLevel(float difficulty, float startPos) : base(difficulty, startPos)
		{
			levelType = LevelType.Planes;
			wavesCount = 3;
		}

		public override Level CreateNew()
		{
			counter++;
			return new PlanesLevel();
		}

		public override void InitializeLevelRect() { }

		public override void Start()
		{
			planesSystem.dynamicPlanesCount = dynamicPlanesCount;
			for (int i = 0; i < 9; i++)
				planesSystem.AddPlaneToQueue(
					Quaternion.identity,
					(i % 3 - 1) * 18 * Vector3.right);
		}

		public override void Finish()
		{
			planesSystem.DeactivateAllPlanes();
		}

		public override int GetTreesLinesCount(float difficulty)
		{
			int wavesCount = 3;
			float halfCharacterSpeed = Mathf.Lerp(10, 20, difficulty);
			float allLaunchesTime = trackDistance * wavesCount / (halfCharacterSpeed + planeSpeed);
			float levelDistance = allLaunchesTime * (halfCharacterSpeed * 2 + 2);
			return Mathf.CeilToInt(levelDistance / GetTreeSpacing(difficulty));
		}

		public override Level CreateNew(float difficulty, float startPos)
		{
			return new StaticPlanesLevel(difficulty, startPos);
		}

		public float CalculateAllLaunchesTime()
		{
			return trackDistance * wavesCount / (CameraMover.instance.moveSpeed + planeSpeed);
		}
	}
}