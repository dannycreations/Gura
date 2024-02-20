using System;
using ObjectModel;
using UnityEngine;

public interface ITackleController
{
	Transform transform { get; }

	GameObject gameObject { get; }

	Transform HookAnchor { get; }

	Transform TopLineAnchor { get; }

	Transform BottomLineAnchor { get; }

	Transform WaterMark { get; }

	float Sensitivity { get; }

	float Windage { get; }

	float LineStrain { get; }

	bool IsOnTip { get; }

	bool IsFlying { get; }

	bool IsInWater { get; }

	bool IsHanging { get; }

	bool IsShowing { get; set; }

	bool IsBitten { get; }

	bool IsFishHooked { get; }

	bool IsHitched { get; set; }

	bool IsBroken { get; set; }

	bool IsFinishAttackRequested { get; set; }

	bool IsAttackFinished { get; set; }

	bool IsLying { get; }

	bool IsMoving { get; }

	bool IsKinematic { get; set; }

	void OnFsmUpdate();

	void OnLateUpdate();

	IFishController AttackingFish { get; set; }

	IFishController Fish { get; set; }

	RodBehaviour Rod { get; set; }

	bool IsActive { get; }

	HookController Hook { get; set; }

	TackleThrowData ThrowData { get; }

	bool Armed { get; set; }

	void ThrowTackle(Vector3 direction);

	float LeaderLength { get; set; }

	float UserSetLeaderLength { get; set; }

	bool HasHitTheGround { get; }

	bool IsPitchTooShort { get; set; }

	float TackleMass { get; }

	void DisturbWater(Vector3 position, float radius, WaterDisturbForce force);

	DragStyle DragStyle { get; }

	float DragQuality { get; }

	bool IsIdle { get; }

	bool IsBeingPulled { get; }

	void ResetDrag();

	void FinishDragPeriod();

	void CreateTackle();

	void Hitch(Vector3 position);

	void UnHitch();

	void UpdateLureDepthStatus();

	void EscapeFish();

	void Destroy();

	bool IsOnTipComplete { get; }

	bool IsShowingComplete { get; }
}
