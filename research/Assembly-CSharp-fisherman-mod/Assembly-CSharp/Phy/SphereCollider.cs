using System;
using UnityEngine;

namespace Phy
{
	public class SphereCollider : ICollider
	{
		public SphereCollider(SphereCollider sourceCollider)
		{
			this.SourceInstanceId = sourceCollider.gameObject.GetInstanceID();
			this.radius = sourceCollider.radius * sourceCollider.transform.lossyScale.x;
			this.SyncPosition(sourceCollider);
		}

		public int SourceInstanceId { get; private set; }

		public void Sync(ICollider source)
		{
			SphereCollider sphereCollider = source as SphereCollider;
			this.position = sphereCollider.position;
			this.radius = sphereCollider.radius;
			this.SourceInstanceId = sphereCollider.SourceInstanceId;
		}

		public void SyncPosition(SphereCollider sourceCollider)
		{
			Vector3 vector = sourceCollider.center;
			vector.x *= sourceCollider.transform.lossyScale.x;
			vector.y *= sourceCollider.transform.lossyScale.y;
			vector.z *= sourceCollider.transform.lossyScale.z;
			vector = sourceCollider.transform.rotation * vector;
			this.position = sourceCollider.transform.position + vector;
		}

		public Vector3 TestPoint(Vector3 point, Vector3 prevPoint, out Vector3 normal)
		{
			Vector3 vector = point - this.position;
			float magnitude = vector.magnitude;
			if (magnitude < this.radius)
			{
				if (!Mathf.Approximately(magnitude, 0f))
				{
					normal = vector / magnitude;
				}
				else
				{
					normal = Vector3.up;
				}
				return normal * (this.radius - magnitude);
			}
			normal = Vector3.zero;
			return Vector3.zero;
		}

		public void DebugDraw(Color c)
		{
		}

		public Vector3 position;

		public float radius;
	}
}
