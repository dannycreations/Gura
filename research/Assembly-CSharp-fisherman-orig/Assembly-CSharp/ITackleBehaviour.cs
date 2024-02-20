using System;
using ObjectModel;
using Phy;
using UnityEngine;

public interface ITackleBehaviour
{
	Transform transform { get; }

	Transform HookAnchor { get; }

	Transform TopLineAnchor { get; }

	GameObject gameObject { get; }

	RodBehaviour Rod { get; }

	int SlotId { get; }

	Fish FishTemplate { get; }

	IFishController Fish { get; set; }

	IFishController AttackingFish { get; set; }

	TackleThrowData ThrowData { get; }

	GameActionAdapter Adapter { get; }

	UnderwaterItemController UnderwaterItem { get; }

	Mass KinematicHandLineMass { get; set; }

	GameFactory.RodSlot RodSlot { get; }

	bool IsAttackFinished { get; }

	bool IsFinishAttackRequested { get; }

	bool IsOnTipComplete { get; set; }

	bool Thrown { get; set; }

	bool IsLying { get; }

	bool IsInWater { get; }

	bool IsOnTip { get; }

	bool IsShowingComplete { get; set; }

	bool HasHitTheGround { get; set; }

	bool IsOutOfTerrain { get; }

	bool IsPitchTooShort { get; }

	bool IsHitched { get; set; }

	bool IsKinematic { get; set; }

	bool HasUnderwaterItem { get; }

	bool UnderwaterItemIsLoading { get; }

	bool ItemIsShowing { get; set; }

	bool IsPulledOut { get; }

	bool IsLiveBait { get; }

	bool IsBaitShown { get; }

	bool IsShowing { get; set; }

	bool IsBeingPulled { get; }

	string UnderwaterItemName { get; }

	string UnderwaterItemCategory { get; }

	int UnderwaterItemId { get; }

	bool FishIsLoading { get; }

	bool IsThrowing { get; set; }

	float Size { get; }

	float Windage { get; }

	bool IsClipStrike { get; set; }

	void SetActive(bool flag);

	void EscapeFish();

	void CheckSurfaceCollisions();

	void TackleOut(float size = 1f);

	void TackleIn(float size = 3f);

	bool CheckGroundHit();

	void Hitch(Vector3 position);

	void UnHitch();

	void UpdateLureDepthStatus();
}
