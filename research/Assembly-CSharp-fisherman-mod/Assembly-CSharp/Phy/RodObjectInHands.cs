using System;
using Mono.Simd;
using Mono.Simd.Math;

namespace Phy
{
	public class RodObjectInHands : RodObject
	{
		public RodObjectInHands(BendingSegment segment, RodBehaviour rod, GameFactory.RodSlot slot, ConnectedBodiesSystem sim)
			: base(segment, rod, slot, sim)
		{
			this.frsim = this.Sim as FishingRodSimulation;
		}

		public RodObjectInHands(ConnectedBodiesSystem sim, RodObjectInHands source)
			: base(sim, source)
		{
			this.frsim = this.Sim as FishingRodSimulation;
			base.RootKinematicSpline = source.RootKinematicSpline;
		}

		public override void Sync(PhyObject source)
		{
			base.Sync(source);
			this.UpdateReferences();
		}

		public override void UpdateReferences()
		{
			base.UpdateReferences();
			if (this.HandKinematicSpline != null)
			{
				this.HandKinematicSpline = this.Sim.DictConnections[this.HandKinematicSpline.UID] as KinematicSpline;
			}
		}

		public override void Simulate(float dt)
		{
			base.Simulate(dt);
			if (base.RootKinematicSpline != null)
			{
				if (this.HandKinematicSpline != null)
				{
					this.HandKinematicSpline.PostponedSolve();
				}
				if (this.Sim.FrameIteration % 3 != 0)
				{
					return;
				}
				Vector4f vector4f = Vector4f.Zero;
				Vector4f vector4f2 = base.RootKinematicSpline.AccumulatedPositionDelta.AsPhyVector(ref vector4f) * FishingRodSimulation.RodTranslateCompensation4f;
				Vector4f position4f = base.Masses[0].Position4f;
				for (int i = 1; i < base.Masses.Count; i++)
				{
					Mass mass = base.Masses[i];
					Vector4f vector4f3 = mass.Position4f - position4f;
					if (!mass.IsLying)
					{
						vector4f = vector4f2;
						mass.Rotation = base.RootKinematicSpline.CurrentRotation;
						Vector4f vector4f4 = (base.RootKinematicSpline.AccumulatedRotationDelta * vector4f3.AsVector3()).AsPhyVector(ref vector4f3);
						vector4f += (vector4f4 - vector4f3) * FishingRodSimulation.RodRotationCompensation4f;
						mass.Position4f += vector4f;
					}
				}
			}
		}

		private FishingRodSimulation frsim;

		public KinematicSpline HandKinematicSpline;
	}
}
