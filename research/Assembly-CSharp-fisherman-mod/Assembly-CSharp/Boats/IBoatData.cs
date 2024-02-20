using System;
using UnityEngine;

namespace Boats
{
	public interface IBoatData
	{
		ushort FactoryID { get; }

		Vector3 Position { get; }

		Quaternion Rotation { get; }
	}
}
