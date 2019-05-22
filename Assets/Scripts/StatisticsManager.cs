﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrajectoryStatistics
{
	public int recordsCount;
	public Vector3[] positions;
	public Quaternion[] rotations;
}

/*  int trajectory records count
	Vector3[trajectory records count] positions
	Quaternion[trajectory records count] rotations
	int planes records count
	EnemyMoveInfo[planes records count] planesMovement
	int zombies records count
	EnemyMoveInfo[zombies records count] zombiesMovement
	int cars records count
	EnemyMoveInfo[cars records count] carsMovement
	int collision records count
	СollisionInfo[collision records count] collisionRecords
 */

public class StatisticsManager
{
	private class СollisionInfo
	{
		public Vector3 pos;
		public float time;

		public СollisionInfo(Vector3 pos, float time)
		{
			this.pos = pos;
			this.time = time;
		}
	}

	public class EnemyMoveInfo
	{
		public Vector3 startPos, endPos;
		public float startTime, endTime;

		public EnemyMoveInfo(Vector3 startPos, float startTime, Vector3 endPos, float endTime)
		{
			this.startPos = startPos; this.startTime = startTime;
			this.endPos = endPos; this.endTime = endTime;
		}
	}

	private static readonly string path = "/data/Statistics";
	private static readonly Queue<EnemyMoveInfo> planesInfo;
	private static readonly Queue<EnemyMoveInfo> zombiesInfo;
	private static readonly Queue<EnemyMoveInfo> carsInfo;
	private static readonly Queue<СollisionInfo> collisionRecords;

	static StatisticsManager()
	{
		planesInfo = new Queue<EnemyMoveInfo>(200);
		zombiesInfo = new Queue<EnemyMoveInfo>(200);
		carsInfo = new Queue<EnemyMoveInfo>(200);
		collisionRecords = new Queue<СollisionInfo>(100);
	}

	public static void Write(TrajectoryStatistics trajectory)
	{
		Debug.Log(Application.persistentDataPath);
		
		int uniqueFileId = PlayerPrefs.GetInt("uniqueStatisticsId", -1) + 1;
		PlayerPrefs.SetInt("uniqueStatisticsId", uniqueFileId);

		if (!Directory.Exists(Application.persistentDataPath + "/data"))
		{
			Directory.CreateDirectory(Application.persistentDataPath + "/data");
		}

		using (BinaryWriter writer = new BinaryWriter(
			File.Open(Application.persistentDataPath + path + uniqueFileId.ToString(), FileMode.Create)))
		{
			writer.Write(trajectory.recordsCount);
			for (int i = 0; i < trajectory.recordsCount; i++)
				WriteVector3(writer, trajectory.positions[i]);
			for (int i = 0; i < trajectory.recordsCount; i++)
				WriteQuaternion(writer, trajectory.rotations[i]);

			Debug.Log("planesInfo.Count " + planesInfo.Count);
			writer.Write(planesInfo.Count);
			while (planesInfo.Count > 0)
				WriteEnemyMoveInfo(writer, planesInfo.Dequeue());

			Debug.Log("zombiesInfo.Count " + zombiesInfo.Count);
			writer.Write(zombiesInfo.Count);
			while (zombiesInfo.Count > 0)
				WriteEnemyMoveInfo(writer, zombiesInfo.Dequeue());

			Debug.Log("carsInfo.Count " + carsInfo.Count);
			writer.Write(carsInfo.Count);
			while (carsInfo.Count > 0)
				WriteEnemyMoveInfo(writer, carsInfo.Dequeue());

			writer.Write(collisionRecords.Count);
			while (collisionRecords.Count > 0)
			{
				var info = collisionRecords.Dequeue();
				WriteVector3(writer, info.pos);
				writer.Write(info.time);
			}
		}
	}

	public static int Read(Vector3[] data)
	{
		int uniqueFileId = PlayerPrefs.GetInt("uniqueStatisticsId", 0);
		string fileName = Application.persistentDataPath + path + uniqueFileId.ToString();
		int recordsCount = 0;
		//string msg = "Read Trajectory\n";
		if (File.Exists(fileName))
		{
			using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
			{
				recordsCount = reader.ReadInt32();
				for (int i = 0; i < recordsCount; i++)
				{
					data[i] = ReadVector3(reader);
					//msg += "i=" + i + " pos=" + data[i] + "\n";
				}
			}
		}
		//Debug.Log(msg);
		return recordsCount;
	}

	private static void WriteVector3(BinaryWriter writer, Vector3 v)
	{
		writer.Write(v.x);
		writer.Write(v.y);
		writer.Write(v.z);
	}

	private static void WriteQuaternion(BinaryWriter writer, Quaternion q)
	{
		writer.Write(q.x);
		writer.Write(q.y);
		writer.Write(q.z);
		writer.Write(q.w);
	}

	private static void WriteEnemyMoveInfo(BinaryWriter writer, EnemyMoveInfo moveInfo)
	{
		WriteVector3(writer, moveInfo.startPos);
		writer.Write(moveInfo.startTime);
		WriteVector3(writer, moveInfo.endPos);
		writer.Write(moveInfo.endTime);
	}

	private static Vector3 ReadVector3(BinaryReader reader)
	{
		return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
	}

	public static void PushPlaneInfo(EnemyMoveInfo plane)
	{
		//Debug.Log("PushPlaneInfo\nstart time " + plane.startTime + " end time " + plane.endTime);
		planesInfo.Enqueue(plane);
	}

	public static void PushZombieInfo(EnemyMoveInfo zombie)
	{
		//Debug.Log("PushZombieInfo\nstart time " + zombie.startTime + " end time " + zombie.endTime);
		zombiesInfo.Enqueue(zombie);
	}

	public static void PushCarInfo(EnemyMoveInfo car)
	{
		//Vector3 camPos = Camera.main.transform.position;
		//Debug.Log("\nPushCarInfo\n start time " + car.startTime + " end time " + car.endTime
		//	+ "\n start pos " + car.startPos + " end pos " + car.endPos + "\n camera " + camPos
		//	+ "\n dist 1 " + Vector3.Distance(car.startPos, camPos) + " dist 2 " + Vector3.Distance(car.startPos, camPos));
		carsInfo.Enqueue(car);
	}

	public static void PushСollisionInfo(Vector3 charPos, float time)
	{
		collisionRecords.Enqueue(new СollisionInfo(charPos, time));
	}
}
