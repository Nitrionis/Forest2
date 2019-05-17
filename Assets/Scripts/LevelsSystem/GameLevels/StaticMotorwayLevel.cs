using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class StaticMotorwayLevel : StaticLevel
	{
		private static int firstPassFirstRoadId;
		private static int secondPassFirstRoadId;
		private static int carSpeedId;
		private static int InitRectCallCount;
		private static Vector3 roadForwardForZOffset;

		static StaticMotorwayLevel()
		{
			carSpeedId = 0;
			firstPassFirstRoadId = 0;
			secondPassFirstRoadId = 0;
			InitRectCallCount = 0;
			roadForwardForZOffset = (1.0f / RoadSystem.Road.roadForward.z) * RoadSystem.Road.roadForward;
		}

		private const float minCarSpeed = 15;
		private const float maxCarSpeed = 25;
		private const float maxSpeedDelta = 15;
		// this is roads count per level
		private const int baseRoadsCount = 3;
		private const int maxRoadsCount = 8;
		// this is the distance between the roads, not the lines
		private const float roadSpacing = 16f;
		// this is list of logic roads representations

		//public class StaticRoad
		public List<RoadSystem.Road> roads;

		private bool isEmpty = false;

		public StaticMotorwayLevel() : base() { isEmpty = true; /*firstPassFirstRoadId++*/; }

		public StaticMotorwayLevel(float difficulty, float startPos)
			: base(difficulty, startPos)
		{
			levelType = LevelType.Motorway;

			int roadCount = GetRoadCountInGroup(difficulty);
			roads = new List<RoadSystem.Road>(roadCount);

			roads.Add(new RoadSystem.Road(roadForwardForZOffset * (startPos + 10.0f)
				+ Vector3.up * roadSystem.roadOffsetByY, GetNextRoadCarSpeed(difficulty)));

			for (int i = 1; i < roadCount; i++)
			{
				float speed = GetNextRoadCarSpeed(difficulty);
				float spacing = roadSpacing;
				if (Random.value < 0.5)
					speed = -speed;
				Vector3 pos = roads[i - 1].center + roadForwardForZOffset * spacing;
				roads.Add(new RoadSystem.Road(pos, speed));
			}
			InitRectCallCount++;

			roadSystem.AddRoadsToQueue(roads);
		}

		public override Level CreateNew()
		{
			throw new System.NotImplementedException();
		}

		public override Level CreateNew(float difficulty, float startPos)
		{
			return new StaticMotorwayLevel(difficulty, startPos);
		}

		private int GetRoadCountInGroup(float difficulty)
		{
			return (int)Mathf.Lerp(
				baseRoadsCount,
				maxRoadsCount,
				difficulty);
		}

		private float GetNextRoadCarSpeed(float difficulty)
		{
			carSpeedId++;
			return Mathf.Lerp(minCarSpeed, Mathf.Lerp(minCarSpeed, maxCarSpeed, difficulty), CustomRandom.Get((uint)carSpeedId));
		}

		public override int GetTreesLinesCount(float difficulty)
		{
			int roadsCount = GetRoadCountInGroup(difficulty);
			return Mathf.CeilToInt(((roadsCount - 1) * roadSpacing + roadsCount * 10.0f + 20.0f) / 10.0f);
		}
	}
}