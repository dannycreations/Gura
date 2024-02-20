using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BiteEditor.ObjectModel;
using Mono.Simd;
using Mono.Simd.Math;
using ObjectModel;
using UnityEngine;

namespace Phy
{
	public class Mass
	{
		public Mass(Simulation sim, float mass, Vector3 position, Mass.MassType type)
		{
			this.Sim = sim;
			this.UID = this.Sim.NewUID;
			this.IID = Mass.NewIID;
			this._type = type;
			this.Position4f = position.AsPhyVector(ref this.Position4f);
			this.VisualPositionOffset = this.Sim.VisualPositionOffset;
			this.MassValue = mass;
			this.Motor4f = Vector4f.Zero;
			this.WaterMotor4f = Vector4f.Zero;
			this._buoyancy4f = new Vector4f(-1f);
			this.BuoyancyAtMinimalSpeed = 0.7f;
			this.WaterDragConstant = 10f;
			this.Collision = Mass.CollisionType.Full;
			Vector3 vector = position;
			Vector3 up = Vector3.up;
			Math3d.GetGroundCollision(vector, out vector, out up, 5f);
			this.heightChunk = new HeightFieldChunk(4, position, 1f, 1f);
			if (this.Sim.PhyActionsListener != null && this.Type != Mass.MassType.Padding)
			{
				this.Sim.PhyActionsListener.MassCreated(this);
			}
			this.colliders = new ICollider[6];
		}

		public Mass(Simulation sim, Mass source)
		{
			this.Sim = sim;
			this.UID = source.UID;
			this.IID = -Mass.NewIID;
			this._type = source.Type;
			this.Sim.DictMasses[this.UID] = this;
			source.CopyMass = this;
			this.SourceMass = source;
			this.MassValue = source.MassValue;
			this.Position4f = source.Position4f;
			this.VisualPositionOffset = this.Sim.VisualPositionOffset;
			this.Rotation = source.Rotation;
			this.Velocity = source.Velocity;
			this.Motor = source.Motor;
			this.WaterMotor = source.WaterMotor;
			this.Buoyancy = source.Buoyancy;
			this.BuoyancySpeedMultiplier = source.BuoyancySpeedMultiplier;
			this.BuoyancyAtMinimalSpeed = source.BuoyancyAtMinimalSpeed;
			this.WaterDragConstant = source.WaterDragConstant;
			this.AirDragConstant = source.AirDragConstant;
			this.groundPoint = source.groundPoint;
			this.FlowVelocity = source.FlowVelocity;
			this.WindVelocity = source.WindVelocity;
			this.IgnoreEnvironment = source.IgnoreEnvironment;
			this.IgnoreEnvForces = source.IgnoreEnvForces;
			this.IsRef = source.IsRef;
			this.IsKinematic = source.IsKinematic;
			this.CurrentVelocityLimit = source.CurrentVelocityLimit;
			this.Collision = source.Collision;
			this.isCollision = source.isCollision;
			this.staticFrictionFactor = source.staticFrictionFactor;
			this.slidingFrictionFactor = source.slidingFrictionFactor;
			this.Radius = source.Radius;
			if (source.heightChunk != null)
			{
				this.heightChunk = new HeightFieldChunk(source.heightChunk);
			}
			this.colliders = new ICollider[6];
			if (source.colliders != null)
			{
				Array.Copy(source.colliders, this.colliders, source.colliders.Length);
			}
			this.extGroundPointPrev = source.extGroundPointPrev;
			this.extGroundPointCurrent = source.extGroundPointCurrent;
			this.extGroundNormalPrev = source.extGroundNormalPrev;
			this.extGroundNormalCurrent = source.extGroundNormalCurrent;
		}

		public float CurrentVelocityLimit
		{
			get
			{
				return this._currentVelocityLimit;
			}
			set
			{
				this._currentVelocityLimit = value;
				this._currentDeltaLimit = value * 0.0004f;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.VelocityLimitChanged(this.UID, value);
				}
			}
		}

		public int UID { get; private set; }

		public int IID { get; private set; }

		public static int NewIID
		{
			get
			{
				return Interlocked.Decrement(ref Mass.IIDCounter);
			}
		}

		public override int GetHashCode()
		{
			return this.IID;
		}

		public Mass.MassType Type
		{
			get
			{
				return this._type;
			}
			set
			{
				this._type = value;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.MassTypeChanged(this.UID, value);
				}
			}
		}

		public float Radius
		{
			get
			{
				return this.radius;
			}
			set
			{
				this.radius = ((value >= 0f) ? value : 0f);
			}
		}

		public float MassValue
		{
			get
			{
				return this.MassValue4f.X;
			}
			set
			{
				this.MassValue4f = new Vector4f(value);
				this._invmassValue_dt4f = new Vector4f(0.0004f / value);
				this._invmassValue = new Vector4f(1f / value);
				this.Weight = this.MassValue * 9.81f;
				if (this._type == Mass.MassType.Rod && this.MassValue < 0.002f)
				{
					this.Weight *= 0.1f;
				}
				this.CompoundWaterResistance = this.MassValue * (this.WaterDragConstant + this.BuoyancySpeedMultiplier * 9.81f);
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.MassChanged(this.UID, value);
				}
			}
		}

		public float InvMassValue
		{
			get
			{
				return this._invmassValue.X;
			}
		}

		public float Weight { get; private set; }

		public Vector4f InvMassValue4f
		{
			get
			{
				return (!this._isKinematic) ? this._invmassValue : Vector4f.Zero;
			}
		}

		public virtual Vector3 Position
		{
			get
			{
				return this.Position4f.AsVector3() + this.visualPositionOffset;
			}
			set
			{
				Vector3 vector = value - this.visualPositionOffset;
				this.Position4f = vector.AsPhyVector(ref this.Position4f);
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.PositionChanged(this.UID, value);
				}
			}
		}

		public Vector3 VisualPositionOffset
		{
			get
			{
				return this.visualPositionOffset;
			}
			set
			{
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.VisualPositionOffsetChanged(this.UID, value);
				}
				else
				{
					Vector4f zero = Vector4f.Zero;
					this.RealVisualDiverge((this.visualPositionOffset - value).AsPhyVector(ref zero));
				}
			}
		}

		public virtual Quaternion Rotation
		{
			get
			{
				return this._rotation;
			}
			set
			{
				this._rotation = value;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.RotationChanged(this.UID, value);
				}
			}
		}

		public Vector3 Velocity
		{
			get
			{
				return this.Velocity4f.AsVector3();
			}
			set
			{
				this.Velocity4f = value.AsPhyVector(ref this.Velocity4f);
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.VelocityChanged(this.UID, value);
				}
			}
		}

		public Vector3 Force
		{
			get
			{
				return this._force4f.AsVector3();
			}
			set
			{
				this._force4f = value.AsPhyVector(ref this._force4f);
			}
		}

		public Vector3 Motor
		{
			get
			{
				return this.Motor4f.AsVector3();
			}
			set
			{
				this.Motor4f = value.AsPhyVector(ref this.Motor4f);
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.MotorChanged(this.UID, value);
				}
			}
		}

		public Vector3 WaterMotor
		{
			get
			{
				return this.WaterMotor4f.AsVector3();
			}
			set
			{
				this.WaterMotor4f = value.AsPhyVector(ref this.WaterMotor4f);
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.WaterMotorChanged(this.UID, value);
				}
			}
		}

		public float Buoyancy
		{
			get
			{
				return this._buoyancy4f.X;
			}
			set
			{
				this._buoyancy4f = new Vector4f(value);
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.BuoyancyChanged(this.UID, value);
				}
			}
		}

		public float WaterDragConstant
		{
			get
			{
				return this.WaterDragConstant4f.X;
			}
			set
			{
				this.WaterDragConstant4f = new Vector4f(value);
				this.CompoundWaterResistance = this.MassValue * (this.WaterDragConstant + this.BuoyancySpeedMultiplier * 9.81f);
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.WaterDragConstantChanged(this.UID, value);
				}
			}
		}

		public float AirDragConstant
		{
			get
			{
				return this.AirDragConstant4f.X;
			}
			set
			{
				this.AirDragConstant4f = new Vector4f(value);
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.AirDragConstantChanged(this.UID, value);
				}
			}
		}

		public float BuoyancySpeedMultiplier
		{
			get
			{
				return this._buoyancySpeedMultiplier4f.X;
			}
			set
			{
				this._buoyancySpeedMultiplier4f = new Vector4f(value);
				this.CompoundWaterResistance = this.MassValue * (this.WaterDragConstant + this.BuoyancySpeedMultiplier * 9.81f);
				this.ScaledBuoyancySpeedMultiplier = this.BuoyancySpeedMultiplier * 1.3f;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.BuoyancySpeedMultiplierChanged(this.UID, value);
				}
			}
		}

		public float CompoundWaterResistance { get; private set; }

		public float ScaledBuoyancySpeedMultiplier { get; private set; }

		public float BuoyancyAtMinimalSpeed
		{
			get
			{
				return this._buoyancyAtMinimalSpeed4f.X;
			}
			set
			{
				this._buoyancyAtMinimalSpeed4f = new Vector4f(value);
			}
		}

		public float GroundHeight
		{
			get
			{
				return this.groundPoint.Y;
			}
			set
			{
				this.extGroundPointPrev = this.extGroundPointCurrent;
				this.extGroundPointCurrent.Y = value;
			}
		}

		public Vector4f GroundPoint4f
		{
			get
			{
				return this.groundPoint;
			}
		}

		public Vector3 GroundPoint
		{
			get
			{
				return this.groundPoint.AsVector3();
			}
			set
			{
				this.extGroundPointPrev = this.extGroundPointCurrent;
				this.extGroundPointCurrent = value.AsPhyVector(ref this.extGroundPointCurrent);
			}
		}

		public Vector4f ExtGroundPointInterpolated
		{
			get
			{
				return Vector4fExtensions.Lerp(this.extGroundPointPrev, this.extGroundPointCurrent, this.Sim.FrameProgress);
			}
		}

		public Vector4f GroundNormal4f
		{
			get
			{
				return this.groundNormal;
			}
		}

		public Vector3 GroundNormal
		{
			get
			{
				return this.groundNormal.AsVector3();
			}
			set
			{
				this.extGroundNormalPrev = this.extGroundNormalCurrent;
				this.extGroundNormalCurrent = value.AsPhyVector(ref this.extGroundNormalCurrent);
			}
		}

		public Vector4f ExtGroundNormalInterpolated
		{
			get
			{
				return Vector4fExtensions.Lerp(this.extGroundNormalPrev, this.extGroundNormalCurrent, this.Sim.FrameProgress);
			}
		}

		public float WaterHeight
		{
			get
			{
				return 0f;
			}
		}

		public bool DisableSimulation
		{
			get
			{
				return this._disableSimulation;
			}
			set
			{
				this._disableSimulation = value;
				if (this.CopyMass != null)
				{
					this.CopyMass.DisableSimulation = value;
				}
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.DisableSimualtionChanged(this.UID, value);
				}
			}
		}

		public Mass.CollisionType Collision
		{
			get
			{
				return this._collision;
			}
			set
			{
				this._collision = value;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.CollisionTypeChanged(this.UID, value);
				}
			}
		}

		public float MotionDamping
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float StaticFrictionFactor
		{
			get
			{
				return this.staticFrictionFactor;
			}
			set
			{
				this.staticFrictionFactor = ((value >= 0f) ? value : 0f);
				if (this.slidingFrictionFactor > this.staticFrictionFactor)
				{
					this.slidingFrictionFactor = this.staticFrictionFactor;
				}
			}
		}

		public float SlidingFrictionFactor
		{
			get
			{
				return this.slidingFrictionFactor;
			}
			set
			{
				this.slidingFrictionFactor = ((value >= 0f) ? value : 0f);
				if (this.slidingFrictionFactor > this.staticFrictionFactor)
				{
					this.staticFrictionFactor = this.slidingFrictionFactor;
				}
			}
		}

		public virtual void EnableMotionMonitor(string name = "MassMotion")
		{
		}

		public void DisableMotionMonitor()
		{
		}

		public bool IsRef
		{
			get
			{
				return this._isRef;
			}
			set
			{
				this._isRef = value;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.IsRefChanged(this.UID, value);
				}
			}
		}

		public bool IsKinematic
		{
			get
			{
				return this._isKinematic;
			}
			set
			{
				this._isKinematic = value;
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.IsKinematicChanged(this.UID, value);
				}
			}
		}

		public virtual void KinematicTranslate(Vector4f offset)
		{
			this.Position += offset.AsVector3();
		}

		public virtual void StopMass()
		{
			this.Force = Vector3.zero;
			this.Velocity = Vector3.zero;
			this.Motor = Vector3.zero;
			this.WaterMotor4f = Vector4f.Zero;
			this.perFrameForcesCache4f = Vector4f.Zero;
			this.perPeriodForcesCache4f = Vector4f.Zero;
			if (this.Sim.PhyActionsListener != null)
			{
				this.Sim.PhyActionsListener.StopMassCalled(this.UID);
			}
		}

		protected virtual void RealVisualDiverge(Vector4f realPositionOffset)
		{
			this.visualPositionOffset -= realPositionOffset.AsVector3();
			this.visualPositionOffset4f = this.visualPositionOffset.AsPhyVector(ref this.visualPositionOffset4f);
			this.Position4f += realPositionOffset;
		}

		public bool IsLying
		{
			get
			{
				return this.Position.y < this.GroundHeight || this.Position.y - this.GroundHeight < this.radius || (this.IsCollision && this.Position.y - this.GroundHeight < this.radius * 2f);
			}
		}

		public virtual bool IsCollision
		{
			get
			{
				return this.isCollision;
			}
		}

		public virtual bool IsTensioned
		{
			get
			{
				if (this.PriorSpring != null)
				{
					Mass mass = this.PriorSpring.Mass1;
					float num = mass.Position4f.Distance(this.Position4f);
					if (num >= this.PriorSpring.SpringLength * 1.0001f)
					{
						return true;
					}
				}
				if (this.NextSpring != null)
				{
					Mass mass2 = this.NextSpring.Mass2;
					float num2 = mass2.Position4f.Distance(this.Position4f);
					if (num2 >= this.NextSpring.SpringLength * 1.0001f)
					{
						return true;
					}
				}
				return false;
			}
		}

		public virtual void UpdateReferences()
		{
		}

		public virtual void DetectRefLeaks(List<Mass> masses, List<ConnectionBase> connections, List<PhyObject> objects, StringBuilder msg)
		{
			if (this.PriorSpring != null && this.NextSpring != null && (!(this.Sim as ConnectedBodiesSystem).DictConnections.ContainsKey(this.NextSpring.UID) || this.NextSpring.Mass1.Sim != this.Sim))
			{
				msg.AppendFormat("{0}.{1} {2}: NextSpring {3}.{4} [{5}.{6}---{7}.{8}] is not present in the system.\n", new object[]
				{
					this.UID,
					this.Type,
					base.GetType(),
					this.NextSpring.UID,
					this.NextSpring.GetType(),
					this.NextSpring.Mass1.UID,
					this.NextSpring.Mass1.Type,
					this.NextSpring.Mass2.UID,
					this.NextSpring.Mass2.Type
				});
				connections.Add(this.NextSpring);
			}
			if (this.NextSpring != null && this.PriorSpring != null && (!(this.Sim as ConnectedBodiesSystem).DictConnections.ContainsKey(this.PriorSpring.UID) || this.PriorSpring.Mass1.Sim != this.Sim))
			{
				msg.AppendFormat("{0}.{1} {2}: PriorSpring {3}.{4} [{5}.{6}---{7}.{8}] is not present in the system.\n", new object[]
				{
					this.UID,
					this.Type,
					base.GetType(),
					this.NextSpring.UID,
					this.PriorSpring.GetType(),
					this.PriorSpring.Mass1.UID,
					this.PriorSpring.Mass1.Type,
					this.PriorSpring.Mass2.UID,
					this.PriorSpring.Mass2.Type
				});
				connections.Add(this.PriorSpring);
			}
		}

		public virtual void SyncMain(Mass source)
		{
			source.extGroundPointCurrent = this.extGroundPointCurrent;
			source.extGroundPointPrev = this.extGroundPointPrev;
			source.extGroundNormalCurrent = this.extGroundNormalCurrent;
			source.extGroundNormalPrev = this.extGroundNormalPrev;
			if (source.heightChunk != null && this.heightChunk != null)
			{
				source.heightChunk.Sync(this.heightChunk);
			}
			if (source.colliders != null)
			{
				Array.Copy(this.colliders, source.colliders, source.colliders.Length);
			}
			this.Position4f = source.Position4f;
			this.Velocity4f = source.Velocity4f;
			this._rotation = source.Rotation;
			this.avgForce = source.avgForce;
			this.avgForceCount = source.avgForceCount;
			this.visualPositionOffset = source.visualPositionOffset;
			this.visualPositionOffset4f = this.visualPositionOffset.AsPhyVector(ref this.visualPositionOffset4f);
			this.IsLimitBreached = source.IsLimitBreached;
			this._currentVelocityLimit = source._currentVelocityLimit;
			this._currentDeltaLimit = source._currentDeltaLimit;
			this.isCollision = source.isCollision;
			Vector3 position = this.Position;
			Vector3 up = Vector3.up;
			Math3d.GetGroundCollision(position, out position, out up, 0.5f);
			this.groundPoint = position.AsPhyVector(ref this.groundPoint);
			this.groundNormal = up.AsPhyVector(ref this.groundPoint);
			source.groundPoint = this.groundPoint;
			source.groundNormal = this.groundNormal;
		}

		public virtual void ApplyForce(Vector3 force, bool capture = false)
		{
			Vector4f zero = Vector4f.Zero;
			this.ApplyForce(force.AsPhyVector(ref zero), capture);
		}

		public virtual void ApplyForce(Vector4f force, bool capture = false)
		{
			if (this._isKinematic)
			{
				return;
			}
			this._force4f += force;
			if (capture)
			{
				this.UpdateAvgForce(force);
			}
			if (this.Sim.PhyActionsListener != null)
			{
				this.Sim.PhyActionsListener.ForceApplied(this.UID, force.AsVector3());
			}
		}

		public virtual void Reset()
		{
			if (this.IsKinematic)
			{
				return;
			}
			this.Force = Vector3.zero;
			if (this.Sim.PhyActionsListener != null)
			{
				this.Sim.PhyActionsListener.ResetCalled(this.UID);
			}
		}

		protected void motionMonitorUpdate()
		{
		}

		protected void UpdateGroundInfo(Vector4f tracePoint, out Vector4f groundPoint, out Vector4f groundNormal)
		{
			if (this.heightChunk != null)
			{
				Vector3 vector;
				groundPoint = this.heightChunk.GetHeightAtPoint(tracePoint.AsVector3(), out vector).AsPhyVector(ref tracePoint);
				groundNormal = vector.AsPhyVector(ref tracePoint);
			}
			else
			{
				groundPoint = this.ExtGroundPointInterpolated;
				groundNormal = this.ExtGroundNormalInterpolated;
			}
		}

		public virtual void Simulate()
		{
			if (this._isKinematic)
			{
				if (this.Collision != Mass.CollisionType.Full)
				{
					this.groundPoint = this.ExtGroundPointInterpolated;
					this.groundNormal = this.ExtGroundNormalInterpolated;
				}
				return;
			}
			Vector4f vector4f = this._force4f * this._invmassValue_dt4f;
			Vector4f velocity4f = this.Velocity4f;
			this.Velocity4f += vector4f;
			if (this.CurrentVelocityLimit > 0f && (Mathf.Abs(this.Velocity4f.X) > this.CurrentVelocityLimit || Mathf.Abs(this.Velocity4f.Y) > this.CurrentVelocityLimit || Mathf.Abs(this.Velocity4f.Z) > this.CurrentVelocityLimit))
			{
				this.Velocity4f *= new Vector4f(this.CurrentVelocityLimit / this.Velocity4f.Magnitude());
				this.IsLimitBreached = true;
			}
			Vector4f vector4f4;
			if (!this.IgnoreEnvForces && this.Position4f.Y < 0.0125f)
			{
				float num = 0.5f * (1f - Mathf.Clamp(this.Position4f.Y, -0.0125f, 0.0125f) / 0.0125f);
				Vector4f vector4f2 = this.Velocity4f - this.FlowVelocity;
				vector4f2.W = 0f;
				float num2 = Mathf.Clamp(vector4f2.Magnitude(), 0f, this.CurrentVelocityLimit * 0.5f);
				float num3 = this.CompoundWaterResistance * num * this._invmassValue.X * 0.0004f;
				Vector4f vector4f3 = this.FlowVelocity * Simulation.TimeQuant4f;
				if (num2 < 1f)
				{
					this.Velocity4f += new Vector4f(num3 * (0.5f * num3 - 1f)) * vector4f2;
					vector4f3 += new Vector4f((1f - 0.5f * num3) * 0.0004f) * vector4f2;
				}
				else
				{
					this.Velocity4f += new Vector4f(1f / (1f + num2 * num3) - 1f) * vector4f2;
					vector4f3 += new Vector4f((1f - 0.5f * num2 * num3) * 0.0004f) * vector4f2;
				}
				vector4f4 = this.Position4f + vector4f3;
			}
			else
			{
				vector4f4 = this.Position4f + this.Velocity4f * Simulation.TimeQuant4f;
			}
			if (this.Collision == Mass.CollisionType.Full)
			{
				this.isCollision = false;
				Vector4f vector4f5 = vector4f4 + this.visualPositionOffset4f;
				Vector4f vector4f6 = Vector4f.Zero;
				Vector3 zero = Vector3.zero;
				Vector3 vector = vector4f5.AsVector3();
				Vector3 vector2 = this.Position;
				for (int i = 0; i < this.colliders.Length; i++)
				{
					if (this.colliders[i] != null)
					{
						vector4f6 = this.colliders[i].TestPoint(vector, vector2, out zero).AsPhyVector(ref this.Position4f);
						if (!vector4f6.IsZero())
						{
							Vector4f vector4f7 = vector4f4 + vector4f6;
							vector4f4 = vector4f7;
							Vector4f vector4f8 = zero.AsPhyVector(ref this.Position4f);
							float num4 = Vector4fExtensions.Dot(vector4f8, this.Velocity4f);
							Vector4f vector4f9 = vector4f8 * new Vector4f(num4);
							Vector4f vector4f10 = this.Velocity4f - vector4f9;
							this.Velocity4f = this.FrictionVelocity(this.Velocity4f, vector4f9);
							this.Velocity4f += ((num4 < 0f) ? (vector4f9.Negative() * Mass.surfaceBounce4f) : vector4f9);
							vector2 = vector;
							vector4f5 = vector4f4 + this.visualPositionOffset4f;
							vector = vector4f5.AsVector3();
							this.isCollision = true;
						}
					}
				}
				if (this.heightChunk != null)
				{
					vector4f6 = this.heightChunk.TestPoint(vector, this.Position, out zero).AsPhyVector(ref this.Position4f);
					if (!vector4f6.IsZero())
					{
						vector4f4 += vector4f6;
						Vector4f vector4f11 = zero.AsPhyVector(ref this.Position4f);
						float num5 = Vector4fExtensions.Dot(vector4f11, this.Velocity4f);
						Vector4f vector4f12 = vector4f11 * new Vector4f(num5);
						Vector4f vector4f13 = this.Velocity4f - vector4f12;
						this.Velocity4f = this.FrictionVelocity(this.Velocity4f, vector4f12);
						this.Velocity4f += ((num5 < 0f) ? (vector4f12.Negative() * Mass.surfaceBounce4f) : vector4f12);
						this.isCollision = true;
					}
				}
			}
			else if (this.Collision == Mass.CollisionType.ExternalPlane)
			{
				this.isCollision = false;
				float num6 = Mathf.Max(0f, this.ExtGroundPointInterpolated.Y - this.Position.y);
				if (num6 > 0f)
				{
					vector4f4 += new Vector4f(0f, num6, 0f, 0f);
					Vector4f extGroundNormalInterpolated = this.ExtGroundNormalInterpolated;
					float num7 = Vector4fExtensions.Dot(extGroundNormalInterpolated, this.Velocity4f);
					Vector4f vector4f14 = extGroundNormalInterpolated * new Vector4f(num7);
					Vector4f vector4f15 = this.Velocity4f - vector4f14;
					this.Velocity4f = this.FrictionVelocity(this.Velocity4f, vector4f14);
					this.Velocity4f += ((num7 < 0f) ? (vector4f14.Negative() * Mass.surfaceBounce4f) : vector4f14);
					this.isCollision = true;
				}
			}
			this.Position4f = vector4f4;
		}

		public Vector4f FrictionVelocity(Vector4f fullVel, Vector4f normalVel)
		{
			Vector4f vector4f = fullVel - normalVel;
			float num = normalVel.Magnitude() * (this.Sim.FrameDeltaTime / 0.0004f);
			float num2 = vector4f.Magnitude();
			float num3 = num * this.StaticFrictionFactor;
			if (num2 > num3)
			{
				float num4 = num * this.SlidingFrictionFactor;
				if (num2 > num4)
				{
					return vector4f * new Vector4f((num2 - num4) / num2);
				}
			}
			return Vector4f.Zero;
		}

		public void ResetAvgForce()
		{
			this.avgForce = Vector4f.Zero;
			this.avgForceCount = 0;
		}

		public void UpdateAvgForce(Vector3 force)
		{
			Vector4f zero = Vector4f.Zero;
			this.avgForce += force.AsPhyVector(ref zero);
			this.avgForceCount++;
		}

		public void UpdateAvgForce(Vector4f force)
		{
			this.avgForce += force;
			this.avgForceCount++;
		}

		public Vector3 AvgForce
		{
			get
			{
				return (this.avgForceCount <= 0) ? Vector3.zero : (this.avgForce * new Vector4f(1f / (float)this.avgForceCount)).AsVector3();
			}
		}

		public override string ToString()
		{
			return string.Format(base.GetType().Name + " #{0}:{1}, Buo:{2}, BuoM:{3}, Motor: {4}, Water Motor: {5}, Water Height: {6}, IsRef:{7}, kinematic:{8}, noEnv:{9}, constraited:{10}, gravityOvr:{11}, motionDamping:{12}, groundH:{13}, waterH:{14}, priorSpring:{15} nextSpring:{16} ", new object[]
			{
				this.UID,
				this.MassValue,
				this.Buoyancy,
				this.BuoyancySpeedMultiplier,
				this.Motor.magnitude,
				this.WaterMotor.magnitude,
				this.WaterHeight,
				(!this.IsRef) ? "N" : "Y",
				(!this.IsKinematic) ? "N" : "Y",
				(!this.IgnoreEnvironment) ? "N" : "Y",
				"N",
				0f,
				this.MotionDamping,
				this.GroundHeight,
				this.WaterHeight,
				(this.PriorSpring == null) ? "null" : string.Concat(new object[]
				{
					"#",
					this.PriorSpring.Mass1.UID,
					" - #",
					this.PriorSpring.Mass2.UID,
					" [uid:",
					this.PriorSpring.UID,
					"]"
				}),
				(this.NextSpring == null) ? "null" : string.Concat(new object[]
				{
					"#",
					this.NextSpring.Mass1.UID,
					" - #",
					this.NextSpring.Mass2.UID,
					" [uid:",
					this.NextSpring.UID,
					"]"
				})
			});
		}

		public void SetFrictionFactor(SurfaceMaterial surface)
		{
			if ((int)((byte)surface) < Mass.frictionFactors.Length)
			{
				FrictionFactor frictionFactor = Mass.frictionFactors[(int)((byte)surface)];
				this.StaticFrictionFactor = frictionFactor.Static;
				this.SlidingFrictionFactor = frictionFactor.Sliding;
			}
			else
			{
				this.StaticFrictionFactor = 0.3f;
				this.SlidingFrictionFactor = 0.25f;
			}
		}

		public SplatMap.LayerName LayerNameUnderObject { get; protected set; }

		public void CheckLayer()
		{
			if (this.IsLying && Init3D.SceneSettings != null && Init3D.SceneSettings.SplatMap != null)
			{
				Vector3f vector3f = new Vector3f(this.Position.x, this.Position.y, this.Position.z);
				SplatMap.Layer layer = Init3D.SceneSettings.SplatMap.GetLayer(vector3f);
				if (layer.Name != this.LayerNameUnderObject)
				{
					this.StaticFrictionFactor = layer.StaticFriction;
					this.SlidingFrictionFactor = layer.SlidingFriction;
					this.LayerNameUnderObject = layer.Name;
				}
			}
		}

		public const int DynamicEffectRecordsMax = 100;

		protected const float DefaultRadius = 0.0125f;

		public const int ForcesUpdatePeriod = 3;

		public const int GroundUpdatePeriod = 5;

		public static readonly Vector4f collisionEpsilon4f = new Vector4f(0.05f);

		public static readonly Vector4f surfaceFriction4f = new Vector4f(0.98f);

		public static readonly Vector4f surfaceBounce4f = new Vector4f(0.15f);

		public static readonly float surfaceStaticFrictionSqr = 0.01f;

		public const float SurfaceHeightDelta = 0.02f;

		public const int HeightFieldChunkSize = 4;

		public const int CollidersCount = 6;

		protected const float AccelerationLimit = 98.100006f;

		public const float VelocityLimitNoFish = 50f;

		public const float VelocityLimitWithFish = 100f;

		private float _currentVelocityLimit = 50f;

		protected float _currentDeltaLimit = 0.02f;

		public Simulation Sim;

		public Mass SourceMass;

		public Mass CopyMass;

		private static int IIDCounter = 0;

		private Mass.MassType _type;

		private float radius = 0.0125f;

		public Vector4f MassValue4f;

		protected Vector4f _invmassValue_dt4f;

		protected Vector4f _invmassValue;

		public Vector4f Position4f;

		protected Vector3 visualPositionOffset;

		protected Vector4f visualPositionOffset4f;

		public Quaternion _rotation;

		public Vector4f Velocity4f;

		protected Vector4f _force4f;

		public Vector4f Motor4f;

		public Vector4f WaterMotor4f;

		public Vector4f perFrameForcesCache4f;

		public Vector4f perPeriodForcesCache4f;

		protected Vector4f _buoyancy4f;

		public Vector4f WaterDragConstant4f;

		public Vector4f AirDragConstant4f;

		protected Vector4f _buoyancySpeedMultiplier4f;

		protected Vector4f _buoyancyAtMinimalSpeed4f;

		protected Vector4f extGroundPointPrev;

		protected Vector4f extGroundPointCurrent;

		protected Vector4f extGroundNormalPrev;

		protected Vector4f extGroundNormalCurrent;

		protected Vector4f groundPoint;

		protected Vector4f groundNormal;

		public ICollider[] colliders;

		public Vector3 sphereOverlapCenter;

		public HeightFieldChunk heightChunk;

		public Vector4f FlowVelocity;

		public Vector4f WindVelocity;

		public bool IgnoreEnvironment;

		public bool IgnoreEnvForces;

		private bool _disableSimulation;

		private Mass.CollisionType _collision;

		public const float DefaultStaticFriction = 0.3f;

		public const float DefaultSlidingFriction = 0.25f;

		protected float staticFrictionFactor = 0.3f;

		protected float slidingFrictionFactor = 0.25f;

		private bool _isRef;

		public bool _isKinematic;

		protected bool isCollision;

		public Spring PriorSpring;

		public Spring NextSpring;

		private Vector4f avgForce;

		private int avgForceCount;

		public bool IsLimitBreached;

		private static FrictionFactor[] frictionFactors = new FrictionFactor[]
		{
			new FrictionFactor
			{
				Static = 0.6f,
				Sliding = 0.5f
			},
			new FrictionFactor
			{
				Static = 0.3f,
				Sliding = 0.25f
			},
			new FrictionFactor
			{
				Static = 0.4f,
				Sliding = 0.35f
			},
			new FrictionFactor
			{
				Static = 0.6f,
				Sliding = 0.5f
			},
			new FrictionFactor
			{
				Static = 0.7f,
				Sliding = 0.6f
			},
			new FrictionFactor
			{
				Static = 0.3f,
				Sliding = 0.25f
			},
			new FrictionFactor
			{
				Static = 0.4f,
				Sliding = 0.35f
			},
			new FrictionFactor
			{
				Static = 0.15f,
				Sliding = 0.1f
			}
		};

		public enum MassType
		{
			Unknown,
			Rod,
			Line,
			Leader,
			Lure,
			Bobber,
			Sinker,
			Wobbler,
			Hook,
			Fish,
			TopWaterLure,
			Boat,
			Plant,
			RigidItem,
			Feeder,
			Bell,
			Auxiliary,
			Padding,
			Leash,
			Boilie,
			Swivel
		}

		public enum CollisionType
		{
			None,
			Full,
			ExternalPlane,
			RigidbodyContacts
		}
	}
}
