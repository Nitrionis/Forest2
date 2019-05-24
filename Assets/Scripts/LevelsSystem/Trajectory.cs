using System;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
	public static class RecordsManager
	{
		public const int MaxrecordsCount = 10 * 60 * 25 + 100;
		private static int[] recordsCount;
		private static Vector3[][] positions;
		private static Quaternion[][] rotations;

		private static int currentBufferIndex;

		static RecordsManager()
		{
			currentBufferIndex = 0;
			recordsCount = new int[] { 0, 0 };
			positions = new Vector3[2][];
			rotations = new Quaternion[2][];
			for (int i = 0; i < 2; i++)
			{
				positions[i] = new Vector3[MaxrecordsCount];
				rotations[i] = new Quaternion[MaxrecordsCount];
			}
		}

		public static Vector3[] GetPreviousTrajectory()
		{
			return positions[1 - currentBufferIndex];
		}

		public static Quaternion[] GetPreviousTrajectoryQuaternions()
		{
			return rotations[1 - currentBufferIndex];
		}

		public static int GetPreviousTrajectoryRecordsCount()
		{
			return recordsCount[1 - currentBufferIndex];
		}

		public static void PushRecord(Vector3 pos, Quaternion rot)
		{
			if (recordsCount[currentBufferIndex] < MaxrecordsCount)
			{
				positions[currentBufferIndex][recordsCount[currentBufferIndex]] = pos;
				rotations[currentBufferIndex][recordsCount[currentBufferIndex]] = rot;
				recordsCount[currentBufferIndex]++;
			}
			else
			{
				Score.GameOver(true);
			}
		}

		public static void Swap()
		{
			currentBufferIndex = (currentBufferIndex + 1) % 2;

			int writeIndex = 1 - currentBufferIndex;

			if (recordsCount[writeIndex] != 0)
			{
				TrajectoryStatistics statistics = new TrajectoryStatistics();
				statistics.recordsCount = recordsCount[writeIndex];
				statistics.positions = positions[writeIndex];
				statistics.rotations = rotations[writeIndex];
				StatisticsManager.Write(statistics);
			}
			
			recordsCount[writeIndex] = StatisticsManager.Read(positions[writeIndex], rotations[writeIndex]);

			recordsCount[currentBufferIndex] = 0;
		}
	}

	private int recordsCount;
	private Vector3[] positions;

	private LineRenderer lineRenderer;
	private int startRecordIndex;
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

		startRecordIndex = 0;
		endRecordIndex = Mathf.Min(
			recordsCount, startRecordIndex + visibleTrajectoryPointsCount);

		visibleTrajectory = new Vector3[visibleTrajectoryPointsCount];
		Array.Copy(positions, visibleTrajectory, Mathf.Min(recordsCount, visibleTrajectoryPointsCount));
	}

	private float time = 0;

	void Update()
	{
		int prevStartIndex = startRecordIndex;
		while (startRecordIndex < recordsCount && positions[startRecordIndex].z < characterPosition.position.z)
			startRecordIndex++;
		if (startRecordIndex == recordsCount)
			return;
		endRecordIndex = Mathf.Min(
			recordsCount, startRecordIndex + visibleTrajectoryPointsCount);

		int newVerticesCount = Mathf.Max(0, endRecordIndex - startRecordIndex);
		
		if (startRecordIndex > 0)
		{
			float t = (characterPosition.position.z - positions[startRecordIndex - 1].z)
				/ (positions[startRecordIndex].z - positions[startRecordIndex - 1].z);

			t = (time += Time.deltaTime);
			//Debug.Log("parametr t " + t);

			//string msg = "Trajectory";

			for (int i = 0, j = startRecordIndex; i < newVerticesCount-1 && j < endRecordIndex; i++, j++)
			{
				visibleTrajectory[i] = Vector3.Lerp(positions[j], positions[j + 1], t);
				//msg += "\n j=" + j + " positions[j]=" + positions[j] + " positions[j + 1]=" + positions[j + 1];
			}
			visibleTrajectory[newVerticesCount - 1] = positions[endRecordIndex - 1];

			//Debug.Log(msg);

			lineRenderer.positionCount = newVerticesCount;
			lineRenderer.SetPositions(visibleTrajectory);
		}
	}

	void FixedUpdate()
    {
		if (Score.isGameStarted)
			RecordsManager.PushRecord(characterPosition.position + Vector3.down * 1f, character.rotation);
	}
}
