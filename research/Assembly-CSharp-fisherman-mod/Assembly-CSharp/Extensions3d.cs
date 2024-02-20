using System;
using CodeStage.AntiCheat.ObscuredTypes;
using ObjectModel;
using UnityEngine;

public static class Extensions3d
{
	public static Point3 ToPoint3(this Vector3 v)
	{
		return new Point3(v.x, v.y, v.z);
	}

	public static Point3 ToPoint3(this Quaternion q)
	{
		Vector3 eulerAngles = q.eulerAngles;
		return new Point3(eulerAngles.x, eulerAngles.y, eulerAngles.z);
	}

	public static Point3 ToPoint3(this ObscuredVector3 v)
	{
		return new Point3(v.x, v.y, v.z);
	}
}
