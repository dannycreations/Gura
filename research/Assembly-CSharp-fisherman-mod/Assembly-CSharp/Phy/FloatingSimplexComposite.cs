using System;
using Mono.Simd;
using Mono.Simd.Math;
using SimplexGeometry;
using UnityEngine;

namespace Phy
{
	public class FloatingSimplexComposite : RigidBody
	{
		public FloatingSimplexComposite(Simulation sim, float mass, Vector3 position, Mass.MassType type)
			: base(sim, mass, position, type, 0.0125f)
		{
			this.LongitudinalSpeed = 0f;
			this.LateralSpeed = 0f;
			this.IsGliding = false;
		}

		public FloatingSimplexComposite(Simulation sim, FloatingSimplexComposite source)
			: base(sim, source)
		{
			this.VolumeBody = source.VolumeBody;
			this.LateralWaterResistance = source.LateralWaterResistance;
			this.LongitudalWaterResistance = source.LongitudalWaterResistance;
			this.RollFactor = source.RollFactor;
			this.RollAngle = source.RollAngle;
			this.IsRollLimit = source.IsRollLimit;
			this.GlidingBoat = source.GlidingBoat;
			this.GlidingOnSpeed = source.GlidingOnSpeed;
			this.GlidingOffSpeed = source.GlidingOffSpeed;
			this.LiftFactor = source.LiftFactor;
			this.TangageFactor = source.TangageFactor;
			this.IsGliding = source.IsGliding;
			this.WaterNoise = source.WaterNoise;
			base.InertiaTensor = source.InertiaTensor;
			this.DynamicTangageStabilizer = source.DynamicTangageStabilizer;
			this.DynamicYawStabilizer = source.DynamicYawStabilizer;
			this.DynamicRollStabilizer = source.DynamicRollStabilizer;
			this.equilibriumVolumeRatio = source.equilibriumVolumeRatio;
		}

		public float LongitudinalSpeed { get; private set; }

		public float LateralSpeed { get; private set; }

		public bool IsGliding { get; private set; }

		public float waterlineY { get; private set; }

		private float computeEquilibriumDisplacement(int steps)
		{
			float num = 0f;
			float num2 = 0.5f;
			for (int i = 0; i < steps; i++)
			{
				this.VolumeBody.ComplexClip(new Vector4f(0f, num, 0f, 0f), Vector4fExtensions.down);
				float num3 = this.VolumeBody.cachedComplexClipBody.Volume * 1000f;
				if (num3 > base.MassValue)
				{
					num -= num2;
				}
				else
				{
					num += num2;
				}
				num2 *= 0.5f;
			}
			return num;
		}

		private float computeDisplacementRatio(float y)
		{
			this.VolumeBody.ComplexClip(new Vector4f(0f, y, 0f, 0f), Vector4fExtensions.down);
			return this.VolumeBody.cachedComplexClipBody.Volume / this.VolumeBody.Volume;
		}

		public virtual void UpdateBody()
		{
			float num = base.MassValue / this.VolumeBody.Volume;
			base.InertiaTensor = this.VolumeBody.CalculateInertiaTensor(this.VolumeBody.Barycenter, num);
			if (this.VolumeBody.Cargo != null)
			{
				base.MassValue = this.VolumeBody.MassValue;
			}
			this.waterlineY = this.computeEquilibriumDisplacement(10);
			this.equilibriumVolumeRatio = this.computeDisplacementRatio(this.waterlineY);
		}

		public void MulInertiaTensor(float factor)
		{
			if (!Mathf.Approximately(factor, 1f))
			{
				Vector4f inertiaTensor = base.InertiaTensor;
				inertiaTensor.X *= factor;
				inertiaTensor.Y *= factor;
				inertiaTensor.Z *= factor;
				base.InertiaTensor = inertiaTensor;
			}
		}

		public override void SyncMain(Mass source)
		{
			base.SyncMain(source);
			FloatingSimplexComposite floatingSimplexComposite = source as FloatingSimplexComposite;
			if (floatingSimplexComposite != null)
			{
				this.LongitudinalSpeed = floatingSimplexComposite.LongitudinalSpeed;
				this.LateralSpeed = floatingSimplexComposite.LateralSpeed;
				this.IsGliding = floatingSimplexComposite.IsGliding;
			}
		}

		public virtual void SyncBoatInertia(FloatingSimplexComposite source)
		{
			this.VolumeBody.SetMass(source.VolumeBody.MassValue);
			this.VolumeBody.SetBarycenter(source.VolumeBody.Barycenter);
			this.VolumeBody.InertiaTensor = source.VolumeBody.InertiaTensor;
		}

		public override void SyncCollisionPlanes(RigidBody source)
		{
			base.SyncCollisionPlanes(source);
			FloatingSimplexComposite floatingSimplexComposite = source as FloatingSimplexComposite;
			this.WaterNoise = floatingSimplexComposite.WaterNoise;
			this.LateralWaterResistance = floatingSimplexComposite.LateralWaterResistance;
			this.LongitudalWaterResistance = floatingSimplexComposite.LongitudalWaterResistance;
			this.DynamicRollStabilizer = floatingSimplexComposite.DynamicRollStabilizer;
			this.DynamicTangageStabilizer = floatingSimplexComposite.DynamicTangageStabilizer;
			this.DynamicYawStabilizer = floatingSimplexComposite.DynamicYawStabilizer;
			this.RollFactor = floatingSimplexComposite.RollFactor;
			this.RollAngle = floatingSimplexComposite.RollAngle;
			this.IsRollLimit = floatingSimplexComposite.IsRollLimit;
			this.GlidingBoat = floatingSimplexComposite.GlidingBoat;
			this.GlidingOnSpeed = floatingSimplexComposite.GlidingOnSpeed;
			this.GlidingOffSpeed = floatingSimplexComposite.GlidingOffSpeed;
			this.LiftFactor = floatingSimplexComposite.LiftFactor;
			this.TangageFactor = floatingSimplexComposite.TangageFactor;
			this.IsGliding = floatingSimplexComposite.IsGliding;
			base.InertiaTensor = floatingSimplexComposite.InertiaTensor;
		}

		private void resolveCollisions()
		{
			if (this.CollisionContacts != null)
			{
				for (int i = 0; i < this.CollisionContacts.Length; i++)
				{
					if (i >= this.CollisionPlanesCount)
					{
						break;
					}
					float distance = this.CollisionContacts[i].distance;
					Vector4f normal = this.CollisionContacts[i].normal;
					Vector4f point = this.CollisionContacts[i].point;
					Vector3 vector = base.WorldToLocal(point.AsVector3());
					Vector4f vector4f = base.PointWorldVelocity4f(vector);
					float num = Vector4fExtensions.Dot(vector4f, normal);
					Vector4f vector4f2 = (vector4f - normal * new Vector4f(num)) * new Vector4f(base.SlidingFrictionFactor);
					Vector4f vector4f3 = vector4f * Simulation.TimeQuant4f;
					float num2 = distance + num * 0.0004f;
					float num3 = 0f;
					if (num2 < 0.05f && num < 0f)
					{
						Vector4f vector4f4 = point - this.Position4f;
						float invMassValue = base.InvMassValue;
						float num4 = Vector4fExtensions.Dot(normal, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f4, normal) / base.GlobalInertiaTensor, vector4f4));
						float num5 = -(1f + this.BounceFactor) * (num + num3) / (invMassValue + num4);
						base.ApplyImpulse(normal * new Vector4f(num5) - vector4f2, vector);
					}
				}
			}
		}

		protected override void ResolveCollisions()
		{
			Vector4f vector4f = this.Position.AsPhyVector(ref this.Position4f);
			Vector4f velocity4f = this.Velocity4f;
			if (this.CollisionContacts != null)
			{
				for (int i = 0; i < this.CollisionContacts.Length; i++)
				{
					if (i >= this.CollisionPlanesCount)
					{
						break;
					}
					float distance = this.CollisionContacts[i].distance;
					Vector4f normal = this.CollisionContacts[i].normal;
					Vector4f point = this.CollisionContacts[i].point;
					Vector3 localPoint = this.CollisionContacts[i].localPoint;
					Vector4f vector4f2 = base.PointWorldVelocity4f(localPoint);
					Vector4f vector4f3 = base.LocalToWorld(localPoint).AsPhyVector(ref point);
					float num = Vector4fExtensions.Dot(vector4f3 - point, normal);
					if (num < 0f)
					{
						float num2 = Vector4fExtensions.Dot(vector4f2, normal);
						Vector4f vector4f4 = vector4f2 - normal * new Vector4f(num2);
						Vector4f vector4f5 = (vector4f2 - normal * new Vector4f(num2)) * new Vector4f(base.SlidingFrictionFactor * this.CollisionContacts[i].surfaceSlidingFriction);
						Vector4f vector4f6 = vector4f2 * Simulation.TimeQuant4f;
						float num3 = num2 * 0.0004f;
						float num4 = -this.ExtrudeFactor * num / this.Sim.FrameDeltaTime;
						RigidBody.CollisionContact[] collisionContacts = this.CollisionContacts;
						int num5 = i;
						collisionContacts[num5].distance = collisionContacts[num5].distance + num4 * 0.0004f;
						if (num2 < 2f * num4)
						{
							num2 = Mathf.Min(num2, 0f);
							Vector4f vector4f7 = point - vector4f;
							Vector4f vector4f8;
							vector4f8..ctor(base.InvMassValue + Vector4fExtensions.Dot(normal, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f7, normal) / base.GlobalInertiaTensor, vector4f7)));
							base.ApplyImpulse((normal * new Vector4f(num4 - (1f + this.BounceFactor * this.CollisionContacts[i].surfaceBounce) * num2) - vector4f5) / vector4f8, localPoint);
						}
					}
				}
			}
			base.collisionImpulse += this.Velocity4f - velocity4f;
		}

		public override void Simulate()
		{
			this.noisePos += 0.0004f * (0.75f + base.Velocity.magnitude * 0.5f);
			this.noisePos %= 10f;
			Quaternion quaternion = Quaternion.Euler(this.WaterNoise * (Mathf.PerlinNoise(0f, this.noisePos) - 0.5f), this.WaterNoise * (Mathf.PerlinNoise(0f, -this.noisePos) - 0.5f), this.WaterNoise * (Mathf.PerlinNoise(this.noisePos, 0f) - 0.5f));
			Vector3 position = this.Position;
			position.y = 0f;
			this.VolumeBody.ComplexClip(base.WorldToLocal4f(position), (quaternion * (this._invrotation * Vector3.down)).AsVector4f());
			float num = this.VolumeBody.cachedComplexClipBody.Volume / this.VolumeBody.Volume;
			Vector3 vector = this.VolumeBody.cachedComplexClipBody.Barycenter.AsVector3();
			base.ApplyForce(new Vector4f(0f, this.VolumeBody.cachedComplexClipBody.Volume * 9.81f * 1000f, 0f, 0f), vector, false);
			Vector4f vector4f = base.PointWorldVelocity4f(vector) - this.FlowVelocity;
			this.IsGliding = false;
			if (!Mathf.Approximately(vector4f.SqrMagnitude(), 0f))
			{
				Vector4f vector4f2 = (this._rotation * Vector3.right).AsVector4f();
				Vector4f vector4f3 = (this._rotation * Vector3.forward).AsVector4f();
				Vector4f vector4f4 = (this._rotation * Vector3.up).AsVector4f();
				this.LongitudinalSpeed = Vector4fExtensions.Dot(vector4f, vector4f3);
				this.LateralSpeed = Vector4fExtensions.Dot(vector4f, vector4f2);
				float num2 = vector4f.Magnitude();
				float num3 = num / this.equilibriumVolumeRatio;
				float num4 = -this.LateralSpeed * this.LateralWaterResistance * num2 * num3;
				float num5 = -this.LongitudinalSpeed * this.LongitudalWaterResistance * ((num2 >= 1f) ? num2 : 1f) * num3;
				float num6 = -Vector4fExtensions.Dot(vector4f, vector4f4) * this.LateralWaterResistance * num2 * num3;
				if (this.GlidingBoat)
				{
					this.IsGliding = ((!this.IsGliding) ? (this.LongitudinalSpeed > this.GlidingOnSpeed) : (this.LongitudinalSpeed > this.GlidingOffSpeed));
					if (this.IsGliding && num5 < 0f && vector.y < 0f && !Mathf.Approximately(this.LiftFactor, 0f))
					{
						float num7 = -num5 * this.LiftFactor;
						base.ApplyForce(vector4f4 * new Vector4f(num7), this.VolumeBody.Barycenter.AsVector3(), false);
					}
					if (!Mathf.Approximately(this.TangageFactor, 0f) && num5 < 0f)
					{
						float num8 = this._rotation.eulerAngles.x;
						if (num8 > 180f)
						{
							num8 -= 360f;
						}
						float num9 = -num5 * this.TangageFactor * (num8 + this.TangageFactor * 2f);
						base.ApplyForce(new Vector4f(0f, num9, 0f, 0f), Vector3.forward, false);
						base.ApplyForce(new Vector4f(0f, -num9, 0f, 0f), -Vector3.forward, false);
					}
				}
				if (!Mathf.Approximately(num4, 0f) && this.LongitudinalSpeed > 1f)
				{
					bool flag = true;
					if (this.IsRollLimit)
					{
						float num10 = this._rotation.eulerAngles.z;
						if (num10 > 180f)
						{
							num10 -= 360f;
						}
						flag = num10 > -this.RollAngle && num10 < this.RollAngle;
					}
					if (flag)
					{
						float num11 = -num4 * this.RollFactor;
						base.ApplyForce(new Vector4f(0f, num11, 0f, 0f), Vector3.right, false);
						base.ApplyForce(new Vector4f(0f, -num11, 0f, 0f), -Vector3.right, false);
					}
				}
				base.ApplyForce(vector4f2 * new Vector4f(num4), vector, false);
				base.ApplyForce(vector4f3 * new Vector4f(num5), vector, false);
				base.ApplyForce(vector4f4 * new Vector4f(num6), vector, false);
				float num12 = num2 * base.MassValue;
				base.ApplyTorque(new Vector4f(-this.AngularVelocity.X * this.DynamicTangageStabilizer * num12, -this.AngularVelocity.Y * this.DynamicYawStabilizer * num12, -this.AngularVelocity.Z * this.DynamicRollStabilizer * num12, 0f));
			}
			base.Simulate();
		}

		public SimplexComposite VolumeBody;

		public float LateralWaterResistance;

		public float LongitudalWaterResistance;

		public float DynamicTangageStabilizer;

		public float DynamicYawStabilizer;

		public float DynamicRollStabilizer;

		public float WaterNoise;

		public float RollFactor;

		public float RollAngle;

		public bool IsRollLimit;

		public bool GlidingBoat;

		public float GlidingOnSpeed;

		public float GlidingOffSpeed;

		public float LiftFactor;

		public float TangageFactor;

		private float noisePos;

		private float equilibriumVolumeRatio;
	}
}
