using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public interface IRodController
{
	Vector3 CurrentTipPosition { get; }

	Vector3 CurrentUnbendTipPosition { get; }

	List<Transform> LineLocatorsProp { get; }

	Transform RodTipLocator { get; }

	List<Transform> FirstRingLocatorsProp { get; }

	AssembledRod AssembledRod { get; }

	void ReInitilizeSimulation(RodOnPodBehaviour.TransitionData transitionData);

	float MaxCastLength { get; }

	float AdjustedAppliedForce { get; }

	float RodTipMoveSpeed { get; }

	float TipMoveSpeedFromTackle { get; }

	float ReelDamper { get; }

	float AppliedForce { get; }

	bool IsOverloaded { get; }

	BendingSegment Segment { get; }

	bool IsUnEquiped { get; set; }

	Transform transform { get; }

	float Length { get; }

	float MaxLoad { get; }

	float ModelLength { get; }

	ObscuredFloat LineOnRodLength { get; }

	bool IsForced { get; }

	bool IsFishingForced { get; }

	void ResetAppledForce();

	void TriggerHapticPulseOnRod(float motor, float duration);
}
