using UnityEngine;

namespace Map
{
	public class ZombieMover
	{
		private bool prevActiveState = false;
		public bool isSpawnActive = false;
		public bool isMoveActive = false;

		private struct Zombie
		{
			public GameObject gameObject;
			public Renderer renderer;
			public Rigidbody rigidbody;
		}

		private Transform character;
		private GameObject zombie;

		private const int maxMobsCount = 8;
		private Zombie[] mobs;

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

		public void FixedUpdate()
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
					Vector3 pos = mobs[i].gameObject.transform.position;
					CameraMover c = CameraMover.instance;
					Vector3 newPos = pos;
					if (isSpawnActive && (pos.z < character.position.z || pos.y < character.position.y - 100))
					{
						Vector3 offset = Quaternion.AngleAxis(Random.Range(
								zombieCreationAngleRange.x,
								zombieCreationAngleRange.y),
							c.upAxis) * c.moveDir;
						newPos = character.position + offset * 54;
						mobs[i].rigidbody.velocity = Vector3.zero;
						mobs[i].rigidbody.rotation = Quaternion.identity;
					}
					Vector3 moveDir = -(newPos - character.position);
					moveDir.y = 0;
					moveDir.Normalize();
					mobs[i].rigidbody.MovePosition(newPos + 5 * Time.fixedDeltaTime * moveDir);
					mobs[i].rigidbody.MoveRotation(Quaternion.LookRotation(moveDir, Vector3.up));
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