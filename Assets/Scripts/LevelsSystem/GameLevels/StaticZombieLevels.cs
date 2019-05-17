using UnityEngine;

namespace Map
{
	public class StaticZombieLevels : StaticLevel
	{
		private const float baseTreesLinesCount = 32;
		private const float maxTreesLinesCount = 64;

		public StaticZombieLevels() : base() { }

		public StaticZombieLevels(float difficulty, float startPos)
			: base(difficulty, startPos)
		{
			levelType = LevelType.Simple;
		}

		public override Level CreateNew(float difficulty, float startPos)
		{
			return new StaticZombieLevels(difficulty, startPos);
		}

		public override Level CreateNew()
		{
			throw new System.NotImplementedException();
		}

		public override int GetTreesLinesCount(float difficulty)
		{
			return (int)Mathf.Lerp(baseTreesLinesCount, maxTreesLinesCount, difficulty);
		}

		public override void Start()
		{
			zombieMover.isMoveActive = true;
			zombieMover.isSpawnActive = true;
			SkyboxChanger.DayToNight();
		}

		public override void Finish()
		{
			zombieMover.isSpawnActive = false;
			zombieMover.isMoveActive = false;
			SkyboxChanger.NightToDay();
		}
	}
}