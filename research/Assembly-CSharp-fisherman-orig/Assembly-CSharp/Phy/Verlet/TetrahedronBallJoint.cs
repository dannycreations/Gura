using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public sealed class TetrahedronBallJoint : VerletConstraint
	{
		public TetrahedronBallJoint(VerletMass[] t1, VerletMass[] t2, float stiffness, float friction = 0.003f)
			: base(t1[0], t2[0], -1)
		{
			this.stiffness4f = new Vector4f(stiffness);
			this.friction4f = new Vector4f(friction);
			this.Tetrahedron1 = t1;
			this.Tetrahedron2 = t2;
			this.BendModifiers = new float[] { 1f, 1f, 1f };
		}

		public TetrahedronBallJoint(Simulation sim, TetrahedronBallJoint source)
			: base(sim.DictMasses[source.Mass1.UID] as VerletMass, sim.DictMasses[source.Mass2.UID] as VerletMass, source.UID)
		{
			this.Tetrahedron1 = new VerletMass[4];
			this.Tetrahedron2 = new VerletMass[4];
			this.Sync(source);
			this.BendModifiers = source.BendModifiers;
		}

		public float Stiffness
		{
			get
			{
				return this.stiffness4f.X;
			}
			set
			{
				this.stiffness4f = new Vector4f(value);
			}
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			TetrahedronBallJoint tetrahedronBallJoint = source as TetrahedronBallJoint;
			this.stiffness4f = tetrahedronBallJoint.stiffness4f;
			this.friction4f = tetrahedronBallJoint.friction4f;
			for (int i = 0; i < this.Tetrahedron1.Length; i++)
			{
				if (this.Tetrahedron1[i] == null || this.Tetrahedron1[i].UID != tetrahedronBallJoint.Tetrahedron1[i].UID)
				{
					this.Tetrahedron1[i] = base.Mass1.Sim.DictMasses[tetrahedronBallJoint.Tetrahedron1[i].UID] as VerletMass;
				}
				if (this.Tetrahedron2[i] == null || this.Tetrahedron2[i].UID != tetrahedronBallJoint.Tetrahedron2[i].UID)
				{
					this.Tetrahedron2[i] = base.Mass1.Sim.DictMasses[tetrahedronBallJoint.Tetrahedron2[i].UID] as VerletMass;
				}
			}
			if (this.FirstChordMass == null || this.FirstChordMass.UID != tetrahedronBallJoint.FirstChordMass.UID)
			{
				this.FirstChordMass = base.Mass1.Sim.DictMasses[tetrahedronBallJoint.FirstChordMass.UID] as VerletMass;
			}
			if (this.SecondChordMass == null || this.SecondChordMass.UID != tetrahedronBallJoint.SecondChordMass.UID)
			{
				this.SecondChordMass = base.Mass1.Sim.DictMasses[tetrahedronBallJoint.SecondChordMass.UID] as VerletMass;
			}
			this.WaveDeviationFreq = tetrahedronBallJoint.WaveDeviationFreq;
			this.WaveDeviationPhase = tetrahedronBallJoint.WaveDeviationPhase;
			this.WaveDeviationAmp = tetrahedronBallJoint.WaveDeviationAmp;
			this.WaveDeviationMult = tetrahedronBallJoint.WaveDeviationMult;
			this.WaveAxis = tetrahedronBallJoint.WaveAxis;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"TetrahedronBallJoint: T1( #",
				this.Tetrahedron1[0].UID,
				" #",
				this.Tetrahedron1[1].UID,
				" #",
				this.Tetrahedron1[2].UID,
				" #",
				this.Tetrahedron1[3].UID,
				" ) T2( #",
				this.Tetrahedron2[0].UID,
				" #",
				this.Tetrahedron2[1].UID,
				" #",
				this.Tetrahedron2[2].UID,
				" #",
				this.Tetrahedron2[3].UID,
				" )"
			});
		}

		protected Quaternion applyWaveDeviation()
		{
			Vector4f vector4f = (this.FirstChordMass.Position4f - this.SecondChordMass.Position4f).Normalized();
			float num = Vector4fExtensions.Dot(this.FirstChordMass.PrevIterationPathDelta, vector4f);
			this.chordPathAccum += Mathf.Min(Mathf.Abs(num), 0.002f) * this.WaveDeviationFreq * 2f;
			this.chordPathAccum %= 1f;
			float num2 = this.WaveDeviationMult * this.WaveDeviationAmp * Mathf.Sin(-this.chordPathAccum * 3.1415927f * 2f + this.WaveDeviationPhase);
			return Quaternion.AngleAxis((this.BendAngle + num2) * 57.29578f, this.WaveAxis);
		}

		public float CurrentBend()
		{
			Vector4f vector4f = (this.Tetrahedron1[0].Position4f - this.Tetrahedron2[0].Position4f).Normalized();
			Vector4f vector4f2 = (this.Tetrahedron2[0].Position4f - (this.Tetrahedron2[1].Position4f + this.Tetrahedron2[2].Position4f + this.Tetrahedron2[3].Position4f) * this.inv3).Normalized();
			return Vector4fExtensions.Dot(vector4f, vector4f2);
		}

		public override void Satisfy()
		{
			Vector4f vector4f = this.Tetrahedron1[0].Position4f + this.Tetrahedron1[1].Position4f + this.Tetrahedron1[2].Position4f + this.Tetrahedron1[3].Position4f + this.Tetrahedron2[0].Position4f + this.Tetrahedron2[1].Position4f + this.Tetrahedron2[2].Position4f + this.Tetrahedron2[3].Position4f;
			Quaternion quaternion = this.applyWaveDeviation();
			Vector4f vector4f2 = (this.Tetrahedron1[1].Position4f + this.Tetrahedron1[2].Position4f + this.Tetrahedron1[3].Position4f) * this.inv3;
			Vector4f vector4f3 = (this.Tetrahedron2[0].Position4f - vector4f2) * this.inv2;
			this.Tetrahedron2[0].Position4f -= vector4f3;
			vector4f3 *= this.inv3;
			this.Tetrahedron1[1].Position4f += vector4f3;
			this.Tetrahedron1[2].Position4f += vector4f3;
			this.Tetrahedron1[3].Position4f += vector4f3;
			Vector4f vector4f4 = (this.Tetrahedron2[1].Position4f + this.Tetrahedron2[2].Position4f + this.Tetrahedron2[3].Position4f) * this.inv3;
			Vector4f vector4f5 = this.Tetrahedron1[0].Position4f - vector4f2;
			Vector4f vector4f6 = vector4f5.Normalized();
			Vector4f vector4f7 = this.Tetrahedron2[0].Position4f - vector4f4;
			Vector4f vector4f8 = vector4f7.Normalized();
			Vector4f vector4f9 = (vector4f2 - this.Tetrahedron1[3].Position4f).Normalized();
			Vector4f vector4f10 = (vector4f4 - this.Tetrahedron2[3].Position4f).Normalized();
			Vector3 vector = -vector4f6.AsVector3();
			Vector3 vector2 = -vector4f8.AsVector3();
			Quaternion quaternion2 = Quaternion.LookRotation(vector4f9.AsVector3(), vector);
			Quaternion quaternion3 = Quaternion.LookRotation(vector4f10.AsVector3(), vector2);
			float num = this.Stiffness * this.StiffnessMultiplier * 0.099999994f;
			Quaternion quaternion4 = Quaternion.Slerp(quaternion2, quaternion3 * Quaternion.Inverse(quaternion), num);
			Quaternion quaternion5 = Quaternion.Slerp(quaternion3, quaternion2 * quaternion, num);
			for (int i = 1; i <= 3; i++)
			{
				Vector4f vector4f11 = this.Tetrahedron1[i].Position4f - vector4f2;
				vector4f11 = (quaternion4 * (Quaternion.Inverse(quaternion2) * vector4f11.AsVector3())).AsPhyVector(ref vector4f11);
				this.Tetrahedron1[i].Position4f = vector4f2 + vector4f11;
				Vector4f vector4f12 = this.Tetrahedron2[i].Position4f - vector4f4;
				vector4f12 = (quaternion5 * (Quaternion.Inverse(quaternion3) * vector4f12.AsVector3())).AsPhyVector(ref vector4f12);
				this.Tetrahedron2[i].Position4f = vector4f4 + vector4f12;
			}
			Vector4f vector4f13 = this.Tetrahedron1[0].Position4f + this.Tetrahedron1[1].Position4f + this.Tetrahedron1[2].Position4f + this.Tetrahedron1[3].Position4f + this.Tetrahedron2[0].Position4f + this.Tetrahedron2[1].Position4f + this.Tetrahedron2[2].Position4f + this.Tetrahedron2[3].Position4f;
			Vector4f vector4f14 = (vector4f13 - vector4f) * TetrahedronBallJoint.inv8;
			this.Tetrahedron1[0].Position4f -= vector4f14;
			this.Tetrahedron1[1].Position4f -= vector4f14;
			this.Tetrahedron1[2].Position4f -= vector4f14;
			this.Tetrahedron1[3].Position4f -= vector4f14;
			this.Tetrahedron2[0].Position4f -= vector4f14;
			this.Tetrahedron2[1].Position4f -= vector4f14;
			this.Tetrahedron2[2].Position4f -= vector4f14;
			this.Tetrahedron2[3].Position4f -= vector4f14;
		}

		public override void Solve()
		{
			for (int i = 0; i < 4; i++)
			{
				Vector4f vector4f = (this.Tetrahedron1[i].PositionDelta4f - this.Tetrahedron2[i].PositionDelta4f) * this.friction4f;
				if (!base.Mass1._isKinematic)
				{
					this.Tetrahedron1[i].Position4f -= vector4f;
				}
				if (!base.Mass2._isKinematic)
				{
					this.Tetrahedron2[i].Position4f += vector4f;
				}
			}
		}

		public const float DefaultFriction = 0.003f;

		public const float BaseStiffness = 0.099999994f;

		private static readonly Vector4f inv8 = new Vector4f(0.125f);

		public float[] BendModifiers;

		public float BendAngle;

		public VerletMass[] Tetrahedron1;

		public VerletMass[] Tetrahedron2;

		public VerletMass FirstChordMass;

		public VerletMass SecondChordMass;

		public float StiffnessMultiplier = 1f;

		public float WaveDeviationFreq;

		public float WaveDeviationPhase;

		public float WaveDeviationAmp;

		public float WaveDeviationMult = 1f;

		public Vector3 WaveAxis;

		private readonly Vector4f inv4 = new Vector4f(0.25f);

		private readonly Vector4f inv3 = new Vector4f(0.33333334f);

		private readonly Vector4f inv2 = new Vector4f(0.5f);

		private Vector4f stiffness4f;

		private Vector4f friction4f;

		private float chordPathAccum;
	}
}
