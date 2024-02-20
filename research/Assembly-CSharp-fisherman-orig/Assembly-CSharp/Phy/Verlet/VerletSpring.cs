using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public class VerletSpring : VerletConstraint
	{
		public VerletSpring(VerletMass mass1, VerletMass mass2, float length, float friction, bool compressible = false)
			: base(mass1, mass2, -1)
		{
			base.UID = mass1.Sim.NewUID;
			base.Mass1 = mass1;
			base.Mass2 = mass2;
			this.vmass1 = mass1;
			this.vmass2 = mass2;
			this.length = length;
			this.compressible = compressible;
			this.friction = new Vector4f(friction);
			this.lengthsqr = length * length;
			this.invMassSum = 1f / (this.vmass1.InvMassValue + this.vmass2.InvMassValue);
		}

		public VerletSpring(Simulation sim, VerletSpring source)
			: base(sim.DictMasses[source.Mass1.UID] as VerletMass, sim.DictMasses[source.Mass2.UID] as VerletMass, source.UID)
		{
			this.Sync(source);
		}

		public bool IsRepulsive
		{
			get
			{
				return !this.compressible;
			}
			set
			{
				this.compressible = !value;
			}
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			VerletSpring verletSpring = source as VerletSpring;
			this.vmass1 = base.Mass1 as VerletMass;
			this.vmass2 = base.Mass2 as VerletMass;
			this.length = verletSpring.length;
			this.compressible = verletSpring.compressible;
			this.friction = verletSpring.friction;
			this.lengthsqr = this.length * this.length;
			this.invMassSum = 1f / (this.vmass1.InvMassValue + this.vmass2.InvMassValue);
		}

		public override void SetMasses(Mass mass1, Mass mass2 = null)
		{
			if (mass1 != null)
			{
				base.Mass1 = mass1;
				this.vmass1 = mass1 as VerletMass;
			}
			if (mass2 != null)
			{
				base.Mass2 = mass2;
				this.vmass2 = mass2 as VerletMass;
			}
			base.ConnectionNeedSyncMark();
			this.invMassSum = 1f / (this.vmass1.InvMassValue + this.vmass2.InvMassValue);
		}

		public override void Satisfy()
		{
			Vector4f vector4f = this.vmass2.Position4f - this.vmass1.Position4f;
			float num = Vector4fExtensions.Dot(vector4f, vector4f);
			if (!this.compressible || num > this.lengthsqr)
			{
				float num2 = (this.lengthsqr / (num + this.lengthsqr) - 0.5f) * this.invMassSum;
				if (!this.vmass1._isKinematic)
				{
					this.vmass1.Position4f -= vector4f * new Vector4f(num2 * this.vmass1.InvMassValue);
				}
				if (!this.vmass2._isKinematic)
				{
					this.vmass2.Position4f += vector4f * new Vector4f(num2 * this.vmass2.InvMassValue);
				}
			}
		}

		public override void Solve()
		{
			Vector4f vector4f = this.vmass1.Position4f - this.vmass2.Position4f;
			float num = vector4f.SqrMagnitude();
			if (!Mathf.Approximately(0f, num))
			{
				vector4f /= new Vector4f(Mathf.Sqrt(num));
				float num2 = Vector4fExtensions.Dot(this.vmass1.PositionDelta4f, vector4f) - Vector4fExtensions.Dot(this.vmass2.PositionDelta4f, vector4f);
				Vector4f vector4f2 = vector4f * new Vector4f(num2) * this.friction;
				if (!this.vmass1._isKinematic)
				{
					this.vmass1.Position4f -= vector4f2;
				}
				if (!this.vmass2._isKinematic)
				{
					this.vmass2.Position4f += vector4f2;
				}
			}
		}

		public override string ToString()
		{
			return string.Format("VerletSpring {0} Len:{1}, Fric:{2}, Compressible:{3}", new object[]
			{
				base.ToString(),
				this.length,
				this.friction,
				(!this.compressible) ? "N" : "Y"
			});
		}

		public float length;

		private float lengthsqr;

		public Vector4f friction;

		public bool compressible;

		private VerletMass vmass1;

		private VerletMass vmass2;

		private float invMassSum;

		private DebugPlotter monitor;
	}
}
