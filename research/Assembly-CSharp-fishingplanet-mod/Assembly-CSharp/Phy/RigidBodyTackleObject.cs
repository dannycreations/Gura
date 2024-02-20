using System;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class RigidBodyTackleObject : TackleObject
	{
		public RigidBodyTackleObject(PhyObjectType type, float normalMass, float hangingMass, float buoyancy, ConnectedBodiesSystem sim, TackleBehaviour tackle)
			: base(type, sim, tackle)
		{
			this.NormalMass = normalMass;
			this.HangingMass = hangingMass;
			this.Buoyancy = buoyancy;
		}

		public RigidBodyTackleObject(ConnectedBodiesSystem sim, RigidBodyTackleObject source)
			: base(sim, source)
		{
			this.NormalMass = source.NormalMass;
			this.HangingMass = source.HangingMass;
			this.Buoyancy = source.Buoyancy;
			if (source.cachedRigidBody != null)
			{
				this.cachedRigidBody = sim.DictMasses[source.cachedRigidBody.UID] as RigidBody;
			}
		}

		public override void Sync(PhyObject source)
		{
			base.Sync(source);
			this.cachedRigidBody = this.Sim.DictMasses[(source as RigidBodyTackleObject).cachedRigidBody.UID] as RigidBody;
		}

		public override Mass HookMass
		{
			get
			{
				return base.Masses[1];
			}
		}

		public virtual RigidBody RigidBody
		{
			get
			{
				return (base.Masses[0] as RigidBody) ?? this.cachedRigidBody;
			}
		}

		public override Vector3 DirectionVector
		{
			get
			{
				if (this.cachedRigidBody != null)
				{
					return -(base.Masses[0].Position - base.Masses[0].PriorSpring.Mass1.Position).normalized;
				}
				return base.Masses[0].Rotation * Vector3.forward;
			}
		}

		public override void RealingMasses()
		{
		}

		public override void SyncTransform(Transform transform, Transform topLineAnchor, Transform hookAnchor, Transform root)
		{
			base.SyncTransform(transform, topLineAnchor, hookAnchor, root);
			if (this.cachedRigidBody != null && base.Masses[0].PriorSpring != null)
			{
				transform.rotation = Quaternion.LookRotation(-(base.Masses[0].Position - base.Masses[0].PriorSpring.Mass1.Position), Vector3.up);
				transform.position = base.Tackle.Rod.PositionCorrection(base.Masses[0], true) + transform.position - hookAnchor.position;
			}
			else
			{
				transform.rotation = base.Masses[0].Rotation;
				transform.position += base.Tackle.Rod.PositionCorrection(base.Masses[0], true) - root.position;
			}
			LureBehaviour lureBehaviour = base.Tackle as LureBehaviour;
			if (lureBehaviour != null && lureBehaviour.SwimbaitOwner != null)
			{
				Mass mass = base.Masses[0];
				if (mass.Position.y < 0f)
				{
					Vector3 eulerAngles = mass.Rotation.eulerAngles;
					float num = eulerAngles.x;
					float num2 = eulerAngles.z;
					if (num < 180f)
					{
						if (num > 15f)
						{
							num -= 90f * Time.deltaTime;
						}
					}
					else if (num < 345f)
					{
						num += 90f * Time.deltaTime;
					}
					if (num2 > 270f)
					{
						num2 -= 360f;
					}
					num2 += (90f - num2) * 10f * Time.deltaTime;
					eulerAngles.x = num;
					eulerAngles.z = num2;
					mass.Rotation = Quaternion.Euler(eulerAngles);
				}
				Vector3 vector = mass.Velocity - mass.FlowVelocity.AsVector3();
				Vector3 vector2;
				vector2..ctor(vector.x, 0f, vector.z);
				float magnitude = vector2.magnitude;
				if (magnitude < 0.05f)
				{
					lureBehaviour.SwimbaitOwner.hinge1Anchor.localRotation = Quaternion.identity;
					lureBehaviour.SwimbaitOwner.hinge2Anchor.localRotation = Quaternion.identity;
					lureBehaviour.SwimbaitOwner.hinge3Anchor.localRotation = Quaternion.identity;
				}
				else
				{
					Vector3 vector3 = mass.Position - base.Masses[2].Position;
					Vector3 vector4 = base.Masses[2].Position - base.Masses[3].Position;
					Vector3 vector5 = base.Masses[3].Position - base.Masses[4].Position;
					Vector3 vector6 = base.Masses[4].Position - base.Masses[5].Position;
					float num3 = Quaternion.FromToRotation(vector3, vector4).eulerAngles.x;
					float num4 = Quaternion.FromToRotation(vector4, vector5).eulerAngles.x;
					float num5 = Quaternion.FromToRotation(vector5, vector6).eulerAngles.x;
					if (num3 > 180f)
					{
						num3 -= 360f;
					}
					if (num4 > 180f)
					{
						num4 -= 360f;
					}
					if (num5 > 180f)
					{
						num5 -= 360f;
					}
					float num6 = 0.5f;
					if (magnitude < 0.12f)
					{
						num6 *= (magnitude - 0.05f) * 14.285715f;
					}
					num3 = Mathf.Clamp(num3 * num6, -45f, 45f);
					num4 = Mathf.Clamp(num4 * num6, -45f, 45f);
					num5 = Mathf.Clamp(num5 * num6, -45f, 45f);
					lureBehaviour.SwimbaitOwner.hinge1Anchor.localRotation = Quaternion.Euler(num3, 0f, 0f);
					lureBehaviour.SwimbaitOwner.hinge2Anchor.localRotation = Quaternion.Euler(num4, 0f, 0f);
					lureBehaviour.SwimbaitOwner.hinge3Anchor.localRotation = Quaternion.Euler(num5, 0f, 0f);
				}
			}
		}

		public override Vector3 GetLineAnchorPoint(Transform topLineAnchor)
		{
			MassToRigidBodySpring massToRigidBodySpring = base.Springs[0] as MassToRigidBodySpring;
			return massToRigidBodySpring.AttachmentPointWorld();
		}

		public override void Hitch(Vector3 position)
		{
			base.Masses[0].IsKinematic = true;
			base.Masses[0].StopMass();
			base.Masses[0].Position = position;
		}

		public override void Unhitch()
		{
			base.Masses[0].IsKinematic = false;
			base.Masses[0].StopMass();
		}

		public void ExchangeFirstMass(Mass newMass)
		{
			base.Masses[0] = newMass;
		}

		public override void HookFish(float fishMass, float fishBuoyancy)
		{
			this.RigidBody.MassValue = fishMass * 0.05f;
			this.RigidBody.Buoyancy = fishBuoyancy;
			this.RigidBody.CalculateInertiaTensor();
			this.cachedRigidBody = this.RigidBody;
		}

		public override void EscapeFish()
		{
			base.Masses[0] = this.cachedRigidBody;
			this.RigidBody.Buoyancy = this.Buoyancy;
			this.RigidBody.MassValue = this.NormalMass;
			this.RigidBody.CalculateInertiaTensor();
			this.cachedRigidBody = null;
		}

		public override void OnEnterHangingState()
		{
			if (this.RigidBody != null)
			{
				this.RigidBody.MassValue = this.HangingMass;
				this.RigidBody.CalculateInertiaTensor();
			}
		}

		public override void OnExitHangingState()
		{
			if (this.RigidBody != null)
			{
				this.RigidBody.MassValue = this.NormalMass;
				this.RigidBody.CalculateInertiaTensor();
			}
		}

		protected float NormalMass;

		protected float HangingMass;

		protected float Buoyancy;

		protected RigidBody cachedRigidBody;

		private const float maxHingeAngle = 45f;

		private const float factorHingeAngle = 0.5f;

		private const float minSwimbaitSpeed = 0.05f;

		private const float normSwimbaitSpeed = 0.12f;

		private const float speedFactor = 14.285715f;

		private const float wPitch = 90f;

		private const float wRoll = 10f;

		private const float maxPitch = 15f;
	}
}
