using System;
using ObjectModel;
using Phy;
using UnityEngine;

public interface IFishController
{
	FishController Owner { get; }

	int SlotId { get; }

	GameFactory.RodSlot RodSlot { get; }

	Vector3 MouthPosition { get; }

	Vector3 ThroatPosition { get; }

	float DistanceToTackle { get; }

	Mass HeadMass { get; }

	Mass TailMass { get; }

	PhyObject FishObject { get; }

	Vector3 HeadRight { get; }

	TackleBehaviour Tackle { get; set; }

	bool IsPassive { get; }

	bool IsBig { get; }

	bool IsGoingTo { get; }

	bool IsShowing { get; }

	Fish CaughtFish { get; set; }

	Fish FishTemplate { get; }

	FishBehavior Behavior { get; set; }

	FishAiBehavior FishAIBehaviour { get; }

	bool IsTasting { get; }

	float CurrentRelativeForce { get; set; }

	Type State { get; }

	FsmBaseState<IFishController> StateInstance { get; }

	Guid InstanceGuid { get; set; }

	int tpmId { get; set; }

	bool IsHandsHoldCondition { get; }

	float PredatorAttackDelay { get; }

	float PredatorHoldDelay { get; }

	float BiteTime { get; }

	float CurrentForce { get; }

	float AttackLure { get; }

	bool FishAiIsBiteTemplate { get; }

	float FishAiMaxBiteTime { get; }

	bool FishAiIsBiting { get; }

	bool FishAiEndOfBite { get; }

	bool IsPathCompleted { get; }

	bool IsAttackDelayed { get; }

	bool IsFrozen { get; set; }

	void SetOnPod(bool onPod);

	void Show();

	void SetVisibility(bool flag);

	void Escape();

	void Swim();

	void PredatorSwim();

	void PredatorAttack();

	void Bite();

	void Attack();

	void KeepHookInMouth(float poolPeriod);

	void Hook(float poolPeriod);

	void Destroy();

	void SyncWithSim();
}
