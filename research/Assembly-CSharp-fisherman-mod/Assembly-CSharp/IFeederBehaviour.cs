using System;
using ObjectModel;
using Phy;
using UnityEngine;

public interface IFeederBehaviour : IFloatBehaviour, ITackleBehaviour
{
	RigidBody RigidBody { get; }

	FeederTackleObject FeederObject { get; }

	bool IsBottom { get; }

	bool IsFeederLying { get; }

	Feeding[] InitFeeding();

	void SetFilled(bool isFilled);

	bool IsFilled { get; }

	void UpdateFeeding(Vector3 position);

	void DestroyFeeding();
}
