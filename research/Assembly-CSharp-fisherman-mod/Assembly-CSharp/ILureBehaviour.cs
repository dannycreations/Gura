using System;
using ObjectModel;
using Phy;
using UnityEngine;

public interface ILureBehaviour : ITackleBehaviour
{
	Lure LureItem { get; }

	void SetHookMass(Mass m);

	void SetLureMasses(Mass m);

	Vector3 DirectionVector { get; }

	void OnEnterHangingState();

	void OnExitHangingState();

	void RealignMasses();

	void ResetDrag();

	void SetFreeze(bool freeze);
}
