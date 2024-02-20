using System;
using System.Collections.Generic;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class RigidBody : Mass
	{
		public RigidBody(Simulation sim, float mass, Vector3 position, Mass.MassType type, float radius = 0.0125f)
			: base(sim, mass, Vector3.zero, type)
		{
			this.InertiaTensor = new Vector4f(0.4f * mass);
			this.Dimensions = new Vector3(2f * radius, 2f * radius, 2f * radius);
			this.Rotation = Quaternion.identity;
			this.BuoyancyNormal = Vector3.up;
			this.RotationalBuoyancy = 0f;
			this.WaterY = 0f;
			this.AxialWaterDragFactors = Vector3.one;
			this.AxialWaterDragEnabled = true;
			this.RotationDamping = 1f;
			this.Position = position;
			this.angKahan = Vector4f.Zero;
			base.Radius = radius;
			this.CollisionContacts = new RigidBody.CollisionContact[20];
			for (int i = 0; i < 20; i++)
			{
				this.CollisionContacts[i] = default(RigidBody.CollisionContact);
			}
			if (Mathf.Approximately(this._globalInertiaTensor.X, 0f) || Mathf.Approximately(this._globalInertiaTensor.Y, 0f) || Mathf.Approximately(this._globalInertiaTensor.Z, 0f))
			{
				throw new ArithmeticException("_globalInertiaTensor of RigidBody " + base.UID + " is NAN.");
			}
		}

		public RigidBody(Simulation sim, RigidBody source)
			: base(sim, source)
		{
			this.InertiaTensor = source.InertiaTensor;
			this.Dimensions = source.Dimensions;
			this.Rotation = source.Rotation;
			this.BuoyancyNormal = source.BuoyancyNormal;
			this.RotationalBuoyancy = source.RotationalBuoyancy;
			this.WaterY = source.WaterY;
			this.AxialWaterDragFactors = source.AxialWaterDragFactors;
			this.AxialWaterDragEnabled = source.AxialWaterDragEnabled;
			this.RotationDamping = source.RotationDamping;
			this.UnderwaterRotationDamping = source.UnderwaterRotationDamping;
			this.AngularVelocity = source.AngularVelocity;
			if (Mathf.Approximately(base.MassValue, 0f) || Mathf.Approximately(this.InertiaTensor.X, 0f) || Mathf.Approximately(this.InertiaTensor.Y, 0f) || Mathf.Approximately(this.InertiaTensor.Z, 0f))
			{
				Debug.LogError(string.Concat(new object[] { "RigidBody ", base.UID, " ", base.IID, " invalid params MassValue = ", base.MassValue, " InertiaTensor = ", this.InertiaTensor, " source.IID = ", source.IID }));
			}
			if (!this.AngularVelocity.IsFinite())
			{
				throw new ArithmeticException("AngularVelocity of RigidBody " + base.UID + " is NAN.");
			}
			if (Mathf.Approximately(this._globalInertiaTensor.X, 0f) || Mathf.Approximately(this._globalInertiaTensor.Y, 0f) || Mathf.Approximately(this._globalInertiaTensor.Z, 0f))
			{
				throw new ArithmeticException("_globalInertiaTensor of RigidBody " + base.UID + " is NAN.");
			}
			this.Torque = source.Torque;
			this.angKahan = Vector4f.Zero;
			this.BounceFactor = source.BounceFactor;
		}

		public Vector4f InertiaTensor
		{
			get
			{
				return this._inertiaTensor4f;
			}
			set
			{
				this._inertiaTensor4f = value;
				this._inertiaTensor3 = this._inertiaTensor4f.AsVector3();
			}
		}

		public Vector4f GlobalInertiaTensor
		{
			get
			{
				return this._globalInertiaTensor;
			}
		}

		public Matrix4x4 GlobalInverseInertiaTensor
		{
			get
			{
				return this._globalInverseInertiaTensor;
			}
		}

		private void _updateGlobalInertiaTensor()
		{
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x.SetTRS(Vector3.zero, this.Rotation, Vector3.one);
			Matrix4x4 matrix4x2 = default(Matrix4x4);
			matrix4x2.m00 = this.InertiaTensor.X;
			matrix4x2.m11 = this.InertiaTensor.Y;
			matrix4x2.m22 = this.InertiaTensor.Z;
			matrix4x2.m33 = 1f;
			Matrix4x4 matrix4x3 = matrix4x2;
			matrix4x3 = matrix4x * matrix4x3 * matrix4x.inverse;
			this._globalInertiaTensor = new Vector4f(matrix4x3.m00, matrix4x3.m11, matrix4x3.m22, 1f);
			this._globalInverseInertiaTensor = matrix4x3.inverse;
		}

		public float RotationDamping
		{
			get
			{
				return this._rotationDamping;
			}
			set
			{
				this._rotationDamping = value;
				this._rotationDampingPowerTimeQuant4f = new Vector4f(Mathf.Pow(value, 0.0004f));
			}
		}

		public override Quaternion Rotation
		{
			get
			{
				return this._rotation;
			}
			set
			{
				base.Rotation = value;
				this._invrotation = Quaternion.Inverse(this._rotation);
				this._updateGlobalInertiaTensor();
			}
		}

		public Quaternion InvRotation
		{
			get
			{
				return this._invrotation;
			}
		}

		public float UnderwaterRotationDamping
		{
			get
			{
				return this._underwaterRotationDamping;
			}
			set
			{
				this._underwaterRotationDamping = value;
				this._underwaterRotationDampingPowerTimeQuant4f = new Vector4f(Mathf.Pow(value, 0.0004f));
			}
		}

		public override bool IsCollision
		{
			get
			{
				if (base.IsCollision)
				{
					return true;
				}
				for (int i = 0; i < this.CollisionPlanesCount; i++)
				{
					if (this.CollisionContacts[i].distance < 0f)
					{
						return true;
					}
				}
				return false;
			}
		}

		public Vector4f collisionImpulse { get; protected set; }

		public static Vector4f SolidBoxInertiaTensor(float mass, float width, float height, float depth)
		{
			float num = mass / 12f;
			return new Vector4f(num * (height * height + depth * depth), num * (width * width + depth * depth), num * (width * width + height * height), 1f);
		}

		public void SetDebugPlotter(DebugPlotter dp)
		{
		}

		public virtual void CalculateInertiaTensor()
		{
		}

		public Vector4f LocalToWorld4f(Vector3 localpoint)
		{
			return (this._rotation * localpoint).AsPhyVector(ref this.Position4f) + this.Position4f;
		}

		public Vector3 LocalToWorld(Vector3 localpoint)
		{
			return this._rotation * localpoint + this.Position;
		}

		public Vector4f WorldToLocal4f(Vector3 worldpoint)
		{
			return (this.InvRotation * (worldpoint - this.Position)).AsPhyVector(ref this.Position4f);
		}

		public Vector3 WorldToLocal(Vector3 worldpoint)
		{
			return this.InvRotation * (worldpoint - this.Position);
		}

		public Vector4f PointWorldVelocity4f(Vector3 localpoint)
		{
			return this.Velocity4f + Vector4fExtensions.Cross(this.AngularVelocity, (this._rotation * localpoint).AsPhyVector(ref this.AngularVelocity));
		}

		public void ApplyForce(Vector4f force, Vector3 localpoint, bool capture = false)
		{
			this.ApplyForce(force, capture);
			if (this._isKinematic)
			{
				return;
			}
			Vector3 vector = this._invrotation * force.AsVector3();
			this.Torque += Vector4fExtensions.Cross((this._rotation * localpoint).AsPhyVector(ref this.Torque), force);
		}

		public void ApplyLocalForce(Vector3 localforce, Vector3 localpoint, bool capture = false)
		{
			this.ApplyForce((this._rotation * localforce).AsPhyVector(ref this.Torque), capture);
			if (this._isKinematic)
			{
				return;
			}
			this.Torque += Vector4fExtensions.Cross((this._rotation * localpoint).AsPhyVector(ref this.Torque), (this._rotation * localforce).AsPhyVector(ref this.Torque));
		}

		public void ApplyLocalForce(Vector3 localforce, bool capture = false)
		{
			this.ApplyForce((this._rotation * localforce).AsPhyVector(ref this._force4f), capture);
		}

		public void ApplyTorque(Vector4f torque)
		{
			if (this._isKinematic)
			{
				return;
			}
			this.Torque += torque;
		}

		public void ApplyImpulse(Vector4f impulse, Vector3 localpoint)
		{
			if (this._isKinematic)
			{
				return;
			}
			Vector4f vector4f = this.LocalToWorld4f(localpoint);
			Vector4f vector4f2 = vector4f - this.Position4f;
			Vector4f vector4f3 = Vector4fExtensions.Cross(vector4f2, impulse);
			Vector4f vector4f4 = vector4f3 / this.GlobalInertiaTensor - this.angKahan;
			Vector4f vector4f5 = this.AngularVelocity + vector4f4;
			this.angKahan = vector4f5 - this.AngularVelocity - vector4f4;
			this.AngularVelocity = vector4f5;
			this.Velocity4f += impulse * this._invmassValue;
		}

		protected void BuoyantRotation()
		{
			if (this._isKinematic)
			{
				return;
			}
			Vector3 vector = Vector3.Cross(this._rotation * this.BuoyancyNormal, Vector3.up);
			this.Torque += (this._invrotation * vector * this.RotationalBuoyancy).AsPhyVector(ref this.Torque);
		}

		protected void AxialWaterDrag()
		{
			Vector3 vector = this._invrotation * (this.FlowVelocity - this.Velocity4f).AsVector3();
			this.ApplyLocalForce(Vector3.right * vector.x * Mathf.Max(Mathf.Abs(vector.x), 1f) * this.AxialWaterDragFactors.x, false);
			this.ApplyLocalForce(Vector3.up * vector.y * Mathf.Max(Mathf.Abs(vector.y), 1f) * this.AxialWaterDragFactors.y, false);
			this.ApplyLocalForce(Vector3.forward * vector.z * Mathf.Max(Mathf.Abs(vector.z), 1f) * this.AxialWaterDragFactors.z, false);
		}

		protected void AxialForceWaterConstraint()
		{
			Vector3 vector = this._invrotation * this._force4f.AsVector3();
			Vector3 normalized = this.AxialWaterDragFactors.normalized;
			vector.x *= 1f - normalized.x;
			vector.y *= 1f - normalized.y;
			vector.z *= 1f - normalized.z;
			this._force4f = (this._rotation * vector).AsPhyVector(ref this._force4f);
		}

		public override void Reset()
		{
			base.Reset();
			this.Torque = Vector4f.Zero;
			this.angKahan = Vector4f.Zero;
		}

		public override void StopMass()
		{
			base.StopMass();
			this.Torque = Vector4f.Zero;
			this.AngularVelocity = Vector4f.Zero;
			this.angKahan = Vector4f.Zero;
		}

		public override void SyncMain(Mass source)
		{
			base.SyncMain(source);
			this._invrotation = Quaternion.Inverse(this._rotation);
			RigidBody rigidBody = source as RigidBody;
			this.collisionImpulse = rigidBody.collisionImpulse;
			rigidBody.collisionImpulse = Vector4f.Zero;
		}

		public static int CollisionHash(Collision c)
		{
			GameObject gameObject = c.gameObject;
			GameObject gameObject2 = c.contacts[0].thisCollider.gameObject;
			return gameObject.GetInstanceID() ^ gameObject2.GetInstanceID();
		}

		public void UpdateCollisions(Dictionary<int, Collision> collisions, LayerMask mask)
		{
			if (collisions != null)
			{
				base.Motor = Vector3.zero;
				int num = 0;
				foreach (Collision collision in collisions.Values)
				{
					if (!(collision.gameObject == null))
					{
						if (((1 << collision.gameObject.layer) & mask.value) != 0)
						{
							for (int i = 0; i < collision.contacts.Length; i++)
							{
								Vector3 point = collision.contacts[i].point;
								Vector3 normal = collision.contacts[i].normal;
								float separation = collision.contacts[i].separation;
								this.CollisionContacts[num].point = point.AsVector4f();
								this.CollisionContacts[num].localPoint = this.WorldToLocal(point + separation * normal);
								this.CollisionContacts[num].normal = normal.AsVector4f();
								this.CollisionContacts[num].distance = separation;
								this.CollisionContacts[num].surfaceBounce = 1f;
								if (collision.gameObject.layer == GlobalConsts.BoatWallsLayer)
								{
									this.CollisionContacts[num].surfaceSlidingFriction = 0f;
									Vector3 vector = normal;
									vector.y = 0f;
									vector = vector.normalized * base.MassValue;
									base.Motor += vector;
								}
								else
								{
									this.CollisionContacts[num].surfaceSlidingFriction = 1f;
								}
								this.CollisionContacts[num].surfaceStaticFriction = 1f;
								if (collision.collider.material != null)
								{
									this.CollisionContacts[num].surfaceSlidingFriction = collision.collider.material.dynamicFriction;
									this.CollisionContacts[num].surfaceStaticFriction = collision.collider.material.staticFriction;
									this.CollisionContacts[num].surfaceBounce = collision.collider.material.bounciness;
								}
								num++;
								if (num == this.CollisionContacts.Length)
								{
									break;
								}
							}
							if (num == this.CollisionContacts.Length)
							{
								break;
							}
						}
					}
				}
				this.CollisionPlanesCount = num;
			}
		}

		public virtual void SyncCollisionPlanes(RigidBody source)
		{
			if (source.CollisionContacts != null)
			{
				if (this.CollisionContacts == null)
				{
					this.CollisionContacts = new RigidBody.CollisionContact[source.CollisionContacts.Length];
					for (int i = 0; i < this.CollisionContacts.Length; i++)
					{
						this.CollisionContacts[i] = default(RigidBody.CollisionContact);
					}
				}
				Array.Copy(source.CollisionContacts, this.CollisionContacts, this.CollisionContacts.Length);
				this.CollisionPlanesCount = source.CollisionPlanesCount;
			}
		}

		protected virtual void ResolveCollisions()
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
					Vector4f vector4f2 = this.PointWorldVelocity4f(localPoint);
					Vector4f vector4f3 = this.LocalToWorld(localPoint).AsPhyVector(ref point);
					float num = Vector4fExtensions.Dot(vector4f3 - point, normal);
					if (Mathf.Abs(localPoint.x) <= this.Dimensions.x && Mathf.Abs(localPoint.y) <= this.Dimensions.y && Mathf.Abs(localPoint.z) <= this.Dimensions.z)
					{
						if (num < 0f)
						{
							float num2 = Vector4fExtensions.Dot(vector4f2, normal);
							Vector4f vector4f4 = vector4f2 - normal * new Vector4f(num2);
							Vector4f vector4f5 = vector4f4 - base.FrictionVelocity(vector4f2, vector4f4);
							Vector4f vector4f6 = vector4f2 * Simulation.TimeQuant4f;
							float num3 = num2 * 0.0004f;
							float num4 = -this.ExtrudeFactor * num / this.Sim.FrameDeltaTime;
							RigidBody.CollisionContact[] collisionContacts = this.CollisionContacts;
							int num5 = i;
							collisionContacts[num5].distance = collisionContacts[num5].distance + num4 * 0.0004f;
							if (num2 < 2f * num4)
							{
								Vector4f vector4f7 = point - vector4f;
								Vector4f vector4f8;
								vector4f8..ctor(base.InvMassValue + Vector4fExtensions.Dot(normal, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f7, normal) / this.GlobalInertiaTensor, vector4f7)));
								this.ApplyImpulse((normal * new Vector4f(num4 - (1f + this.BounceFactor) * num2) - vector4f5) / vector4f8, localPoint);
							}
						}
					}
				}
			}
			this.collisionImpulse += this.Velocity4f - velocity4f;
		}

		public override void Simulate()
		{
			if (this.Position4f.Y <= this.WaterY && this.AxialWaterDragEnabled)
			{
				this.AxialWaterDrag();
			}
			if (base.Collision == Mass.CollisionType.RigidbodyContacts)
			{
				this.ResolveCollisions();
			}
			base.Simulate();
			if (this._isKinematic)
			{
				return;
			}
			if (this.Position4f.Y <= this.WaterY)
			{
				this.AngularVelocity *= this._underwaterRotationDampingPowerTimeQuant4f;
			}
			else
			{
				this.AngularVelocity *= this._rotationDampingPowerTimeQuant4f;
			}
			Vector4f vector4f = this.GlobalInverseInertiaTensor.MultiplyPoint(this.Torque.AsVector3()).AsPhyVector(ref this.Torque);
			Vector4f vector4f2 = vector4f * Simulation.TimeQuant4f - this.angKahan;
			Vector4f vector4f3 = this.AngularVelocity + vector4f2;
			this.angKahan = vector4f3 - this.AngularVelocity - vector4f2;
			this.AngularVelocity = vector4f3;
			float num = this.AngularVelocity.Magnitude();
			if (num > 62.831856f)
			{
				this.AngularVelocity *= new Vector4f(62.831856f / num);
			}
			Vector4f vector4f4 = this.AngularVelocity * Simulation.TimeQuant4f;
			float num2 = vector4f4.Magnitude();
			if (!Mathf.Approximately(num2, 0f))
			{
				Vector3 vector = vector4f4.AsVector3() / num2;
				this._rotation = Quaternion.AngleAxis(num2 * 57.29578f, vector) * this._rotation;
			}
			this._rotation = Vector4fExtensions.NormalizeQuaternion(this._rotation);
			this._invrotation = Quaternion.Inverse(this._rotation);
			this._updateGlobalInertiaTensor();
		}

		protected Vector3 _inertiaTensor3;

		protected Vector4f _inertiaTensor4f;

		public Vector3 Dimensions;

		private Vector4f _globalInertiaTensor;

		private Matrix4x4 _globalInverseInertiaTensor;

		public Vector4f AngularVelocity;

		private Vector4f angKahan;

		public Vector4f Torque;

		public Vector3 BuoyancyNormal;

		public float RotationalBuoyancy;

		public float WaterY;

		private float _rotationDamping;

		private Vector4f _rotationDampingPowerTimeQuant4f;

		protected Quaternion _invrotation;

		private float _underwaterRotationDamping;

		private Vector4f _underwaterRotationDampingPowerTimeQuant4f;

		public Vector3 AxialWaterDragFactors;

		public bool AxialWaterDragEnabled;

		public const float MaxAngularVelocity = 62.831856f;

		public const int MaxCollisions = 20;

		public RigidBody.CollisionContact[] CollisionContacts;

		public int CollisionPlanesCount;

		public float BounceFactor;

		public float ExtrudeFactor = 0.01f;

		public struct CollisionContact
		{
			public Vector4f point;

			public Vector4f normal;

			public Vector3 localPoint;

			public float intrusion;

			public float distance;

			public float surfaceSlidingFriction;

			public float surfaceStaticFriction;

			public float surfaceBounce;
		}
	}
}
