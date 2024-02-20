using System;
using UnityEngine;

namespace Phy
{
	public class RodObject : PhyObject
	{
		public RodObject(BendingSegment segment, RodBehaviour rod, GameFactory.RodSlot slot, ConnectedBodiesSystem sim)
			: base(PhyObjectType.Rod, sim)
		{
			RodSegmentConfig rodSegmentConfig = RodObject.DecodeRodTemplate(segment.action);
			RodSegmentConfig quiver = RodSegmentConfig.Quiver;
			if (segment.quiverLength > segment.rodLength)
			{
				segment.quiverLength = segment.rodLength;
			}
			float num = segment.rodLength;
			if (rod.IsQuiver)
			{
				num -= segment.quiverLength;
			}
			int num2 = rodSegmentConfig.Config.Length;
			int num3 = 0;
			float num4 = 1f;
			float num5 = 1f;
			if (rod.IsQuiver)
			{
				num3 = quiver.Config.Length;
				num4 = quiver.GetNormalizeLengthFactor();
				num5 = quiver.GetNormalizeMassFactor();
			}
			this.PointsCount = num2 + num3;
			Vector3 forward = segment.firstTransform.forward;
			Mass mass = null;
			float num6 = segment.curveTest / rodSegmentConfig.CurveTest;
			float num7 = segment.quiverTest / quiver.CurveTest;
			float normalizeLengthFactor = rodSegmentConfig.GetNormalizeLengthFactor();
			float normalizeMassFactor = rodSegmentConfig.GetNormalizeMassFactor();
			float num8 = 0f;
			for (int i = 0; i < this.PointsCount; i++)
			{
				bool flag = i < num2;
				SegmentConfig segmentConfig;
				float num9;
				float num10;
				if (flag)
				{
					segmentConfig = rodSegmentConfig.Config[i];
					num9 = num * segmentConfig.SegmentLength * normalizeLengthFactor;
					num10 = segment.weight * segmentConfig.RelativeMass * normalizeMassFactor;
				}
				else
				{
					segmentConfig = quiver.Config[i - num2];
					num9 = segment.quiverLength * segmentConfig.SegmentLength * num4;
					num10 = segment.quiverMass * segmentConfig.RelativeMass * num5;
				}
				num8 += num9;
				Vector3 vector = segment.firstTransform.position + forward.normalized * num8;
				Mass mass2 = new Mass(this.Sim, num10, vector, Mass.MassType.Rod);
				base.Masses.Add(mass2);
				if (mass != null)
				{
					Bend bend = new Bend(mass, mass2, segmentConfig.BendConstant * ((!flag) ? num7 : num6), 5000f, segmentConfig.FrictionConstant, new Vector3(0f, 0f, num9), num9);
					base.Connections.Add(bend);
				}
				mass = mass2;
			}
			this.TipMass = mass;
			this.RootMass = base.Masses[0];
			this.RootMass.Rotation = segment.firstTransform.rotation;
			if (num2 < this.PointsCount)
			{
				this.RodMass = base.Masses[num2 - 1];
			}
			else
			{
				this.RodMass = base.Masses[num2 - 2];
			}
			this.RootKinematicSpline = new KinematicSpline(this.RootMass, true);
			base.Connections.Add(this.RootKinematicSpline);
			if (slot.Bell != null)
			{
				BellController controller = slot.Bell.Controller;
				int num11 = controller.BellPosFromTip;
				if (num11 < 1 || num11 > base.Masses.Count)
				{
					num11 = 1;
				}
				Mass mass3 = base.Masses[base.Masses.Count - num11];
				slot.Bell.SetPhyBell(mass3);
			}
		}

		public RodObject(ConnectedBodiesSystem sim, RodObject source)
			: base(sim, source)
		{
			this.TipMass = source.TipMass;
			this.RootMass = source.RootMass;
			this.RootKinematicSpline = source.RootKinematicSpline;
			this.PointsCount = source.PointsCount;
			this.lineTension = source.lineTension;
		}

		public int PointsCount { get; protected set; }

		public Mass RootMass { get; protected set; }

		public Mass RodMass { get; protected set; }

		public Mass TipMass { get; protected set; }

		public KinematicSpline RootKinematicSpline { get; protected set; }

		public float TipDivergence
		{
			get
			{
				if (this.RootMass == null || this.RodMass == null || this.TipMass == null)
				{
					return 0f;
				}
				Vector3 vector = this.TipMass.Position - this.RootMass.Position;
				return (Vector3.Project(vector, this.RodMass.Position - this.RootMass.Position) - vector).magnitude;
			}
		}

		public float LineTension
		{
			get
			{
				return this.lineTension;
			}
		}

		public override void Sync(PhyObject source)
		{
			base.Sync(source);
			this.UpdateReferences();
		}

		public override void UpdateReferences()
		{
			base.UpdateReferences();
			if (this.TipMass != null)
			{
				this.TipMass = this.Sim.DictMasses[this.TipMass.UID];
			}
			if (this.RootKinematicSpline != null)
			{
				this.RootKinematicSpline = this.Sim.DictConnections[this.RootKinematicSpline.UID] as KinematicSpline;
			}
		}

		public override void SyncMain(PhyObject source)
		{
			base.SyncMain(source);
			RodObject rodObject = source as RodObject;
			if (rodObject != null)
			{
				this.lineTension = rodObject.lineTension;
				rodObject.lineTension = 0f;
			}
		}

		public override void Simulate(float dt)
		{
			base.Simulate(dt);
			this.RootKinematicSpline.PostponedSolve();
			if (this.TipMass != null && this.TipMass.NextSpring != null)
			{
				float tension = this.TipMass.NextSpring.Tension;
				if (this.lineTension < tension)
				{
					this.lineTension = tension;
				}
			}
		}

		public static RodSegmentConfig DecodeRodTemplate(float action)
		{
			if (action == 0.7f)
			{
				return RodSegmentConfig.Fast;
			}
			if (action == 0.5f)
			{
				return RodSegmentConfig.Moderate;
			}
			if (action == 0.3f)
			{
				return RodSegmentConfig.Slow;
			}
			return RodSegmentConfig.Fast;
		}

		protected float lineTension;
	}
}
