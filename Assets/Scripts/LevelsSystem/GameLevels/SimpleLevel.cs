using UnityEngine;

namespace Map
{
	public class SimpleLevel : Level
	{
		private const float baseLevelLength = 50;

		public SimpleLevel(bool isEmpty = false) : base(isEmpty)
		{
			Debug.Log("new SimpleLevel");
		}

		public override Level CreateNew()
		{
			return new SimpleLevel();
		}

		public override void InitializeLevelRect()
		{
			float levelLength = baseLevelLength * (1 - difficulty);
			float levelStartPos = lastlLevel.levelStartEnd.y;
			levelStartEnd = new Vector2(levelStartPos, levelStartPos + levelLength);
		}
	}
}