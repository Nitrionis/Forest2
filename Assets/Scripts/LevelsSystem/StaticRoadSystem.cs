using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class StaticRoadSystem : RoadSystem
	{
		public StaticRoadSystem(LevelGeneratorParameters parameters, ForestBase forest) : base(parameters, forest)
		{
			
		}

		protected override void SpawnCar(Road.Car car, Vector3 position)
		{
			car.startPos = position;
			car.startTime = Score.timer;
		}

		protected override void DestroyCar(Road.Car car, Vector3 position)
		{
			car.endPos = position;
			car.endTime = Score.timer;

			var carInfo = new StatisticsManager.EnemyMoveInfo(
					car.startPos, car.startTime, car.endPos, car.endTime);
			StatisticsManager.PushCarInfo(carInfo);
		}
	}
}