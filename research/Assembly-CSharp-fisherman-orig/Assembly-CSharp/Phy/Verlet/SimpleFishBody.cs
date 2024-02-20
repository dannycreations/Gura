using System;
using Mono.Simd;
using UnityEngine;

namespace Phy.Verlet
{
	public class SimpleFishBody : AbstractFishBody
	{
		public SimpleFishBody(ConnectedBodiesSystem sim, float totalMass, Vector3 mouthPosition, Vector3 chordDirection, float length)
			: base(sim, totalMass)
		{
			VerletMass verletMass = new VerletMass(sim, totalMass * 0.5f, mouthPosition, Mass.MassType.Fish);
			VerletMass verletMass2 = new VerletMass(sim, totalMass * 0.5f, mouthPosition + chordDirection * length, Mass.MassType.Fish);
			verletMass.Buoyancy = 0f;
			verletMass.StopMass();
			verletMass.IgnoreEnvironment = true;
			verletMass.Collision = Mass.CollisionType.Full;
			verletMass.StaticFrictionFactor = 0.01f;
			verletMass.SlidingFrictionFactor = 0.01f;
			verletMass2.Buoyancy = 0f;
			verletMass2.StopMass();
			verletMass2.IgnoreEnvironment = true;
			verletMass2.Collision = Mass.CollisionType.Full;
			verletMass2.StaticFrictionFactor = 0.01f;
			verletMass2.SlidingFrictionFactor = 0.01f;
			VerletSpring verletSpring = new VerletSpring(verletMass, verletMass2, length, 0.1f, false);
			base.Masses.Add(verletMass);
			base.Masses.Add(verletMass2);
			base.Connections.Add(verletSpring);
			base.thrust = new VerletFishThrustController(this);
			base.Connections.Add(base.thrust);
			this.UpdateSim();
			Vector4f zero = Vector4f.Zero;
			this.groundAlign(ref zero);
			this.KinematicTranslate(zero);
		}

		public SimpleFishBody(ConnectedBodiesSystem sim, SimpleFishBody source)
			: base(sim, source)
		{
			this.BendStiffnessMultiplier = source.BendStiffnessMultiplier;
		}

		public override Mass Root
		{
			get
			{
				return base.Masses[1];
			}
		}

		public override Mass Mouth
		{
			get
			{
				return base.Masses[0];
			}
		}

		public override void SetLocomotionWaveMult(float mult)
		{
		}

		public override void SetBendParameters(float maxAngle, float reboundPoint, float stiffnessMult)
		{
		}

		public override void VisualChordRotation(float newChordRotation)
		{
		}

		public override void RollStabilizer(Vector3 destUp, float intensity = 1f)
		{
		}

		public override void DebugDraw()
		{
		}

		public override float GetSegmentAxialOrientation(int segment)
		{
			return 0f;
		}

		public override float TetrahedronHeight
		{
			get
			{
				return 0f;
			}
		}

		public Vector3 ChordDirection
		{
			get
			{
				return (this.Mouth.Position - this.Root.Position).normalized;
			}
		}

		public override Vector3 GetSegmentRight(int segment)
		{
			return Vector3.Cross(this.ChordDirection, Vector3.up).normalized;
		}

		public override Vector3 GetSegmentUp(int segment)
		{
			return Vector3.up;
		}

		public override Vector3 GetSegmentBendAxis(int segment)
		{
			return Vector3.zero;
		}

		public override Vector3 GetSegmentBendDirection(int segment)
		{
			return Vector3.zero;
		}

		public override void StartBendStrain(float stiffnessMult = -1f)
		{
		}

		public override void StopBendStrain()
		{
		}

		public override Vector3[] GetIdealTetrahedronPositions(Vector3 baseCenter, Vector3 right, Vector3 up, Vector3 forward)
		{
			return new Vector3[0];
		}

		public override float UnderwaterRatio
		{
			get
			{
				return Mathf.Clamp01(1f - 10f * (this.Root.Position.y + this.Mouth.Position.y));
			}
		}

		public override void groundAlign(ref Vector4f currentTranslate)
		{
		}

		public override Mass ReplaceFirstMass(Mass newMass)
		{
			newMass.MassValue = base.Masses[0].MassValue;
			newMass.Buoyancy = base.Masses[0].Buoyancy;
			newMass.IsRef = true;
			newMass.Position = base.Masses[0].Position;
			newMass.WaterDragConstant = base.Masses[0].WaterDragConstant;
			newMass.Type = Mass.MassType.Fish;
			newMass.Collision = Mass.CollisionType.Full;
			newMass.SlidingFrictionFactor = 0.01f;
			newMass.StaticFrictionFactor = 0.01f;
			Mass mass = base.Masses[0];
			base.Masses.RemoveAt(0);
			base.Masses.Insert(0, newMass);
			base.Connections[0].SetMasses(newMass, null);
			base.thrust.SetMasses(newMass, null);
			this.Sim.RefreshObjectArrays(true);
			if (this.Sim.PhyActionsListener != null)
			{
				this.Sim.PhyActionsListener.ConnectionNeedSyncMark(base.thrust.UID);
				this.Sim.PhyActionsListener.ObjectNeedSyncMark(base.UID);
			}
			return mass;
		}
	}
}
