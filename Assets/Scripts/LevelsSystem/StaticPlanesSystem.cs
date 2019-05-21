using UnityEngine;

namespace Map
{
	public class StaticPlanesSystem : PlanesSystem
	{
		public StaticPlanesSystem(LevelGeneratorParameters parameters) : base(parameters)
		{
			
		}

		protected override void DeactivatePlane()
		{
			//Debug.Log("override void DeactivatePlane()");
			var plane = activePlanes.Peek();
			plane.endPos = plane.rigidbody.position;
			plane.endTime = Score.timer;
			var planeInfo = new StatisticsManager.EnemyMoveInfo(
				plane.startPos, plane.startTime, plane.endPos, plane.endTime);
			StatisticsManager.PushPlaneInfo(planeInfo);
			base.DeactivatePlane();
		}

		protected override void RespawnPlane(PlaneResp planeResp)
		{
			var plane = freePlanes.Dequeue();
			activePlanes.Enqueue(plane);
			if (plane.gameObject.activeSelf == false)
				plane.gameObject.SetActive(true);

			plane.startPos = character.transform.position
				+ planeResp.rotation * planeResp.rotation * Vector3.forward * 70 + Vector3.up * 4
				+ planeResp.positionOffset;

			plane.rigidbody.position = plane.startPos;
			plane.startTime = Score.timer;
			plane.rigidbody.rotation = planeResp.rotation;
		}
	}
}