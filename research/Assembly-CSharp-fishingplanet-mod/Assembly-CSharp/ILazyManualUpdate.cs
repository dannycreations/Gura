using System;
using UnityEngine;

public interface ILazyManualUpdate
{
	void ManualUpdate();

	void Init(Transform target);
}
