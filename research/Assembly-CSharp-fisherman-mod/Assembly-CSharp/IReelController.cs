using System;
using UnityEngine;

public interface IReelController
{
	GameObject gameObject { get; }

	float CurrentFriction { get; }

	float CurrentFrictionForce { get; }

	float LineAdjustedFrictionForce { get; }

	int FrictionSectionsCount { get; }

	float CurrentRelativeSpeed { get; }

	float CurrentSpeed { get; }

	void IncrementReelSpeed();

	void DecrementReelSpeed();

	float CurrentForce { get; }

	bool IsReeling { get; }

	void BlockLineLengthChange(float duration);

	int CurrentReelSpeedSection { get; }

	int CurrentFrictionSection { get; }

	float MaxLoad { get; }

	float AppliedForce { get; }

	bool IsOverloaded { get; }

	float IndicatorForce { get; }

	RullerTackleType ForceRuller { get; }

	bool IsIndicatorOn { get; set; }

	bool IsFrictioning { get; }

	bool IsForced { get; }

	float LineDrainSpeed { get; }

	bool IncreaseLineLength();

	float UpdateLineLengthOnReeling();

	void UpdateFrictionState(float relativeForce = 0f);

	bool HasMaxSpeed { get; }

	void SetFightMode();

	void SetHighSpeedMode();

	void SetNormalSpeedMode();

	bool IsFightingMode { get; }

	void ResetAppledForce();
}
