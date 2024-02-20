using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public class VerletMass : Mass
	{
		public VerletMass(Simulation sim, float mass, Vector3 position, Mass.MassType type)
			: base(sim, mass, position, type)
		{
			this.Position = position;
			this.prevPosition4f = this.Position4f;
			this.isStatic = true;
			base.Collision = Mass.CollisionType.Full;
		}

		public VerletMass(Simulation sim, VerletMass source)
			: base(sim, source)
		{
			this.prevPosition4f = this.Position4f;
			this.kahanAccum = Vector4f.Zero;
			this.kahanAccum2 = Vector4f.Zero;
			this.kinematicPrevPosition4f = source.kinematicPrevPosition4f;
			this.kinematicNextPosition4f = source.kinematicNextPosition4f;
			this.isStatic = source.isStatic;
			base.Collision = source.Collision;
		}

		public Vector4f PositionDelta4f
		{
			get
			{
				return this.Position4f - this.prevPosition4f;
			}
			set
			{
				this.prevPosition4f = this.Position4f - value;
			}
		}

		public Vector4f PrevIterationPathDelta { get; private set; }

		public Vector4f PrevPosition4f
		{
			get
			{
				return this.prevPosition4f;
			}
		}

		public override Vector3 Position
		{
			set
			{
				if (!this._isKinematic)
				{
					Vector3 vector = value - this.visualPositionOffset;
					this.prevPosition4f += vector.AsPhyVector(ref this.prevPosition4f) - this.Position4f;
					this.Position4f = vector.AsPhyVector(ref this.Position4f);
				}
				else
				{
					this.isStatic = false;
					this.kinematicPrevPosition4f = this.Position4f;
					this.kinematicNextPosition4f = (value - this.visualPositionOffset).AsPhyVector(ref this.kinematicNextPosition4f);
				}
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.PositionChanged(base.UID, value);
				}
			}
		}

		public override void EnableMotionMonitor(string name = "MassMotion")
		{
			base.EnableMotionMonitor(name);
		}

		public override void StopMass()
		{
			base.StopMass();
			this.prevPosition4f = this.Position4f;
			this.Velocity4f = Vector4f.Zero;
			this.kahanAccum = Vector4f.Zero;
			this.kahanAccum2 = Vector4f.Zero;
		}

		protected override void RealVisualDiverge(Vector4f realPositionOffset)
		{
			base.RealVisualDiverge(realPositionOffset);
			this.prevPosition4f += realPositionOffset;
		}

		public override void KinematicTranslate(Vector4f offset)
		{
			if (this.Sim.PhyActionsListener == null)
			{
				this.Position4f += offset;
				this.prevPosition4f += offset;
			}
			else
			{
				this.Sim.PhyActionsListener.KinematicTranslateCalled(base.UID, offset.AsVector3());
			}
		}

		public void ApplyImpulse(Vector4f step, bool safe = true)
		{
			Vector4f vector4f = step.Negative() - this.kahanAccum2;
			Vector4f vector4f2 = this.prevPosition4f + vector4f;
			this.kahanAccum2 = vector4f2 - this.prevPosition4f - vector4f;
			this.prevPosition4f = vector4f2;
			if (safe)
			{
				Vector4f vector4f3 = this.Position4f - this.prevPosition4f;
				if (base.CurrentVelocityLimit > 0f && (Mathf.Abs(vector4f3.X) > this._currentDeltaLimit || Mathf.Abs(vector4f3.Y) > this._currentDeltaLimit || Mathf.Abs(vector4f3.Z) > this._currentDeltaLimit))
				{
					float num = vector4f3.Magnitude();
					vector4f3 *= new Vector4f(this._currentDeltaLimit / num);
				}
			}
		}

		public override bool IsTensioned
		{
			get
			{
				return false;
			}
		}

		public override void Simulate()
		{
			if (this._isKinematic)
			{
				this.prevPosition4f = this.Position4f;
				if (!this.isStatic)
				{
					this.Position4f = Vector4fExtensions.Lerp(this.kinematicPrevPosition4f, this.kinematicNextPosition4f, this.Sim.FrameProgress);
					if (Mathf.Approximately(this.Sim.FrameProgress, 1f))
					{
						this.isStatic = true;
					}
				}
				Vector4f vector4f = this.Position4f - this.prevPosition4f;
				this.PrevIterationPathDelta = this.Position4f - this.prevPosition4f;
				return;
			}
			Vector4f vector4f2 = this._force4f * this._invmassValue_dt4f;
			Vector4f position4f = this.Position4f;
			Vector3 vector = this.Position;
			Vector4f vector4f3 = this.Position4f - this.prevPosition4f;
			if (base.CurrentVelocityLimit > 0f && (Mathf.Abs(vector4f3.X) > this._currentDeltaLimit || Mathf.Abs(vector4f3.Y) > this._currentDeltaLimit || Mathf.Abs(vector4f3.Z) > this._currentDeltaLimit))
			{
				float num = vector4f3.Magnitude();
				vector4f3 *= new Vector4f(this._currentDeltaLimit / num);
				this.IsLimitBreached = true;
			}
			Vector4f vector4f4 = vector4f3 + vector4f2 * Simulation.TimeQuant4f;
			this.Velocity4f = vector4f4 * Simulation.InvTimeQuant4f;
			Vector4f vector4f5 = this.kahanAccum;
			Vector4f vector4f6 = vector4f4 - this.kahanAccum;
			Vector4f vector4f7 = this.Position4f + vector4f6;
			this.kahanAccum = vector4f7 - this.Position4f - vector4f6;
			this.Position4f = vector4f7;
			this.prevPosition4f = position4f;
			this.PrevIterationPathDelta = vector4f4;
			if (base.Collision == Mass.CollisionType.Full)
			{
				this.isCollision = false;
				Vector3 zero = Vector3.zero;
				Vector4f vector4f8 = Vector4f.Zero;
				Vector3 vector2 = this.Position;
				Vector3 vector3 = vector;
				for (int i = 0; i < this.colliders.Length; i++)
				{
					if (this.colliders[i] != null)
					{
						vector4f8 = this.colliders[i].TestPoint(vector2, vector3, out zero).AsPhyVector(ref this.Position4f);
						if (!vector4f8.IsZero())
						{
							Vector4f vector4f9 = zero.AsPhyVector(ref this.Position4f);
							float num2 = Vector4fExtensions.Dot(vector4f9, this.Velocity4f);
							Vector4f vector4f10 = vector4f9 * new Vector4f(num2);
							Vector4f vector4f11 = this.Velocity4f - vector4f10;
							this.Velocity4f = base.FrictionVelocity(this.Velocity4f, vector4f10);
							this.Velocity4f += ((num2 < 0f) ? (vector4f10.Negative() * Mass.surfaceBounce4f) : vector4f10);
							vector4f5 = this.kahanAccum;
							vector4f6 = vector4f8 - this.kahanAccum;
							vector4f7 = this.Position4f + vector4f6;
							this.kahanAccum = vector4f7 - this.Position4f - vector4f6;
							this.Position4f = vector4f7;
							this.PrevIterationPathDelta = this.Velocity4f * Simulation.TimeQuant4f;
							this.prevPosition4f = this.Position4f - this.PrevIterationPathDelta;
							vector = this.Position;
							vector3 = vector2;
							vector2 = this.Position;
							this.isCollision = true;
						}
					}
				}
				if (this.heightChunk != null)
				{
					vector4f8 = this.heightChunk.TestPoint(this.Position, vector, out zero).AsPhyVector(ref this.Position4f);
					if (!vector4f8.IsZero())
					{
						Vector4f vector4f12 = zero.AsPhyVector(ref this.Position4f);
						float num3 = Vector4fExtensions.Dot(vector4f12, this.Velocity4f);
						vector4f5 = this.kahanAccum;
						vector4f6 = vector4f8 - this.kahanAccum;
						vector4f7 = this.Position4f + vector4f6;
						this.kahanAccum = vector4f7 - this.Position4f - vector4f6;
						this.Position4f = vector4f7;
						Vector4f vector4f13 = vector4f12 * new Vector4f(num3);
						Vector4f vector4f14 = this.Velocity4f - vector4f13;
						this.Velocity4f = base.FrictionVelocity(this.Velocity4f, vector4f13);
						this.Velocity4f += ((num3 < 0f) ? (vector4f13.Negative() * Mass.surfaceBounce4f) : vector4f13);
						this.PrevIterationPathDelta = this.Velocity4f * Simulation.TimeQuant4f;
						this.prevPosition4f = this.Position4f - this.PrevIterationPathDelta;
						this.isCollision = true;
					}
				}
			}
			else if (base.Collision == Mass.CollisionType.ExternalPlane)
			{
				this.isCollision = false;
				float num4 = Mathf.Max(0f, base.ExtGroundPointInterpolated.Y - this.Position.y);
				if (num4 > 0f)
				{
					Vector4f extGroundNormalInterpolated = base.ExtGroundNormalInterpolated;
					float num5 = Vector4fExtensions.Dot(extGroundNormalInterpolated, this.Velocity4f);
					vector4f5 = this.kahanAccum;
					vector4f6 = new Vector4f(0f, num4, 0f, 0f) - this.kahanAccum;
					vector4f7 = this.Position4f + vector4f6;
					this.kahanAccum = vector4f7 - this.Position4f - vector4f6;
					this.Position4f = vector4f7;
					Vector4f vector4f15 = extGroundNormalInterpolated * new Vector4f(num5);
					Vector4f vector4f16 = this.Velocity4f - vector4f15;
					this.Velocity4f = base.FrictionVelocity(this.Velocity4f, vector4f15);
					this.Velocity4f += ((num5 < 0f) ? (vector4f15.Negative() * Mass.surfaceBounce4f) : vector4f15);
					this.PrevIterationPathDelta = this.Velocity4f * Simulation.TimeQuant4f;
					this.prevPosition4f = this.Position4f - this.PrevIterationPathDelta;
					this.isCollision = true;
				}
			}
			this.kinematicPrevPosition4f = this.Position4f;
			this.kinematicNextPosition4f = this.Position4f;
		}

		protected Vector4f prevPosition4f;

		protected Vector4f kahanAccum = Vector4f.Zero;

		protected Vector4f kahanAccum2 = Vector4f.Zero;

		protected bool isStatic;

		private Vector4f kinematicPrevPosition4f;

		private Vector4f kinematicNextPosition4f;

		private static readonly Vector4f[] defaultCollisionPoints = new Vector4f[]
		{
			Vector4fExtensions.right * new Vector4f(10000f),
			Vector4fExtensions.left * new Vector4f(10000f),
			Vector4fExtensions.up * new Vector4f(10000f),
			Vector4fExtensions.down * new Vector4f(10000f),
			Vector4fExtensions.forward * new Vector4f(10000f),
			Vector4fExtensions.back * new Vector4f(10000f)
		};

		private static readonly Vector4f[] defaultCollisionNormals = new Vector4f[]
		{
			Vector4fExtensions.left,
			Vector4fExtensions.right,
			Vector4fExtensions.down,
			Vector4fExtensions.up,
			Vector4fExtensions.back,
			Vector4fExtensions.forward
		};
	}
}
