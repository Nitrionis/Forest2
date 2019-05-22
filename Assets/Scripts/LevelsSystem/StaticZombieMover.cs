using UnityEngine;

namespace Map
{
	public class StaticZombieMover : ZombieMover
	{
		public StaticZombieMover(LevelGeneratorParameters parameters) : base(parameters)
		{
			for (int i = 0; i < maxMobsCount; i++)
			{
				mobs[i].startPos.z = -1;
			}
		}

		public override void FixedUpdate()
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

						if (mobs[i].startPos.z > 0)
						{
							mobs[i].endPos = pos;
							mobs[i].endTime = Score.timer;

							var zombieInfo = new StatisticsManager.EnemyMoveInfo(
									mobs[i].startPos, mobs[i].startTime, mobs[i].endPos, mobs[i].endTime);
							StatisticsManager.PushZombieInfo(zombieInfo);
						}

						mobs[i].startPos = newPos;
						mobs[i].startTime = Score.timer;

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
				{
					var mob = mobs[i];
					mob.renderer.enabled = false;

					if (mob.startPos.z > 0)
					{
						mob.endPos = mob.rigidbody.position;
						mob.endTime = Score.timer;

						var zombieInfo = new StatisticsManager.EnemyMoveInfo(
								mob.startPos, mob.startTime, mob.endPos, mob.endTime);
						StatisticsManager.PushZombieInfo(zombieInfo);
					}

					mob.startPos.z = -1f;
				}
			}
		}
	}
}