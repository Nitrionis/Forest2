using UnityEngine;

namespace Map
{
	public abstract class StaticLevel : Level
	{
		public int treesLinesCount;
		public readonly float treesLineSpacing;
		public new readonly float difficulty;

		public StaticLevel() : base(true) {}

		public StaticLevel(float difficulty, float startPos) : base()
		{
			this.difficulty = difficulty;
			treesLineSpacing = GetTreeSpacing(difficulty);
			treesLinesCount = GetTreesLinesCount(difficulty);
			levelStartEnd = new Vector2(startPos, startPos + (treesLinesCount * treesLineSpacing));
		}

		public abstract int GetTreesLinesCount(float difficulty);

		public virtual float GetTreeSpacing(float difficulty)
		{
			return 12.0f;
		}

		public abstract Level CreateNew(float difficulty, float startPos);

		public override void InitializeLevelRect() { }
	}
}
