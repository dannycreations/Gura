using System;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class CapsuleCollider : ICollider
	{
		public CapsuleCollider(CapsuleCollider sourceCollider)
		{
			this.SourceInstanceId = sourceCollider.gameObject.GetInstanceID();
			this.length = (sourceCollider.height - 2f * this.radius) * sourceCollider.transform.lossyScale[sourceCollider.direction];
			this.radius = sourceCollider.radius * Mathf.Max(sourceCollider.transform.lossyScale[(sourceCollider.direction + 1) % 3], sourceCollider.transform.lossyScale[(sourceCollider.direction + 2) % 3]);
			this.axis = sourceCollider.transform.rotation * (new Vector3[]
			{
				Vector3.right,
				Vector3.up,
				Vector3.forward
			})[sourceCollider.direction];
			this.SyncPosition(sourceCollider);
		}

		public int SourceInstanceId { get; private set; }

		public void Sync(ICollider source)
		{
			CapsuleCollider capsuleCollider = source as CapsuleCollider;
			this.positionA = capsuleCollider.positionA;
			this.positionB = capsuleCollider.positionB;
			this.axis = capsuleCollider.axis;
			this.length = capsuleCollider.length;
			this.radius = capsuleCollider.radius;
			this.SourceInstanceId = capsuleCollider.SourceInstanceId;
		}

		public void SyncPosition(CapsuleCollider sourceCollider)
		{
			Vector3 vector = sourceCollider.center;
			vector.x *= sourceCollider.transform.lossyScale.x;
			vector.y *= sourceCollider.transform.lossyScale.y;
			vector.z *= sourceCollider.transform.lossyScale.z;
			vector = sourceCollider.transform.rotation * vector;
			this.positionA = sourceCollider.transform.position - this.axis * this.length * 0.5f + vector;
			this.positionB = sourceCollider.transform.position + this.axis * this.length * 0.5f + vector;
		}

		public Vector3 TestPoint(Vector3 point, Vector3 prevPoint, out Vector3 normal)
		{
			Vector3 vector = point - this.positionA;
			float num = Vector3.Dot(vector, this.axis);
			if (num > 0f && num < this.length)
			{
				Vector3 vector2 = vector - this.axis * num;
				float magnitude = vector2.magnitude;
				if (magnitude < this.radius)
				{
					if (!Mathf.Approximately(magnitude, 0f))
					{
						normal = vector2 / magnitude;
						return this.positionA + this.axis * num + this.radius * normal - point;
					}
					normal = Vector3.Cross(this.axis, Vector3.up);
					if (Vector3Extension.Approximately(normal, Vector3.zero))
					{
						normal = Vector3.Cross(this.axis, Vector3.right);
					}
					normal = normal.normalized;
					return this.positionA + this.axis * num + this.radius * normal - point;
				}
			}
			else if (num <= 0f && num > -this.radius)
			{
				float magnitude2 = vector.magnitude;
				if (magnitude2 < this.radius)
				{
					if (magnitude2 > 0f)
					{
						normal = vector / magnitude2;
						return this.positionA + normal * this.radius - point;
					}
					normal = -this.axis;
					return this.positionA + normal * this.radius - point;
				}
			}
			else if (num >= this.length && num < this.length + this.radius)
			{
				Vector3 vector3 = point - this.positionB;
				float magnitude3 = vector3.magnitude;
				if (magnitude3 < this.radius)
				{
					if (magnitude3 > 0f)
					{
						normal = vector3 / magnitude3;
						return this.positionB + normal * this.radius - point;
					}
					normal = this.axis;
					return this.positionA + normal * this.radius - point;
				}
			}
			normal = Vector3.zero;
			return Vector3.zero;
		}

		public void DebugDraw(Color c)
		{
		}

		protected Vector3 positionA;

		protected Vector3 positionB;

		protected Vector3 axis;

		protected float length;

		protected float radius;
	}
}
