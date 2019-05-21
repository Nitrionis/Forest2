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

			public Vector3 lastCarStartPos;

			public class Car
			{
				public Rigidbody rigidbody;
				public Vector3 startPos, endPos;
				public float startTime, endTime;

				public Car(Rigidbody rigidbody)
				{
					this.rigidbody = rigidbody;
				}
			}
			public List<Car> cars;
			public float carsSpeed = 20;
			
			public Road(Vector3 center, float carsSpeed)
			{
				this.center = center;
				this.carsSpeed = carsSpeed;
				cars = new List<Car>(4);
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

		protected static RoadSystem instance;

		protected Transform character;
		protected GameObject roadPrefab;
		protected GameObject carPrefab;
		protected Quaternion inversCarRotation;
		// usedRoads is roads game objects which rendering now
		protected Queue<RoadObject> usedRoads;
		// free roads
		protected Queue<RoadObject> unusedRoads;
		// roadsRenderingNow is roads which rendering now
		protected Queue<Road> roadsRenderingNow;
		// roadsAwaitingRendering is roads planned for rendering
		protected Queue<Road> roadsAwaitingRendering;

		// cars which not renering now
		protected Queue<Road.Car> unusedCars;

		protected const int roadsCount = 6;
		// max roads per level = 16, max active levels count = 4
		protected const int maxLogicRoadsCount = 64;
		protected const float halfRoadWidth = 6;
		protected const float roadDestroyDistance = 30;

		public float roadOffsetByY;

		protected ForestBase forest;

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
			unusedCars = new Queue<Road.Car>(carsCount);
			for (int i = 0; i < carsCount; i++)
			{
				unusedCars.Enqueue(new Road.Car(GameObject.Instantiate(
					carPrefab, Vector3.zero, carPrefab.transform.rotation).GetComponent<Rigidbody>()));
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
				//if (roadsRenderingNow.Count > 0)
				//	Debug.Log("roadsRenderingNow.Count " + roadsRenderingNow.Count);
				float sdist;
				for (int i = 0; i < r.cars.Count; i++)
				{
					Road.Car car = r.cars[i];
					Rigidbody rb = car.rigidbody;

					bool state = false;
					sdist = rb.position.x - car.startPos.x;
					if (sdist > 0)
						state = rb.position.x - character.position.x > 50;
					if (sdist < 0)
						state = rb.position.x - character.position.x < -50;
					if (character.position.z - rb.position.z > 10 || state)
					{
						unusedCars.Enqueue(car);
						r.cars.RemoveAt(i);
						//Debug.Log("Destroy state" + state);
						DestroyCar(car, rb.position);
					}
					else
					{
						rb.MovePosition(rb.position + Time.fixedDeltaTime * r.carsSpeed * Vector3.right);
					}
				}
				 
				sdist = r.center.z - character.position.z;
				if (sdist < 50 && sdist > 10 && r.cars.Count < 3 && unusedCars.Count > 0)
				{
					Road.Car car = unusedCars.Dequeue();
					Rigidbody rb = car.rigidbody;

					float speedSign = Mathf.Sign(r.carsSpeed);
					float xOffset = speedSign * -50;
					Vector3 carPos = new Vector3(character.position.x + xOffset, r.center.y, r.center.z);

					float carSpacing = 10 + 10 * Level.difficulty;

					//if (r.cars.Count > 0 && Vector3.Distance(carPos, r.cars[r.cars.Count - 1].rigidbody.position) < carSpacing)
					//	carPos = r.cars[r.cars.Count - 1].rigidbody.position + Mathf.Sign(xOffset) * carSpacing * Vector3.left;

					if (r.cars.Count > 0 && (
						Mathf.Abs(carPos.x - r.lastCarStartPos.x) < carSpacing 
						|| speedSign * (r.lastCarStartPos.x - carPos.x) < 0))
						carPos = r.lastCarStartPos + speedSign * carSpacing * Vector3.left;

					rb.position = carPos;
					rb.rotation = r.carsSpeed < 0 ? inversCarRotation : carPrefab.transform.rotation;
					rb.velocity = Vector3.zero;
					r.cars.Add(car);
					r.lastCarStartPos = carPos;
					SpawnCar(car, carPos);
				}
			}
		}

		protected virtual void SpawnCar(Road.Car car, Vector3 position)
		{

		}

		protected virtual void DestroyCar(Road.Car car, Vector3 position)
		{

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
				Road.Car car = r.cars[i];
				Rigidbody rb = car.rigidbody;
				unusedCars.Enqueue(car);
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