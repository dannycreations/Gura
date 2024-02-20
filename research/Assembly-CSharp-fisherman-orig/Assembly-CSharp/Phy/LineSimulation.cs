using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phy
{
	public class LineSimulation : ConnectedBodiesSystem
	{
		public LineSimulation()
			: base("LineSimulation")
		{
			this.Objects = new List<PhyObject>();
		}

		public new List<PhyObject> Objects { get; private set; }

		public Mass LineTipMass { get; private set; }

		public Vector3 RodAppliedForce { get; private set; }

		public Vector3 TackleAppliedForce { get; private set; }

		public new void Clear()
		{
			base.Masses.Clear();
			base.Connections.Clear();
			this.Objects.Clear();
		}

		public void AddLine(float lineLength)
		{
			PhyObject phyObject = new PhyObject(PhyObjectType.Line, GameFactory.Player.RodSlot.Sim);
			this.lineSegsCount = (int)(lineLength / 0.05f) + 1;
			Mass mass = null;
			for (int i = 0; i < this.lineSegsCount; i++)
			{
				Mass mass2 = new Mass(GameFactory.Player.RodSlot.Sim, 0.001f, this.LinePosition - new Vector3(0f, 0.05f * (float)i, 0f), Mass.MassType.Unknown);
				base.Masses.Add(mass2);
				phyObject.Masses.Add(mass2);
				if (mass != null)
				{
					Spring spring = new Spring(mass, mass2, 5000f, 0.05f, 0.2f);
					base.Connections.Add(spring);
					phyObject.Connections.Add(spring);
				}
				mass = mass2;
			}
			this.LineTipMass = mass;
			this.Objects.Add(phyObject);
		}

		public void AddLure(float size, float mass)
		{
			PhyObject phyObject = new PhyObject(PhyObjectType.Lure, GameFactory.Player.RodSlot.Sim);
			Mass mass2 = new Mass(GameFactory.Player.RodSlot.Sim, mass / 2f, this.LineTipMass.Position, Mass.MassType.Unknown);
			base.Masses.Add(mass2);
			phyObject.Masses.Add(mass2);
			Spring spring = new Spring(this.LineTipMass, mass2, 5000f, 0f, 0.2f);
			base.Connections.Add(spring);
			phyObject.Connections.Add(spring);
			Mass mass3 = new Mass(GameFactory.Player.RodSlot.Sim, mass / 2f, this.LineTipMass.Position - new Vector3(0f, size, 0f), Mass.MassType.Unknown);
			base.Masses.Add(mass3);
			phyObject.Masses.Add(mass3);
			Spring spring2 = new Spring(mass2, mass3, 5000f, size, 0.2f);
			base.Connections.Add(spring2);
			phyObject.Connections.Add(spring2);
			this.Objects.Add(phyObject);
		}

		public void ResetTransformCycle()
		{
			this.currentTime = 0f;
			this.initialLinePosition = this.LinePosition;
			PhyObject phyObject = this.Objects[0];
			phyObject.Masses[0].ResetAvgForce();
			phyObject.Masses[phyObject.Masses.Count - 1].ResetAvgForce();
		}

		protected override void Simulate(float dt)
		{
			base.Simulate(dt);
			this.currentTime += dt;
			float num = this.currentTime / this.TotalTime;
			this.LinePosition = Vector3.Lerp(this.initialLinePosition, this.FinalLinePosition, num);
			base.Masses[0].Position = this.LinePosition;
			base.Masses[0].Velocity = Vector3.zero;
		}

		public override void Update(float dt)
		{
			this.TotalTime = dt;
			base.Update(dt);
			PhyObject phyObject = this.Objects[0];
			this.RodAppliedForce = phyObject.Masses[0].AvgForce;
			this.TackleAppliedForce = phyObject.Masses[phyObject.Masses.Count - 1].AvgForce;
		}

		public const float LineSpringConstant = 5000f;

		public const float LineFrictionConstant = 0.2f;

		public const float InitialLineSegmentLength = 0.05f;

		public const float LineSegmentMass = 0.001f;

		public Vector3 LinePosition;

		private float currentTime;

		private Vector3 initialLinePosition;

		public Vector3 FinalLinePosition;

		public float TotalTime;

		public float CurrentLineLength;

		public float CurrentLineLengthSpeed;

		private int lineSegsCount;
	}
}
