using System;
using ObjectModel;
using UnityEngine;

public static class Point3Extensions
{
	public static Vector3 ToVector3(this Point3 v)
	{
		return new Vector3(v.X, v.Y, v.Z);
	}

	public static Quaternion ToQuaternion(this Point3 v)
	{
		return Quaternion.Euler(v.X, v.Y, v.Z);
	}
}
