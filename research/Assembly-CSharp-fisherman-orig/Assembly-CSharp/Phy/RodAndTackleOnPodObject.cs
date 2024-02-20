using System;
using UnityEngine;

namespace Phy
{
	public class RodAndTackleOnPodObject : RodObject
	{
		public RodAndTackleOnPodObject(BendingSegment segment, RodBehaviour rod, TackleBehaviour tackle, GameFactory.RodSlot slot, ConnectedBodiesSystem sim, RodOnPodBehaviour.TransitionData td)
			: base(segment, rod, slot, sim)
		{
			base.RootMass.IsKinematic = true;
			if (td.tackleType == FishingRodSimulation.TackleType.Float)
			{
				this.tackleMass = this.addMass(td.bobberMass, td.tacklePosition, Mass.MassType.Bobber);
				this.tackleMass.Buoyancy = td.tackleBuoyancy;
				this.tackleMass.WaterDragConstant = 40f;
				Vector3 vector = td.tacklePosition + td.leaderLength * Vector3.down;
				Vector3 tacklePosition = td.tacklePosition;
				Vector3 up = Vector3.up;
				Math3d.GetGroundCollision(tacklePosition, out tacklePosition, out up, 5f);
				if (tacklePosition.y > vector.y)
				{
					vector.y = tacklePosition.y;
				}
				this.hookMass = this.addMass(td.tackleMass - td.bobberMass, vector, Mass.MassType.Hook);
				if (td.tackleMass > 0f)
				{
					this.hookMass.Buoyancy += (1f + td.baitBuoyancy) * td.baitMass / td.tackleMass;
				}
				this.leaderSpring = new Spring(this.tackleMass, this.hookMass, 0f, td.leaderLength, 0.002f);
				base.Masses.Add(this.hookMass);
				base.Connections.Add(this.leaderSpring);
				this.tackleMass.Collision = Mass.CollisionType.ExternalPlane;
				this.hookMass.Collision = Mass.CollisionType.ExternalPlane;
			}
			else if (td.tackleType == FishingRodSimulation.TackleType.Feeder || td.tackleType == FishingRodSimulation.TackleType.CarpClassic || td.tackleType == FishingRodSimulation.TackleType.CarpMethod || td.tackleType == FishingRodSimulation.TackleType.CarpPVABag || td.tackleType == FishingRodSimulation.TackleType.CarpPVAStick)
			{
				float num = 0.001f + td.baitMass;
				this.tackleMass = this.addMass(0.001f, td.tacklePosition, Mass.MassType.Line);
				this.tackleMass.Collision = Mass.CollisionType.ExternalPlane;
				this.hookMass = this.addMass(num, td.tacklePosition, Mass.MassType.Hook);
				this.hookMass.Collision = Mass.CollisionType.ExternalPlane;
				if (td.tackleMass > 0f)
				{
					this.hookMass.Buoyancy += (1f + td.baitBuoyancy) * td.baitMass / num;
				}
				float num2 = td.tackleSize * 0.5f;
				Vector3 tacklePosition2 = td.tacklePosition;
				Vector3 tacklePosition3 = td.tacklePosition;
				Vector3 up2 = Vector3.up;
				Math3d.GetGroundCollision(tacklePosition3, out tacklePosition3, out up2, 5f);
				if (tacklePosition3.y > tacklePosition2.y)
				{
					tacklePosition2.y = tacklePosition3.y;
				}
				this.feederBody = new RigidBody(sim, td.tackleMass, tacklePosition2, Mass.MassType.Feeder, num2)
				{
					Buoyancy = -1f,
					BuoyancySpeedMultiplier = 0f,
					IgnoreEnvironment = false,
					RotationDamping = 0.8f,
					UnderwaterRotationDamping = 0.8f,
					AxialWaterDragFactors = td.axialDragFactors * td.solidFactor,
					AxialWaterDragEnabled = true,
					WaterDragConstant = 0f,
					AirDragConstant = 0.001f * (1f + td.tackleMass * 100f),
					BounceFactor = td.bounceFactor,
					StaticFrictionFactor = td.staticFrictionFactor,
					SlidingFrictionFactor = td.slidingFrictionFactor,
					ExtrudeFactor = td.extrudeFactor,
					Collision = Mass.CollisionType.ExternalPlane
				};
				this.feederBody.GroundPoint = tacklePosition3;
				this.feederBody.GroundPoint = tacklePosition3;
				this.feederBody.GroundNormal = up2;
				this.feederBody.GroundNormal = up2;
				MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(this.tackleMass, this.feederBody, 0f, 0.01f, 0.002f, Vector3.forward * num2);
				this.leaderSpring = new MassToRigidBodySpring(this.hookMass, this.feederBody, 0f, td.leaderLength, 0.002f, Vector3.back * num2);
				base.Connections.Add(this.leaderSpring);
				base.Connections.Add(massToRigidBodySpring);
				base.Masses.Add(this.hookMass);
				base.Masses.Add(this.feederBody);
			}
			else if (td.tackleType == FishingRodSimulation.TackleType.Spod)
			{
				float num3 = 0.001f + td.baitMass;
				this.tackleMass = this.addMass(0.001f, td.tacklePosition, Mass.MassType.Line);
				this.hookMass = this.addMass(num3, td.tacklePosition, Mass.MassType.Hook);
				this.tackleMass.Collision = Mass.CollisionType.ExternalPlane;
				this.hookMass.Collision = Mass.CollisionType.ExternalPlane;
				float num4 = td.tackleSize * 0.5f;
				Vector3 tacklePosition4 = td.tacklePosition;
				Vector3 tacklePosition5 = td.tacklePosition;
				Vector3 up3 = Vector3.up;
				Math3d.GetGroundCollision(tacklePosition5, out tacklePosition5, out up3, 5f);
				if (tacklePosition5.y > tacklePosition4.y)
				{
					tacklePosition4.y = tacklePosition5.y;
				}
				this.feederBody = new RigidBody(sim, td.tackleMass, tacklePosition4, Mass.MassType.Feeder, num4)
				{
					Buoyancy = 0.4f,
					BuoyancySpeedMultiplier = 0f,
					IgnoreEnvironment = false,
					RotationDamping = 0.8f,
					UnderwaterRotationDamping = 0.8f,
					AxialWaterDragFactors = td.axialDragFactors * td.solidFactor,
					AxialWaterDragEnabled = true,
					WaterDragConstant = 0f,
					AirDragConstant = 0.001f * (1f + td.tackleMass * 100f),
					BounceFactor = td.bounceFactor,
					StaticFrictionFactor = td.staticFrictionFactor,
					SlidingFrictionFactor = td.slidingFrictionFactor,
					ExtrudeFactor = td.extrudeFactor,
					Collision = Mass.CollisionType.ExternalPlane
				};
				this.feederBody.GroundPoint = tacklePosition5;
				this.feederBody.GroundPoint = tacklePosition5;
				this.feederBody.GroundNormal = up3;
				this.feederBody.GroundNormal = up3;
				MassToRigidBodySpring massToRigidBodySpring2 = new MassToRigidBodySpring(this.tackleMass, this.feederBody, 0f, 0.01f, 0.002f, Vector3.forward * num4);
				base.Connections.Add(massToRigidBodySpring2);
				base.Masses.Add(this.hookMass);
				base.Masses.Add(this.feederBody);
				Animator component = td.tackleObject.GetComponent<Animator>();
				component.ResetTrigger("Close");
				component.SetTrigger("Open");
			}
			else if (td.tackleType == FishingRodSimulation.TackleType.Wobbler)
			{
				this.tackleMass = this.addMass(0.001f, td.tacklePosition, Mass.MassType.Line);
				this.hookMass = this.addMass(0.001f, td.tacklePosition, Mass.MassType.Hook);
				this.tackleMass.Collision = Mass.CollisionType.ExternalPlane;
				this.hookMass.Collision = Mass.CollisionType.ExternalPlane;
				float num5 = td.tackleMass * 9.81f / (td.tackleSize * td.tackleSize * td.tackleSize * 0.09f * 1000f);
				float num6 = (tackle as ILureBehaviour).LureItem.Wobbler.SinkRate / 5f;
				RigidLure rigidLure = new RigidLure(sim, td.tackleMass, td.tacklePosition, new Vector3(0.01f, 0.01f, td.tackleSize), Mass.MassType.Wobbler)
				{
					Buoyancy = -1f,
					BuoyancySpeedMultiplier = 0f,
					RotationDamping = 0.02f,
					IgnoreEnvironment = false,
					UnderwaterRotationDamping = 0.02f,
					BuoyancyVolumetric = (tackle as ILureBehaviour).LureItem.Buoyancy + 1f,
					WorkingDepth = (tackle as ILureBehaviour).LureItem.Wobbler.WorkingDepth,
					BuoyancyCenter = new Vector3(0.25f * td.tackleSize, 0f, 0f),
					WingletAttack = new Vector3(0f, 0f, num6),
					WingletLift = new Vector3(-num6 * 0.5f, 0f, 0f),
					WingletPosition = new Vector3(0f, 0f, td.tackleSize * 0.5f),
					AxialWaterDragEnabled = true,
					AxialWaterDragFactors = new Vector3(0.01f, 3f, 0.01f)
				};
				Vector3 tacklePosition6 = td.tacklePosition;
				Vector3 tacklePosition7 = td.tacklePosition;
				Vector3 up4 = Vector3.up;
				Math3d.GetGroundCollision(tacklePosition7, out tacklePosition7, out up4, 5f);
				if (tacklePosition7.y > tacklePosition6.y)
				{
					tacklePosition6.y = tacklePosition7.y;
				}
				rigidLure.GroundPoint = tacklePosition7;
				rigidLure.GroundPoint = tacklePosition7;
				rigidLure.GroundNormal = up4;
				rigidLure.GroundNormal = up4;
				MassToRigidBodySpring massToRigidBodySpring3 = new MassToRigidBodySpring(this.tackleMass, rigidLure, 0f, 0.01f, 0.002f, Vector3.forward * td.tackleSize * 0.5f);
				this.leaderSpring = new Spring(this.hookMass, this.tackleMass, 0f, 0.01f, 0.002f);
				base.Connections.Add(this.leaderSpring);
				base.Connections.Add(massToRigidBodySpring3);
				base.Masses.Add(this.hookMass);
				base.Masses.Add(rigidLure);
			}
			else
			{
				this.tackleMass = this.addMass(td.tackleMass, td.tacklePosition, Mass.MassType.Lure);
				this.tackleMass.Buoyancy = td.tackleBuoyancy;
				this.hookMass = this.tackleMass;
				this.tackleMass.Collision = Mass.CollisionType.ExternalPlane;
				this.hookMass.Collision = Mass.CollisionType.ExternalPlane;
			}
			float num7 = RodBehaviour.CorrectLineLengthForPodTransition(td.tacklePosition, base.TipMass.Position, base.RootMass.Position, td.rodLength, td.lineLength);
			this.lineSpring = new Spring(base.TipMass, this.tackleMass, 0f, num7, 0.002f);
			base.Masses.Add(this.tackleMass);
			base.Connections.Add(this.lineSpring);
			for (int i = 0; i < base.Masses.Count; i++)
			{
				base.Masses[i].Collision = Mass.CollisionType.ExternalPlane;
			}
			sim.RefreshObjectArrays(true);
		}

		public RodAndTackleOnPodObject(ConnectedBodiesSystem sim, RodAndTackleOnPodObject source)
			: base(sim, source)
		{
			this.tackleMass = source.tackleMass;
			this.lineSpring = source.lineSpring;
			this.leaderSpring = source.leaderSpring;
			this.hookMass = source.hookMass;
			this.feederBody = source.feederBody;
			this.fishObject = source.fishObject;
			this.fishForce = source.fishForce;
			this.fishToHookSpring = source.fishToHookSpring;
		}

		public Mass tackleMass { get; private set; }

		public Spring lineSpring { get; private set; }

		public Spring leaderSpring { get; private set; }

		public Mass hookMass { get; private set; }

		public RigidBody feederBody { get; private set; }

		private Mass addMass(float mass, Vector3 position, Mass.MassType type)
		{
			Vector3 vector = position;
			Vector3 up = Vector3.up;
			Math3d.GetGroundCollision(vector, out vector, out up, 5f);
			if (vector.y > position.y)
			{
				position.y = vector.y;
			}
			return new Mass(this.Sim, mass, position, type)
			{
				Collision = Mass.CollisionType.ExternalPlane,
				GroundPoint = vector,
				GroundPoint = vector,
				GroundNormal = up,
				GroundNormal = up
			};
		}

		public SimplifiedFishObject fishObject;

		public float fishForce;

		public Spring fishToHookSpring;
	}
}
