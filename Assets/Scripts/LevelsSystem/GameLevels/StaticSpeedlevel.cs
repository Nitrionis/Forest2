using UnityEngine;

namespace Map
{
	public class StaticSpeedLevel : StaticLevel
	{
		public static ParticleSystem speedParticleSystem;
		private const float easySpeed = 25;
		private const float hardSpeed = 35;

		private const float easyTreesSpacing = 16;
		private const float hardTreesSpacing = 16;

		private const int easyLinesCount = 40;
		private const int hardLinesCount = 30;

		private float speed;
		private float targetSpeed;
		private float startSpeed;
		private float timeCount;

		public StaticSpeedLevel() : base() { }

		public StaticSpeedLevel(float difficulty, float startPos)
			: base(difficulty, startPos)
		{
			levelType = LevelType.Speed;
			levelStartEnd = new Vector2(startPos - 10f, startPos + (treesLinesCount * treesLineSpacing) - 60f);
			//Debug.Log("StaticSpeedLevel " + levelStartEnd);
		}

		public override Level CreateNew(float difficulty, float startPos)
		{
			return new StaticSpeedLevel(difficulty, startPos);
		}

		public override Level CreateNew()
		{
			throw new System.NotImplementedException();
		}

		public override int GetTreesLinesCount(float difficulty)
		{
			return (int)Mathf.Lerp(easyLinesCount, hardLinesCount, difficulty);
		}

		public override float GetTreeSpacing(float difficulty)
		{
			return Mathf.Lerp(easyTreesSpacing, hardTreesSpacing, difficulty);
		}

		public override void Start()
		{
			speed = 0;
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
	}
}