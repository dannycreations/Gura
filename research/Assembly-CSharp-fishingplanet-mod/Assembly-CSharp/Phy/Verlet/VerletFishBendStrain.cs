using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public sealed class VerletFishBendStrain : ConnectionBase
	{
		public VerletFishBendStrain(VerletFishBody body)
			: base(body.Masses[0], body.Masses[1], -1)
		{
			this.StiffnessMultiplier = body.BendStiffnessMultiplier;
			this.body = body;
		}

		public VerletFishBendStrain(Simulation sim, VerletFishBendStrain source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass2.UID], source.UID)
		{
			this.Sync(source);
			this.body = source.body;
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			VerletFishBendStrain verletFishBendStrain = source as VerletFishBendStrain;
			this.StiffnessMultiplier = verletFishBendStrain.StiffnessMultiplier;
			this.strain = verletFishBendStrain.strain;
			this.strainswitch = verletFishBendStrain.strainswitch;
			this.prevBend = verletFishBendStrain.prevBend;
		}

		public override void UpdateReferences()
		{
			base.UpdateReferences();
			this.body = (base.Mass1.Sim as ConnectedBodiesSystem).DictObjects[this.body.UID] as VerletFishBody;
		}

		public override void Solve()
		{
			float num = this.currentBend();
			if (this.strain != 0f)
			{
				for (int i = 1; i < this.body.joints.Length - 1; i++)
				{
					this.body.joints[i].BendAngle = Mathf.Lerp(this.body.BendMaxAngle * this.strain, this.body.joints[i].BendAngle, Mathf.Exp(-0.0023999999f));
					this.body.joints[i].StiffnessMultiplier = Mathf.Lerp(this.StiffnessMultiplier, this.body.joints[i].StiffnessMultiplier, Mathf.Exp(-0.0023999999f));
				}
				if (Mathf.Abs(num) >= this.body.BendReboundPoint && (this.prevBend == 0f || Mathf.Approximately(Mathf.Sign(num), -Mathf.Sign(this.prevBend))))
				{
					this.strain = -this.strain;
					this.prevBend = num;
					this.strainswitch = false;
				}
			}
			else
			{
				for (int j = 0; j < this.body.joints.Length; j++)
				{
					this.body.joints[j].BendAngle = 0f;
					this.body.joints[j].StiffnessMultiplier = 1f;
				}
			}
		}

		private float currentBend()
		{
			Vector4f vector4f = Vector4f.Zero;
			for (int i = 0; i < this.body.joints.Length; i++)
			{
				vector4f += this.body.joints[i].Tetrahedron1[0].Position4f;
			}
			vector4f /= new Vector4f((float)this.body.joints.Length);
			Vector4f vector4f2 = (this.body.joints[0].Tetrahedron1[0].Position4f - vector4f).Normalized();
			Vector4f vector4f3 = (vector4f - this.body.joints[this.body.joints.Length - 1].Tetrahedron2[0].Position4f).Normalized();
			Vector4f vector4f4 = Vector4fExtensions.Cross(vector4f2, vector4f3);
			float num = Vector4fExtensions.Dot(vector4f2, vector4f3);
			Vector4f vector4f5 = this.body.GetSegmentBendDirection(0).AsPhyVector(ref vector4f2);
			return -Vector4fExtensions.Dot(vector4f5, vector4f4);
		}

		public void StartBendStrain()
		{
			if (this.strain == 0f)
			{
				this.strain = 1f;
				this.prevBend = 0f;
				this.strainswitch = true;
				base.ConnectionNeedSyncMark();
			}
		}

		public void StopBendStrain()
		{
			this.strain = 0f;
			for (int i = 0; i < this.body.joints.Length; i++)
			{
				this.body.joints[i].BendAngle = 0f;
				this.body.joints[i].StiffnessMultiplier = 1f;
				this.body.joints[i].ConnectionNeedSyncMark();
			}
			base.ConnectionNeedSyncMark();
		}

		public override string ToString()
		{
			return "VerletFishBendStrain";
		}

		public float StiffnessMultiplier;

		private VerletFishBody body;

		private float strain;

		private float prevBend;

		private bool strainswitch;
	}
}
