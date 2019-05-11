﻿using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public class ForestStatic : ForestBase
	{
		private static Vector3[] prevPositions;

		private const float undefinedPosition = -1000000;

		static ForestStatic()
		{
			prevPositions = new Vector3[11520000];
			var value = new Vector3(0,0, undefinedPosition);
			for (int i = 0; i < prevPositions.Length; i++)
				prevPositions[i] = value;
			Debug.Log("static ForestStatic()");
		}

		public class StaticTreesLine : TreesLine
		{
			public static readonly float lineLeft = -24000.0f;
			public static readonly float lineRight = 24000.0f;

			public int firstTreeId;
			public float treesSpacing = 10.0f;
			public float visibleLeft = 0, visibleRight = 0;

			public StaticTreesLine(Vector3 startPosition, int width, int firstTreeId) 
				: base(startPosition, width)
			{
				this.firstTreeId = firstTreeId;
			}

			public int GetTreesCount()
			{
				return (int)(2 * lineRight / treesSpacing);
			}
			
			public uint GetTreeUniqueId(Tree tree)
			{
				return (uint)firstTreeId + (uint)Mathf.Round((tree.position.x + lineRight) / treesSpacing);
			}

			public uint GetLocalTreeId(float treePosX)
			{
				return (uint)Mathf.Round((treePosX + lineRight) / treesSpacing);
			}
		}
		private StaticTreesLine[] grid;
		private int lastFirstTreeId;

		public ForestStatic(LevelGeneratorParameters parameters) : base(parameters)
		{
			grid = new StaticTreesLine[gridDepth];
			int firstTreeId = 0;
			for (int i = 0; i < grid.Length; i++)
			{
				Vector3 startPos = parameters.startForesPposition + i * treesSpacing * groundForward;
				grid[i] = new StaticTreesLine(startPos, gridWidth, firstTreeId);
				grid[i].visibleLeft = startPos.x - (gridWidth / 2) * treesSpacing;
				grid[i].visibleRight = startPos.x + (gridWidth / 2) * treesSpacing;

				firstTreeId += grid[i].GetTreesCount();
			}
			lastFirstTreeId = firstTreeId;

			pool = new Stack<TreeObject>(treesCount);
			for (int i = 0; i < treesCount; i++)
				pool.Push(new TreeObject(GameObject.Instantiate(treePrefab, 100 * Vector3.down, Quaternion.identity).transform));

			Vector3 forward = character.forward;
			Vector3 posForCulling = character.position + Vector3.back * 8;
			for (int d = 0; d < grid.Length; d++)
			{
				for (int x = 0; x < gridWidth; x++)
				{
					var tree = grid[d].trees[x];
					tree.rotationIndex = 0;
					tree.scaleIndex = 0;
					tree.position = grid[d].position + (x - gridWidth / 2) * treesSpacing * Vector3.right;
					if ((Vector3.Angle(forward, tree.position - posForCulling) <= halfFOV)
						&& (Vector3.Distance(tree.position, character.position) < 80)
						&& (pool.Count > 0))
					{
						var go = pool.Pop();
						go.mainTransform.position = tree.position;
						go.treeTransform.rotation = rotationPresets[tree.rotationIndex];
						go.mainTransform.localScale = scalePresets[tree.scaleIndex];
						tree.treeObject = go;
					}
				}
			}
		}

		public override void Update()
		{
			ForestMoveX(startGridIndex);
			ForestMoveZ(startGridIndex);

			TreesCulling();
		}

		public override Vector3 GetLastGridLinePosition()
		{
			int indexOfEnd = (startGridIndex + gridDepth - 1) % gridDepth;
			return grid[indexOfEnd].position;
		}

		protected Vector3 GetRandomOffset(StaticTreesLine line, Tree tree)
		{
			uint uniqueTreeId = line.GetTreeUniqueId(tree);
			tree.scaleIndex = (int)(3.99f * CustomRandom.Get(uniqueTreeId + 2));
			if (prevPositions[uniqueTreeId].z != undefinedPosition)
			{
				float distance = Vector3.Distance(prevPositions[uniqueTreeId], tree.position);
				if (distance > 0.1f)
					Debug.Log("Error id " + uniqueTreeId + " distance " + distance + " prevPosZ " + prevPositions[uniqueTreeId].z);
			}
			prevPositions[uniqueTreeId] = tree.position;
			return (0.18f + 0.14f * CustomRandom.Get(uniqueTreeId) 
				* spacingDependencyByScale[tree.scaleIndex])
				* gradients[(int)(15.99f * CustomRandom.Get(uniqueTreeId + 1))] * treesSpacing;
		}

		protected override void ForestMoveX(int sdi)
		{
			Vector3 pos = grid[sdi].position;
			float delta = pos.x - character.position.x;
			if (Mathf.Abs(delta) > treesSpacing)
			{
				if (delta < 0)
				{
					pos += Vector3.right * treesSpacing;
					MoveRight(pos);
				}
				else
				{
					pos += Vector3.left * treesSpacing;
					MoveLeft(pos);
				}
			}
		}

		protected override void MoveLeft(Vector3 pos)
		{
			for (int i = 0; i < gridDepth; i++)
			{
				var treesLine = grid[i];
				treesLine.position.x = pos.x;

				treesLine.visibleLeft -= treesLine.treesSpacing;
				treesLine.visibleRight -= treesLine.treesSpacing;
				// TODO update this
				float leftPos = treesLine.visibleLeft;
				

				var tree = treesLine.trees[treesLine.gridRightIndex];
				tree.position = new Vector3(leftPos, tree.position.y, tree.position.z);
				tree.position += GetRandomOffset(treesLine, tree);

				if (tree.treeObject != null)
					tree.treeObject.mainTransform.position = tree.position;

				treesLine.gridRightIndex -= 1;
				if (treesLine.gridRightIndex < 0)
					treesLine.gridRightIndex = gridWidth - 1;
				treesLine.gridLeftIndex -= 1;
				if (treesLine.gridLeftIndex < 0)
					treesLine.gridLeftIndex = gridWidth - 1;

				tree.isActive = !RoadSystem.CheckCollision(tree.position);
			}
		}

		protected override void MoveRight(Vector3 pos)
		{
			for (int i = 0; i < gridDepth; i++)
			{
				var treesLine = grid[i];
				treesLine.position.x = pos.x;

				treesLine.visibleLeft += treesLine.treesSpacing;
				treesLine.visibleRight += treesLine.treesSpacing;
				// TODO update this
				float rightPos = treesLine.visibleRight;
				// TODO

				var tree = treesLine.trees[treesLine.gridLeftIndex];
				tree.position = new Vector3(rightPos, tree.position.y, tree.position.z);
				tree.position += GetRandomOffset(treesLine, tree);

				if (tree.treeObject != null)
					tree.treeObject.mainTransform.position = tree.position;

				treesLine.gridRightIndex = (treesLine.gridRightIndex + 1) % gridWidth;
				treesLine.gridLeftIndex = (treesLine.gridLeftIndex + 1) % gridWidth;

				tree.isActive = !RoadSystem.CheckCollision(tree.position);
			}
		}

		protected override void ForestMoveZ(int sdi)
		{
			if (character.position.z - grid[sdi].position.z > treesSpacing * 3)
			{
				var treesLine = grid[sdi];
				treesLine.gridLeftIndex = 0;
				treesLine.gridRightIndex = gridWidth - 1;
				// TODO not work
				lastFirstTreeId += treesLine.GetTreesCount();
				treesLine.firstTreeId = lastFirstTreeId;

				int indexOfEnd = (sdi + gridDepth - 1) % gridDepth;
				Vector3 pos = grid[indexOfEnd].position + treesSpacing * groundForward;
				treesLine.position = pos;
				float startTreePos = pos.x - (gridWidth / 2) * treesLine.treesSpacing;
				uint firstTreeLocalId = treesLine.GetLocalTreeId(startTreePos);
				pos.x = StaticTreesLine.lineLeft + firstTreeLocalId * treesLine.treesSpacing;

				for (int x = 0; x < gridWidth; x++)
				{
					var tree = treesLine.trees[x];
					//tree.scaleIndex recalculated in GetRandomOffset
					tree.rotationIndex = (int)(3.99f * Random.value);
					pos.x += treesLine.treesSpacing;
					tree.position = pos;
					tree.position += GetRandomOffset(treesLine, tree);
					if (tree.treeObject != null)
					{
						pool.Push(tree.treeObject);
						tree.treeObject = null;
					}
					tree.isActive = !RoadSystem.CheckCollision(tree.position);
				}
				startGridIndex = (startGridIndex + 1) % gridDepth;
			}
		}

		protected override void TreesCulling()
		{
			//float minDist = float.PositiveInfinity;
			Vector3 forward = characterCamera.forward;
			Vector3 posForCulling = character.position - characterCamera.forward * 10;
			for (int d = 0; d < grid.Length; d++)
			{
				for (int x = 0; x < gridWidth; x++)
				{
					var tree = grid[d].trees[x];
					float distance = Vector3.Distance(tree.position, character.position);

					//minDist = Mathf.Min(minDist, distance);

					float distanceScaleFactor = scalePresets[tree.scaleIndex].x;
					if (tree.isActive && distance < collisionDistance[tree.scaleIndex])
						CameraMover.FatalСollision();
					//else if (distance < 2f + 1f * distanceScaleFactor)
					//	CameraMover.NonfatalСollision();

					float angle = Vector3.Angle(forward, tree.position - posForCulling);
					if ((angle <= halfFOV && distance < 55) || distance < 10)
					{
						if ((pool.Count > 0) && (tree.treeObject == null) && tree.isActive)
						{
							var go = pool.Pop();
							go.mainTransform.position = tree.position;
							go.treeTransform.rotation = rotationPresets[tree.rotationIndex];
							go.mainTransform.localScale = scalePresets[tree.scaleIndex];
							tree.treeObject = go;
						}
					}
					else if (tree.treeObject != null)
					{
						pool.Push(tree.treeObject);
						if (!tree.isActive)
							tree.treeObject.mainTransform.position += Vector3.down * 16;
						tree.treeObject = null;
					}
				}
			}
		}

		public override TreesLine[] GetGrid()
		{
			return grid;
		}
	}
}
