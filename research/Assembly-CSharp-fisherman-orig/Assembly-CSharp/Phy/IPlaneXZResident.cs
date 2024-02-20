using System;

namespace Phy
{
	public interface IPlaneXZResident
	{
		bool TestAABB(float minX, float maxX, float minZ, float maxZ);

		bool ProcreateFactor(float minX, float maxX, float minZ, float maxZ);

		float DistanceToPoint(float px, float pz);

		int ID { get; }
	}
}
