using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public sealed class TetrahedronTorsionSpring : VerletConstraint
	{
		public TetrahedronTorsionSpring(VerletMass mass1, VerletMass[] t)
			: base(mass1, t[0], -1)
		{
			this.vmass1 = base.Mass1 as VerletMass;
			this.Tetrahedron = t;
		}

		public TetrahedronTorsionSpring(Simulation sim, TetrahedronTorsionSpring source)
			: base(sim.DictMasses[source.Mass1.UID] as VerletMass, sim.DictMasses[source.Mass2.UID] as VerletMass, source.UID)
		{
			this.Tetrahedron = new VerletMass[4];
			this.torsion = source.torsion;
			this.Sync(source);
			Vector3 vector = (this.Tetrahedron[1].Position + this.Tetrahedron[2].Position + this.Tetrahedron[3].Position) / 3f;
			Vector3 normalized = (this.Tetrahedron[1].Position - vector).normalized;
			this.initMass1Forward = Quaternion.Inverse(base.Mass1.Rotation) * normalized;
			this.prevMass1Forward = (base.Mass1.Rotation * Vector3.forward).AsPhyVector(ref this.prevMass1Forward);
		}

		public float BendFriction
		{
			get
			{
				return this.bendFriction4f.X;
			}
			set
			{
				this.bendFriction4f = new Vector4f(value);
			}
		}

		public float SpringFriction
		{
			get
			{
				return this.springFriction4f.X;
			}
			set
			{
				this.springFriction4f = new Vector4f(value);
			}
		}

		public float Torsion
		{
			get
			{
				return this.torsion;
			}
			set
			{
				this.torsion = value;
			}
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			TetrahedronTorsionSpring tetrahedronTorsionSpring = source as TetrahedronTorsionSpring;
			for (int i = 0; i < this.Tetrahedron.Length; i++)
			{
				if (this.Tetrahedron[i] == null || this.Tetrahedron[i].UID != tetrahedronTorsionSpring.Tetrahedron[i].UID)
				{
					this.Tetrahedron[i] = base.Mass1.Sim.DictMasses[tetrahedronTorsionSpring.Tetrahedron[i].UID] as VerletMass;
				}
			}
			this.vmass1 = base.Mass1 as VerletMass;
			this.SpringLength = tetrahedronTorsionSpring.SpringLength;
			this.springFriction4f = tetrahedronTorsionSpring.springFriction4f;
			this.TorsionFriction = tetrahedronTorsionSpring.TorsionFriction;
			this.TorsionStiffness = tetrahedronTorsionSpring.TorsionStiffness;
			this.bendFriction4f = tetrahedronTorsionSpring.bendFriction4f;
			this.BendStiffness = tetrahedronTorsionSpring.BendStiffness;
			this.invMassSum = 1f / (this.vmass1.InvMassValue + this.Tetrahedron[0].InvMassValue);
			this.lengthsqr = this.SpringLength * this.SpringLength;
		}

		public override void Satisfy()
		{
			Vector4f vector4f = this.Tetrahedron[0].Position4f - this.vmass1.Position4f;
			float num = Vector4fExtensions.Dot(vector4f, vector4f);
			if (num > this.lengthsqr)
			{
				float num2 = (this.lengthsqr / (num + this.lengthsqr) - 0.5f) * this.invMassSum;
				if (!this.vmass1._isKinematic)
				{
					this.vmass1.Position4f -= vector4f * new Vector4f(num2 * this.vmass1.InvMassValue);
				}
				if (!this.Tetrahedron[0]._isKinematic)
				{
					this.Tetrahedron[0].Position4f += vector4f * new Vector4f(num2 * this.Tetrahedron[0].InvMassValue);
				}
			}
		}

		public override void Solve()
		{
			Vector4f vector4f = Vector4fExtensions.onethird * (this.Tetrahedron[1].Position4f + this.Tetrahedron[2].Position4f + this.Tetrahedron[3].Position4f);
			Vector4f vector4f2 = (this.Tetrahedron[0].Position4f - vector4f).Normalized();
			Vector4f vector4f3 = (this.Tetrahedron[1].PrevPosition4f - vector4f).Normalized();
			Vector4f vector4f4 = (this.Tetrahedron[1].Position4f - vector4f).Normalized();
			Vector4f vector4f5 = (base.Mass1.Rotation * Vector3.forward).AsPhyVector(ref vector4f4);
			Vector4f vector4f6 = (base.Mass1.Rotation * Vector3.up).AsPhyVector(ref vector4f4);
			float num = Mathf.Asin(Vector4fExtensions.Dot(vector4f2, Vector4fExtensions.Cross(vector4f4, vector4f3)));
			float num2 = Mathf.Asin(Vector4fExtensions.Dot(Vector4fExtensions.up, Vector4fExtensions.Cross(vector4f5, this.prevMass1Forward)));
			float num3 = num - num2 - this.torsionKahanAccum;
			float num4 = this.torsion + num3;
			this.torsionKahanAccum = num4 - this.torsion - num3;
			this.torsion = Mathf.Clamp(num4, -9.424778f, 9.424778f);
			this.prevMass1Forward = vector4f5;
			Vector4f vector4f7;
			vector4f7..ctor(-this.torsion * this.TorsionStiffness);
			for (int i = 1; i <= 3; i++)
			{
				Vector4f vector4f8 = this.Tetrahedron[i].Position4f - vector4f;
				Vector4f vector4f9 = Vector4fExtensions.Cross(vector4f8, vector4f2);
				Vector4f vector4f10 = vector4f9 * vector4f7;
				Vector4f vector4f11 = vector4f9 * new Vector4f(-this.TorsionFriction * Vector4fExtensions.Dot(vector4f9, this.Tetrahedron[i].Velocity4f));
				this.Tetrahedron[i].ApplyForce(vector4f10 + vector4f11, false);
			}
			Vector4f vector4f12 = (this.vmass1.PositionDelta4f - this.Tetrahedron[0].PositionDelta4f) * this.springFriction4f;
			if (!this.vmass1._isKinematic)
			{
				this.vmass1.Position4f -= vector4f12;
			}
			if (!this.Tetrahedron[0]._isKinematic)
			{
				this.Tetrahedron[0].Position4f += vector4f12;
			}
			Vector3 eulerAngles = base.Mass1.Rotation.eulerAngles;
			if (!Mathf.Approximately(this.BendStiffness, 0f))
			{
				Quaternion quaternion = Quaternion.LookRotation(vector4f4.AsVector3(), vector4f2.AsVector3());
				Vector3 eulerAngles2 = quaternion.eulerAngles;
				float num5 = Mathf.Clamp01(Vector4fExtensions.Dot(vector4f6, vector4f2));
				Quaternion quaternion2 = Quaternion.Slerp(quaternion, Quaternion.Euler(eulerAngles2.x, eulerAngles.y, eulerAngles2.z), this.BendStiffness * num5);
				for (int j = 1; j <= 3; j++)
				{
					Vector4f vector4f13 = this.Tetrahedron[j].Position4f - vector4f;
					vector4f13 = (quaternion2 * (Quaternion.Inverse(quaternion) * vector4f13.AsVector3())).AsPhyVector(ref vector4f13);
					this.Tetrahedron[j].Position4f = vector4f + vector4f13;
				}
			}
			if (!Mathf.Approximately(this.BendFriction, 0f))
			{
				for (int k = 1; k <= 3; k++)
				{
					Vector4f vector4f14 = (this.Tetrahedron[k].PositionDelta4f - this.vmass1.PositionDelta4f) * this.bendFriction4f;
					if (!this.Tetrahedron[k]._isKinematic)
					{
						this.Tetrahedron[k].Position4f -= vector4f12;
					}
					if (!this.vmass1._isKinematic)
					{
						this.vmass1.Position4f += vector4f12;
					}
				}
			}
		}

		public float TorsionStiffness;

		public float TorsionFriction;

		public float BendStiffness;

		public float SpringLength;

		public VerletMass[] Tetrahedron;

		private VerletMass vmass1;

		private float torsion;

		private float torsionKahanAccum;

		private float lengthsqr;

		private float invMassSum;

		private Vector4f springFriction4f;

		private Vector4f bendFriction4f;

		private Vector3 initMass1Forward;

		private Vector4f prevMass1Forward;
	}
}
