using UnityEngine;

namespace Map
{
	public class StaticZombieLevels : StaticLevel
	{
		public StaticZombieLevels() : base() { }

		public StaticZombieLevels(float difficulty, float startPos)
			: base(difficulty, startPos)
		{
			levelType = LevelType.Simple;
			//Debug.Log("StaticSimpleLevel " + levelStartEnd);
		}

		public override Level CreateNew(float difficulty, float startPos)
		{
			return new StaticSimpleLevel(difficulty, startPos);
		}

		public override Level CreateNew()
		{
			throw new System.NotImplementedException();
		}

		public override int GetTreesLinesCount(float difficulty)
		{
			return 5;
		}
	}
}