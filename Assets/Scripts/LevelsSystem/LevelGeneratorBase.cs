using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public abstract class LevelGeneratorBase
	{
		protected class EmptyLevel : Level
		{
			public EmptyLevel() : base(true) { }

			public override Level CreateNew() { return null; }

			public override void InitializeLevelRect()
			{
				levelStartEnd = new Vector2(0, character.position.z);
			}
		}

		public float easyMoveSpeed;
		public float hardMoveSpeed;

		protected int prevLevelId = -1;

		public LevelGeneratorBase(LevelGeneratorParameters parameters)
		{
			easyMoveSpeed = parameters.easyMoveSpeed;
			hardMoveSpeed = parameters.hardMoveSpeed;
		}
		
		public abstract void Update();
		public abstract void FixedUpdate();
	}
}