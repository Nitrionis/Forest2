using UnityEngine;

namespace Map
{
	public abstract class StaticLevel : Level
	{
		public int treesLinesCount;
		public readonly float treesLineSpacing;
		public new readonly float difficulty;

		public StaticLevel() : base(true) {}

		public StaticLevel(float difficulty) : base()
		{
			this.difficulty = difficulty;
			treesLineSpacing = GetTreeSpacing(difficulty);
			InitializeLevelRect();
		}

		public abstract int GetTreesLinesCount(float difficulty);

		public override void InitializeLevelRect()
		{
			treesLinesCount = GetTreesLinesCount(difficulty);
			levelStartEnd = new Vector2(
				lastlLevel.levelStartEnd.y,
				lastlLevel.levelStartEnd.y + (treesLinesCount * treesLineSpacing));
		}

		public virtual float GetTreeSpacing(float difficulty)
		{
			return 10.0f;
		}

		public abstract Level CreateNew(float difficulty);
	}
}
