using System;
using UnityEngine;

public static class Packer
{
	public static float ToFloat(float x, float y, float z, float w)
	{
		return (float)((Mathf.FloorToInt(w * 63f) << 18) + (Mathf.FloorToInt(z * 63f) << 12) + (Mathf.FloorToInt(y * 63f) << 6) + Mathf.FloorToInt(x * 63f));
	}

	public static float ToFloat(Vector4 factor)
	{
		return Packer.ToFloat(Mathf.Clamp01(factor.x), Mathf.Clamp01(factor.y), Mathf.Clamp01(factor.z), Mathf.Clamp01(factor.w));
	}

	public static float ToFloat(float x, float y, float z)
	{
		return (float)((Mathf.FloorToInt(z * 255f) << 16) + (Mathf.FloorToInt(y * 255f) << 8) + Mathf.FloorToInt(x * 255f));
	}

	public static float ToFloat(float x, float y)
	{
		return (float)((Mathf.FloorToInt(y * 4095f) << 12) + Mathf.FloorToInt(x * 4095f));
	}
}
