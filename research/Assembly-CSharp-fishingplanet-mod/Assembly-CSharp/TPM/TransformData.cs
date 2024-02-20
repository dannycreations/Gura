using System;
using System.IO;
using UnityEngine;

namespace TPM
{
	public struct TransformData
	{
		public void WriteToStream(Stream stream)
		{
			Serializer.WriteQuaternion(stream, this.rotation);
			Serializer.WriteVector3(stream, this.position);
		}

		public void ReadFromStream(Stream stream)
		{
			this.rotation = Serializer.ReadQuaternion(stream);
			this.position = Serializer.ReadVector3(stream);
		}

		public Quaternion rotation;

		public Vector3 position;
	}
}
