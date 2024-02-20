using System;
using CodeStage.AntiCheat.ObscuredTypes;
using Phy;
using UnityEngine;

public interface ILineController
{
	void OnLateUpdate();

	void SetActive(bool flag);

	GameObject gameObject { get; }

	GameObject mainLineObject { get; }

	GameObject rodLineObject { get; }

	GameObject leaderLineObject { get; }

	float LineLength { get; }

	ObscuredFloat SecuredLineLength { get; }

	float FullLineLength { get; }

	void GradualFinalLineLengthChange(float length, int steps);

	float MinLineLength { get; }

	float MinLineLengthWithFish { get; set; }

	float MinLineLengthOnPitch { get; set; }

	ObscuredFloat LineLengthOnSpool { get; set; }

	ObscuredFloat MaxLineLength { get; set; }

	float AvailableLineLengthOnSpool { get; }

	float GetLinePhysicsLength();

	float GetLineAirStrain();

	Vector3[] GetMainLineSpline(int resampledCount);

	float MaxLoad { get; }

	float Thickness { get; }

	void CreateLine(Vector3 lineTipPosition, float extraLength);

	float AppliedForce { get; }

	bool IsOverloaded { get; }

	void RefreshObjectsFromSim();

	SpringDrivenObject LineObj { get; }

	SpringDrivenObject LeaderObj { get; }

	void ResetToMinLength();

	Mass GetKinematicMass();

	Spring GetKinematicMassSpring();

	void ReleaseKinematicMass(Mass kinematicLineMass);

	void SetLineSpringConstant(float springConstant);

	float SinkerMass { get; set; }

	bool IsTensioned { get; }

	bool IsSlacked { get; }

	void ResetLineWidthChange(float newTargetWidth);

	void TransitToNewLineWidth();

	void ResetAppledForce();

	void SetVisibility(bool flag);

	bool IsBraid { get; }
}
