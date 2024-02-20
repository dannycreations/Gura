using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class Bend : ConnectionBase
	{
		public Bend(Mass mass1, Mass mass2, float bendConstant, float springConstant, float frictionConstant, Vector3 localUnbentPosition, float springLength)
			: base(mass1, mass2, -1)
		{
			this.BendConstant = bendConstant;
			this.SpringConstant = springConstant;
			this.FrictionConstant = frictionConstant;
			this.LocalUnbentPosition = localUnbentPosition;
			this.SpringLength = springLength;
			this._frictionConstant4f = new Vector4f(this.FrictionConstant);
		}

		public Bend(Simulation sim, Bend source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass2.UID], source.UID)
		{
			this.BendConstant = source.BendConstant;
			this.SpringConstant = source.SpringConstant;
			this.FrictionConstant = source.FrictionConstant;
			this.LocalUnbentPosition = source.LocalUnbentPosition;
			this.SpringLength = source.SpringLength;
			this._frictionConstant4f = new Vector4f(this.FrictionConstant);
		}

		public Vector3 UnbentPosition
		{
			get
			{
				return base.Mass1._rotation * this.LocalUnbentPosition + base.Mass1.Position;
			}
		}

		private Vector4f unbentPositionActual
		{
			get
			{
				return (base.Mass1._rotation * this.LocalUnbentPosition).AsPhyVector(ref base.Mass1.Position4f) + base.Mass1.Position4f;
			}
		}

		protected Vector3 LocalMass2Position
		{
			get
			{
				return Quaternion.Inverse(base.Mass1._rotation) * (base.Mass2.Position - base.Mass1.Position);
			}
		}

		public override void Solve()
		{
			Vector3 localMass2Position = this.LocalMass2Position;
			base.Mass2._rotation = base.Mass1._rotation * Quaternion.FromToRotation(this.LocalUnbentPosition, localMass2Position);
			Vector4f vector4f = Vector4f.Zero;
			vector4f = this.unbentPositionActual - base.Mass2.Position4f;
			float num = vector4f.Magnitude();
			if (Mathf.Approximately(num, 0f))
			{
				return;
			}
			Vector4f vector4f2 = base.Mass1.Position4f - base.Mass2.Position4f;
			float num2 = vector4f2.Magnitude();
			if (Mathf.Approximately(num2, 0f))
			{
				return;
			}
			Vector4f vector4f3 = vector4f / new Vector4f(num);
			Vector4f vector4f4 = vector4f2 / new Vector4f(num2);
			Vector4f vector4f5 = vector4f3 * new Vector4f(num * this.BendConstant);
			vector4f5 += vector4f4 * new Vector4f((num2 - this.SpringLength) * this.SpringConstant);
			vector4f5 += (base.Mass1.Velocity4f - base.Mass2.Velocity4f) * this._frictionConstant4f;
			Vector4f vector4f6 = vector4f5 * Simulation.TimeQuant4f;
			base.Mass2.Velocity4f += vector4f6 * base.Mass2.InvMassValue4f;
			base.Mass1.Velocity4f -= vector4f6 * base.Mass1.InvMassValue4f;
			base.Mass2.UpdateAvgForce(vector4f5);
			base.Mass1.UpdateAvgForce(vector4f5.Negative());
		}

		public override string ToString()
		{
			return string.Format("  Bend {0} Const:{1}, Spring:{2}, Len:{3}, Fric:{4}, LocalUnbentLength:{5}, Constrainted:{6}, BendAngleCorrection:{7}", new object[]
			{
				base.ToString(),
				this.BendConstant,
				this.SpringConstant,
				this.SpringLength,
				this.FrictionConstant,
				this.LocalUnbentPosition.magnitude,
				"N",
				0
			});
		}

		public float BendConstant;

		public float SpringConstant;

		public Vector3 LocalUnbentPosition;

		public float SpringLength;

		public float FrictionConstant;

		protected Vector4f _frictionConstant4f;
	}
}
