using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public interface ITreesSpacingController
	{
		float GetSpacingByUniqueTreesLineId(int uniqueLineId);
	}

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
			//Debug.Log("static ForestStatic()");
		}

		public class StaticTreesLine : TreesLine
		{
			public static readonly float lineLeft = -24000.0f;
			public static readonly float lineRight = 24000.0f;

			public int uniqueLineId;
			public int firstTreeId;
			public float treesSpacing = 10.0f;

			public StaticTreesLine(Vector3 startPosition, int width, int firstTreeId, int uniqueLineId) 
				: base(startPosition, width)
			{
				this.firstTreeId = firstTreeId;
				this.uniqueLineId = uniqueLineId;
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

			public float GetPositionXByUniqueId(uint uniqueTreeId)
			{
				return lineLeft + (uniqueTreeId - firstTreeId) * treesSpacing;
			}
		}
		private StaticTreesLine[] grid;
		private int lastFirstTreeId;

		private ITreesSpacingController spacingController;

		public ForestStatic(LevelGeneratorParameters parameters, ITreesSpacingController spacingController) 
			: base(parameters)
		{
			this.spacingController = spacingController;

			grid = new StaticTreesLine[gridDepth];
			int firstTreeId = 0;
			for (int i = 0; i < grid.Length; i++)
			{
				int lineId = i - gridDepth;
				float spacing = spacingController.GetSpacingByUniqueTreesLineId(lineId);

				Vector3 startPos = parameters.startForesPposition + i * spacing * groundForward;
				grid[i] = new StaticTreesLine(startPos, gridWidth, firstTreeId, lineId);
				grid[i].treesSpacing = spacing;

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
				var line = grid[d];
				for (int x = 0; x < gridWidth; x++)
				{
					var tree = line.trees[x];
					tree.rotationIndex = 0;
					tree.scaleIndex = 0;
					tree.position = line.position + (x - gridWidth / 2) * line.treesSpacing * Vector3.right;
					//tree.debugObject.transform.position = tree.position;
					if ((Vector3.Angle(forward, tree.position - posForCulling) <= halfFOV)
						&& (Vector3.Distance(tree.position, character.position) < 65)
						&& (pool.Count > 0))
					{
						var go = pool.Pop();
						go.mainTransform.position = tree.position;
						go.treeTransform.rotation = rotationPresets[tree.rotationIndex];
						go.mainTransform.localScale = scalePresets[tree.scaleIndex];
						tree.treeObject = go;
					}
				}
				//Debug.Log("Line spacing " + line.treesSpacing + " pos " + line.position);
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

			CheckTreePosition(tree, uniqueTreeId);

			prevPositions[uniqueTreeId] = tree.position;
			return (0.18f + 0.14f * CustomRandom.Get(uniqueTreeId) 
				* spacingDependencyByScale[tree.scaleIndex])
				* gradients[(int)(15.99f * CustomRandom.Get(uniqueTreeId + 1))] * line.treesSpacing;
		}

		protected Vector3 GetRandomOffset(StaticTreesLine line, Tree tree, uint uniqueTreeId)
		{
			tree.scaleIndex = (int)(3.99f * CustomRandom.Get(uniqueTreeId + 2));

			CheckTreePosition(tree, uniqueTreeId);

			prevPositions[uniqueTreeId] = tree.position;
			return (0.18f + 0.14f * CustomRandom.Get(uniqueTreeId)
				* spacingDependencyByScale[tree.scaleIndex])
				* gradients[(int)(15.99f * CustomRandom.Get(uniqueTreeId + 1))] * line.treesSpacing;
		}

		private void CheckTreePosition(Tree tree, uint uniqueTreeId)
		{
			if (prevPositions[uniqueTreeId].z != undefinedPosition)
			{
				float distance = Vector3.Distance(prevPositions[uniqueTreeId], tree.position);
				if (distance > 0.1f)
					Debug.Log("Error id " + uniqueTreeId + " distance " + distance + " prevPosZ " + prevPositions[uniqueTreeId].z);
			}
		}

		protected override void ForestMoveX(int sdi)
		{
			var line = grid[sdi];
			Vector3 pos = line.position;
			float delta = pos.x - character.position.x;
			if (Mathf.Abs(delta) > line.treesSpacing)
			{
				if (delta < 0)
				{
					pos += Vector3.right * line.treesSpacing;
					MoveRight(pos);
				}
				else
				{
					pos += Vector3.left * line.treesSpacing;
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

				uint uniqueTreeId = treesLine.GetTreeUniqueId(treesLine.trees[treesLine.gridLeftIndex]) - 1;
				float leftPos = treesLine.GetPositionXByUniqueId(uniqueTreeId);

				var tree = treesLine.trees[treesLine.gridRightIndex];
				tree.position = new Vector3(leftPos, treesLine.position.y, treesLine.position.z);

				tree.position += GetRandomOffset(treesLine, tree, uniqueTreeId);

				//tree.debugObject.transform.position = tree.position;


				if (tree.treeObject != null)
				{
					pool.Push(tree.treeObject);
					tree.treeObject = null;
				}

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

				uint uniqueTreeId = treesLine.GetTreeUniqueId(treesLine.trees[treesLine.gridRightIndex]) + 1;
				float rightPos = treesLine.GetPositionXByUniqueId(uniqueTreeId);

				var tree = treesLine.trees[treesLine.gridLeftIndex];
				tree.position = new Vector3(rightPos, treesLine.position.y, treesLine.position.z);

				tree.position += GetRandomOffset(treesLine, tree, uniqueTreeId);

				//tree.debugObject.transform.position = tree.position;

				if (tree.treeObject != null)
				{
					pool.Push(tree.treeObject);
					tree.treeObject = null;
				}

				treesLine.gridRightIndex = (treesLine.gridRightIndex + 1) % gridWidth;
				treesLine.gridLeftIndex = (treesLine.gridLeftIndex + 1) % gridWidth;

				tree.isActive = !RoadSystem.CheckCollision(tree.position);
			}
		}

		protected override void ForestMoveZ(int sdi)
		{
			if (character.position.z - grid[sdi].position.z > 30)
			{
				var treesLine = grid[sdi];
				treesLine.gridLeftIndex = 0;
				treesLine.gridRightIndex = gridWidth - 1;

				lastFirstTreeId += treesLine.GetTreesCount();
				treesLine.firstTreeId = lastFirstTreeId;

				int indexOfEnd = (sdi + gridDepth - 1) % gridDepth;
				var endTreesLine = grid[indexOfEnd];

				treesLine.uniqueLineId = endTreesLine.uniqueLineId + 1;
				treesLine.treesSpacing = spacingController.GetSpacingByUniqueTreesLineId(treesLine.uniqueLineId);

				Vector3 pos = endTreesLine.position + (endTreesLine.treesSpacing + treesLine.treesSpacing) * 0.5f * groundForward;
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

					//tree.debugObject.transform.position = tree.position;
				}
				startGridIndex = (startGridIndex + 1) % gridDepth;

				//Debug.Log("Line spacing " + treesLine.treesSpacing + " pos " + treesLine.position);
			}
		}

		protected override void TreesCulling()
		{
			Score.poolSize = pool.Count;
			//float minDist = float.PositiveInfinity;
			Vector3 forward = characterCamera.forward;
			Vector3 posForCulling = character.position - characterCamera.forward * 10;

			int activeCount = 0;

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
					if ((angle <= halfFOV && distance < 65) || distance < 10)
					{
						activeCount++;
						//tree.debugMaterial.SetColor("_Color", Color.green);
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
						//tree.debugMaterial.SetColor("_Color", Color.red);
						pool.Push(tree.treeObject);
						if (!tree.isActive)
							tree.treeObject.mainTransform.position += Vector3.down * 16;
						tree.treeObject = null;
					}
				}
			}

			//Debug.Log("ActiveCount " + activeCount);
		}

		public override TreesLine[] GetGrid()
		{
			return grid;
		}
	}
}
