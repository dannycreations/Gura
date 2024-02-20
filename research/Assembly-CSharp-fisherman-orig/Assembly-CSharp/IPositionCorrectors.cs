using System;
using UnityEngine;

public interface IPositionCorrectors
{
	Vector3 ModelGroundCorrection { get; }

	Vector3 DebugModelUpMovement { get; }

	Vector3 DebugHandsUpMovement { get; }

	float DebugForwardDist { get; }

	Vector3 Forward { get; }

	Vector3 PhotoModeFloatingLineDisplacement { get; }

	Vector3 PhotoModeSpinningLineDisplacement { get; }

	Vector3 PhotoModeCastingLineDisplacement { get; }
}
