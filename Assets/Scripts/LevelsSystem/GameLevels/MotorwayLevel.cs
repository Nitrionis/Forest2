using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class MotorwayLevel : Level
	{
		private const float minCarSpeed = 10;
		private const float maxCarSpeed = 20;
		// this is roads count per level
		private const int baseRoadsCount = 3;
		private const int maxRoadsCount = 8;
		// this is the distance between the roads, not the lines
		private const float baseRoadSpacing = 36;
		private const float minRoadSpacing = 16;
		// this is list of logic roads representations
		private List<RoadSystem.Road> roads;


		public MotorwayLevel(bool isEmpty = false) : base(isEmpty)
		{
			levelId = LevelId.Motorway;
			Debug.Log("new MotorwayLevel");
			if (!isEmpty)
				roadSystem.AddRoadsToQueue(roads);
		}

		public override Level CreateNew()
		{
			return new MotorwayLevel();
		}

		public override void InitializeLevelRect()
		{
			float levelStartPos = lastlLevel.levelStartEnd.y + 10;

			int roadCount = GetRoadCountInGroup();
			roads = new List<RoadSystem.Road>(roadCount);
			
			roads.Add(
				new RoadSystem.Road(
					RoadSystem.Road.roadForward + Vector3.up * roadSystem.roadOffsetByY, 
					GetNextRoadCarSpeed()));

			//roads[0].center += RoadSystem.Road.roadForward * (10 + levelStartPos - roads[0].GetZRange().x)
			//	+ Vector3.up * roadSystem.roadOffsetByY;
			
			for (int i = 1; i < roadCount; i++)
			{
				float spacing = GetNextRoadSpacing();
				Debug.Log(spacing);
				Vector3 pos = roads[0].center + i * RoadSystem.Road.roadForward * spacing;// * GetNextRoadSpacing();
				roads.Add(new RoadSystem.Road(pos, 20));
			}
			float levelEndPos = roads[roadCount - 1].center.z + 10;
			levelStartEnd = new Vector2(levelStartPos, levelEndPos);
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

		private float GetNextRoadSpacing()
		{
			return Random.Range(Mathf.Lerp(baseRoadSpacing, minRoadSpacing, difficulty), baseRoadSpacing);
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