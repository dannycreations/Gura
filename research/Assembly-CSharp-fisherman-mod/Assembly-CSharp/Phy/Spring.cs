using System;
using Mono.Simd;
using Mono.Simd.Math;
using Phy.Verlet;
using UnityEngine;

namespace Phy
{
	public class Spring : ConnectionBase, IImpulseConstraint
	{
		public Spring(Mass mass1, Mass mass2, float springConstant, float springLength, float frictionConstant)
			: base(mass1, mass2, -1)
		{
			if (this.SpringLength < 0f)
			{
				throw new ArgumentException("SpringLength can't ne negative");
			}
			this.SpringConstant = springConstant;
			this._springLength = springLength;
			this._targetSpringLength = springLength;
			this.FrictionConstant = frictionConstant;
			this._affectMass1Factor = Vector4fExtensions.one3;
			this._affectMass2Factor = Vector4fExtensions.one3;
			this._impulseThreshold2 = 0f;
			this._impulseVerletMax2 = 100f;
			if (mass1.NextSpring == null)
			{
				mass1.NextSpring = this;
			}
			if (mass2.PriorSpring == null)
			{
				mass2.PriorSpring = this;
			}
			this._massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
		}

		public Spring(Simulation sim, Spring source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass2.UID], source.UID)
		{
			base.Mass1.NextSpring = this;
			base.Mass2.PriorSpring = this;
			this.SpringConstant = source.SpringConstant;
			this._springLength = source._springLength;
			this._targetSpringLength = source._springLength;
			this.FrictionConstant = source.FrictionConstant;
			this.IsRepulsive = source.IsRepulsive;
			this.vmass2 = base.Mass2 as VerletMass;
			this._affectMass1Factor = Vector4fExtensions.one3;
			this._affectMass2Factor = Vector4fExtensions.one3;
			this._impulseThreshold2 = source.ImpulseThreshold2;
			this._impulseVerletMax2 = source.ImpulseVerletMax2;
			this._massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
		}

		public Spring(Spring source, Mass mass1, Mass mass2)
			: base(mass1, mass2, -1)
		{
			base.Mass1.NextSpring = this;
			base.Mass2.PriorSpring = this;
			this.SpringConstant = source.SpringConstant;
			this._springLength = source._springLength;
			this._targetSpringLength = source._springLength;
			this.FrictionConstant = source.FrictionConstant;
			this.IsRepulsive = source.IsRepulsive;
			this.vmass2 = base.Mass2 as VerletMass;
			this._affectMass1Factor = Vector4fExtensions.one3;
			this._affectMass2Factor = Vector4fExtensions.one3;
			this._impulseThreshold2 = source.ImpulseThreshold2;
			this._impulseVerletMax2 = source.ImpulseVerletMax2;
			this._massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
		}

		public float SpringConstant
		{
			get
			{
				return this._springConstant;
			}
			set
			{
				this._springConstant = value;
				base.ConnectionNeedSyncMark();
			}
		}

		public float SpringLength
		{
			get
			{
				return this._targetSpringLength;
			}
			set
			{
				this._targetSpringLength = value;
				base.ConnectionNeedSyncMark();
			}
		}

		public float CurrentSpringLength
		{
			get
			{
				return this._springLength;
			}
		}

		public float Tension
		{
			get
			{
				return this._tension;
			}
		}

		public void SetSpringLengthImmediate(float length)
		{
			this._immediateSpringLength = length;
			base.ConnectionNeedSyncMark();
		}

		public float FrictionConstant
		{
			get
			{
				return this._frictionConstant4f.X;
			}
			set
			{
				this._frictionConstant4f = new Vector4f(value);
				base.ConnectionNeedSyncMark();
			}
		}

		public float AffectMass1Factor
		{
			get
			{
				return this._affectMass1Factor.X;
			}
			set
			{
				this._affectMass1Factor = new Vector4f(value);
				base.ConnectionNeedSyncMark();
			}
		}

		public float AffectMass2Factor
		{
			get
			{
				return this._affectMass2Factor.X;
			}
			set
			{
				this._affectMass2Factor = new Vector4f(value);
				base.ConnectionNeedSyncMark();
			}
		}

		public float ImpulseThreshold2
		{
			get
			{
				return this._impulseThreshold2;
			}
			set
			{
				this._impulseThreshold2 = value;
				base.ConnectionNeedSyncMark();
			}
		}

		public float ImpulseVerletMax2
		{
			get
			{
				return this._impulseVerletMax2;
			}
			set
			{
				this._impulseVerletMax2 = value;
				base.ConnectionNeedSyncMark();
			}
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			base.Mass1.NextSpring = this;
			base.Mass2.PriorSpring = this;
			Spring spring = source as Spring;
			this._targetSpringLength = spring._targetSpringLength;
			this._springConstant = spring.SpringConstant;
			this.vmass2 = base.Mass2 as VerletMass;
			this._affectMass1Factor = spring._affectMass1Factor;
			this._affectMass2Factor = spring._affectMass2Factor;
			this._impulseThreshold2 = spring._impulseThreshold2;
			this._impulseVerletMax2 = spring._impulseVerletMax2;
			this._frictionConstant4f = spring._frictionConstant4f;
			if (spring._immediateSpringLength >= 0f)
			{
				this._springLength = spring._immediateSpringLength;
				this._oldSpringLength = spring._immediateSpringLength;
				this._targetSpringLength = spring._immediateSpringLength;
				spring._immediateSpringLength = -1f;
			}
			this._massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
		}

		public override void SyncSimUpdate()
		{
			base.SyncSimUpdate();
			this._massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
		}

		public void EnableMotionMonitor(string name)
		{
			this.dp = new DebugPlotter("/monitor_output/" + name + ".adv", new string[0], true);
			this.dp_strain = this.dp.AddValue("strain");
			this.dp_deltalength = this.dp.AddValue("deltalength");
		}

		public static void SatisfyImpulseConstraintMass0(Spring s, Mass mass1, Mass mass2, float springLength, Vector4f friction, float impulseThreshold2)
		{
			if (mass1.IsKinematic && mass2.IsKinematic)
			{
				return;
			}
			Vector4f vector4f = mass1.Velocity4f - mass2.Velocity4f;
			Vector4f vector4f2 = mass2.Position4f - mass1.Position4f - vector4f * Simulation.TimeQuant4f;
			float num = vector4f2.SqrMagnitude();
			Vector4f vector4f3 = Vector4fExtensions.Zero;
			Vector4f vector4f4 = Vector4fExtensions.Zero;
			s._tension = 0f;
			s.DeltaLength = 0f;
			if (num > 1E-12f)
			{
				Vector4f vector4f5 = Vector4f.Zero;
				if (s.IsRepulsive || num > springLength * springLength)
				{
					num = Mathf.Sqrt(num);
					Vector4f vector4f6;
					vector4f6..ctor(num);
					vector4f5 = vector4f2 / vector4f6;
					s._tension = (num - springLength) * s._massesInvDenom.X * Simulation.InvTimeQuant4f.X;
					vector4f3 = vector4f5 * new Vector4f(s._tension);
					if (s._tension > impulseThreshold2)
					{
						vector4f4 = vector4f3.Negative() + vector4f5 * new Vector4f(impulseThreshold2);
					}
					s.DeltaLength = num - springLength;
				}
				if (s._tension >= impulseThreshold2)
				{
					Vector4f vector4f7 = (vector4f + vector4f3 - vector4f4) * s._massesInvDenom * friction;
					vector4f3 -= vector4f7;
					vector4f4 += vector4f7;
				}
				mass1.Velocity4f += vector4f3 * mass1.InvMassValue4f * s._affectMass1Factor;
				if (mass2 is VerletMass)
				{
					if (s._tension > s.ImpulseVerletMax2)
					{
						vector4f4 += vector4f5 * new Vector4f(s._tension - s.ImpulseVerletMax2);
					}
					(mass2 as VerletMass).ApplyImpulse(vector4f4 * Simulation.TimeQuant4f * mass2.InvMassValue4f * s._affectMass2Factor, false);
				}
				else
				{
					mass2.Velocity4f += vector4f4 * mass2.InvMassValue4f * s._affectMass2Factor;
				}
			}
		}

		public static float SatisfyImpulseConstraintMass(Spring s, Mass mass1, Mass mass2, float springLength, Vector4f friction, float impulseThreshold2, bool isHitch = false)
		{
			float num = 0f;
			Vector4f vector4f = s._massesInvDenom;
			Vector4f vector4f2 = mass2.InvMassValue4f;
			if (isHitch)
			{
				vector4f = mass1.MassValue4f;
				vector4f2 = Vector4fExtensions.Zero;
			}
			Vector4f vector4f3 = mass1.Velocity4f - mass2.Velocity4f;
			Vector4f vector4f4 = mass2.Position4f - mass1.Position4f - vector4f3 * Simulation.TimeQuant4f;
			float num2 = vector4f4.SqrMagnitude();
			Vector4f vector4f5 = Vector4fExtensions.Zero;
			Vector4f vector4f6 = Vector4fExtensions.Zero;
			s.DeltaLength = 0f;
			if (num2 > 1E-12f)
			{
				Vector4f vector4f7 = Vector4f.Zero;
				if (s.IsRepulsive || num2 > springLength * springLength)
				{
					num2 = Mathf.Sqrt(num2);
					Vector4f vector4f8;
					vector4f8..ctor(num2);
					vector4f7 = vector4f4 / vector4f8;
					num = (num2 - springLength) * vector4f.X * Simulation.InvTimeQuant4f.X;
					vector4f5 = vector4f7 * new Vector4f(num);
					if (num > impulseThreshold2)
					{
						vector4f6 = vector4f5.Negative() + vector4f7 * new Vector4f(impulseThreshold2);
					}
					s.DeltaLength = num2 - springLength;
				}
				if (num >= impulseThreshold2)
				{
					Vector4f vector4f9 = (vector4f3 + vector4f5 - vector4f6) * vector4f * friction;
					vector4f5 -= vector4f9;
					vector4f6 += vector4f9;
				}
				mass1.Velocity4f += vector4f5 * mass1.InvMassValue4f * s._affectMass1Factor;
				if (mass2 is VerletMass)
				{
					if (num > s.ImpulseVerletMax2)
					{
						vector4f6 += vector4f7 * new Vector4f(num - s.ImpulseVerletMax2);
					}
					(mass2 as VerletMass).ApplyImpulse(vector4f6 * Simulation.TimeQuant4f * vector4f2 * s._affectMass2Factor, false);
				}
				else
				{
					mass2.Velocity4f += vector4f6 * vector4f2 * s._affectMass2Factor;
				}
			}
			return num;
		}

		public virtual void SatisfyImpulseConstraint()
		{
			if (base.Mass1.IsKinematic && base.Mass2.IsKinematic)
			{
				return;
			}
			Vector4f vector4f = base.Mass1.Velocity4f - base.Mass2.Velocity4f;
			Vector4f vector4f2 = base.Mass2.Position4f - base.Mass1.Position4f - vector4f * Simulation.TimeQuant4f;
			float num = vector4f2.SqrMagnitude();
			Vector4f vector4f3 = Vector4fExtensions.Zero;
			Vector4f vector4f4 = Vector4fExtensions.Zero;
			if (num > 1E-12f)
			{
				Vector4f vector4f5;
				vector4f5..ctor(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
				float num2 = 0f;
				num = Mathf.Sqrt(num);
				Vector4f vector4f6;
				vector4f6..ctor(num);
				Vector4f vector4f7 = vector4f2 / vector4f6;
				if (this.IsRepulsive || num > this._springLength * this._springLength)
				{
					num2 = (num - this._springLength) * this._massesInvDenom.X * Simulation.InvTimeQuant4f.X;
					vector4f3 = vector4f7 * new Vector4f(num2);
					if (num2 > this._impulseThreshold2)
					{
						vector4f4 = vector4f3.Negative() + vector4f7 * new Vector4f(this._impulseThreshold2);
					}
				}
				if (num2 >= this._impulseThreshold2)
				{
					float num3 = Vector4fExtensions.Dot(base.Mass2.Velocity4f, vector4f2) - Vector4fExtensions.Dot(base.Mass1.Velocity4f, vector4f2);
					Vector4f vector4f8 = vector4f7 * new Vector4f(num3) * this._frictionConstant4f;
					vector4f3 -= vector4f8;
					vector4f4 += vector4f8;
				}
				base.Mass1.Velocity4f += vector4f3 * base.Mass1.InvMassValue4f;
				if (this.vmass2 == null)
				{
					base.Mass2.Velocity4f += vector4f4 * base.Mass2.InvMassValue4f;
				}
				else
				{
					this.vmass2.ApplyImpulse(vector4f4 * Simulation.TimeQuant4f * base.Mass2.InvMassValue4f, false);
				}
			}
		}

		public override void Solve()
		{
			if (base.Mass1.Sim.FrameIteration == 0)
			{
				this._oldSpringLength = this._springLength;
			}
			if (!Mathf.Approximately(this._targetSpringLength, this._springLength))
			{
				this._springLength = Mathf.Lerp(this._oldSpringLength, this._targetSpringLength, base.Mass1.Sim.FrameProgress);
			}
			if (!base.Mass1.IsKinematic || !base.Mass2.IsKinematic)
			{
				this._tension = Spring.SatisfyImpulseConstraintMass(this, base.Mass1, base.Mass2, this._springLength, this._frictionConstant4f, this.ImpulseThreshold2, false);
			}
		}

		public override string ToString()
		{
			return string.Format("  Spring {0} UID:{6} Const:{1}, Len:{2}, Fric:{3}, Constrainted:{4}, Repulsive: {5}", new object[]
			{
				base.ToString(),
				this.SpringConstant,
				this.SpringLength,
				this.FrictionConstant,
				"N",
				(!this.IsRepulsive) ? "N" : "Y",
				base.UID
			});
		}

		public float EquilibrantLength(float forceMagnitude)
		{
			return this._springLength + forceMagnitude / this._springConstant;
		}

		public override float CalculatePotentialEnergy()
		{
			this.springVector = this._mass1.Position4f - this._mass2.Position4f;
			float num = this.springVector.Magnitude() - this._springLength;
			if (num > 0f || this.IsRepulsive)
			{
				return this._springConstant * num * num;
			}
			return 0f;
		}

		public const float LongitudalFrictionMult = 10f;

		public const float LongitudalFrictionPow = 4f;

		private const float MinDistance = 1E-06f;

		private const float SqrMinDistance = 1E-12f;

		protected float _springConstant;

		protected float _springLength;

		protected float _targetSpringLength;

		protected float _oldSpringLength;

		protected float _immediateSpringLength = -1f;

		protected Vector4f _massesInvDenom;

		protected float _tension;

		protected Vector4f _frictionConstant4f;

		public bool IsRepulsive;

		protected Vector4f _affectMass1Factor;

		protected Vector4f _affectMass2Factor;

		protected float _impulseThreshold2;

		protected float _impulseVerletMax2;

		private Vector4f springVector;

		private DebugPlotter dp;

		private DebugPlotter.Value dp_strain;

		private DebugPlotter.Value dp_deltalength;

		private VerletMass vmass2;
	}
}
