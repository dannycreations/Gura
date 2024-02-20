using System;
using System.IO;
using UnityEngine;

namespace TPM
{
	public class LineData
	{
		public void UpdateLineData(Vector3[] mainAndLeaderPoints_, Vector3 sinkersFirstPoint_, bool isLeaderVisible_)
		{
			this.isLeaderVisible = isLeaderVisible_;
			for (int i = 0; i < this.mainAndLeaderPoints.Length; i++)
			{
				this.mainAndLeaderPoints[i] = mainAndLeaderPoints_[i];
			}
			this.sinkersFirstPoint = sinkersFirstPoint_;
		}

		public void WriteToStream(Stream stream)
		{
			Serializer.WriteVectors(stream, this.mainAndLeaderPoints);
			Serializer.WriteVector3(stream, this.sinkersFirstPoint);
			Serializer.WriteBool(stream, this.isLeaderVisible);
		}

		public void ReadFromStream(Stream stream)
		{
			Serializer.ReadVectorsArray(stream, this.mainAndLeaderPoints);
			this.sinkersFirstPoint = Serializer.ReadVector3(stream);
			this.isLeaderVisible = Serializer.ReadBool(stream);
		}

		public void ReplaceData(LineData newLine)
		{
			this.UpdateLineData(newLine.mainAndLeaderPoints, newLine.sinkersFirstPoint, newLine.isLeaderVisible);
		}

		public Vector3 sinkersFirstPoint;

		public Vector3[] mainAndLeaderPoints = new Vector3[3];

		public bool isLeaderVisible;
	}
}
