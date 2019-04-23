using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public class PlanesSystem
	{
		private static PlanesSystem instance;

		private static Quaternion roteteUp;

		static PlanesSystem()
		{
			roteteUp = Quaternion.Euler(0.04f, 0, 0);
		}

		private Transform character;
		private GameObject planePrefab;

		public class Plane
		{
			public readonly GameObject gameObject;
			public readonly Transform transform;
			public readonly Rigidbody rigidbody;

			public Plane(GameObject gameObject)
			{
				this.gameObject = gameObject;
				transform = gameObject.transform;
				rigidbody = gameObject.GetComponent<Rigidbody>();
				gameObject.SetActive(false);
			}
		}
		private const int maxPlanesCount = 5;
		public int dynamicPlanesCount;
		private Queue<Plane> freePlanes;
		private Queue<Plane> activePlanes;

		public struct PlaneResp
		{
			public Quaternion rotation;
			public Vector3 positionOffset;

			public PlaneResp(Quaternion rotation, Vector3 positionOffset)
			{
				this.rotation = rotation;
				this.positionOffset = positionOffset;
			}
		}
		private Queue<PlaneResp> spawnRequests;

		public PlanesSystem(LevelGeneratorParameters parameters)
		{
			instance = this;
			character = parameters.character;
			planePrefab = parameters.planePrefab;

			freePlanes = new Queue<Plane>(maxPlanesCount);
			activePlanes = new Queue<Plane>(maxPlanesCount);
			for (int i = 0; i < maxPlanesCount; i++)
				freePlanes.Enqueue(new Plane(GameObject.Instantiate(planePrefab)));

			spawnRequests = new Queue<PlaneResp>(64);
		}

		public void FixedUpdate()
		{
			if (activePlanes.Count > 0)
			{
				var plane = activePlanes.Peek();
				if (plane.rigidbody.position.z + 5 < character.transform.position.z)
					DeactivatePlane();
			}
			if (activePlanes.Count < dynamicPlanesCount
				&& freePlanes.Count > 0
				&& spawnRequests.Count > 0)
				RespawnPlane(spawnRequests.Dequeue());
			foreach (var p in activePlanes)
			{
				p.rigidbody.rotation *= roteteUp;
				p.rigidbody.MovePosition(p.rigidbody.position - p.transform.forward * 16 * Time.fixedDeltaTime);
			}
		}

		private void DeactivatePlane()
		{
			//Debug.Log("DeactivatePlane " + dynamicPlanesCount);
			var plane = activePlanes.Dequeue();
			freePlanes.Enqueue(plane);
		}

		public void DeactivateAllPlanes()
		{
			Debug.Log("DeactivateAllPlanes " + activePlanes.Count);
			while (activePlanes.Count > 0)
			{
				var plane = activePlanes.Dequeue();
				freePlanes.Enqueue(plane);
			}
		}
		
		public void AddPlaneToQueue(Quaternion startRotation, Vector3 offset)
		{
			spawnRequests.Enqueue(new PlaneResp(startRotation, offset));
		}

		private void RespawnPlane(PlaneResp planeResp)
		{
			//Debug.Log("RespawnPlane " + dynamicPlanesCount);
			var plane = freePlanes.Dequeue();
			activePlanes.Enqueue(plane);
			if (plane.gameObject.activeSelf == false)
				plane.gameObject.SetActive(true);
			plane.rigidbody.position =
				character.transform.position
				+ planeResp.rotation * planeResp.rotation * Vector3.forward * 70 + Vector3.up * 4
				+ planeResp.positionOffset;
			plane.rigidbody.rotation = planeResp.rotation;
		}
	}
}