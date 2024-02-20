using System;
using Ovr;
using UnityEngine;

public static class OVRExtensions
{
	public static Matrix4x4 ToMatrix4x4(this Matrix4f ovrMat)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x[0, 0] = ovrMat.m[0, 0];
		matrix4x[0, 1] = ovrMat.m[0, 1];
		matrix4x[0, 2] = ovrMat.m[0, 2];
		matrix4x[0, 3] = ovrMat.m[0, 3];
		matrix4x[1, 0] = ovrMat.m[1, 0];
		matrix4x[1, 1] = ovrMat.m[1, 1];
		matrix4x[1, 2] = ovrMat.m[1, 2];
		matrix4x[1, 3] = ovrMat.m[1, 3];
		matrix4x[2, 0] = ovrMat.m[2, 0];
		matrix4x[2, 1] = ovrMat.m[2, 1];
		matrix4x[2, 2] = ovrMat.m[2, 2];
		matrix4x[2, 3] = ovrMat.m[2, 3];
		matrix4x[3, 0] = ovrMat.m[3, 0];
		matrix4x[3, 1] = ovrMat.m[3, 1];
		matrix4x[3, 2] = ovrMat.m[3, 2];
		matrix4x[3, 3] = ovrMat.m[3, 3];
		return matrix4x;
	}

	public static Vector2 ToVector2(this Sizei size)
	{
		return new Vector2((float)size.w, (float)size.h);
	}

	public static Vector2 ToVector2(this global::Vector2i vec)
	{
		return new Vector2((float)vec.x, (float)vec.y);
	}

	public static Vector2 ToVector2(this Vector2f vec)
	{
		return new Vector2(vec.x, vec.y);
	}

	public static Vector3 ToVector3(this Vector3f vec, bool rhToLh = true)
	{
		Vector3 vector;
		vector..ctor(vec.x, vec.y, vec.z);
		if (rhToLh)
		{
			vector.z = -vector.z;
		}
		return vector;
	}

	public static Quaternion ToQuaternion(this Quatf quat, bool rhToLh = true)
	{
		Quaternion quaternion;
		quaternion..ctor(quat.x, quat.y, quat.z, quat.w);
		if (rhToLh)
		{
			quaternion.x = -quaternion.x;
			quaternion.y = -quaternion.y;
		}
		return quaternion;
	}

	public static OVRPose ToPose(this Posef pose, bool rhToLh = true)
	{
		return new OVRPose
		{
			position = pose.Position.ToVector3(rhToLh),
			orientation = pose.Orientation.ToQuaternion(rhToLh)
		};
	}
}
