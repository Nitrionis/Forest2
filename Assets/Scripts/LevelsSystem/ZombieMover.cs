using UnityEngine;

namespace Map
{
	public class ZombieMover
	{
		protected bool prevActiveState = false;
		public bool isSpawnActive = false;
		public bool isMoveActive = false;

		protected struct Zombie
		{
			public GameObject gameObject;
			public Renderer renderer;
			public Rigidbody rigidbody;

			public Vector3 startPos, endPos;
			public float startTime, endTime;
		}

		protected Transform character;
		protected GameObject zombie;

		protected const int maxMobsCount = 8;
		protected Zombie[] mobs;

		public Vector2 zombieCreationAngleRange = new Vector2(-30, 30);

		public ZombieMover(LevelGeneratorParameters parameters)
		{
			character = parameters.character;
			zombie = parameters.zombiePrefab;

			mobs = new Zombie[maxMobsCount];
			for (int i = 0; i < maxMobsCount; i++)
			{
				mobs[i].gameObject = GameObject.Instantiate(zombie);
				mobs[i].rigidbody = mobs[i].gameObject.GetComponent<Rigidbody>();
				mobs[i].renderer = mobs[i].gameObject.transform.Find("zombie_zombie4").GetComponent<Renderer>();
			}
		}

		public virtual void FixedUpdate()
		{
			if (isMoveActive)
			{
				if (!prevActiveState && isSpawnActive)
				{
					prevActiveState = true;
					for (int i = 0; i < maxMobsCount; i++)
						mobs[i].renderer.enabled = true;
				}
				for (int i = 0; i < maxMobsCount; i++)
				{
					var rb = mobs[i].rigidbody;
					Vector3 pos = rb.position;
					CameraMover c = CameraMover.instance;
					Vector3 newPos = pos;
					if (isSpawnActive && (pos.z < character.position.z || pos.y < character.position.y - 100))
					{
						Vector3 offset = Quaternion.AngleAxis(Random.Range(
								zombieCreationAngleRange.x,
								zombieCreationAngleRange.y),
							c.upAxis) * c.moveDir;
						newPos = character.position + offset * 54;
						rb.position = newPos;
						rb.velocity = Vector3.zero;
						rb.rotation = Quaternion.identity;
					}
					else
					{
						Vector3 moveDir = -(newPos - character.position);
						moveDir.y = 0;
						moveDir.Normalize();
						rb.MovePosition(newPos + 5 * Time.fixedDeltaTime * moveDir);
						rb.MoveRotation(Quaternion.LookRotation(moveDir, Vector3.up));
					}
				}
			}
			else if (!isMoveActive && prevActiveState)
			{
				prevActiveState = false;
				for (int i = 0; i < maxMobsCount; i++)
					mobs[i].renderer.enabled = false;
			}
		}
	}
}