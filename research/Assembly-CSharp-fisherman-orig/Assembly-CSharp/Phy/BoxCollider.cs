using System;
using UnityEngine;

namespace Phy
{
	public class BoxCollider : ICollider
	{
		public BoxCollider(BoxCollider sourceCollider)
		{
			this.SourceInstanceId = sourceCollider.gameObject.GetInstanceID();
			this.SyncPosition(sourceCollider);
			this.scale = sourceCollider.size * 0.5f;
			this.scale.x = this.scale.x * sourceCollider.transform.lossyScale.x;
			this.scale.y = this.scale.y * sourceCollider.transform.lossyScale.y;
			this.scale.z = this.scale.z * sourceCollider.transform.lossyScale.z;
		}

		public Quaternion rotation
		{
			get
			{
				return this._rotation;
			}
			set
			{
				this._rotation = value;
				this._invRotation = Quaternion.Inverse(value);
			}
		}

		public int SourceInstanceId { get; private set; }

		public void Sync(ICollider source)
		{
			BoxCollider boxCollider = source as BoxCollider;
			if (boxCollider != null)
			{
				this.position = boxCollider.position;
				this.scale = boxCollider.scale;
				this.rotation = boxCollider.rotation;
			}
		}

		public void SyncPosition(BoxCollider sourceCollider)
		{
			Vector3 vector = sourceCollider.center;
			vector.x *= sourceCollider.transform.lossyScale.x;
			vector.y *= sourceCollider.transform.lossyScale.y;
			vector.z *= sourceCollider.transform.lossyScale.z;
			vector = sourceCollider.transform.rotation * vector;
			this.position = sourceCollider.transform.position + vector;
			this.rotation = sourceCollider.transform.rotation;
		}

		public void DebugDraw(Color c)
		{
			Vector3 vector = this.position + this.rotation * new Vector3(-this.scale.x, this.scale.y, -this.scale.z);
			Vector3 vector2 = this.position + this.rotation * new Vector3(-this.scale.x, -this.scale.y, -this.scale.z);
			Vector3 vector3 = this.position + this.rotation * new Vector3(this.scale.x, -this.scale.y, -this.scale.z);
			Vector3 vector4 = this.position + this.rotation * new Vector3(this.scale.x, this.scale.y, -this.scale.z);
			Vector3 vector5 = this.position + this.rotation * new Vector3(-this.scale.x, this.scale.y, this.scale.z);
			Vector3 vector6 = this.position + this.rotation * new Vector3(-this.scale.x, -this.scale.y, this.scale.z);
			Vector3 vector7 = this.position + this.rotation * new Vector3(this.scale.x, -this.scale.y, this.scale.z);
			Vector3 vector8 = this.position + this.rotation * new Vector3(this.scale.x, this.scale.y, this.scale.z);
			Debug.DrawLine(vector, vector2, c);
			Debug.DrawLine(vector2, vector3, c);
			Debug.DrawLine(vector3, vector4, c);
			Debug.DrawLine(vector4, vector, c);
			Debug.DrawLine(vector5, vector6, c);
			Debug.DrawLine(vector6, vector7, c);
			Debug.DrawLine(vector7, vector8, c);
			Debug.DrawLine(vector8, vector5, c);
			Debug.DrawLine(vector, vector5, c);
			Debug.DrawLine(vector2, vector6, c);
			Debug.DrawLine(vector3, vector7, c);
			Debug.DrawLine(vector4, vector8, c);
		}

		public Vector3 TestPoint(Vector3 point, Vector3 prevPoint, out Vector3 normal)
		{
			Vector3 vector = this._invRotation * (point - this.position);
			vector.x /= this.scale.x;
			vector.y /= this.scale.y;
			vector.z /= this.scale.z;
			if (vector.x >= -1f && vector.x <= 1f && vector.y >= -1f && vector.y <= 1f && vector.z >= -1f && vector.z <= 1f)
			{
				int num = 0;
				float num2 = 1f - Mathf.Abs(vector.x);
				float num3 = 1f - Mathf.Abs(vector.y);
				float num4 = 1f - Mathf.Abs(vector.z);
				float num5 = num2;
				if (num3 < num2 && num3 < num4)
				{
					num = 1;
					num5 = num3;
				}
				else if (num4 < num2)
				{
					num = 2;
					num5 = num4;
				}
				Vector3 vector2 = this._invRotation * (prevPoint - this.position);
				vector2.x /= this.scale.x;
				vector2.y /= this.scale.y;
				vector2.z /= this.scale.z;
				Vector3 normalized = (vector - vector2).normalized;
				Vector3 vector3 = this.xyz[num] * num5 * Mathf.Sign(vector[num]);
				normal = vector3.normalized;
				vector3.x *= this.scale.x;
				vector3.y *= this.scale.y;
				vector3.z *= this.scale.z;
				vector3 = this.rotation * vector3;
				return vector3;
			}
			normal = Vector3.zero;
			return Vector3.zero;
		}

		public Vector3 position;

		public Vector3 scale;

		private Quaternion _rotation;

		private Quaternion _invRotation;

		private readonly Vector3[] xyz = new Vector3[]
		{
			Vector3.right,
			Vector3.up,
			Vector3.forward
		};
	}
}
