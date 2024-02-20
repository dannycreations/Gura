using System;
using UnityEngine;

public interface IManualUpdate
{
	bool ManualUpdate();

	bool IsCloseEnough { get; }

	bool Choosen { get; set; }

	ushort ServerId { get; }

	GameObject GameObject { get; }

	bool IsPossibleToActivate { get; }
}
