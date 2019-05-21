using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	public abstract class ForestBase
	{
		protected const int treesCount = 64;
		public class TreeObject
		{
			public Transform mainTransform;
			public Transform treeTransform;

			public TreeObject(Transform mainTransform)
			{
				this.mainTransform = mainTransform;
				treeTransform = mainTransform.Find("RotatablePart");
			}
		}
		protected Stack<TreeObject> pool;

		public class Tree
		{
			public bool isActive;
			public int scaleIndex;
			public int rotationIndex;
			public Vector3 position;
			public TreeObject treeObject;
			//public GameObject debugObject;
			//public Material debugMaterial;
		}

		public class TreesLine
		{
			public Vector3 position;
			public Tree[] trees;

			public int gridLeftIndex;
			public int gridRightIndex;

			public TreesLine(Vector3 startPosition, int width)
			{
				position = startPosition;
				trees = new Tree[width];
				gridLeftIndex = 0;
				gridRightIndex = width - 1;
				for (int i = 0; i < width; i++)
				{
					trees[i] = new Tree();
					trees[i].isActive = true;

					//trees[i].debugObject = GameObject.Instantiate(TreeDebug.treeDebugObject, startPosition, Quaternion.identity);
					//trees[i].debugMaterial = trees[i].debugObject.GetComponent<Renderer>().material;
				}
			}
		}

		protected static ForestBase instance;

		public Transform character;
		public Transform characterCamera;
		public GameObject treePrefab;

		public float treeRadius = 1;
		public float halfFOV = 45;
		public float treesSpacing = 10.0f;

		public const int gridDepth = 12;
		public const int gridWidth = 20;

		protected int startGridIndex = 0;

		protected Vector3 groundForward = Quaternion.Euler(10, 0, 0) * Vector3.forward;

		protected Vector3[] gradients;
		protected Vector3[] scalePresets;
		protected Quaternion[] rotationPresets;

		protected float[] spacingDependencyByScale;

		protected float[] collisionDistance = new float[4] { 2.8f, 2.5f, 3.1f, 3.4f };

		public ForestBase(LevelGeneratorParameters parameters)
		{
			instance = this;

			character = parameters.character;
			characterCamera = parameters.characterCamera;
			treePrefab = parameters.treePrefab;

			treeRadius = parameters.treeRadius;
			halfFOV = parameters.halfFOV;
			treesSpacing = parameters.treesSpacing;

			gradients = new Vector3[16];
			Vector3 upAxis = Vector3.Cross(groundForward, Vector3.right);
			for (int i = 0; i < gradients.Length; i++)
				gradients[i] = Quaternion.AngleAxis(i * 360 / gradients.Length, upAxis) * groundForward;

			scalePresets = new Vector3[4]
			{
				1.0f * Vector3.one,
				0.8f * Vector3.one,
				1.2f * Vector3.one,
				1.4f * Vector3.one
			};
			spacingDependencyByScale = new float[4]
			{
				1.1f, 1.0f, 0.90f, 0.80f
			};

			rotationPresets = new Quaternion[8];
			for (int i = 0; i < rotationPresets.Length; i++)
				rotationPresets[i] = Quaternion.AngleAxis(i * 360 / rotationPresets.Length, Vector3.up);
		}

		public abstract void Update();

		public abstract Vector3 GetLastGridLinePosition();

		protected abstract void ForestMoveX(int sdi);

		protected abstract void MoveLeft(Vector3 pos);

		protected abstract void MoveRight(Vector3 pos);

		protected abstract void ForestMoveZ(int sdi);

		protected abstract void TreesCulling();

		public abstract TreesLine[] GetGrid();
	}
}