using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public abstract class Level
	{
		public static Queue<Level> levels;
		public static Level lastlLevel;

		public static Transform character;
		public static float characterSpeed;

		public static Forest forest;
		public static RoadSystem roadSystem;
		public static ZombieMover zombieMover;
		public static PlanesSystem planesSystem;
		// difficulty is value between 0 and 1
		public static float difficulty;

		public Vector2 levelStartEnd;

		public const float fogDistance = 50.0f;
		public const float levelMinLength = 50.0f;

		public bool firstStartcall = true;

		protected enum LevelId
		{
			Undefined,
			Motorway
		}
		protected LevelId levelId;


		public Level(bool isEmpty)
		{
			levelId = LevelId.Undefined;
			if (!isEmpty)
			{
				InitializeLevelRect();
			}
		}

		public virtual void Start() { }
		public virtual void Update() { }
		public virtual void FixedUpdate() { }
		public virtual void Finish() { }

		public abstract void InitializeLevelRect();
		public abstract Level CreateNew();

		public bool IsLevelActive()
		{
			return character.position.z > levelStartEnd.x && character.position.z < levelStartEnd.y;
		}
		public bool IsLevelСompleted()
		{
			return character.position.z > levelStartEnd.y;
		}

		public virtual float GetMinZOffsetFromCharacter()
		{
			return 0.0f;
		}
	}
}