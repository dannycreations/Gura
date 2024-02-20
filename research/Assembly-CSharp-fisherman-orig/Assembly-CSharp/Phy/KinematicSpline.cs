using System;
using UnityEngine;

namespace Phy
{
	public sealed class KinematicSpline : ConnectionBase
	{
		public KinematicSpline(Mass mass, bool isPassive = false)
			: base(mass, mass, -1)
		{
			this.bezierCurve = new BezierCurve(2);
			this.nextPoint = mass.Position;
			this.prevPoint = mass.Position;
			for (int i = 0; i <= this.bezierCurve.Order; i++)
			{
				this.bezierCurve.AnchorPoints[i] = this.nextPoint;
			}
			this.InertiaFactor = 0.1f;
			this.CurrentRotationDelta = Quaternion.identity;
			this.AccumulatedRotationDelta = Quaternion.identity;
			this.prevRotation = mass.Rotation;
			this.nextRotation = mass.Rotation;
			this.prevDeltaPeriodPoint = mass.Position;
			this.prevDeltaPeriodRotation = mass.Rotation;
			this.IsPassive = isPassive;
		}

		public KinematicSpline(Simulation sim, KinematicSpline source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass1.UID], source.UID)
		{
			this.bezierCurve = new BezierCurve(2);
			this.Sync(source);
			this.CurrentRotationDelta = Quaternion.identity;
			this.AccumulatedRotationDelta = Quaternion.identity;
			this.prevDeltaPeriodPoint = this.nextPoint;
			this.prevDeltaPeriodRotation = this.nextRotation;
		}

		public Vector3 CurrentPositionDelta { get; private set; }

		public Quaternion CurrentRotationDelta { get; private set; }

		public Vector3 AccumulatedPositionDelta { get; private set; }

		public Quaternion AccumulatedRotationDelta { get; private set; }

		public Quaternion CurrentRotation { get; private set; }

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			KinematicSpline kinematicSpline = source as KinematicSpline;
			this.nextPoint = kinematicSpline.nextPoint;
			this.prevPoint = this._mass1.Position;
			this.startVelocity = kinematicSpline.startVelocity;
			this.finalVelocity = kinematicSpline.finalVelocity;
			for (int i = 0; i <= this.bezierCurve.Order; i++)
			{
				this.bezierCurve.AnchorPoints[i] = kinematicSpline.bezierCurve.AnchorPoints[i];
			}
			this.InertiaFactor = kinematicSpline.InertiaFactor;
			this.IsPassive = kinematicSpline.IsPassive;
			this.prevRotation = kinematicSpline.prevRotation;
			this.nextRotation = kinematicSpline.nextRotation;
		}

		public void SetNextPointAndRotation(Vector3 point, Quaternion rotation)
		{
			this.nextPoint = point;
			this.bezierCurve.SetT(1f);
			Vector3 vector = this.bezierCurve.Derivative();
			this.bezierCurve.SetT(0f);
			this.bezierCurve.AnchorPoints[0] = this._mass1.Position;
			this.bezierCurve.AnchorPoints[1] = (this._mass1.Position + this.nextPoint) * 0.5f;
			this.bezierCurve.AnchorPoints[2] = this.nextPoint;
			this.prevPoint = this._mass1.Position;
			this.prevRotation = this.nextRotation;
			this.nextRotation = rotation;
			this.startVelocity = this._mass1.Velocity;
			this.finalVelocity = 2f * (this.nextPoint - this.prevPoint) / Time.deltaTime - this.startVelocity;
			if (base.Mass1.Sim.PhyActionsListener != null)
			{
				base.Mass1.Sim.PhyActionsListener.ConnectionNeedSyncMark(base.UID);
			}
		}

		public void Stop()
		{
			this.prevPoint = this.nextPoint;
			this.prevRotation = this.nextRotation;
			this.startVelocity = Vector3.zero;
			this.finalVelocity = Vector3.zero;
			this.prevDeltaPeriodPoint = this.nextPoint;
			this.prevDeltaPeriodRotation = this.nextRotation;
			this.CurrentPositionDelta = Vector3.zero;
			if (base.Mass1.Sim.PhyActionsListener != null)
			{
				base.Mass1.Sim.PhyActionsListener.ConnectionNeedSyncMark(base.UID);
			}
		}

		public override void Solve()
		{
			if (!base.Mass1._isKinematic)
			{
				return;
			}
			float num = (float)this._mass1.Sim.NumOfIterations * 0.0004f;
			float frameProgress = this._mass1.Sim.FrameProgress;
			Vector3 position = base.Mass1.Position;
			Quaternion currentRotation = this.CurrentRotation;
			this.newpos = Vector3.Lerp(this.prevPoint, this.nextPoint, frameProgress);
			this.CurrentRotation = Quaternion.Slerp(this.prevRotation, this.nextRotation, this._mass1.Sim.FrameProgress);
			if (!this.IsPassive)
			{
				base.Mass1.Position = this.newpos;
				base.Mass1.Rotation = this.CurrentRotation;
				base.Mass1.Velocity = (base.Mass1.Position - position) / 0.0004f;
			}
			this.CurrentPositionDelta = this.newpos - position;
			this.CurrentRotationDelta = this.CurrentRotation * Quaternion.Inverse(currentRotation);
			if (base.Mass1.Sim.FrameIteration % 3 == 0)
			{
				this.AccumulatedPositionDelta = this.newpos - this.prevDeltaPeriodPoint;
				this.AccumulatedRotationDelta = this.CurrentRotation * Quaternion.Inverse(this.prevDeltaPeriodRotation);
				this.prevDeltaPeriodPoint = this.newpos;
				this.prevDeltaPeriodRotation = this.CurrentRotation;
			}
		}

		public void PostponedSolve()
		{
			if (this.IsPassive)
			{
				base.Mass1.Velocity = (this.newpos - base.Mass1.Position) / 0.0004f;
				base.Mass1.Position = this.newpos;
				base.Mass1.Rotation = this.CurrentRotation;
			}
		}

		public const int DeltaAccumPeriod = 3;

		public float InertiaFactor;

		public bool IsPassive;

		protected Vector3 nextPoint;

		protected Vector3 prevPoint;

		protected Quaternion nextRotation;

		protected Quaternion prevRotation;

		protected BezierCurve bezierCurve;

		private Vector3 newpos;

		private Vector3 prevDeltaPeriodPoint;

		private Quaternion prevDeltaPeriodRotation;

		protected Vector3 finalVelocity;

		protected Vector3 startVelocity;
	}
}
