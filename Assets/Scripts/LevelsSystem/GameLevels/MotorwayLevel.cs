using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class MotorwayLevel : Level
	{
		private static int InitRectCallCount;
		private static Vector3 roadForwardForZOffset;

		static MotorwayLevel()
		{
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
		private const float baseRoadSpacing = 36;
		private const float minRoadSpacing = 16;
		// this is list of logic roads representations
		public List<RoadSystem.Road> roads;


		public MotorwayLevel(bool isEmpty = false) : base(isEmpty)
		{
			levelId = LevelId.Motorway;
			if (!isEmpty)
			{
				//Debug.Log("new MotorwayLevel");
				roadSystem.AddRoadsToQueue(roads);
			}
		}

		public override Level CreateNew()
		{
			return new MotorwayLevel();
		}

		public override void InitializeLevelRect()
		{
			float levelStartPos = lastlLevel.levelStartEnd.y + 10;
			//Debug.Log("CurrLevelStatPos " + levelStartPos);

			int roadCount = GetRoadCountInGroup();
			roads = new List<RoadSystem.Road>(roadCount);
			
			roads.Add(new RoadSystem.Road(roadForwardForZOffset * levelStartPos
				+ Vector3.up * roadSystem.roadOffsetByY, GetNextRoadCarSpeed()));

			if (InitRectCallCount > 1)
			{
				//var prevRoads = ((MotorwayLevel)lastlLevel).roads;
				//Debug.Log("Prev level last road pos " + prevRoads[prevRoads.Count - 1].center +
				//	"\nCurr level first road pos " + roads[0].center
				//	+ "\nPrev level end pos" + lastlLevel.levelStartEnd.y
				//	+ "\nCurr level start pos" + levelStartPos);
			}

			for (int i = 1; i < roadCount; i++)
			{
				float speed = GetNextRoadCarSpeed();
				float spacing = GetNextRoadSpacing(speed);
				if (Random.value < 0.5)
					speed = -speed;
				Vector3 pos = roads[i-1].center + roadForwardForZOffset * spacing;
				roads.Add(new RoadSystem.Road(pos, speed));
			}
			float levelEndPos = roads[roadCount - 1].center.z + 10;
			levelStartEnd = new Vector2(levelStartPos, levelEndPos);
			//Debug.Log("Level distance " + levelStartEnd);
			
			InitRectCallCount++;
		}

		public override float GetMinZOffsetFromCharacter()
		{
			return fogDistance;
		}

		private int GetRoadCountInGroup()
		{
			return (int)Mathf.Lerp(
				baseRoadsCount,
				maxRoadsCount, 
				difficulty);
		}

		private float GetNextRoadSpacing(float speed)
		{
			//return 16;
			return Random.Range(
				Mathf.Lerp(
					baseRoadSpacing, minRoadSpacing, difficulty - (maxCarSpeed - speed)/maxSpeedDelta
				), baseRoadSpacing);
		}

		private float GetNextRoadCarSpeed()
		{
			return Mathf.Lerp(minCarSpeed, Mathf.Lerp(minCarSpeed, maxCarSpeed, difficulty), Random.value);
		}

		//private float CalculateCarSpawnTime(RoadSystem.Road road)
		//{
		//	return 0;
		//}
	}
}