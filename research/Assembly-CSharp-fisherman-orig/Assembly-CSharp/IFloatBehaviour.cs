using System;
using ObjectModel;
using Phy;
using UnityEngine;

public interface IFloatBehaviour : ITackleBehaviour
{
	float UserSetLeaderLength { get; set; }

	float LeaderLength { get; set; }

	bool CheckPitchIsTooShort();

	void HookSetFreeze(bool freeze);

	Mass GetHookMass(int index);

	Mass GetBobberMainMass { get; }

	Mass GetKinematicLeaderMass(int index = 0);

	void SetHookMass(int index, Mass m);

	bool HookIsIdle { get; }

	Transform HookTransform { get; }

	Transform BaitTransform { get; }

	Bait BaitItem { get; }

	bool IsStrikeTimedOut { get; set; }

	float StrikeTimeEnd { get; set; }
}
