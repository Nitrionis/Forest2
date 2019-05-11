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

		public static ForestBase forest;
		public static RoadSystem roadSystem;
		public static ZombieMover zombieMover;
		public static PlanesSystem planesSystem;
		// difficulty is value between 0 and 1
		public static float difficulty;

		public Vector2 levelStartEnd;

		public const float fogDistance = 50.0f;
		public const float levelMinLength = 50.0f;

		public bool firstStartcall = true;

		protected enum LevelType
		{
			Undefined	= -1,
			Simple		= 0,
			Speed		= 1,
			Motorway	= 2,
			Planes		= 3,
			Zombies		= 4
		}
		protected LevelType levelType;


		public Level(bool isEmpty)
		{
			levelType = LevelType.Undefined;
			if (!isEmpty)
			{
				InitializeLevelRect();
			}
		}

		public Level()
		{
			levelType = LevelType.Undefined;
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