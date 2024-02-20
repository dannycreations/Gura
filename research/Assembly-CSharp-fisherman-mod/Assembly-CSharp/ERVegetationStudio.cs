using System;
using EasyRoads3Dv3;
using UnityEngine;

public class ERVegetationStudio : ScriptableObject
{
	public static bool VegetationStudio()
	{
		return false;
	}

	public static void CreateVegetationMaskLine(GameObject go, float grassPerimeter, float plantPerimeter, float treePerimeter, float objectPerimeter, float largeObjectPerimeter)
	{
	}

	public static void UpdateVegetationMaskLine(GameObject go, ERVSData[] vsData, float grassPerimeter, float plantPerimeter, float treePerimeter, float objectPerimeter, float largeObjectPerimeter)
	{
	}

	public static void UpdateHeightmap(Bounds bounds)
	{
	}

	public static void RemoveVegetationMaskLine(GameObject go)
	{
	}
}
