using UnityEngine;
using System.Collections.Generic;

namespace Map
{
	public class Forest : ForestBase
	{
		private TreesLine[] grid;

		public Forest(LevelGeneratorParameters parameters) : base(parameters)
		{
			grid = new TreesLine[gridDepth];
			for (int i = 0; i < grid.Length; i++)
				grid[i] = new TreesLine(parameters.startForesPposition + i * treesSpacing * groundForward, gridWidth);

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

		protected Vector3 GetRandomOffset(int scalePresetIndex)
		{
			return (0.18f + 0.14f * Random.value * spacingDependencyByScale[scalePresetIndex]) 
				* gradients[(int)(Random.value * 15.99f)] * treesSpacing;
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
			float leftPos = pos.x - (treesSpacing * gridWidth / 2);
			for (int i = 0; i < gridDepth; i++)
			{
				var treesLine = grid[i];
				treesLine.position.x = pos.x;

				var tree = treesLine.trees[treesLine.gridRightIndex];
				tree.position = new Vector3(leftPos, tree.position.y, tree.position.z) + GetRandomOffset(tree.scaleIndex);

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
			float rightPos = pos.x + (treesSpacing * gridWidth / 2);
			for (int i = 0; i < gridDepth; i++)
			{
				var treesLine = grid[i];
				treesLine.position.x = pos.x;

				var tree = treesLine.trees[treesLine.gridLeftIndex];
				tree.position = new Vector3(rightPos, tree.position.y, tree.position.z) + GetRandomOffset(tree.scaleIndex);

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
				int indexOfEnd = (sdi + gridDepth - 1) % gridDepth;
				Vector3 pos = grid[indexOfEnd].position + treesSpacing * groundForward;
				for (int x = 0; x < gridWidth; x++)
				{
					var tree = treesLine.trees[x];
					tree.scaleIndex = (int)(3.99f * Random.value);
					tree.rotationIndex = (int)(3.99f * Random.value);
					tree.position = pos + (x - gridWidth / 2) * treesSpacing * Vector3.right + GetRandomOffset(tree.scaleIndex);
					if (tree.treeObject != null)
					{
						pool.Push(tree.treeObject);
						tree.treeObject = null;
					}
					tree.isActive = !RoadSystem.CheckCollision(tree.position);
				}
				treesLine.position = pos;
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