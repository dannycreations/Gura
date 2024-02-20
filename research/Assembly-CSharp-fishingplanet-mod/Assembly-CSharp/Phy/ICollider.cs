using System;
using UnityEngine;

namespace Phy
{
	public interface ICollider
	{
		int SourceInstanceId { get; }

		void Sync(ICollider source);

		Vector3 TestPoint(Vector3 point, Vector3 prevPoint, out Vector3 normal);

		void DebugDraw(Color c);
	}
}
