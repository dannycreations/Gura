using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy.Verlet
{
	public abstract class AbstractFishBody : VerletObject, IFishObject
	{
		public AbstractFishBody(ConnectedBodiesSystem sim, float totalMass)
			: base(PhyObjectType.Fish, sim)
		{
			this.TotalMass = totalMass;
		}

		public AbstractFishBody(ConnectedBodiesSystem sim, AbstractFishBody source)
			: base(sim, source)
		{
			this.TotalMass = source.TotalMass;
			this.Edge = source.Edge;
		}

		public float TotalMass { get; private set; }

		public abstract Mass Mouth { get; }

		public abstract Mass Root { get; }

		public abstract float UnderwaterRatio { get; }

		public VerletFishThrustController thrust { get; protected set; }

		public AbstractFishBody.HookStateEnum HookState
		{
			get
			{
				return this._hookState;
			}
			set
			{
				if (this.Sim.PhyActionsListener != null)
				{
					this.Sim.PhyActionsListener.FishIsHookedChanged(base.UID, (int)value);
				}
				this._hookState = value;
			}
		}

		public void ComputeWaterDrag(float NominalForce, float MaxSpeed)
		{
			float num = NominalForce / (MaxSpeed * MaxSpeed * this.TotalMass);
			for (int i = 0; i < base.Masses.Count; i++)
			{
				base.Masses[i].WaterDragConstant = num;
			}
		}

		public float ComputeSpeedForce(float targetSpeed)
		{
			return targetSpeed * targetSpeed * this.TotalMass * base.Masses[0].WaterDragConstant;
		}

		public void SetThrust(Vector3 force, bool reverse)
		{
			this.thrust.Force = force.AsPhyVector(ref this.Mouth.Position4f);
			this.thrust.Reverse = reverse;
		}

		public abstract void SetLocomotionWaveMult(float mult);

		public abstract void SetBendParameters(float maxAngle, float reboundPoint, float stiffnessMult);

		public abstract void VisualChordRotation(float newChordRotation);

		public abstract float TetrahedronHeight { get; }

		public float Edge { get; protected set; }

		public float ChordRotation { get; protected set; }

		public abstract void RollStabilizer(Vector3 destUp, float intensity = 1f);

		public abstract void DebugDraw();

		public abstract float GetSegmentAxialOrientation(int segment);

		public abstract Vector3 GetSegmentRight(int segment);

		public abstract Vector3 GetSegmentUp(int segment);

		public abstract Vector3 GetSegmentBendAxis(int segment);

		public abstract Vector3 GetSegmentBendDirection(int segment);

		public abstract void StartBendStrain(float stiffnessMult = -1f);

		public abstract void StopBendStrain();

		public abstract Vector3[] GetIdealTetrahedronPositions(Vector3 baseCenter, Vector3 right, Vector3 up, Vector3 forward);

		public abstract void groundAlign(ref Vector4f currentTranslate);

		public abstract Mass ReplaceFirstMass(Mass newMass);

		private AbstractFishBody.HookStateEnum _hookState;

		public float BendStiffnessMultiplier;

		public enum HookStateEnum
		{
			NotHooked,
			HookedSwimming,
			HookedShowing
		}
	}
}
