using System;
using UnityEngine;

public interface IWaterController
{
	FishWaterTile FishWaterTileInstance { get; }

	FishWaterBase FishWaterBaseInstance { get; }

	void AddWaterDisturb(Vector3 position, float radius, WaterDisturbForce force);

	void AddWaterDisturb(Vector3 position, float radius, float force);

	QueuePool<WaterDisturb> WaterStates { get; }

	void SetDynWaterPosition(float x, float z);

	Vector3 PlayerPosition { get; set; }

	Vector3 PlayerForward { get; set; }

	FishDisturbanceFreq FishDisturbanceFreq { get; set; }

	void ResetColors(float time);

	void SetColor(Color baseCol, Color abyssCol, float time);
}
