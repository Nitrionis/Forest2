using System;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
	private static class RecordsManager
	{
		public const int MaxrecordsCount = 60 * 60 * 10;
		private static int[] recordsCount;
		private static Vector3[][] positions;

		private static int currentBufferIndex;

		static RecordsManager()
		{
			currentBufferIndex = 0;
			recordsCount = new int[] { 0, 0 };
			positions = new Vector3[2][];
			for (int i = 0; i < 2; i++)
			{
				positions[i] = new Vector3[MaxrecordsCount];
				for (int j = 0; j < MaxrecordsCount; j++)
					positions[i][j] = Vector3.zero;
			}
		}

		public static Vector3[] GetPreviousTrajectory()
		{
			return positions[1 - currentBufferIndex];
		}

		public static int GetPreviousTrajectoryRecordsCount()
		{
			return recordsCount[1 - currentBufferIndex];
		}

		public static void PushRecord(Vector3 pos)
		{
			positions[currentBufferIndex][recordsCount[currentBufferIndex]] = pos;
			recordsCount[currentBufferIndex]++;
		}

		public static void Swap()
		{
			currentBufferIndex = (currentBufferIndex + 1) % 2;
			recordsCount[currentBufferIndex] = 0;
		}
	}

	private int recordsCount;
	private Vector3[] positions;

	private LineRenderer lineRenderer;
	private int startRecordIndex = 0;
	private int endRecordIndex;

	public Transform character;
	public Transform characterPosition;
	public float trajectoryDistance;
	private int visibleTrajectoryPointsCount;
	private Vector3[] visibleTrajectory;

	void Start()
    {
		RecordsManager.Swap();
		lineRenderer = GetComponent<LineRenderer>();
		recordsCount = RecordsManager.GetPreviousTrajectoryRecordsCount();
		positions = RecordsManager.GetPreviousTrajectory();
		visibleTrajectoryPointsCount = (int)(trajectoryDistance / Time.fixedDeltaTime);
		endRecordIndex = Mathf.Min(
			recordsCount, startRecordIndex + visibleTrajectoryPointsCount);
		visibleTrajectory = new Vector3[visibleTrajectoryPointsCount];
		Array.Copy(positions, visibleTrajectory, Mathf.Min(recordsCount, visibleTrajectoryPointsCount));
	}

    void FixedUpdate()
    {
		if (Score.isGameStarted)
			RecordsManager.PushRecord(characterPosition.position + Vector3.down * 1f);

		int prevStartIndex = startRecordIndex;
		while (startRecordIndex < recordsCount && positions[startRecordIndex].z < characterPosition.position.z)
			startRecordIndex++;
		if (startRecordIndex == recordsCount)
			return;
		endRecordIndex = Mathf.Min(
			recordsCount, startRecordIndex + visibleTrajectoryPointsCount);
		
		if (prevStartIndex != startRecordIndex)
		{
			int newVerticesCount = Mathf.Min(
				recordsCount, 
				visibleTrajectoryPointsCount);
			Array.Copy(
				positions, startRecordIndex, 
				visibleTrajectory, 0, 
				newVerticesCount);
			lineRenderer.positionCount = newVerticesCount;
			lineRenderer.SetPositions(visibleTrajectory);
		}
	}
}
