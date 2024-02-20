using System;
using UnityEngine;

public interface IWaterFlowController
{
	Vector3 GetStreamSpeed(Vector3 position);

	FishWaterBase WaterBaseInstance { get; }
}
