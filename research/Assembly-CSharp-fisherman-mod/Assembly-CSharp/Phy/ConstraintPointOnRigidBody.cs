using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class ConstraintPointOnRigidBody : PointOnRigidBody
	{
		public ConstraintPointOnRigidBody(Simulation sim, RigidBody rigidBody, Vector3 localPosition, Mass.MassType type)
			: base(sim, rigidBody, localPosition, type)
		{
		}

		public ConstraintPointOnRigidBody(Simulation sim, ConstraintPointOnRigidBody source)
			: base(sim, source)
		{
			this.BounceFactor = source.BounceFactor;
			this.FrictionFactor = source.FrictionFactor;
			this.RaycastLocalDirections = source.RaycastLocalDirections;
		}

		public void SyncCollisionPlanes(ConstraintPointOnRigidBody source)
		{
			if (this.RaycastLocalDirections == null)
			{
				this.RaycastLocalDirections = source.RaycastLocalDirections;
			}
			if (source.CollisionPlanesPoints != null)
			{
				if (this.CollisionPlanesPoints == null)
				{
					this.CollisionPlanesPoints = new Vector4f[source.CollisionPlanesPoints.Length];
					this.CollisionPlanesNormals = new Vector4f[source.CollisionPlanesNormals.Length];
				}
				Array.Copy(source.CollisionPlanesPoints, this.CollisionPlanesPoints, this.CollisionPlanesPoints.Length);
				Array.Copy(source.CollisionPlanesNormals, this.CollisionPlanesNormals, this.CollisionPlanesNormals.Length);
			}
		}

		public override void Simulate()
		{
			base.Simulate();
			Vector4f vector4f = Vector4fExtensions.up;
			float num = Vector4fExtensions.Dot(this.Velocity4f, vector4f);
			Vector4f vector4f2 = (this.Velocity4f - vector4f * new Vector4f(num)) * new Vector4f(this.FrictionFactor);
			if (base.Collision == Mass.CollisionType.ExternalPlane)
			{
				if (this.Position4f.Y < base.GroundHeight)
				{
					this.RigidBody.ApplyForce(vector4f * this.RigidBody.MassValue4f * new Vector4f(1f), base.LocalPosition, false);
				}
				if (this.Position4f.Y + this.Velocity4f.Y * 0.0004f <= base.GroundHeight && num < 0f)
				{
					Vector4f vector4f3 = this.Position4f - this.RigidBody.Position4f;
					float invMassValue = this.RigidBody.InvMassValue;
					float num2 = Vector4fExtensions.Dot(vector4f, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f3, vector4f) / this.RigidBody.GlobalInertiaTensor, vector4f3));
					float num3 = -(1f + this.BounceFactor) * num / (invMassValue + num2);
					this.RigidBody.ApplyImpulse(vector4f * new Vector4f(num3) - vector4f2, base.LocalPosition);
				}
			}
			if (this.CollisionPlanesPoints != null)
			{
				for (int i = 0; i < this.CollisionPlanesPoints.Length; i++)
				{
					vector4f = this.CollisionPlanesNormals[i];
					Vector4f vector4f4 = this.CollisionPlanesPoints[i];
					num = Vector4fExtensions.Dot(this.Velocity4f, vector4f);
					vector4f2 = (this.Velocity4f - vector4f * new Vector4f(num)) * new Vector4f(this.FrictionFactor);
					Vector4f vector4f5 = this.Position4f + this.Velocity4f * Simulation.TimeQuant4f - vector4f4;
					Vector4f vector4f6 = this.Position4f - vector4f4;
					if (Vector4fExtensions.Dot(vector4f5, vector4f) <= 0f && num <= 0.001f)
					{
						float num4 = Mathf.Min(Vector4fExtensions.Dot(vector4f6, vector4f), 0f);
						Vector4f vector4f7 = this.Position4f - this.RigidBody.Position4f;
						float invMassValue2 = this.RigidBody.InvMassValue;
						float num5 = Vector4fExtensions.Dot(vector4f, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f7, vector4f) / this.RigidBody.GlobalInertiaTensor, vector4f7));
						float num6 = -(1f + this.BounceFactor) * (num + num4 * 0.0004f) / (invMassValue2 + num5);
						this.RigidBody.ApplyImpulse(vector4f * new Vector4f(num6) - vector4f2, base.LocalPosition);
					}
				}
			}
		}

		public float BounceFactor;

		public float FrictionFactor;

		public Vector3[] RaycastLocalDirections;

		public Vector4f[] CollisionPlanesPoints;

		public Vector4f[] CollisionPlanesNormals;
	}
}
