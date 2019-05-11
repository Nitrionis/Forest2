using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class RoadSystem
	{
		// graphical road representation
		public class RoadObject
		{
			public Transform transform;
			public Renderer renderer;

			public RoadObject(Transform transform, Renderer renderer)
			{
				this.transform = transform;
				this.renderer = renderer;
			}
		}
		// logical road representation
		public class Road
		{
			static public Transform character;
			static public readonly Vector3 roadUp;
			static public readonly Vector3 roadForward;
			static public readonly Quaternion identityQuaternion;

			static Road()
			{
				identityQuaternion = Quaternion.Euler(10, 0, 0);
				roadForward = identityQuaternion * Vector3.forward;
				roadUp = Vector3.Cross(roadForward, Vector3.right);
			}

			public Vector3 center;
			public RoadObject roadObject;
			public List<Rigidbody> cars;
			public float carsSpeed = 20;
			
			public Road(Vector3 center, float carsSpeed)
			{
				this.center = center;
				this.carsSpeed = carsSpeed;
				cars = new List<Rigidbody>(4);
			}

			public void ActivateRenderer(RoadObject roadObject)
			{
				this.roadObject = roadObject;
				//center.x = character.position.x;
				roadObject.transform.position = center;
				roadObject.transform.rotation = identityQuaternion;
				roadObject.renderer.enabled = true;
			}
			// Returns signed distance
			public float GetDistance(Vector3 p)
			{
				return p.z - center.z;
			}
		}

		private static RoadSystem instance;

		private Transform character;
		private GameObject roadPrefab;
		private GameObject carPrefab;
		private Quaternion inversCarRotation;
		// usedRoads is roads game objects which rendering now
		private Queue<RoadObject> usedRoads;
		// free roads
		private Queue<RoadObject> unusedRoads;
		// roadsRenderingNow is roads which rendering now
		private Queue<Road> roadsRenderingNow;
		// roadsAwaitingRendering is roads planned for rendering
		private Queue<Road> roadsAwaitingRendering;

		// cars which not renering now
		private Queue<Rigidbody> unusedCars;

		private const int roadsCount = 6;
		// max roads per level = 16, max active levels count = 4
		private const int maxLogicRoadsCount = 64;
		private const float halfRoadWidth = 6;
		private const float roadDestroyDistance = 30;

		public float roadOffsetByY;

		private ForestBase forest;

		public RoadSystem(LevelGeneratorParameters parameters, ForestBase forest)
		{
			instance = this;

			this.forest = forest;

			character = parameters.character;
			roadPrefab = parameters.roadPrefab;
			carPrefab = parameters.carPrefab;
			inversCarRotation = carPrefab.transform.rotation * Quaternion.AngleAxis(180, Vector3.up);

			Road.character = character;

			roadOffsetByY = parameters.roadOffsetByY;

			roadsRenderingNow = new Queue<Road>(roadsCount);
			roadsAwaitingRendering = new Queue<Road>(maxLogicRoadsCount);

			var meshFilter = roadPrefab.GetComponent<MeshFilter>();
			Mesh mesh = meshFilter.mesh;
			//Debug.Log(mesh.bounds);
			Bounds bounds = mesh.bounds;
			bounds.extents = new Vector3(1000000, 0, 5);
			mesh.bounds = bounds;


			unusedRoads = new Queue<RoadObject>(roadsCount);
			for (int i = 0; i < roadsCount; i++)
			{
				var gameObject = GameObject.Instantiate(roadPrefab, Vector3.zero, Quaternion.identity);
				gameObject.SetActive(true);
				var renderer = gameObject.GetComponent<Renderer>();
				renderer.enabled = true;
				unusedRoads.Enqueue(new RoadObject(gameObject.transform, renderer));
			}
			usedRoads = new Queue<RoadObject>(roadsCount);

			int carsCount = 8;
			unusedCars = new Queue<Rigidbody>(carsCount);
			for (int i = 0; i < carsCount; i++)
			{
				unusedCars.Enqueue(GameObject.Instantiate(
					carPrefab, Vector3.zero, carPrefab.transform.rotation).GetComponent<Rigidbody>());
			}
		}

		public void Update()
		{
			if (roadsRenderingNow.Count > 0)
			{
				var firstRoad = roadsRenderingNow.Peek();
				float distance = firstRoad.GetDistance(character.position);
				if (distance > roadDestroyDistance)
				{
					//Debug.Log("DeactivateFirstRoadInQueue");
					DeactivateFirstRoadInQueue();
				}
			}
			if (unusedRoads.Count > 0
				&& roadsAwaitingRendering.Count > 0
				&& roadsRenderingNow.Count < roadsCount)
			{
				//Debug.Log("Get Next Road from unusedRoads");
				var gameObject = unusedRoads.Dequeue();
				usedRoads.Enqueue(gameObject);

				var road = roadsAwaitingRendering.Dequeue();
				roadsRenderingNow.Enqueue(road);

				road.ActivateRenderer(gameObject);

				CheckGlobalCollision(road);
			}
		}

		public void FixedUpdate()
		{
			MoveCars();
		}

		public void MoveCars()
		{
			foreach (Road r in roadsRenderingNow)
			{
				for (int i = 0; i < r.cars.Count; i++)
				{
					Rigidbody rb = r.cars[i];
					if (character.position.z - 10 < rb.position.z && Vector3.Distance(rb.position, character.position) < 65)
					{
						rb.MovePosition(rb.position + Time.fixedDeltaTime * r.carsSpeed * Vector3.right);
					}
					else
					{
						unusedCars.Enqueue(rb);
						r.cars.RemoveAt(i);
					}
				}
				if (r.cars.Count < 3 && unusedCars.Count > 0)
				{
					Rigidbody rb = unusedCars.Dequeue();
					float xOffset = r.carsSpeed < 0 ? 50 : -50;
					Vector3 carPos = new Vector3(character.position.x + xOffset, r.center.y, r.center.z);
					float carSpacing = 10 + 10 * Level.difficulty;
					if (r.cars.Count > 0 && Vector3.Distance(carPos, r.cars[r.cars.Count - 1].position) < carSpacing)
						carPos = r.cars[r.cars.Count - 1].position + Mathf.Sign(-xOffset) * carSpacing * Vector3.left;
					rb.position = carPos;
					rb.rotation = r.carsSpeed < 0 ? inversCarRotation : carPrefab.transform.rotation;
					rb.velocity = Vector3.zero;
					r.cars.Add(rb);
				}
			}
		}

		public static bool CheckCollision(Vector3 p)
		{
			foreach (Road r in instance.roadsRenderingNow)
			{
				float sdist = r.GetDistance(p);
				if (sdist > -halfRoadWidth && sdist < halfRoadWidth)
					return true;
			}
			return false;
		}

		private void DeactivateFirstRoadInQueue()
		{
			unusedRoads.Enqueue(usedRoads.Dequeue());
			var r = roadsRenderingNow.Dequeue();
			for (int i = 0; i < r.cars.Count; i++)
			{
				Rigidbody rb = r.cars[i];
				unusedCars.Enqueue(rb);
			}
		}

		public void AddRoadsToQueue(List<Road> roads)
		{
			foreach (var r in roads)
				roadsAwaitingRendering.Enqueue(r);
		}

		public void CheckGlobalCollision(Road road)
		{
			//Debug.Log("CheckGlobalCollision");
			var grid = forest.GetGrid();
			for (int d = 0; d < grid.Length; d++)
			{
				for (int x = 0; x < grid[d].trees.Length; x++)
				{
					var tree = grid[d].trees[x];
					if (tree.isActive)
					{
						float sdist = road.GetDistance(tree.position);
						float dist = Mathf.Abs(sdist);
						tree.isActive = dist < -halfRoadWidth || dist > halfRoadWidth;
						if (!tree.isActive && tree.treeObject != null)
							tree.treeObject.mainTransform.position += Vector3.down * 16;
					}
				}
			}
		}
	}
}