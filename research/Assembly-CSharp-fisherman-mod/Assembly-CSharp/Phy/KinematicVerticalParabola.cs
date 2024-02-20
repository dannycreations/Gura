using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public sealed class KinematicVerticalParabola : ConnectionBase
	{
		public KinematicVerticalParabola(Mass mass, Mass compensatorMass, float duration, float timePower, VerticalParabola parabola)
			: base(mass, compensatorMass, -1)
		{
			this.parabola = parabola;
			this.startTime = mass.Sim.InternalTime;
			this.duration = duration;
			this.timePower = timePower;
		}

		public KinematicVerticalParabola(Simulation sim, KinematicVerticalParabola source)
			: base(sim.DictMasses[source.Mass1.UID], sim.DictMasses[source.Mass2.UID], source.UID)
		{
			this.parabola = new VerticalParabola(source.Mass1.Position, source.parabola.FinishPoint, source.parabola.MidPointY);
			this.startTime = 0f;
			this.Sync(source);
		}

		public override void Sync(ConnectionBase source)
		{
			base.Sync(source);
			KinematicVerticalParabola kinematicVerticalParabola = source as KinematicVerticalParabola;
			this.DestinationPoint = kinematicVerticalParabola.DestinationPoint;
			this.prevDestinationPoint = this.DestinationPoint;
			this.duration = kinematicVerticalParabola.duration;
			this.timePower = kinematicVerticalParabola.timePower;
		}

		public float TimeSpent
		{
			get
			{
				return base.Mass1.Sim.InternalTime - this.startTime;
			}
		}

		public float Progress
		{
			get
			{
				return Mathf.Pow((base.Mass1.Sim.InternalTime - this.startTime) / this.duration, this.timePower);
			}
		}

		public float Duration
		{
			get
			{
				return this.duration;
			}
		}

		public override void Solve()
		{
			if (!base.Mass1._isKinematic)
			{
				return;
			}
			if (this.startTime == 0f)
			{
				this.startTime = base.Mass1.Sim.InternalTime;
			}
			if (base.Mass1.Sim.InternalTime > this.startTime + this.duration)
			{
				return;
			}
			float progress = this.Progress;
			Vector3 position = base.Mass1.Position;
			Vector3 vector = base.Mass2.Position - position;
			vector.y = 0f;
			vector *= KinematicVerticalParabola.CompensationFactor * Mathf.Pow(this.Progress, 0.2f);
			float magnitude = vector.magnitude;
			if (magnitude > KinematicVerticalParabola.CompensationMax)
			{
				vector = vector.normalized * KinematicVerticalParabola.CompensationMax;
			}
			Vector3 point = this.parabola.GetPoint(progress);
			Vector3 vector2 = Vector3.Lerp(this.prevDestinationPoint, this.DestinationPoint, base.Mass1.Sim.FrameProgress);
			Vector3 vector3 = position;
			Vector3 vector4 = Vector3.Lerp(point + vector, vector2, progress * progress * progress * progress);
			if (progress < 0.5f || position.y >= base.Mass1.GroundHeight)
			{
				base.Mass1.Position = vector4;
				base.Mass1.Velocity4f = (vector4 - vector3).AsPhyVector(ref base.Mass1.Velocity4f) / Simulation.TimeQuant4f;
				float num = base.Mass1.Velocity4f.Magnitude();
				if (num > base.Mass1.CurrentVelocityLimit && base.Mass1.CurrentVelocityLimit > 0f)
				{
					base.Mass1.Velocity4f *= new Vector4f(base.Mass1.CurrentVelocityLimit / num);
				}
			}
		}

		public VerticalParabola parabola;

		public Vector3 DestinationPoint;

		private Vector3 prevDestinationPoint;

		private float duration;

		private float startTime;

		private float timePower;

		private static readonly float CompensationFactor = 0.8f;

		private static readonly float CompensationMax = 0.5f;
	}
}
