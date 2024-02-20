using System;
using UnityEngine;

namespace Phy
{
	public class PointXZResident : IPlaneXZResident
	{
		public bool TestAABB(float minX, float maxX, float minZ, float maxZ)
		{
			return Math3d.PointOverlapAABBXZ(this.position, minX, maxX, minZ, maxZ);
		}

		public bool ProcreateFactor(float minX, float maxX, float minZ, float maxZ)
		{
			return true;
		}

		public float DistanceToPoint(float px, float pz)
		{
			return MathHelper.Hypot(px - this.position.x, pz - this.position.z);
		}

		public int ID
		{
			get
			{
				return 0;
			}
		}

		public Vector3 position;
	}
}
