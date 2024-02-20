using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phy
{
	public interface ISimulation
	{
		List<Mass> Masses { get; }

		List<ConnectionBase> Connections { get; }

		void Update(float dt);

		void SetVelocity(Vector3 velocity);

		void SetAngularVelocity(Vector3 velocity);

		void SyncMasses();
	}
}
