using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public sealed class TetrahedronRollStabilizer : VerletConstraint
	{
		public TetrahedronRollStabilizer(VerletMass[] t, float stiffness, Vector3 targetDirection)
			: base(t[0], t[1], -1)
		{
			this.Tetrahedron = t;
			this.BaseStiffness = stiffness;
			this.TargetDirection = targetDirection;
		}

		public TetrahedronRollStabilizer(Simulation sim, TetrahedronRollStabilizer source)
			: base(sim.DictMasses[source.Mass1.UID] as VerletMass, sim.DictMasses[source.Mass2.UID] as VerletMass, source.UID)
		{
			this.Tetrahedron = new VerletMass[4];
			this.Sync(source);
		}

		public Vector3 TargetDirection
		{
			get
			{
				return this.target4f.AsVector3();
			}
			set
			{
				this.target4f = value.AsPhyVector(ref this.target4f);
			}
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			TetrahedronRollStabilizer tetrahedronRollStabilizer = source as TetrahedronRollStabilizer;
			for (int i = 0; i < this.Tetrahedron.Length; i++)
			{
				this.Tetrahedron[i] = base.Mass1.Sim.DictMasses[tetrahedronRollStabilizer.Tetrahedron[i].UID] as VerletMass;
			}
			this.target4f = tetrahedronRollStabilizer.target4f;
			this.BaseStiffness = tetrahedronRollStabilizer.BaseStiffness;
			this.StiffnessMultiplier = tetrahedronRollStabilizer.StiffnessMultiplier;
		}

		public override void Satisfy()
		{
			if (Mathf.Approximately(this.StiffnessMultiplier, 0f))
			{
				return;
			}
			Vector4f vector4f = (this.Tetrahedron[1].Position4f + this.Tetrahedron[2].Position4f + this.Tetrahedron[3].Position4f) * Vector4fExtensions.onethird3;
			Vector4f vector4f2 = this.Tetrahedron[1].Position4f - vector4f;
			Vector4f vector4f3 = this.Tetrahedron[2].Position4f - vector4f;
			Vector4f vector4f4 = this.Tetrahedron[3].Position4f - vector4f;
			float num = vector4f2.Magnitude();
			float num2 = vector4f3.Magnitude();
			float num3 = vector4f4.Magnitude();
			Vector4f vector4f5 = Vector4fExtensions.Cross(this.Tetrahedron[1].Position4f - this.Tetrahedron[3].Position4f, this.Tetrahedron[2].Position4f - this.Tetrahedron[3].Position4f).Normalized();
			float num4 = Vector4fExtensions.Dot(vector4f5, this.target4f);
			Vector4f vector4f6 = vector4f4 / new Vector4f(num3);
			Vector4f vector4f7 = Vector4fExtensions.Cross(vector4f5, vector4f6).Normalized();
			float num5 = Mathf.Atan2(Vector4fExtensions.Dot(vector4f7, this.target4f), Vector4fExtensions.Dot(vector4f6, this.target4f));
			float num6 = Mathf.Atan2(Vector4fExtensions.Dot(vector4f7, vector4f2), Vector4fExtensions.Dot(vector4f6, vector4f2));
			float num7 = Mathf.Atan2(Vector4fExtensions.Dot(vector4f7, vector4f3), Vector4fExtensions.Dot(vector4f6, vector4f3));
			float num8 = 0f;
			num5 = Mathf.Clamp(num5, -0.05f, 0.05f);
			float num9 = num5 * this.BaseStiffness * this.StiffnessMultiplier * 1f;
			num6 += num9;
			num7 += num9;
			num8 += num9;
			Vector4f vector4f8 = this.Tetrahedron[1].Position4f + this.Tetrahedron[2].Position4f + this.Tetrahedron[3].Position4f;
			this.Tetrahedron[1].Position4f = vector4f + vector4f6 * new Vector4f(Mathf.Cos(num6) * num) + vector4f7 * new Vector4f(Mathf.Sin(num6) * num);
			this.Tetrahedron[2].Position4f = vector4f + vector4f6 * new Vector4f(Mathf.Cos(num7) * num2) + vector4f7 * new Vector4f(Mathf.Sin(num7) * num2);
			this.Tetrahedron[3].Position4f = vector4f + vector4f6 * new Vector4f(Mathf.Cos(num8) * num3) + vector4f7 * new Vector4f(Mathf.Sin(num8) * num3);
			Vector4f vector4f9 = this.Tetrahedron[1].Position4f + this.Tetrahedron[2].Position4f + this.Tetrahedron[3].Position4f;
			Vector4f vector4f10 = (vector4f9 - vector4f8) * Vector4fExtensions.onethird3;
			this.Tetrahedron[1].Position4f -= vector4f10;
			this.Tetrahedron[2].Position4f -= vector4f10;
			this.Tetrahedron[3].Position4f -= vector4f10;
		}

		public override void Solve()
		{
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"TetrahedronRollStabilizer: T1( #",
				this.Tetrahedron[0].UID,
				" #",
				this.Tetrahedron[1].UID,
				" #",
				this.Tetrahedron[2].UID,
				" #",
				this.Tetrahedron[3].UID,
				" ) "
			});
		}

		public const float StiffnessTimeConst = 1f;

		public VerletMass[] Tetrahedron;

		public float BaseStiffness;

		public float StiffnessMultiplier = 1f;

		private Vector4f target4f;
	}
}
