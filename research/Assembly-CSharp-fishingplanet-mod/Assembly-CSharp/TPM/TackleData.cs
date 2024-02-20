using System;
using System.IO;
using UnityEngine;

namespace TPM
{
	public class TackleData
	{
		public void WriteToStream(Stream stream)
		{
			Serializer.WriteQuaternion(stream, this.rotation);
			Serializer.WriteQuaternion(stream, this.hookRotation);
			Serializer.WriteQuaternion(stream, this.sinkerRotation);
			if (this.throwTargetPosition != null && this.throwStartAngle != null)
			{
				Serializer.WriteBool(stream, true);
				Serializer.WriteVector3(stream, this.throwTargetPosition.Value);
				Serializer.WriteFloat(stream, this.throwStartAngle.Value);
			}
			else
			{
				Serializer.WriteBool(stream, false);
			}
		}

		public void ReadFromStream(Stream stream)
		{
			this.rotation = Serializer.ReadQuaternion(stream);
			this.hookRotation = Serializer.ReadQuaternion(stream);
			this.sinkerRotation = Serializer.ReadQuaternion(stream);
			if (Serializer.ReadBool(stream))
			{
				this.throwTargetPosition = new Vector3?(Serializer.ReadVector3(stream));
				this.throwStartAngle = new float?(Serializer.ReadFloat(stream));
			}
			else
			{
				this.throwTargetPosition = null;
				this.throwStartAngle = null;
			}
		}

		public void ReplaceData(TackleData src)
		{
			this.rotation = src.rotation;
			this.hookRotation = src.hookRotation;
			this.sinkerRotation = src.sinkerRotation;
			this.throwTargetPosition = src.throwTargetPosition;
			this.throwStartAngle = src.throwStartAngle;
		}

		public Quaternion rotation;

		public Quaternion hookRotation;

		public Quaternion sinkerRotation;

		public float? throwStartAngle;

		public Vector3? throwTargetPosition;
	}
}
