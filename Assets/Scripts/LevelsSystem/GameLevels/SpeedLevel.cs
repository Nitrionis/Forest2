using UnityEngine;

namespace Map
{
	public class SpeedLevel : Level
	{
		public static ParticleSystem speedParticleSystem;
		private const float easySpeed = 25;
		private const float hardSpeed = 40;

		private const float defaultTreesSpacing = 11;
		private const float easyTreesSpacing = 16;
		private const float hardTreesSpacing = 11;

		private float levelDuration = 20;

		private float speed;
		private float targetSpeed;
		private float startSpeed;
		private float timeCount;
		private float treesSpacing;

		public SpeedLevel(bool isEmpty = false) : base(isEmpty){}

		public override Level CreateNew()
		{
			return new SpeedLevel();
		}

		public override void InitializeLevelRect()
		{
			timeCount = 0;
			levelStartEnd = new Vector2(
				lastlLevel.levelStartEnd.y,
				lastlLevel.levelStartEnd.y + hardSpeed * levelDuration);
			treesSpacing = Mathf.Lerp(easyTreesSpacing, hardTreesSpacing, difficulty);
		}

		public override void FixedUpdate()
		{
			if (levelStartEnd.y - character.position.z < 50)
			{
				forest.treesSpacing = defaultTreesSpacing;
			}
			else if (levelStartEnd.x - character.position.z < 100)
			{
				forest.treesSpacing = treesSpacing;
			}
		}

		public override void Update()
		{
			if (IsLevelActive())
			{
				timeCount += Time.deltaTime * 0.02f;
				speed = Mathf.Lerp(speed, targetSpeed, timeCount);
				CameraMover.LockSpeed(speed);
				if (speedParticleSystem.isStopped)
					speedParticleSystem.Play();
			}
		}

		public override void Start()
		{
			speed = 0;// CameraMover.instance.moveSpeed;
			targetSpeed = Mathf.Lerp(easySpeed, hardSpeed, difficulty);
			CameraMover.LockSpeed(speed);
			speedParticleSystem.Play();
			Score.scoreCoef = 3f;
		}

		public override void Finish()
		{
			CameraMover.UnlockSpeed();
			speedParticleSystem.Stop();
			Score.scoreCoef = 1f;
		}

		//private void Change
	}
}