using System;
using System.Linq;
using UnityEngine;

namespace Phy
{
	public class RodAndLineSimulation : ConnectedBodiesSystem
	{
		public RodAndLineSimulation(Vector3 rodPosition, float rodLength, float rodSegmentLength, float rodMass, float lineLength, float lineSegmentLength, float lineSegmentMass)
			: base("RodAndLineSimulation")
		{
			this.RodPosition = rodPosition;
			this.RodVelocity = Vector3.zero;
			this.RodRotation = Quaternion.Euler(0f, 0f, -60f);
			this.RodAngularVelocity = Vector3.zero;
			this.CurrentLineLength = lineLength;
			this.lineSegmentLength = lineSegmentLength;
			this.lineSegmentMass = lineSegmentMass;
			this.RodSegCount = (int)(rodLength / rodSegmentLength) + 2;
			float num = 0f;
			Mass mass = null;
			for (int i = 0; i < this.RodSegCount; i++)
			{
				float segmentRelativeMass = this.GetSegmentRelativeMass(num, rodLength);
				float num2 = 100000f * this.GetSegmentRelativeSpring(num, rodLength);
				Mass mass2 = new Mass(GameFactory.Player.RodSlot.Sim, segmentRelativeMass, rodPosition + this.RodRotation * new Vector3(0f, rodSegmentLength * (float)i, 0f), Mass.MassType.Unknown);
				base.Masses.Add(mass2);
				if (i > 0)
				{
					base.Connections.Add(new Bend(mass, mass2, num2, num2, 4f, new Vector3(0f, rodSegmentLength, 0f), rodSegmentLength));
				}
				mass = mass2;
				num += rodSegmentLength;
			}
			this.rodTipMass = mass;
			base.Masses.First<Mass>().Rotation = this.RodRotation;
			Vector3 vector = rodPosition + this.RodRotation * new Vector3(0f, rodSegmentLength * (float)this.RodSegCount, 0f);
			this.lineSegsCount = (int)(lineLength / lineSegmentLength) + 1;
			bool flag = true;
			for (int j = 1; j <= this.lineSegsCount; j++)
			{
				Mass mass3 = new Mass(GameFactory.Player.RodSlot.Sim, lineSegmentMass, vector + new Vector3(0f, 0f, lineSegmentLength * (float)j), Mass.MassType.Unknown);
				base.Masses.Add(mass3);
				Spring spring = new Spring(mass, mass3, 5000f, lineSegmentLength, 2f);
				base.Connections.Add(spring);
				mass = mass3;
				if (flag)
				{
					this.FirstLineMassIndex = base.Masses.Count - 1;
					this.firstLineConnectionIndex = base.Connections.Count - 1;
					this.firstLineMass = mass;
					this.firstLineConnection = spring;
					flag = false;
				}
			}
		}

		public Vector3 RodPosition { get; set; }

		public Vector3 RodVelocity { get; set; }

		public Quaternion RodRotation { get; set; }

		public Vector3 RodAngularVelocity { get; set; }

		public int RodSegCount { get; private set; }

		public float CurrentLineLength { get; set; }

		public float CurrentLineLengthSpeed { get; set; }

		public int FirstLineMassIndex { get; private set; }

		private float GetSegmentRelativeMass(float currentLength, float rodLength)
		{
			float num = currentLength / rodLength;
			return 0.324f / Mathf.Tan(num + 0.3f);
		}

		private float GetSegmentRelativeSpring(float currentLength, float rodLength)
		{
			float num = currentLength / rodLength;
			return 0.324f / Mathf.Tan(num + 0.3f);
		}

		protected override void Simulate(float dt)
		{
			base.Simulate(dt);
			this.RodPosition += this.RodVelocity * dt;
			if (this.RodPosition.y < 0f)
			{
				this.RodPosition = new Vector3(this.RodPosition.x, 0f, this.RodPosition.z);
				this.RodVelocity = new Vector3(this.RodVelocity.x, 0f, this.RodVelocity.z);
			}
			base.Masses[0].Position = this.RodPosition;
			base.Masses[0].Velocity = this.RodVelocity;
			if (this.RodAngularVelocity != Vector3.zero)
			{
				this.RodRotation = Quaternion.Euler(this.RodRotation.eulerAngles + this.RodAngularVelocity * dt);
				base.Masses[0].Rotation = this.RodRotation;
			}
			if (this.CurrentLineLengthSpeed != 0f)
			{
				float num = this.CurrentLineLengthSpeed * dt;
				if (this.CurrentLineLength + num < 2f)
				{
					num = this.CurrentLineLength - 2f;
				}
				float num2 = num * 0.5f;
				float num3 = num * 0.5f;
				float num4 = num3 / (float)(this.lineSegsCount - 1);
				this.lineSegmentLength += num4;
				for (int i = this.firstLineConnectionIndex + 1; i < base.Connections.Count; i++)
				{
					Spring spring = (Spring)base.Connections[i];
					spring.SpringLength = this.lineSegmentLength;
				}
				if (this.firstLineConnection.SpringLength + num2 < 0f)
				{
					float num5 = -(this.firstLineConnection.SpringLength + num2);
					this.RemoveSegment(num5);
				}
				else if (this.firstLineConnection.SpringLength + num2 > this.lineSegmentLength)
				{
					float num6 = this.firstLineConnection.SpringLength + num2 - this.lineSegmentLength;
					this.AddSegment(num6);
				}
				else
				{
					this.firstLineConnection.SpringLength += num2;
				}
				this.CurrentLineLength += num;
			}
		}

		public override void SetVelocity(Vector3 velocity)
		{
			this.RodVelocity = velocity;
		}

		public override void SetAngularVelocity(Vector3 velocity)
		{
			this.RodAngularVelocity = velocity;
		}

		public void SetLineLengthSpeed(float speed)
		{
			this.CurrentLineLengthSpeed = speed;
		}

		private void AddSegment(float extendExcess)
		{
			Vector3 vector = this.rodTipMass.Position + (this.firstLineMass.Position - this.rodTipMass.Position).normalized * extendExcess;
			Mass mass = new Mass(GameFactory.Player.RodSlot.Sim, this.lineSegmentMass, vector, Mass.MassType.Unknown)
			{
				Buoyancy = this.firstLineMass.Buoyancy,
				Motor = this.firstLineMass.Motor,
				Velocity = this.firstLineMass.Velocity
			};
			Spring spring = new Spring(this.rodTipMass, mass, 5000f, extendExcess, 2f);
			this.firstLineConnection.Mass1 = mass;
			this.firstLineConnection.SpringLength = this.lineSegmentLength;
			this.firstLineConnection = spring;
			this.firstLineMass = mass;
			base.Masses.Insert(this.FirstLineMassIndex, mass);
			base.Connections.Insert(this.firstLineConnectionIndex, spring);
			this.RefreshObjectArrays(true);
			this.lineSegsCount++;
		}

		private void RemoveSegment(float shrinkExcess)
		{
			float num = this.lineSegmentLength - shrinkExcess;
			Mass mass = base.Masses[this.FirstLineMassIndex + 1];
			Spring spring = (Spring)base.Connections[this.firstLineConnectionIndex + 1];
			spring.Mass1 = this.rodTipMass;
			spring.SpringLength = num;
			this.firstLineConnection = spring;
			this.firstLineMass = mass;
			base.Masses.RemoveAt(this.FirstLineMassIndex);
			base.Connections.RemoveAt(this.firstLineConnectionIndex);
			this.RefreshObjectArrays(true);
			this.lineSegsCount--;
		}

		public const float RodSpringConstant = 100000f;

		public const float LineSpringConstant = 5000f;

		public const float RodFrictionConstant = 4f;

		public const float LineFrictionConstant = 2f;

		public const float MinLineLength = 2f;

		private const float LodFraction = 0.5f;

		private float lineSegmentLength;

		private readonly float lineSegmentMass;

		private readonly int firstLineConnectionIndex;

		private readonly Mass rodTipMass;

		private Mass firstLineMass;

		private Spring firstLineConnection;

		private int lineSegsCount;
	}
}
