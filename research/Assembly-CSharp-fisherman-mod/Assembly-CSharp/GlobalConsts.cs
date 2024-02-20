using System;
using UnityEngine;

public static class GlobalConsts
{
	public static string[] PossibleTextures
	{
		get
		{
			return GlobalConsts._possibleTextures;
		}
	}

	public static bool IsDebugLoading
	{
		get
		{
			return false;
		}
	}

	public const float BOARDING_DIST = 7.5f;

	public const float UNBOARDING_CAST_DIST = 5f;

	public const float UNBOARDING_MOVEMENT_DIST = 3f;

	public const float UNBOARDING_MIN_SPACE = 1.2f;

	public static int BoatAZLayer = LayerMask.NameToLayer("BoatActionZone");

	public static LayerMask BoatAZMask = 1 << GlobalConsts.BoatAZLayer;

	public static LayerMask DefaultMask = int.MaxValue;

	public static LayerMask FishMask = GlobalConsts.DefaultMask & ~((1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Unimportant")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Walls")) | (1 << LayerMask.NameToLayer("BoatWalls")) | (1 << LayerMask.NameToLayer("Boat")) | (1 << LayerMask.NameToLayer("RodOnPod")) | (1 << LayerMask.NameToLayer("RodPod")) | (1 << LayerMask.NameToLayer("Tackle")) | (1 << LayerMask.NameToLayer("SimplifiedTerrain")) | GlobalConsts.BoatAZMask);

	public static LayerMask BoatMask = GlobalConsts.DefaultMask & ~((1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Unimportant")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Walls")) | (1 << LayerMask.NameToLayer("SimplifiedTerrain")) | GlobalConsts.BoatAZMask);

	public static LayerMask RigidBodyMask = GlobalConsts.DefaultMask & ~((1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Unimportant")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Walls")) | (1 << LayerMask.NameToLayer("BoatWalls")) | (1 << LayerMask.NameToLayer("SimplifiedTerrain")));

	public static int BoatWallsLayer = LayerMask.NameToLayer("BoatWalls");

	public static LayerMask WallsMask = 1 << LayerMask.NameToLayer("Walls");

	public static LayerMask RodOnPodMask = 1 << LayerMask.NameToLayer("RodOnPod");

	public static LayerMask RodPodMask = 1 << LayerMask.NameToLayer("RodPod");

	public static LayerMask ObstaclesExceptTerrainMask = GlobalConsts.DefaultMask & ~((1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Unimportant")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Walls")));

	public static LayerMask TerrainMask = 1 << LayerMask.NameToLayer("Terrain");

	public static int TerrainLayer = LayerMask.NameToLayer("Terrain");

	public static LayerMask PhyCollidersMask = GlobalConsts.FishMask & ~GlobalConsts.TerrainMask;

	public static LayerMask GroundObstacleMask = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("Default"));

	public static LayerMask WaterMask = 1 << LayerMask.NameToLayer("Water");

	public static int PhotoModeLayer = LayerMask.NameToLayer("Photomode");

	public static int UnimportantLayer = LayerMask.NameToLayer("Unimportant");

	public static LayerMask InteractiveObjectsMask = 1 << LayerMask.NameToLayer("InteractiveObjects");

	public static float BobberScale = 1f;

	public static Matrix4x4 Camera2World = Matrix4x4.identity;

	public static bool isScreenRendered = false;

	public static bool LensWetnessRainActive = false;

	public static bool LensWetnessFishActive = false;

	public static float BgVolume = 1f;

	public static bool InGameVolume = false;

	public const bool USE_MESH_BAKER_EVERYWHERE = true;

	private static readonly string[] _possibleTextures = new string[] { "_MainTex", "_BumpMap", "_SpecTex" };

	public static int TransferIn = 2;

	public const int CharacterLimit = 32;
}
