
namespace Map
{
	public class StaticSimpleLevel : StaticLevel
	{
		public StaticSimpleLevel() : base() { }

		public StaticSimpleLevel(float difficulty) 
			: base(difficulty)
		{
			levelType = LevelType.Simple;
		}

		public override Level CreateNew(float difficulty)
		{
			return new StaticSimpleLevel(difficulty);
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