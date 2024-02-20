using System;
using System.Collections.Generic;
using Assets.Scripts.Phy.Simulations;
using CodeStage.AntiCheat.ObscuredTypes;
using Mono.Simd;
using Mono.Simd.Math;
using ObjectModel;
using Phy.Verlet;
using UnityEngine;

namespace Phy
{
	public class FishingRodSimulation : ConnectedBodiesSystem
	{
		public FishingRodSimulation(string name, bool isMain)
			: base(name)
		{
			this.IsMain = isMain;
			if (this.IsMain)
			{
			}
		}

		public GameFactory.RodSlot RodSlot { get; private set; }

		public PlayerController Player { get; private set; }

		public bool IsOnPod { get; private set; }

		public int RodPointsCount { get; private set; }

		public Mass HandsMass { get; private set; }

		public Mass RodTipMass { get; private set; }

		public Mass LineTipMass { get; private set; }

		public Mass LineTensionDetectorMass { get; private set; }

		public Mass LineFakeMass { get; private set; }

		public Spring LineTipNextSpring { get; private set; }

		public float AdaptiveLineSegmentMass
		{
			get
			{
				if (this.HookedFishObject != null)
				{
					return Mathf.Max(2E-05f, (this.HookedFishObject as AbstractFishBody).TotalMass * 8E-05f);
				}
				return 2E-05f;
			}
		}

		public float CurrentLeaderLength
		{
			get
			{
				return this.leaderLength;
			}
			set
			{
				this.SetLeaderLength(value);
			}
		}

		public Vector3 RodAppliedForce { get; private set; }

		public Vector3 HandsAppliedForce { get; private set; }

		public Vector3 TackleApliedForce { get; private set; }

		public Mass TackleTipMass { get; private set; }

		public PhyObject TackleObject
		{
			get
			{
				return this.tackleObject;
			}
		}

		public FishingRodSimulation.TackleType CurrentTackleType
		{
			get
			{
				return (this.RodSlot == null || this.RodSlot.Tackle == null) ? FishingRodSimulation.TackleType.None : this.RodSlot.Tackle.TackleType;
			}
		}

		public bool isLureTackle
		{
			get
			{
				return this.CurrentTackleType == FishingRodSimulation.TackleType.Lure || this.CurrentTackleType == FishingRodSimulation.TackleType.Wobbler || this.CurrentTackleType == FishingRodSimulation.TackleType.Topwater;
			}
		}

		public bool IsFeederTackle
		{
			get
			{
				return this.CurrentTackleType == FishingRodSimulation.TackleType.Feeder || this.CurrentTackleType == FishingRodSimulation.TackleType.CarpClassic || this.CurrentTackleType == FishingRodSimulation.TackleType.CarpPVAStick;
			}
		}

		public float lureBuoyancy { get; private set; }

		public float lureBuoyancySpeedMultiplier { get; private set; }

		public float baitMass { get; private set; }

		public float baitBuoyancy { get; private set; }

		public void SetOnPod(bool onPod)
		{
			this.Player = ((!onPod) ? GameFactory.Player : null);
			this.IsOnPod = onPod;
		}

		public override void Clear()
		{
			base.Clear();
			this.RodSlot = GameFactory.RodSlots[0];
			this.HandsMass = null;
			this.RodTipMass = null;
			this.LineTipMass = null;
			this.LineFakeMass = null;
			this.TackleTipMass = null;
			this.rodObject = null;
			this.lineObject = null;
			this.leaderObject = null;
			this.leashObject = null;
			this.tackleObject = null;
			this.hookObject = null;
			this.sinkerObject = null;
			this.magnetObject = null;
			this.HookedFishObject = null;
			this.priorRootPosition = null;
			this.priorRootRotation = null;
		}

		public void AddRod(BendingSegment segment, GameFactory.RodSlot rodSlot)
		{
			this.RodSlot = rodSlot;
			this.rodObject = new RodObjectInHands(segment, this.RodSlot.Rod, this.RodSlot, this.RodSlot.Sim);
			this.rodLength = segment.rodLength;
			if (this.RodSlot.Tackle is Lure1stBehaviour)
			{
				this.rodImpulseThresholdMult = FishingRodSimulation.RodCurveTestImpulseThresholdMult(segment.curveTest);
			}
			else
			{
				this.rodImpulseThresholdMult = 1f;
			}
			this.RodPointsCount = this.rodObject.PointsCount;
			this.HandsMass = this.rodObject.RootMass;
			this.HandsMass.IsRef = true;
			this.HandsMass.IsKinematic = true;
			this.HandsMass.StopMass();
			this.RodTipMass = this.rodObject.TipMass;
			this.RodTipMass.IsRef = true;
			this.RodPosition = segment.Nodes[0].position;
			this.RodRotation = segment.Nodes[0].rotation;
			this.rodObject.Masses[0].Rotation = this.RodRotation;
			if (this.RodKinematicSplineMovementEnabled)
			{
				this.RodKinematicSplineMovement = this.rodObject.RootKinematicSpline;
			}
			this.tackleLyingImpulseThreshold = 0f;
			this.tackleImpulseThreshold = 0f;
			this.lineImpulseThreshold = 0f;
			this.lineLyingImpulseThreshold = 0f;
			this.rodObject.UpdateSim();
		}

		public static float RodCurveTestImpulseThresholdMult(float curveTest)
		{
			return 1f + Mathf.Sqrt(Mathf.Max(curveTest - RodSegmentConfig.Fast.CurveTest, 0f));
		}

		public void AddLine(Vector3 lineTipPosition, float extraLength = 0f)
		{
			this.lineObject = this.BuildNonUniformLine(lineTipPosition, extraLength);
			this.firstLineMassIndex = base.Masses.Count;
			this.firstLineConnectionIndex = base.Connections.Count;
			this.firstLineMass = this.lineObject.Masses[1];
			this.firstLineConnection = this.lineObject.Springs[0];
			base.Objects.Add(this.lineObject);
			for (int i = 1; i < this.lineObject.Masses.Count; i++)
			{
				Mass mass = this.lineObject.Masses[i];
				base.Masses.Add(mass);
			}
			for (int j = 0; j < this.lineObject.Springs.Count; j++)
			{
				Spring spring = this.lineObject.Springs[j];
				base.Connections.Add(spring);
			}
			this.MinLineLength = this.RodSlot.Line.MinLineLength;
		}

		private SpringDrivenObject BuildLine(float lineLength)
		{
			SpringDrivenObject springDrivenObject = new SpringDrivenObject(PhyObjectType.Line, this.RodSlot.Sim, null);
			float num = 10f;
			Mass mass = this.RodTipMass;
			Vector3 position = mass.Position;
			springDrivenObject.Masses.Add(mass);
			int num2 = (int)(lineLength / 0.05f);
			this.lineSegmentLength = 0.05f;
			for (int i = 1; i <= num2; i++)
			{
				Mass mass2 = new Mass(this.RodSlot.Sim, 2E-05f, position - new Vector3(0f, 0.05f * (float)i, 0f), Mass.MassType.Line)
				{
					Buoyancy = 0.02f,
					WaterDragConstant = num,
					AirDragConstant = 0.0001f
				};
				springDrivenObject.Masses.Add(mass2);
				Spring spring = new Spring(mass, mass2, 500f, 0.05f, 0.002f);
				springDrivenObject.Springs.Add(spring);
				mass = mass2;
			}
			this.LineTipMass = mass;
			this.LineFakeMass = springDrivenObject.Masses[Mathf.Max(0, springDrivenObject.Masses.Count - 5)];
			this.LineFakeMass.IgnoreEnvironment = true;
			return springDrivenObject;
		}

		private SpringDrivenObject BuildNonUniformLine(Vector3 lineTipPosition, float extraLength = 0f)
		{
			SpringDrivenObject springDrivenObject = new SpringDrivenObject(PhyObjectType.Line, this.RodSlot.Sim, null);
			Vector3 vector = lineTipPosition - this.RodTipMass.Position;
			float magnitude = vector.magnitude;
			float num = 10f;
			if (Mathf.Approximately(magnitude, 0f))
			{
				return null;
			}
			vector /= magnitude;
			this.FinalLineLength = magnitude + extraLength;
			float num2 = magnitude / 0.05f;
			int num3;
			float num4;
			if (num2 <= 1f)
			{
				num3 = Mathf.CeilToInt(num2 / 1f);
				num4 = 1f - ((float)num3 - num2);
			}
			else
			{
				float num5 = num2 - 1f;
				float num6 = 0.8333333f;
				float num7 = 0.5f - num6 + Mathf.Sqrt((num6 - 0.5f) * (num6 - 0.5f) + 2f * num5 * num6);
				int num8 = Mathf.FloorToInt(num7);
				num4 = num7 - (float)num8;
				num3 = 2 + num8;
			}
			Mass mass = this.RodTipMass;
			springDrivenObject.Masses.Add(mass);
			float num9 = 0f;
			for (int i = 0; i < num3; i++)
			{
				float num10 = 0.05f;
				if (i == 0)
				{
					num10 = num4 * 0.05f;
				}
				else if (i > 1)
				{
					num10 = 0.05f * (1f + ((float)(i - 1 - 1) + num4) * 1.2f);
				}
				num9 += num10;
				Mass mass2 = new Mass(this.RodSlot.Sim, 2E-05f, this.RodTipMass.Position + vector * num9, Mass.MassType.Line)
				{
					Buoyancy = 0.02f,
					WaterDragConstant = num,
					AirDragConstant = 0.0001f
				};
				springDrivenObject.Masses.Add(mass2);
				Spring spring = new Spring(mass, mass2, 500f, num10 + extraLength * num10 / magnitude, 0.002f);
				springDrivenObject.Springs.Add(spring);
				mass = mass2;
			}
			this.LineTipMass = mass;
			this.PriorLineLength = this.CurrentLineLength;
			this.CurrentLineLength = 0f;
			for (int j = 0; j < springDrivenObject.Springs.Count; j++)
			{
				this.CurrentLineLength += springDrivenObject.Springs[j].SpringLength;
			}
			float num11 = (float)springDrivenObject.Springs.Count * 2E-05f * this.rodImpulseThresholdMult;
			this.lineImpulseThreshold = 0.02f * num11;
			this.lineLyingImpulseThreshold = 0.01f * num11;
			this.LineFakeMass = springDrivenObject.Masses[Mathf.Max(0, springDrivenObject.Masses.Count - 5)];
			return springDrivenObject;
		}

		private SpringDrivenObject BuildLine(PhyObjectType type, float lineLength, Mass tip, Vector3 direction, float segmentLength = 0.05f, float segmentMass = 2E-05f, Mass.MassType masstype = Mass.MassType.Line, float buoyancy = 0.02f, TackleBehaviour takle = null, int segsCount = 0)
		{
			SpringDrivenObject springDrivenObject = new SpringDrivenObject(type, this.RodSlot.Sim, takle);
			Mass mass = tip;
			Vector3 position = mass.Position;
			springDrivenObject.Masses.Add(mass);
			if (segsCount < 1)
			{
				segsCount = (int)(lineLength / segmentLength);
			}
			else
			{
				segmentLength = lineLength / (float)segsCount;
			}
			for (int i = 1; i <= segsCount; i++)
			{
				Mass mass2 = new Mass(this.RodSlot.Sim, segmentMass, position + direction * segmentLength * (float)i, masstype)
				{
					Buoyancy = buoyancy,
					WaterDragConstant = 10f,
					AirDragConstant = 0.0001f
				};
				base.Masses.Add(mass2);
				springDrivenObject.Masses.Add(mass2);
				Spring spring = new Spring(mass, mass2, 500f, segmentLength, 0.002f);
				base.Connections.Add(spring);
				springDrivenObject.Springs.Add(spring);
				mass = mass2;
			}
			return springDrivenObject;
		}

		private RigidBody AttachRigidBody(Mass hitch, RigidBodyController controller, float massValue, Mass.MassType massType)
		{
			Mass mass = new Mass(this.RodSlot.Sim, 2E-05f, hitch.Position, massType)
			{
				Buoyancy = 0.02f,
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(mass);
			Spring spring = new Spring(hitch, mass, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			Vector3 vector = Vector3.Scale(controller.topLineAnchor.localPosition - controller.center.localPosition, controller.transform.localScale);
			float magnitude = vector.magnitude;
			RigidBody rigidBody = new RigidBody(this.RodSlot.Sim, massValue, mass.Position - vector, massType, magnitude)
			{
				Buoyancy = controller.Buoyancy,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragEnabled = false,
				WaterDragConstant = 0f,
				AirDragConstant = 0.001f * (1f + massValue * 1000f),
				BounceFactor = controller.BounceFactor,
				ExtrudeFactor = controller.ExtrudeFactor,
				SlidingFrictionFactor = controller.CollisionFrictionFactor,
				Collision = Mass.CollisionType.Full
			};
			rigidBody.InertiaTensor = RigidBody.SolidBoxInertiaTensor(massValue, controller.Width, magnitude * 2f, controller.Depth);
			base.Masses.Add(rigidBody);
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass, rigidBody, 0.0001f, 0f, 0.002f, vector);
			base.Connections.Add(massToRigidBodySpring);
			return rigidBody;
		}

		private RigidBody AttachRigidBody(Mass hitch, RigidBodyController controller, float massValue, Mass.MassType massType, out Mass tip)
		{
			RigidBody rigidBody = this.AttachRigidBody(hitch, controller, massValue, massType);
			Vector3 vector = Vector3.Scale(controller.bottomLineAnchor.localPosition - controller.center.localPosition, controller.transform.localScale);
			tip = new Mass(this.RodSlot.Sim, 2E-05f, rigidBody.Position + vector, massType)
			{
				Buoyancy = 0.02f,
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(tip);
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(tip, rigidBody, 0.0001f, 0f, 0.002f, vector);
			base.Connections.Add(massToRigidBodySpring);
			return rigidBody;
		}

		private Mass AttachMass(Mass hitch, Vector3 position, float length, Mass.MassType massType = Mass.MassType.Auxiliary, float massValue = 2E-05f, float buoyancy = 0.02f, float springConstant = 500f, float frictionConstant = 0.002f, bool isRepulsive = false, PhyObject phyObject = null)
		{
			Mass mass = new Mass(this.RodSlot.Sim, massValue, position, massType)
			{
				Buoyancy = buoyancy
			};
			base.Masses.Add(mass);
			Spring spring = new Spring(hitch, mass, springConstant, length, frictionConstant)
			{
				IsRepulsive = isRepulsive
			};
			base.Connections.Add(spring);
			if (phyObject != null)
			{
				phyObject.Masses.Add(mass);
				phyObject.Connections.Add(spring);
			}
			return mass;
		}

		private Mass AttachMass(Mass hitch, Vector3 shiftPosition, Mass.MassType massType = Mass.MassType.Auxiliary, float massValue = 2E-05f, float buoyancy = 0.02f, float springConstant = 500f, float frictionConstant = 0.002f, bool isRepulsive = false, PhyObject phyObject = null)
		{
			return this.AttachMass(hitch, hitch.Position + shiftPosition, shiftPosition.magnitude, massType, massValue, buoyancy, springConstant, frictionConstant, isRepulsive, phyObject);
		}

		private Mass AttachMass(Mass hitch, float length, Mass.MassType massType = Mass.MassType.Auxiliary, float massValue = 2E-05f, float buoyancy = 0.02f, float springConstant = 500f, float frictionConstant = 0.002f, bool isRepulsive = false, PhyObject phyObject = null)
		{
			return this.AttachMass(hitch, hitch.Position, length, massType, massValue, buoyancy, springConstant, frictionConstant, isRepulsive, phyObject);
		}

		public void AddTopWater(float size, float mass, Vector3 direction, float leaderLength, LureController lure, float walkingFactor, float imbalanceRatio)
		{
			this.baitMass = 0f;
			this.lureBuoyancy = -1f;
			this.lureBuoyancySpeedMultiplier = 0f;
			this.lureMass = mass;
			this.tackleObject = new RigidBodyTackleObject(PhyObjectType.Lure, mass, mass, this.lureBuoyancy, this.RodSlot.Sim, this.RodSlot.Tackle);
			this.tackleLyingImpulseThreshold = 0.002f * mass;
			this.tackleImpulseThreshold = 0.02f * mass;
			Mass mass2 = this.LineTipMass;
			if (lure.Behaviour.Swivel != null && lure.Behaviour.Shackle != null)
			{
				Mass mass3;
				this.tackleObject.swivelBody = this.AttachRigidBody(this.LineTipMass, lure.Behaviour.Swivel, 0.005f, Mass.MassType.Swivel, out mass3);
				leaderLength = Mathf.Clamp(leaderLength * 0.25f, 0.05f, 0.32f);
				lure.Behaviour.lineEndAnchor = lure.Behaviour.Swivel.topLineAnchor;
				lure.Behaviour.leaderHitchAnchor = lure.Behaviour.Swivel.bottomLineAnchor;
				float num = Mathf.Min(0.05f, leaderLength * 0.3f);
				this.leaderObject = this.BuildLine(PhyObjectType.Leader, leaderLength, mass3, direction, num, 2E-05f, Mass.MassType.Leader, 0.02f, null, 0);
				Mass mass4 = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
				mass2 = this.AttachMass(mass4, lure.Behaviour.ShackleShift, Mass.MassType.Auxiliary, 2E-05f, 0.02f, 500f, 0.002f, false, null);
				lure.Behaviour.leaderEndAnchor = lure.Behaviour.Shackle.topLineAnchor;
			}
			Mass mass5 = new Mass(this.RodSlot.Sim, 2E-05f, mass2.Position, Mass.MassType.Lure)
			{
				Buoyancy = 0.02f,
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(mass5);
			this.lineObject.Masses.Add(mass5);
			Spring spring = new Spring(mass2, mass5, 500f, 0.001f, 0.002f);
			base.Connections.Add(spring);
			float num2 = mass * 9.81f / (size * size * size * 0.09f * 1000f);
			TopWaterLure topWaterLure = new TopWaterLure(this.RodSlot.Sim, mass, mass2.Position - new Vector3(0f, size * 0.5f, 0f), size, size * 0.3f)
			{
				Buoyancy = -1f,
				Radius = size * 0.15f,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragFactors = new Vector3(3f, 3f, 0.1f),
				BuoyancyVolumetric = num2 * 2f,
				FactorLevel = 16f,
				MaxForceFactor = 10f,
				AxialWaterDragEnabled = false,
				WaterDragConstant = 0f,
				LateralWaterResistance = 0.1f,
				LongitudalWaterResistance = 0.4f,
				WalkingFactor = walkingFactor,
				ImbalanceRatio = imbalanceRatio,
				AirDragConstant = 0.001f * (1f + mass * 100f),
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(topWaterLure);
			this.tackleObject.Masses.Add(topWaterLure);
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass5, topWaterLure, 0.0001f, 0f, 0.002f, Vector3.forward * size * 0.5f);
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			PointOnRigidBody pointOnRigidBody = new PointOnRigidBody(this.RodSlot.Sim, topWaterLure, Vector3.back * size * 0.5f, Mass.MassType.Hook)
			{
				IgnoreEnvironment = true
			};
			base.Masses.Add(pointOnRigidBody);
			this.tackleObject.Masses.Add(pointOnRigidBody);
			this.TackleTipMass = pointOnRigidBody;
			this.LineTensionDetectorMass = this.LineTipMass;
			base.Objects.Add(this.tackleObject);
			this.RefreshObjectArrays(true);
		}

		public void AddWobbler(float size, float distance2Hook, float mass, float buoyancy, float buoyancySpeedMultiplier, Vector3 direction, float leaderLength, LureController lure, float sinkRate, float workingDepth, float walkingFactor = 0f, float imbalanceRatio = 0f)
		{
			this.lureBuoyancy = -1f;
			this.lureBuoyancySpeedMultiplier = 0f;
			this.baitMass = 0f;
			this.lureMass = mass;
			this.tackleLyingImpulseThreshold = 0.002f * mass;
			this.tackleImpulseThreshold = 0.02f * mass;
			this.tackleObject = new RigidBodyTackleObject(PhyObjectType.Lure, mass, mass, this.lureBuoyancy, this.RodSlot.Sim, this.RodSlot.Tackle);
			Mass mass2 = this.LineTipMass;
			if (lure.Behaviour.Swivel != null && lure.Behaviour.Shackle != null)
			{
				Mass mass3;
				this.tackleObject.swivelBody = this.AttachRigidBody(this.LineTipMass, lure.Behaviour.Swivel, 0.005f, Mass.MassType.Swivel, out mass3);
				leaderLength = Mathf.Clamp(leaderLength * 0.25f, 0.05f, 0.32f);
				lure.Behaviour.lineEndAnchor = lure.Behaviour.Swivel.topLineAnchor;
				lure.Behaviour.leaderHitchAnchor = lure.Behaviour.Swivel.bottomLineAnchor;
				float num = Mathf.Min(0.05f, leaderLength * 0.3f);
				this.leaderObject = this.BuildLine(PhyObjectType.Leader, leaderLength, mass3, direction, num, 2E-05f, Mass.MassType.Leader, 0.02f, null, 0);
				Mass mass4 = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
				mass2 = this.AttachMass(mass4, lure.Behaviour.ShackleShift, Mass.MassType.Auxiliary, 2E-05f, 0.02f, 500f, 0.002f, false, null);
				lure.Behaviour.leaderEndAnchor = lure.Behaviour.Shackle.topLineAnchor;
			}
			Mass mass5 = new Mass(this.RodSlot.Sim, 2E-05f, mass2.Position, Mass.MassType.Wobbler)
			{
				Buoyancy = 0.02f,
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(mass5);
			this.lineObject.Masses.Add(mass5);
			Spring spring = new Spring(mass2, mass5, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			float num2;
			if (buoyancy > 0f)
			{
				num2 = mass * 9.81f / (size * size * size * 0.09f * 1000f) * 10f;
			}
			else
			{
				num2 = (1f + buoyancy) * mass * 9.81f;
			}
			TopWaterLure topWaterLure = new TopWaterLure(this.RodSlot.Sim, mass, mass2.Position - new Vector3(0f, size * 0.5f, 0f), size, size * 0.3f)
			{
				Buoyancy = -1f,
				Radius = size * 0.15f,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragFactors = new Vector3(3f, 3f, 0.01f),
				BuoyancyVolumetric = num2,
				AxialWaterDragEnabled = false,
				WaterDragConstant = 0f,
				LateralWaterResistance = ((buoyancy >= 0f) ? 1f : 0.15f),
				LongitudalWaterResistance = 0.1f,
				FactorLevel = ((buoyancy >= 0f) ? 16f : 2f),
				MaxForceFactor = 15f,
				AirDragConstant = 0.001f * (1f + mass * 300f),
				WobblingEnabled = true,
				IsFloatUp = (buoyancy < 0f),
				SinkRate = sinkRate,
				SinkYLevel = -workingDepth,
				Collision = Mass.CollisionType.Full,
				WalkingFactor = walkingFactor,
				ImbalanceRatio = imbalanceRatio
			};
			base.Masses.Add(topWaterLure);
			this.tackleObject.Masses.Add(topWaterLure);
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass5, topWaterLure, 0.0001f, 0.01f, 0.002f, Vector3.forward * size * 0.5f);
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			PointOnRigidBody pointOnRigidBody = new PointOnRigidBody(this.RodSlot.Sim, topWaterLure, Vector3.back * size * 0.5f, Mass.MassType.Hook)
			{
				IgnoreEnvironment = true
			};
			base.Masses.Add(pointOnRigidBody);
			this.tackleObject.Masses.Add(pointOnRigidBody);
			this.TackleTipMass = pointOnRigidBody;
			this.LineTensionDetectorMass = this.LineTipMass;
			base.Objects.Add(this.tackleObject);
			this.RefreshObjectArrays(true);
		}

		public void AddSwimbait(float size, float distance2Hook, float mass, float buoyancy, float buoyancySpeedMultiplier, Vector3 direction, float leaderLength, LureController lure)
		{
			float num = size * 0.2f;
			float num2 = mass * 0.01f;
			this.lureBuoyancy = buoyancy;
			this.lureBuoyancySpeedMultiplier = buoyancySpeedMultiplier;
			this.baitMass = 0f;
			this.lureMass = mass;
			this.tackleLyingImpulseThreshold = 0.002f * mass;
			this.tackleImpulseThreshold = 0.02f * mass;
			this.tackleObject = new RigidBodyTackleObject(PhyObjectType.Lure, mass, mass, this.lureBuoyancy, this.RodSlot.Sim, this.RodSlot.Tackle);
			Mass mass2 = this.LineTipMass;
			lure.Behaviour.lineEndAnchor = lure.Behaviour.TopLineAnchor;
			if (lure.Behaviour.Swivel != null && lure.Behaviour.Shackle != null)
			{
				Mass mass3;
				this.tackleObject.swivelBody = this.AttachRigidBody(this.LineTipMass, lure.Behaviour.Swivel, 0.005f, Mass.MassType.Swivel, out mass3);
				leaderLength = Mathf.Clamp(leaderLength * 0.25f, 0.05f, 0.32f);
				lure.Behaviour.lineEndAnchor = lure.Behaviour.Swivel.topLineAnchor;
				lure.Behaviour.leaderHitchAnchor = lure.Behaviour.Swivel.bottomLineAnchor;
				float num3 = Mathf.Min(0.05f, leaderLength * 0.3f);
				this.leaderObject = this.BuildLine(PhyObjectType.Leader, leaderLength, mass3, direction, num3, 2E-05f, Mass.MassType.Leader, 0.02f, null, 0);
				Mass mass4 = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
				mass2 = this.AttachMass(mass4, lure.Behaviour.ShackleShift, Mass.MassType.Auxiliary, 2E-05f, 0.02f, 500f, 0.002f, false, null);
				lure.Behaviour.leaderEndAnchor = lure.Behaviour.Shackle.topLineAnchor;
			}
			Mass mass5 = new Mass(this.RodSlot.Sim, 2E-05f, mass2.Position, Mass.MassType.Wobbler)
			{
				Buoyancy = 0.02f,
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(mass5);
			this.lineObject.Masses.Add(mass5);
			Spring spring = new Spring(mass2, mass5, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			TopWaterLure topWaterLure = new TopWaterLure(this.RodSlot.Sim, mass, mass2.Position - new Vector3(0f, num * 0.5f, 0f), size, size * 0.3f)
			{
				Buoyancy = buoyancy,
				Radius = size * 0.15f,
				BuoyancySpeedMultiplier = buoyancySpeedMultiplier,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				BuoyancyVolumetric = 0f,
				AxialWaterDragEnabled = false,
				WaterDragConstant = 10f,
				LateralWaterResistance = ((buoyancy >= 0f) ? 1f : 0.15f),
				LongitudalWaterResistance = 0.1f,
				FactorLevel = ((buoyancy >= 0f) ? 16f : 2f),
				MaxForceFactor = 15f,
				AirDragConstant = 0.001f * (1f + mass * 300f),
				WobblingEnabled = false,
				IsFloatUp = (buoyancy < 0f),
				SinkRate = 0f,
				Collision = Mass.CollisionType.Full,
				WalkingFactor = 0f,
				ImbalanceRatio = 0f
			};
			base.Masses.Add(topWaterLure);
			this.tackleObject.Masses.Add(topWaterLure);
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass5, topWaterLure, 0.0001f, 0.01f, 0.002f, Vector3.forward * num * 0.5f)
			{
				IsRepulsive = true
			};
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			PointOnRigidBody pointOnRigidBody = new PointOnRigidBody(this.RodSlot.Sim, topWaterLure, Vector3.back * size * 0.5f, Mass.MassType.Hook)
			{
				IgnoreEnvironment = true
			};
			base.Masses.Add(pointOnRigidBody);
			this.tackleObject.Masses.Add(pointOnRigidBody);
			this.TackleTipMass = pointOnRigidBody;
			this.LineTensionDetectorMass = this.LineTipMass;
			Mass mass6 = new Mass(this.RodSlot.Sim, 2E-05f, topWaterLure.Position - new Vector3(0f, num * 0.1f, 0f), Mass.MassType.Wobbler);
			base.Masses.Add(mass6);
			this.tackleObject.Masses.Add(mass6);
			massToRigidBodySpring = new MassToRigidBodySpring(mass6, topWaterLure, 0.0001f, 0.01f, 0.002f, Vector3.back * num * 0.1f)
			{
				IsRepulsive = true
			};
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			Mass mass7 = new Mass(this.RodSlot.Sim, num2, mass6.Position - new Vector3(0f, num, 0f), Mass.MassType.Wobbler)
			{
				Buoyancy = 0f,
				WaterDragConstant = 0f
			};
			mass7.StopMass();
			base.Masses.Add(mass7);
			this.tackleObject.Masses.Add(mass7);
			Spring spring2 = new Spring(mass6, mass7, 500f, num, 0.1f)
			{
				IsRepulsive = true
			};
			base.Connections.Add(spring2);
			this.tackleObject.Springs.Add(spring2);
			Mass mass8 = new Mass(this.RodSlot.Sim, num2, mass7.Position - new Vector3(0f, num, 0f), Mass.MassType.Wobbler)
			{
				Buoyancy = 0f,
				WaterDragConstant = 0f
			};
			mass8.StopMass();
			base.Masses.Add(mass8);
			this.tackleObject.Masses.Add(mass8);
			spring2 = new Spring(mass7, mass8, 500f, num, 0.1f)
			{
				IsRepulsive = true
			};
			base.Connections.Add(spring2);
			this.tackleObject.Springs.Add(spring2);
			Mass mass9 = new Mass(this.RodSlot.Sim, num2, mass8.Position - new Vector3(0f, num, 0f), Mass.MassType.Wobbler)
			{
				Buoyancy = 0f,
				WaterDragConstant = 0f
			};
			mass9.StopMass();
			base.Masses.Add(mass9);
			this.tackleObject.Masses.Add(mass9);
			spring2 = new Spring(mass8, mass9, 500f, num, 0.1f)
			{
				IsRepulsive = true
			};
			base.Connections.Add(spring2);
			this.tackleObject.Springs.Add(spring2);
			base.Objects.Add(this.tackleObject);
			this.RefreshObjectArrays(true);
		}

		public void AddSpod(float size, float baitMass, float baitBuoyancy, Vector3 direction, FeederController controller)
		{
			FeederSpodTackleObject feederSpodTackleObject = new FeederSpodTackleObject(PhyObjectType.Feeder, controller.MassValue, controller.MassValue, controller.Buoyancy, controller.GetComponent<Animator>(), this.RodSlot.Sim, this.RodSlot.Tackle);
			this.baitMass = baitMass;
			this.baitBuoyancy = baitBuoyancy;
			this.tackleObject = feederSpodTackleObject;
			RigidBody rigidBody = new RigidBody(this.RodSlot.Sim, controller.MassValue, this.LineTipMass.Position + (controller.CenterTransform.position - controller.topLineAnchor.position), Mass.MassType.Feeder, size * 0.5f)
			{
				Buoyancy = 0.4f,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragFactors = controller.AxialDragFactors * controller.SolidFactor,
				AxialWaterDragEnabled = true,
				WaterDragConstant = 0f,
				AirDragConstant = 0.001f * (1f + controller.MassValue * 1000f),
				BounceFactor = controller.BounceFactor,
				SlidingFrictionFactor = controller.CollisionFrictionFactor,
				ExtrudeFactor = controller.ExtrudeFactor,
				Collision = Mass.CollisionType.Full
			};
			rigidBody.InertiaTensor = RigidBody.SolidBoxInertiaTensor(controller.MassValue, controller.Width, size, controller.Depth);
			base.Masses.Add(rigidBody);
			this.tackleObject.Masses.Add(rigidBody);
			feederSpodTackleObject.rigidBody = rigidBody;
			this.tackleObject.CheckLayerMass = rigidBody;
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(this.LineTipMass, rigidBody, controller.TorsionalFrictionConstant, 0f, controller.FrictionConstant, controller.topLineAnchor.localPosition - controller.CenterTransform.localPosition);
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			this.LineTensionDetectorMass = this.LineTipMass;
			this.TackleTipMass = rigidBody;
			base.Objects.Add(this.tackleObject);
			this.RefreshObjectArrays(true);
		}

		public void AddCarpPVABag(float size, float hookSize, float baitMass, float baitBuoyancy, Vector3 direction, FeederController controller)
		{
			FeederPVABagTackleObject feederPVABagTackleObject = new FeederPVABagTackleObject(PhyObjectType.Feeder, controller.MassValue, controller.MassValue, controller.Buoyancy, this.RodSlot.Sim, controller.Behaviour)
			{
				bagObject = controller.SecondaryTackleObject
			};
			this.baitMass = baitMass;
			this.baitBuoyancy = baitBuoyancy;
			this.tackleObject = feederPVABagTackleObject;
			RigidBody rigidBody = new RigidBody(this.RodSlot.Sim, controller.MassValue, this.LineTipMass.Position + (controller.CenterTransform.position - controller.topLineAnchor.position), Mass.MassType.Feeder, size * 0.5f)
			{
				Buoyancy = -1f,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragFactors = controller.AxialDragFactors * controller.SolidFactor,
				AxialWaterDragEnabled = true,
				WaterDragConstant = 0f,
				AirDragConstant = 0.001f * (1f + controller.MassValue * 1000f),
				BounceFactor = controller.BounceFactor,
				SlidingFrictionFactor = controller.CollisionFrictionFactor,
				ExtrudeFactor = controller.ExtrudeFactor,
				Collision = Mass.CollisionType.Full
			};
			rigidBody.InertiaTensor = RigidBody.SolidBoxInertiaTensor(controller.MassValue, controller.Width, size, controller.Depth);
			base.Masses.Add(rigidBody);
			this.tackleObject.Masses.Add(rigidBody);
			feederPVABagTackleObject.rigidBody = rigidBody;
			this.tackleObject.CheckLayerMass = rigidBody;
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(this.LineTipMass, rigidBody, controller.TorsionalFrictionConstant, 0f, controller.FrictionConstant, controller.topLineAnchor.localPosition - controller.CenterTransform.localPosition);
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			RigidBodyController component = controller.SecondaryTackleObject.GetComponent<RigidBodyController>();
			component.RigidBody = rigidBody;
			feederPVABagTackleObject.bagRoot = component.center;
			feederPVABagTackleObject.leaderToBagConnection = massToRigidBodySpring;
			feederPVABagTackleObject.tackleSize = size;
			feederPVABagTackleObject.hookSize = hookSize;
			feederPVABagTackleObject.baitMass = baitMass;
			feederPVABagTackleObject.baitBuoyancy = baitBuoyancy;
			feederPVABagTackleObject.tackleInitDirection = direction;
			feederPVABagTackleObject.controller = controller;
			this.LineTensionDetectorMass = this.LineTipMass;
			this.TackleTipMass = rigidBody;
			base.Objects.Add(this.tackleObject);
			this.RefreshObjectArrays(true);
		}

		public void AddCarpMethod(float size, float hookSize, float baitMass, float baitBuoyancy, Vector3 direction, FeederController controller)
		{
			FeederMethodTackleObject feederMethodTackleObject = new FeederMethodTackleObject(PhyObjectType.Feeder, controller.MassValue, controller.MassValue, controller.Buoyancy, this.RodSlot.Sim, this.RodSlot.Tackle);
			this.baitMass = baitMass;
			this.baitBuoyancy = baitBuoyancy;
			this.tackleObject = feederMethodTackleObject;
			Mass mass = new Mass(this.RodSlot.Sim, 2E-05f, this.LineTipMass.Position, Mass.MassType.Feeder)
			{
				Buoyancy = 0.02f,
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(mass);
			this.lineObject.Masses.Add(mass);
			this.tackleObject.Masses.Add(mass);
			Spring spring = new Spring(this.LineTipMass, mass, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			RigidBody rigidBody = new RigidBody(this.RodSlot.Sim, controller.MassValue, this.LineTipMass.Position + (controller.CenterTransform.position - controller.topLineAnchor.position), Mass.MassType.Feeder, size * 0.5f)
			{
				Buoyancy = -1f,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragFactors = controller.AxialDragFactors * controller.SolidFactor,
				AxialWaterDragEnabled = true,
				WaterDragConstant = 0f,
				AirDragConstant = 0.001f * (1f + controller.MassValue * 1000f),
				BounceFactor = controller.BounceFactor,
				SlidingFrictionFactor = controller.CollisionFrictionFactor,
				ExtrudeFactor = controller.ExtrudeFactor,
				Collision = Mass.CollisionType.Full
			};
			rigidBody.InertiaTensor = RigidBody.SolidBoxInertiaTensor(controller.MassValue, controller.Width, size, controller.Depth);
			base.Masses.Add(rigidBody);
			this.tackleObject.Masses.Add(rigidBody);
			feederMethodTackleObject.rigidBody = rigidBody;
			this.tackleObject.CheckLayerMass = rigidBody;
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass, rigidBody, controller.TorsionalFrictionConstant, 0f, controller.FrictionConstant, controller.topLineAnchor.localPosition - controller.CenterTransform.localPosition);
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			this.leaderObject = new SpringDrivenObject(PhyObjectType.Leader, this.RodSlot.Sim, null);
			this.hookObject = new SpringDrivenObject(PhyObjectType.Hook, this.RodSlot.Sim, null);
			Mass mass2 = new Mass(this.RodSlot.Sim, 0.001f, rigidBody.Position, Mass.MassType.Leader);
			MassToRigidBodySpring massToRigidBodySpring2 = new MassToRigidBodySpring(mass2, rigidBody, controller.TorsionalFrictionConstant, 0.1f, 0f, controller.bottomLineAnchor.localPosition - controller.CenterTransform.localPosition);
			base.Masses.Add(mass2);
			base.Connections.Add(massToRigidBodySpring2);
			this.leaderObject.Connections.Add(massToRigidBodySpring2);
			this.leaderObject.Masses.Add(mass2);
			Mass mass3 = new Mass(this.RodSlot.Sim, 0.001f, rigidBody.Position, Mass.MassType.Hook);
			feederMethodTackleObject.hookMass = mass3;
			base.Masses.Add(mass3);
			this.hookObject.Masses.Add(mass3);
			spring = new Spring(mass2, mass3, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			this.hookObject.Springs.Add(spring);
			this.leaderObject.Connections.Add(spring);
			this.leaderObject.Masses.Add(mass3);
			VerletMass verletMass = new VerletMass(this.RodSlot.Sim, 0.001f, mass2.Position, Mass.MassType.Hook)
			{
				Radius = 0.05f,
				IgnoreEnvironment = false,
				Buoyancy = baitBuoyancy,
				AirDragConstant = 0.002f
			};
			verletMass.StopMass();
			base.Masses.Add(verletMass);
			this.hookObject.Masses.Add(verletMass);
			if ((controller.Behaviour as Feeder1stBehaviour).IsFilled)
			{
				MassToRigidBodySpring massToRigidBodySpring3 = new MassToRigidBodySpring(verletMass, rigidBody, controller.TorsionalFrictionConstant, 0f, controller.FrictionConstant, new Vector3(0f, 0f, -hookSize * 0.5f));
				base.Connections.Add(massToRigidBodySpring3);
				feederMethodTackleObject.hookStickingConnection = massToRigidBodySpring3;
			}
			spring = new Spring(mass3, verletMass, 500f, hookSize, 0.002f);
			base.Connections.Add(spring);
			this.hookObject.Springs.Add(spring);
			this.TackleTipMass = verletMass;
			this.LineTensionDetectorMass = this.LineTipMass;
			base.Objects.Add(this.tackleObject);
			base.Objects.Add(this.leaderObject);
			this.RefreshObjectArrays(true);
		}

		public void AddCarpClassic(float size, float hookSize, float baitMass, float baitBuoyancy, Vector3 direction, FeederController controller, FeederTackleObject feederTackleObject = null)
		{
			if (feederTackleObject == null)
			{
				feederTackleObject = new FeederTackleObject(PhyObjectType.Feeder, controller.MassValue, controller.MassValue, controller.Buoyancy, this.RodSlot.Sim, this.RodSlot.Tackle);
			}
			this.baitMass = baitMass;
			this.baitBuoyancy = baitBuoyancy;
			this.tackleObject = feederTackleObject;
			this.leashObject = this.BuildLine(PhyObjectType.Leash, controller.FeederLineLength, this.LineTipMass, direction, 0.05f, 2E-05f, Mass.MassType.Leash, 0.02f, null, 0);
			Mass mass = this.leashObject.Masses[this.leashObject.Masses.Count - 1];
			Mass mass2 = new Mass(this.RodSlot.Sim, 2E-05f, mass.Position, Mass.MassType.Feeder)
			{
				Buoyancy = 0.02f,
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(mass2);
			this.lineObject.Masses.Add(mass2);
			this.tackleObject.Masses.Add(mass2);
			Spring spring = new Spring(mass, mass2, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			RigidBody rigidBody = new RigidBody(this.RodSlot.Sim, controller.MassValue, mass2.Position + (controller.CenterTransform.position - controller.topLineAnchor.position), Mass.MassType.Feeder, size * 0.5f)
			{
				Buoyancy = -1f,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragFactors = controller.AxialDragFactors * controller.SolidFactor,
				AxialWaterDragEnabled = true,
				WaterDragConstant = 0f,
				AirDragConstant = 0.001f * (1f + controller.MassValue * 1000f),
				BounceFactor = controller.BounceFactor,
				SlidingFrictionFactor = controller.CollisionFrictionFactor,
				ExtrudeFactor = controller.ExtrudeFactor,
				Collision = Mass.CollisionType.Full
			};
			rigidBody.InertiaTensor = RigidBody.SolidBoxInertiaTensor(controller.MassValue, controller.Width, size, controller.Depth);
			base.Masses.Add(rigidBody);
			this.tackleObject.Masses.Add(rigidBody);
			feederTackleObject.rigidBody = rigidBody;
			this.tackleObject.CheckLayerMass = rigidBody;
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass2, rigidBody, controller.TorsionalFrictionConstant, 0f, controller.FrictionConstant, controller.topLineAnchor.localPosition - controller.CenterTransform.localPosition);
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			Mass lineTipMass = this.LineTipMass;
			this.LineTensionDetectorMass = lineTipMass;
			FeederBehaviour feederBehaviour = controller.Behaviour as FeederBehaviour;
			feederBehaviour.leaderHitch = lineTipMass;
			feederBehaviour.leashHitch = this.LineTipMass;
			float num = feederBehaviour.UserSetLeaderLength / this.MinLineLength * 0.05f;
			this.leaderObject = this.BuildLine(PhyObjectType.Leader, feederBehaviour.UserSetLeaderLength, lineTipMass, direction, num, 2E-05f, Mass.MassType.Leader, 0.02f, null, 0);
			Mass mass3 = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
			this.hookObject = new SpringDrivenObject(PhyObjectType.Hook, this.RodSlot.Sim, null);
			Mass mass4 = new Mass(this.RodSlot.Sim, 0.001f, mass3.Position, Mass.MassType.Hook);
			feederTackleObject.hookMass = mass4;
			base.Masses.Add(mass4);
			this.hookObject.Masses.Add(mass4);
			this.leaderObject.Masses.Add(mass4);
			spring = new Spring(mass3, mass4, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			this.hookObject.Springs.Add(spring);
			VerletMass verletMass = new VerletMass(this.RodSlot.Sim, 0.001f, mass3.Position, Mass.MassType.Hook)
			{
				Radius = 0.05f,
				IgnoreEnvironment = false,
				Buoyancy = baitBuoyancy,
				AirDragConstant = 0.002f
			};
			verletMass.StopMass();
			base.Masses.Add(verletMass);
			this.hookObject.Masses.Add(verletMass);
			spring = new Spring(mass4, verletMass, 500f, hookSize, 0.002f);
			base.Connections.Add(spring);
			this.hookObject.Springs.Add(spring);
			this.TackleTipMass = verletMass;
			this.LineTensionDetectorMass = this.LineTipMass;
			Mass mass5 = new Mass(this.RodSlot.Sim, baitMass, mass3.Position, Mass.MassType.Boilie);
			spring = new Spring(verletMass, mass5, 500f, hookSize * 0.5f, 0.01f);
			this.hookObject.Springs.Add(spring);
			base.Connections.Add(spring);
			this.hookObject.Masses.Add(mass5);
			base.Masses.Add(mass5);
			this.LineTipNextSpring = this.leashObject.Springs[0];
			base.Objects.Add(this.leashObject);
			base.Objects.Add(this.tackleObject);
			base.Objects.Add(this.leaderObject);
			base.Objects.Add(this.hookObject);
			this.RefreshObjectArrays(true);
		}

		public void AddCarpPVAStick(float size, float hookSize, float baitMass, float baitBuoyancy, Vector3 direction, FeederController controller)
		{
			FeederPVAStickTackleObject feederPVAStickTackleObject = new FeederPVAStickTackleObject(PhyObjectType.Feeder, controller.MassValue, controller.MassValue, controller.Buoyancy, this.RodSlot.Sim, this.RodSlot.Tackle)
			{
				stickObject = controller.SecondaryTackleObject
			};
			this.baitMass = baitMass;
			this.baitBuoyancy = baitBuoyancy;
			this.tackleObject = feederPVAStickTackleObject;
			Mass lineTipMass = this.LineTipMass;
			this.LineTensionDetectorMass = lineTipMass;
			FeederBehaviour feederBehaviour = controller.Behaviour as FeederBehaviour;
			feederBehaviour.leaderHitch = lineTipMass;
			feederBehaviour.leashHitch = this.LineTipMass;
			float num = feederBehaviour.UserSetLeaderLength / this.MinLineLength * 0.05f;
			this.leaderObject = this.BuildLine(PhyObjectType.Leader, feederBehaviour.UserSetLeaderLength, lineTipMass, direction, num, 2E-05f, Mass.MassType.Leader, 0.02f, null, 0);
			Mass mass = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
			this.hookObject = new SpringDrivenObject(PhyObjectType.Hook, this.RodSlot.Sim, null);
			Mass mass2 = new Mass(this.RodSlot.Sim, 0.001f, mass.Position, Mass.MassType.Hook);
			feederPVAStickTackleObject.hookMass = mass2;
			base.Masses.Add(mass2);
			this.hookObject.Masses.Add(mass2);
			this.leaderObject.Masses.Add(mass2);
			VerletMass verletMass = new VerletMass(this.RodSlot.Sim, 0.001f + baitMass, mass.Position, Mass.MassType.Hook)
			{
				Radius = 0.05f,
				IgnoreEnvironment = false,
				Buoyancy = baitBuoyancy,
				AirDragConstant = 0.001f * (1f + baitMass * 1000f)
			};
			verletMass.StopMass();
			base.Masses.Add(verletMass);
			this.hookObject.Masses.Add(verletMass);
			Spring spring = new Spring(mass2, verletMass, 500f, hookSize, 0.002f);
			base.Connections.Add(spring);
			this.hookObject.Springs.Add(spring);
			this.TackleTipMass = verletMass;
			if (feederPVAStickTackleObject.stickObject != null)
			{
				float num2 = (float)(feederBehaviour.RodAssembly.FeederInterface as Feeder).Weight.Value;
				RigidBody rigidBody = new RigidBody(this.RodSlot.Sim, num2, mass.Position + (controller.CenterTransform.position - controller.topLineAnchor.position), Mass.MassType.Feeder, size * 0.5f)
				{
					Buoyancy = 0f,
					BuoyancySpeedMultiplier = 0f,
					IgnoreEnvironment = false,
					RotationDamping = 0.1f,
					UnderwaterRotationDamping = 0.8f,
					AxialWaterDragEnabled = false,
					WaterDragConstant = 0f,
					AirDragConstant = 0.001f * (1f + num2 * 1000f),
					BounceFactor = controller.BounceFactor,
					SlidingFrictionFactor = controller.CollisionFrictionFactor,
					ExtrudeFactor = controller.ExtrudeFactor,
					Collision = Mass.CollisionType.Full
				};
				rigidBody.InertiaTensor = RigidBody.SolidBoxInertiaTensor(num2, 0.02f, size, 0.02f);
				base.Masses.Add(rigidBody);
				feederPVAStickTackleObject.stickBody = rigidBody;
				RigidBodyController component = controller.SecondaryTackleObject.GetComponent<RigidBodyController>();
				component.RigidBody = rigidBody;
				MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass, rigidBody, 0f, 0f, 0f, component.topLineAnchor.localPosition - component.center.localPosition);
				base.Connections.Add(massToRigidBodySpring);
				feederPVAStickTackleObject.leaderToStickConnection = massToRigidBodySpring;
				Vector3 vector = component.bottomLineAnchor.localPosition - component.center.localPosition;
				MassToRigidBodySpring massToRigidBodySpring2 = new MassToRigidBodySpring(mass2, rigidBody, 0f, 0f, 0f, vector - vector.normalized * hookSize);
				feederPVAStickTackleObject.stickToHookConnection = massToRigidBodySpring2;
				base.Connections.Add(massToRigidBodySpring2);
				this.hookObject.Springs.Add(massToRigidBodySpring2);
			}
			else
			{
				Spring spring2 = new Spring(mass, mass2, 0f, 0.03f, 0.002f);
				base.Connections.Add(spring2);
			}
			this.leashObject = this.BuildLine(PhyObjectType.Leash, controller.FeederLineLength, this.LineTipMass, direction, 0.05f, 2E-05f, Mass.MassType.Leash, 0.02f, null, 0);
			Mass mass3 = this.leashObject.Masses[this.leashObject.Masses.Count - 1];
			Mass mass4 = new Mass(this.RodSlot.Sim, 2E-05f, mass3.Position, Mass.MassType.Feeder)
			{
				Buoyancy = 0.02f,
				Collision = Mass.CollisionType.Full
			};
			base.Masses.Add(mass4);
			this.lineObject.Masses.Add(mass4);
			this.tackleObject.Masses.Add(mass4);
			spring = new Spring(mass3, mass4, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			RigidBody rigidBody2 = new RigidBody(this.RodSlot.Sim, controller.MassValue, mass4.Position + (controller.CenterTransform.position - controller.topLineAnchor.position), Mass.MassType.Sinker, size * 0.5f)
			{
				Buoyancy = -1f,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragFactors = controller.AxialDragFactors * controller.SolidFactor,
				AxialWaterDragEnabled = true,
				WaterDragConstant = 0f,
				AirDragConstant = 0.001f * (1f + controller.MassValue * 1000f),
				BounceFactor = controller.BounceFactor,
				SlidingFrictionFactor = controller.CollisionFrictionFactor,
				ExtrudeFactor = controller.ExtrudeFactor,
				Collision = Mass.CollisionType.Full
			};
			rigidBody2.InertiaTensor = RigidBody.SolidBoxInertiaTensor(controller.MassValue, controller.Width, size, controller.Depth);
			base.Masses.Add(rigidBody2);
			this.tackleObject.Masses.Add(rigidBody2);
			feederPVAStickTackleObject.rigidBody = rigidBody2;
			this.tackleObject.CheckLayerMass = rigidBody2;
			MassToRigidBodySpring massToRigidBodySpring3 = new MassToRigidBodySpring(mass4, rigidBody2, controller.TorsionalFrictionConstant, 0f, controller.FrictionConstant, controller.topLineAnchor.localPosition - controller.CenterTransform.localPosition);
			base.Connections.Add(massToRigidBodySpring3);
			this.tackleObject.Springs.Add(massToRigidBodySpring3);
			this.LineTipNextSpring = this.leashObject.Springs[0];
			base.Objects.Add(this.tackleObject);
			base.Objects.Add(this.leaderObject);
			base.Objects.Add(this.hookObject);
			base.Objects.Add(this.leashObject);
			this.RefreshObjectArrays(true);
		}

		public void AddFeeder(float size, float hookSize, float baitMass, float baitBuoyancy, Vector3 direction, FeederController controller)
		{
			FeederTackleObject feederTackleObject = new FeederTackleObject(PhyObjectType.Feeder, controller.MassValue, controller.MassValue, controller.Buoyancy, this.RodSlot.Sim, this.RodSlot.Tackle);
			this.baitMass = baitMass;
			this.baitBuoyancy = baitBuoyancy;
			this.tackleObject = feederTackleObject;
			FeederBehaviour feederBehaviour = controller.Behaviour as FeederBehaviour;
			this.leashObject = this.BuildLine(PhyObjectType.Leash, controller.FeederLineLength, this.LineTipMass, direction, 0.05f, 2E-05f, Mass.MassType.Leash, 0.02f, null, 0);
			Mass mass = this.leashObject.Masses[this.leashObject.Masses.Count - 1];
			Mass mass2 = this.AttachMass(mass, 0f, Mass.MassType.Feeder, 2E-05f, 0.02f, 500f, 0.002f, false, null);
			mass2.Collision = Mass.CollisionType.Full;
			this.lineObject.Masses.Add(mass2);
			this.tackleObject.Masses.Add(mass2);
			RigidBody rigidBody = new RigidBody(this.RodSlot.Sim, controller.MassValue, mass2.Position + (controller.CenterTransform.position - controller.topLineAnchor.position), Mass.MassType.Feeder, size * 0.5f)
			{
				Buoyancy = -1f,
				BuoyancySpeedMultiplier = 0f,
				IgnoreEnvironment = false,
				RotationDamping = 0.8f,
				UnderwaterRotationDamping = 0.8f,
				AxialWaterDragFactors = controller.AxialDragFactors * controller.SolidFactor,
				AxialWaterDragEnabled = true,
				WaterDragConstant = 0f,
				AirDragConstant = 0.001f * (1f + controller.MassValue * 1000f),
				BounceFactor = controller.BounceFactor,
				SlidingFrictionFactor = controller.CollisionFrictionFactor,
				ExtrudeFactor = controller.ExtrudeFactor,
				Collision = Mass.CollisionType.RigidbodyContacts
			};
			rigidBody.InertiaTensor = RigidBody.SolidBoxInertiaTensor(controller.MassValue, controller.Width, size, controller.Depth);
			base.Masses.Add(rigidBody);
			this.tackleObject.Masses.Add(rigidBody);
			feederTackleObject.rigidBody = rigidBody;
			this.tackleObject.CheckLayerMass = rigidBody;
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass2, rigidBody, controller.TorsionalFrictionConstant, 0f, controller.FrictionConstant, controller.topLineAnchor.localPosition - controller.CenterTransform.localPosition);
			base.Connections.Add(massToRigidBodySpring);
			this.tackleObject.Springs.Add(massToRigidBodySpring);
			feederBehaviour.leashHitch = this.LineTipMass;
			this.LineTensionDetectorMass = this.LineTipMass;
			Mass lineTipMass = this.LineTipMass;
			if (feederBehaviour.Swivel == null)
			{
				feederBehaviour.lineEnd = this.LineTipMass;
				feederBehaviour.leaderHitch = lineTipMass;
			}
			else
			{
				Mass mass3 = this.AttachMass(this.LineTipMass, 0f, Mass.MassType.Line, 2E-05f, 0.02f, 500f, 0.002f, false, null);
				mass3.Collision = Mass.CollisionType.Full;
				this.lineObject.Masses.Add(mass3);
				this.tackleObject.Masses.Add(mass3);
				feederTackleObject.swivelBody = this.AttachRigidBody(mass3, feederBehaviour.Swivel, 0.005f, Mass.MassType.Swivel, out lineTipMass);
				feederBehaviour.lineEndAnchor = feederBehaviour.Swivel.topLineAnchor;
				feederBehaviour.leaderHitchAnchor = feederBehaviour.Swivel.bottomLineAnchor;
			}
			float num = feederBehaviour.UserSetLeaderLength / this.MinLineLength * 0.05f;
			this.leaderObject = this.BuildLine(PhyObjectType.Leader, feederBehaviour.UserSetLeaderLength, lineTipMass, direction, num, 2E-05f, Mass.MassType.Leader, 0.02f, null, 0);
			Mass mass4 = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
			Mass mass5 = mass4;
			if (feederBehaviour.Shackle != null)
			{
				feederBehaviour.leaderEndAnchor = feederBehaviour.Shackle.topLineAnchor;
			}
			this.hookObject = new SpringDrivenObject(PhyObjectType.Hook, this.RodSlot.Sim, null);
			Mass mass6 = new Mass(this.RodSlot.Sim, 0.001f, mass5.Position, Mass.MassType.Hook);
			feederTackleObject.hookMass = mass6;
			base.Masses.Add(mass6);
			this.hookObject.Masses.Add(mass6);
			this.leaderObject.Masses.Add(mass6);
			Spring spring = new Spring(mass5, mass6, 500f, 0f, 0.002f);
			base.Connections.Add(spring);
			this.hookObject.Springs.Add(spring);
			VerletMass verletMass = new VerletMass(this.RodSlot.Sim, 0.001f + baitMass, mass5.Position, Mass.MassType.Hook)
			{
				Radius = 0.05f,
				IgnoreEnvironment = false,
				Buoyancy = baitBuoyancy,
				AirDragConstant = 0.001f * (1f + baitMass * 1000f)
			};
			verletMass.StopMass();
			base.Masses.Add(verletMass);
			this.hookObject.Masses.Add(verletMass);
			spring = new Spring(mass6, verletMass, 500f, hookSize, 0.002f);
			base.Connections.Add(spring);
			this.hookObject.Springs.Add(spring);
			this.TackleTipMass = verletMass;
			this.LineTipNextSpring = this.leashObject.Springs[0];
			base.Objects.Add(this.leashObject);
			base.Objects.Add(this.tackleObject);
			base.Objects.Add(this.leaderObject);
			base.Objects.Add(this.hookObject);
			this.RefreshObjectArrays(true);
		}

		public void AddLure(float size, float distance2Hook, float mass, float buoyancy, float buoyancySpeedMultiplier, Vector3 direction, float leaderLength, LureController lure)
		{
			this.lureBuoyancy = buoyancy;
			this.lureBuoyancySpeedMultiplier = buoyancySpeedMultiplier;
			this.baitMass = 0f;
			this.lureMass = mass;
			if (this.lureBuoyancy < 0f)
			{
				this.lureBuoyancy += mass * -17.894737f + -0.34842104f + 1f;
			}
			this.tackleLyingImpulseThreshold = 0.002f * mass;
			this.tackleImpulseThreshold = 0.02f * mass;
			BasicLureTackleObject basicLureTackleObject = new BasicLureTackleObject(PhyObjectType.Lure, this.RodSlot.Sim, this.RodSlot.Tackle)
			{
				NeedCheckSurface = true
			};
			this.tackleObject = basicLureTackleObject;
			Mass mass2 = this.LineTipMass;
			if (lure.Behaviour.Swivel != null && lure.Behaviour.Shackle != null)
			{
				Mass mass3;
				basicLureTackleObject.swivelBody = this.AttachRigidBody(this.LineTipMass, lure.Behaviour.Swivel, 0.005f, Mass.MassType.Swivel, out mass3);
				leaderLength = Mathf.Clamp(leaderLength * 0.25f, 0.05f, 0.32f);
				lure.Behaviour.lineEndAnchor = lure.Behaviour.Swivel.topLineAnchor;
				lure.Behaviour.leaderHitchAnchor = lure.Behaviour.Swivel.bottomLineAnchor;
				float num = Mathf.Min(0.05f, leaderLength * 0.3f);
				this.leaderObject = this.BuildLine(PhyObjectType.Leader, leaderLength, mass3, direction, num, 2E-05f, Mass.MassType.Leader, 0.02f, null, 0);
				Mass mass4 = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
				mass2 = this.AttachMass(mass4, lure.Behaviour.ShackleShift, Mass.MassType.Auxiliary, 2E-05f, 0.02f, 500f, 0.002f, false, null);
				lure.Behaviour.leaderEndAnchor = lure.Behaviour.Shackle.topLineAnchor;
			}
			Mass mass5 = new Mass(this.RodSlot.Sim, mass * 0.5f, mass2.Position, Mass.MassType.Lure)
			{
				Buoyancy = this.lureBuoyancy,
				BuoyancySpeedMultiplier = buoyancySpeedMultiplier,
				WaterDragConstant = 10f,
				AirDragConstant = 0.001f * (1f + mass * 100f)
			};
			base.Masses.Add(mass5);
			this.tackleObject.Masses.Add(mass5);
			this.lineObject.Masses.Add(mass5);
			Spring spring = new Spring(mass2, mass5, 1000f, 0.001f, 0.002f);
			base.Connections.Add(spring);
			this.tackleObject.Springs.Add(spring);
			Mass mass6 = new Mass(this.RodSlot.Sim, mass * 0.5f, mass2.Position - new Vector3(0f, size, 0f), Mass.MassType.Lure)
			{
				Buoyancy = this.lureBuoyancy,
				BuoyancySpeedMultiplier = buoyancySpeedMultiplier,
				WaterDragConstant = 10f,
				AirDragConstant = 0.001f * (1f + mass * 100f)
			};
			mass6.EnableMotionMonitor("LureMass");
			base.Masses.Add(mass6);
			this.tackleObject.Masses.Add(mass6);
			spring = new Spring(mass5, mass6, 1000f, size, 0.002f);
			base.Connections.Add(spring);
			this.tackleObject.Springs.Add(spring);
			VerletMass verletMass = new VerletMass(this.RodSlot.Sim, 0.001f, mass2.Position - new Vector3(0f, distance2Hook, 0f), Mass.MassType.Hook)
			{
				Radius = 0.05f,
				Buoyancy = this.lureBuoyancy,
				BuoyancySpeedMultiplier = buoyancySpeedMultiplier,
				Collision = Mass.CollisionType.Full,
				IgnoreEnvironment = false
			};
			verletMass.StopMass();
			verletMass.EnableMotionMonitor("LureHookMass");
			base.Masses.Add(verletMass);
			this.tackleObject.Masses.Add(verletMass);
			spring = new Spring(mass6, verletMass, 500f, distance2Hook, 0.002f);
			base.Connections.Add(spring);
			this.tackleObject.Springs.Add(spring);
			this.TackleTipMass = verletMass;
			this.LineTensionDetectorMass = this.LineTipMass;
			base.Objects.Add(this.tackleObject);
			this.RefreshObjectArrays(true);
		}

		public void AddRig(float size, float distance2Hook, float mass, float buoyancy, float buoyancySpeedMultiplier, Vector3 direction, float leaderLength, float sinkerMass, RodTemplate rodTemplate, RigidBodyController sinker, LureController lure)
		{
			leaderLength = Mathf.Clamp(leaderLength * 0.25f, 0.05f, 0.32f);
			float num = leaderLength * 0.6f;
			if (rodTemplate == RodTemplate.TexasRig)
			{
				num = 0.05f;
			}
			this.lureBuoyancy = buoyancy;
			this.lureBuoyancySpeedMultiplier = buoyancySpeedMultiplier;
			this.baitMass = 0f;
			this.lureMass = mass;
			if (this.lureBuoyancy < 0f)
			{
				this.lureBuoyancy += mass * -17.894737f + -0.34842104f + 1f;
			}
			this.tackleLyingImpulseThreshold = 0.002f * mass;
			this.tackleImpulseThreshold = 0.02f * mass;
			RigTackleObject rigTackleObject = new RigTackleObject(PhyObjectType.Lure, mass, mass, buoyancy, this.RodSlot.Sim, this.RodSlot.Tackle)
			{
				NeedCheckSurface = true
			};
			this.tackleObject = rigTackleObject;
			float num2 = leaderLength;
			if (rodTemplate == RodTemplate.CarolinaRig)
			{
				num2 -= num;
			}
			else if (rodTemplate == RodTemplate.ThreewayRig)
			{
				num2 = 0.25f;
			}
			Mass mass2;
			if (rodTemplate != RodTemplate.ThreewayRig)
			{
				mass2 = this.LineTipMass;
			}
			else
			{
				mass2 = this.AttachMass(this.LineTipMass, 0f, Mass.MassType.Leash, 2E-05f, 0.02f, 500f, 0.002f, false, null);
			}
			rigTackleObject.smallSinker = mass2;
			lure.Behaviour.lineEnd = this.LineTipMass;
			lure.Behaviour.leashHitch = this.LineTipMass;
			this.leashObject = this.BuildLine(PhyObjectType.Leash, num2, mass2, direction, 0.05f, 2E-05f, Mass.MassType.Leash, 0.02f, null, 0);
			Mass mass3 = this.leashObject.Masses[this.leashObject.Masses.Count - 1];
			Mass mass4 = null;
			if (rodTemplate == RodTemplate.ThreewayRig)
			{
				rigTackleObject.rigidBody = this.AttachRigidBody(mass3, sinker, sinkerMass, Mass.MassType.Sinker);
			}
			else
			{
				rigTackleObject.rigidBody = this.AttachRigidBody(mass3, sinker, sinkerMass, Mass.MassType.Sinker, out mass4);
			}
			lure.Behaviour.leashEndAnchor = sinker.topLineAnchor;
			Mass mass5 = mass4 ?? this.LineTipMass;
			if (rodTemplate == RodTemplate.ThreewayRig)
			{
				lure.Behaviour.leaderHitch = mass5;
			}
			else
			{
				lure.Behaviour.leaderHitchAnchor = sinker.bottomLineAnchor;
			}
			float num3 = Mathf.Min(0.05f, num * 0.3f);
			this.leaderObject = this.BuildLine(PhyObjectType.Leader, num, mass5, direction, num3, 2E-05f, Mass.MassType.Leader, 0.02f, null, 0);
			Mass mass6 = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
			Mass mass7 = this.AttachMass(mass6, 0f, Mass.MassType.Auxiliary, 2E-05f, 0.02f, 500f, 0.002f, false, null);
			this.leaderObject.Masses.Add(mass7);
			Mass mass8 = new Mass(this.RodSlot.Sim, mass * 0.5f, mass7.Position, Mass.MassType.Lure)
			{
				Buoyancy = this.lureBuoyancy,
				BuoyancySpeedMultiplier = buoyancySpeedMultiplier,
				WaterDragConstant = 10f,
				AirDragConstant = 0.001f * (1f + mass * 100f)
			};
			base.Masses.Add(mass8);
			this.tackleObject.Masses.Add(mass8);
			Spring spring = new Spring(mass7, mass8, 500f, 0.001f, 0.002f);
			base.Connections.Add(spring);
			this.tackleObject.Springs.Add(spring);
			Mass mass9 = new Mass(this.RodSlot.Sim, mass * 0.5f, mass7.Position - new Vector3(0f, size, 0f), Mass.MassType.Lure)
			{
				Buoyancy = this.lureBuoyancy,
				BuoyancySpeedMultiplier = buoyancySpeedMultiplier,
				WaterDragConstant = 10f,
				AirDragConstant = 0.001f * (1f + mass * 100f)
			};
			mass9.EnableMotionMonitor("LureMass");
			base.Masses.Add(mass9);
			this.tackleObject.Masses.Add(mass9);
			Spring spring2 = new Spring(mass8, mass9, 500f, size, 0.002f);
			base.Connections.Add(spring2);
			this.tackleObject.Springs.Add(spring2);
			VerletMass verletMass = new VerletMass(this.RodSlot.Sim, 0.001f, mass7.Position - new Vector3(0f, distance2Hook, 0f), Mass.MassType.Hook)
			{
				Radius = 0.05f,
				Buoyancy = this.lureBuoyancy,
				BuoyancySpeedMultiplier = buoyancySpeedMultiplier,
				Collision = Mass.CollisionType.Full,
				IgnoreEnvironment = false
			};
			verletMass.StopMass();
			verletMass.EnableMotionMonitor("LureHookMass");
			base.Masses.Add(verletMass);
			this.tackleObject.Masses.Add(verletMass);
			rigTackleObject.hookMass = verletMass;
			Spring spring3 = new Spring(mass9, verletMass, 500f, distance2Hook, 0.002f);
			base.Connections.Add(spring3);
			this.tackleObject.Springs.Add(spring3);
			this.TackleTipMass = verletMass;
			this.LineTensionDetectorMass = mass7;
			base.Objects.Add(this.leashObject);
			base.Objects.Add(this.tackleObject);
			base.Objects.Add(this.leaderObject);
			this.RefreshObjectArrays(true);
		}

		public void AddBobber(float size, float hookSize, float mass, float bobberBuoyancy, float sinkerMass, float baitMass, float baitBuoyancy, float sensitivity, Vector3 direction)
		{
			this.bobberSize = size;
			this.baitMass = baitMass;
			this.baitBuoyancy = baitBuoyancy;
			this.lureMass = 0f;
			this.tackleObject = new SpringDrivenObject(PhyObjectType.Bobber, this.RodSlot.Sim, this.RodSlot.Tackle);
			this.tackleLyingImpulseThreshold = 0.005f * mass;
			this.tackleImpulseThreshold = 0.005f * mass;
			Mass mass2 = new Mass(this.RodSlot.Sim, mass, this.LineTipMass.Position, Mass.MassType.Bobber)
			{
				Buoyancy = bobberBuoyancy * 0.5f,
				WaterDragConstant = 40f,
				AirDragConstant = 0.005f
			};
			base.Masses.Add(mass2);
			this.tackleObject.Masses.Add(mass2);
			this.lineObject.Masses.Add(mass2);
			Spring spring = new Spring(this.LineTipMass, mass2, 500f, 0.001f, 0.002f);
			base.Connections.Add(spring);
			this.tackleObject.Springs.Add(spring);
			Mass mass3 = new Mass(this.RodSlot.Sim, mass, this.LineTipMass.Position - new Vector3(0f, size, 0f), Mass.MassType.Bobber)
			{
				Buoyancy = Mathf.Clamp((sinkerMass + baitMass - mass) / mass, 3f, bobberBuoyancy * 0.5f),
				WaterDragConstant = 40f,
				AirDragConstant = 0.005f
			};
			base.Masses.Add(mass3);
			this.tackleObject.Masses.Add(mass3);
			Spring spring2 = new Spring(mass2, mass3, 500f, size, 0.002f)
			{
				IsRepulsive = true
			};
			base.Connections.Add(spring2);
			this.tackleObject.Springs.Add(spring2);
			FloatBehaviour floatBehaviour = this.RodSlot.Tackle as FloatBehaviour;
			Mass mass4 = mass3;
			float num = floatBehaviour.UserSetLeaderLength / this.MinLineLength * 0.05f;
			this.leaderObject = this.BuildLine(PhyObjectType.Leader, floatBehaviour.UserSetLeaderLength, mass4, direction, num, 2E-05f, Mass.MassType.Leader, -1f, null, 0);
			Mass mass5 = this.leaderObject.Masses[this.leaderObject.Masses.Count - 1];
			Mass mass6 = mass5;
			Leader leader = floatBehaviour.RodAssembly.LeaderInterface as Leader;
			if (leader != null)
			{
				Mass mass7 = mass5;
				if (floatBehaviour.Swivel != null)
				{
					Mass mass8 = new Mass(this.RodSlot.Sim, 2E-05f, mass5.Position, Mass.MassType.Line)
					{
						Buoyancy = 0.02f,
						Collision = Mass.CollisionType.Full
					};
					base.Masses.Add(mass8);
					this.lineObject.Masses.Add(mass8);
					this.tackleObject.Masses.Add(mass8);
					Spring spring3 = new Spring(mass5, mass8, 500f, 0f, 0.002f);
					base.Connections.Add(spring3);
					this.tackleObject.swivelBody = this.AttachRigidBody(mass8, floatBehaviour.Swivel, 0.005f, Mass.MassType.Swivel, out mass7);
					this.tackleObject.swivelBody.Buoyancy *= 0.2f;
					floatBehaviour.leaderEndAnchor = floatBehaviour.Swivel.topLineAnchor;
					floatBehaviour.leashHitchAnchor = floatBehaviour.Swivel.bottomLineAnchor;
				}
				float num2 = Mathf.Clamp(leader.LeaderLength * 0.25f, 0.05f, 0.32f);
				this.leashObject = this.BuildLine(PhyObjectType.Leash, num2, mass7, direction, 0.05f, 2E-05f, Mass.MassType.Leash, 0.02f, null, 0);
				Mass mass9 = this.leashObject.Masses[this.leashObject.Masses.Count - 1];
				mass6 = mass9;
				if (floatBehaviour.Shackle != null)
				{
					floatBehaviour.leashEndAnchor = floatBehaviour.Shackle.topLineAnchor;
				}
			}
			int count = this.leaderObject.Masses.Count;
			this.sinkerObject = new PhyObject(PhyObjectType.Sinker, this.RodSlot.Sim);
			if (count >= 6)
			{
				this.sinkerObject.Masses.AddRange(new Mass[]
				{
					this.leaderObject.Masses[count - 3],
					this.leaderObject.Masses[count - 2],
					this.leaderObject.Masses[count - 1]
				});
			}
			else if (count >= 4)
			{
				this.sinkerObject.Masses.AddRange(new Mass[]
				{
					this.leaderObject.Masses[count - 3],
					this.leaderObject.Masses[count - 2],
					this.leaderObject.Masses[count - 1]
				});
			}
			this.sinkerObject.Masses[0].MassValue = sinkerMass * 0.5f;
			this.sinkerObject.Masses[1].MassValue = sinkerMass * 0.35f;
			this.sinkerObject.Masses[2].MassValue = sinkerMass * 0.15f;
			this.sinkerObject.Masses[0].Type = Mass.MassType.Sinker;
			this.sinkerObject.Masses[1].Type = Mass.MassType.Sinker;
			this.sinkerObject.Masses[2].Type = Mass.MassType.Sinker;
			this.hookObject = new SpringDrivenObject(PhyObjectType.Hook, this.RodSlot.Sim, null);
			Mass mass10 = new Mass(this.RodSlot.Sim, 0.001f, mass6.Position, Mass.MassType.Hook);
			base.Masses.Add(mass10);
			this.hookObject.Masses.Add(mass10);
			Spring spring4 = new Spring(mass6, mass10, 500f, 0f, 0.002f);
			base.Connections.Add(spring4);
			this.hookObject.Springs.Add(spring4);
			VerletMass verletMass = new VerletMass(this.RodSlot.Sim, 0.001f + baitMass, mass6.Position, Mass.MassType.Hook)
			{
				Radius = 0.05f,
				IgnoreEnvironment = false,
				Buoyancy = baitBuoyancy,
				AirDragConstant = 0.001f * (1f + baitMass * 1000f)
			};
			verletMass.StopMass();
			base.Masses.Add(verletMass);
			this.hookObject.Masses.Add(verletMass);
			Spring spring5 = new Spring(mass10, verletMass, 500f, hookSize, 0.002f);
			base.Connections.Add(spring5);
			this.hookObject.Springs.Add(spring5);
			this.TackleTipMass = verletMass;
			this.LineTensionDetectorMass = this.LineTipMass;
			base.Objects.Add(this.tackleObject);
			base.Objects.Add(this.leaderObject);
			base.Objects.Add(this.hookObject);
			base.Objects.Add(this.sinkerObject);
			if (this.leashObject != null)
			{
				base.Objects.Add(this.leashObject);
			}
			this.RefreshObjectArrays(true);
		}

		public Magnet AddMagnet(PhyObject fishObject)
		{
			if (this.magnetObject != null)
			{
				throw new Exception("Magnet already exist");
			}
			if (this.hookObject != null)
			{
				Mass mass = this.hookObject.Masses[0];
				this.magnetObject = new PhyObject(PhyObjectType.Magnet, this.RodSlot.Sim);
				Mass mass2 = fishObject.Masses[0];
				this.magnetObject.Masses.Add(mass2);
				Magnet magnet = new Magnet(mass2, mass, 25f, 0.03f, 25f, 0.06f);
				this.magnetObject.Connections.Add(magnet);
				base.Connections.Add(magnet);
				base.Objects.Add(this.magnetObject);
				this.RefreshObjectArrays(true);
				return magnet;
			}
			return null;
		}

		public void DestroyMagnet(Magnet magnet)
		{
			if (this.magnetObject == null)
			{
				return;
			}
			if (this.magnetObject.Connections[0] != magnet)
			{
				throw new InvalidOperationException("A try to destroy unknown magnet");
			}
			magnet.ClearReferenceMass();
			base.RemoveConnection(magnet);
			base.RemoveObject(this.magnetObject);
			this.magnetObject = null;
			this.RefreshObjectArrays(true);
		}

		public VerletFishBody AddVerletFish(Vector3 mouthPosition, float templateLength, float mass, float waveAmp, float waveFreq, Vector3 waveAxis)
		{
			int num = 5;
			float num2 = templateLength / (float)num / (Mathf.Sqrt(6f) / 3f);
			return new VerletFishBody(this, mass, mouthPosition, Vector3.right, Vector3.up, Vector3.forward, num2, num, waveAmp, waveFreq, waveAxis, new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }, new float[] { 0.1f, 0.02f, 0.02f, 0.01f }, new float[] { 0.003f, 0.003f, 0.003f, 0f });
		}

		protected void FloatHookAutoreel(float minLineLength)
		{
			float num = (this.RodSlot.Rod.CurrentTipPosition - this.LineTipMass.Position).magnitude;
			float linePhysicsLength = (this.RodSlot.Line as Line1stBehaviour).GetLinePhysicsLength();
			num = Mathf.Max(num, minLineLength + 1f);
			if (num < this.CurrentLineLength)
			{
				(this.RodSlot.Line as Line1stBehaviour).GradualFinalLineLengthChange(num, 30);
			}
			Debug.Log(string.Concat(new object[] { "FloatHookAutoreel ", num, " < ", this.CurrentLineLength, " phy = ", linePhysicsLength }));
		}

		public void AutoTension()
		{
			if (this.RodTipMass != null && this.LineTensionDetectorMass != null)
			{
				float magnitude = (this.RodTipMass.Position - this.LineTensionDetectorMass.Position).magnitude;
				float num = this.CurrentLineTensionSegmentLength + this.AutoTensionsBias;
				float num2 = this.FinalLineLength;
				if (Mathf.Abs(magnitude - num) > this.AutoTensionsEpsillon)
				{
					num2 += Time.deltaTime * Mathf.Clamp((magnitude - num) * this.AutoTensionsSpeedFactor, -this.AutoTensionsMaxSpeed, this.AutoTensionsMaxSpeed);
				}
				if (this.FinalLineLength > this.RodSlot.Line.MinLineLength && num2 > this.RodSlot.Line.MinLineLength)
				{
					this.FinalLineLength = num2;
				}
			}
		}

		public void ReplacedHookedFishObject(PhyObject fishObject)
		{
			this.HookedFishObject = fishObject;
			(this.HookedFishObject as AbstractFishBody).HookState = AbstractFishBody.HookStateEnum.HookedSwimming;
			if (this.RodSlot.SimThread != null)
			{
				float adaptiveLineSegmentMass = this.AdaptiveLineSegmentMass;
				this.RodSlot.SimThread.UpdateLineParams(adaptiveLineSegmentMass, 1000f);
			}
			else
			{
				this.UpdateLineMass(this.AdaptiveLineSegmentMass);
			}
			if (this.isLureTackle)
			{
				for (int i = 0; i < this.tackleObject.Masses.Count; i++)
				{
					Mass mass = this.tackleObject.Masses[i];
					mass.Buoyancy = 0f;
					mass.BuoyancySpeedMultiplier = 0f;
				}
			}
			this.TurnLimitsOn(true);
		}

		public void HookFish(PhyObject fishObject)
		{
			(this.RodSlot.Rod as Rod1stBehaviour).TriggerHapticPulseOnRod(0.35f, 0.5f);
			if (this.HookedFishObject != null)
			{
				throw new InvalidOperationException("Can't hook new fish before unhooking prior one");
			}
			if (this.Player != null && this.RodSlot.Tackle is Float1stBehaviour)
			{
				this.FloatHookAutoreel((!this.Player.IsPitching) ? this.RodSlot.Line.MinLineLength : this.RodSlot.Line.MinLineLengthOnPitch);
			}
			this.RodSlot.SuspendReelClip();
			this.HookedFishObject = fishObject;
			if (this.RodSlot.SimThread != null)
			{
				float adaptiveLineSegmentMass = this.AdaptiveLineSegmentMass;
				this.RodSlot.SimThread.UpdateLineParams(adaptiveLineSegmentMass, 1000f);
			}
			else
			{
				this.UpdateLineMass(this.AdaptiveLineSegmentMass);
			}
			(fishObject as AbstractFishBody).HookState = AbstractFishBody.HookStateEnum.HookedSwimming;
			this.IsHorizontalStabilizationOn = true;
			if (this.isLureTackle)
			{
				for (int i = 0; i < this.tackleObject.Masses.Count; i++)
				{
					Mass mass = this.tackleObject.Masses[i];
					mass.Buoyancy = -1f;
					mass.BuoyancySpeedMultiplier = 0f;
				}
				(this.tackleObject as TackleObject).HookFish((fishObject as AbstractFishBody).TotalMass, 0f);
			}
			AbstractFishBody abstractFishBody = fishObject as AbstractFishBody;
			Mass mass2;
			if (this.tackleObject is RigidBodyTackleObject && !(this.tackleObject is FeederTackleObject) && !(this.tackleObject is RigTackleObject))
			{
				RigidBody rigidBody = (this.TackleTipMass as PointOnRigidBody).RigidBody;
				VerletMass verletMass = new VerletMass(this.RodSlot.Sim, rigidBody.MassValue, rigidBody.Position, Mass.MassType.Unknown)
				{
					Radius = 0.05f,
					Collision = Mass.CollisionType.Full,
					IgnoreEnvironment = true
				};
				verletMass.StopMass();
				rigidBody.PriorSpring.Mass1.NextSpring = null;
				Spring spring = new Spring(rigidBody.PriorSpring.Mass1, verletMass, 500f, (rigidBody.PriorSpring.Mass1.Position - rigidBody.Position).magnitude, 0.002f);
				base.Masses.Add(verletMass);
				base.Connections.Add(spring);
				rigidBody.DisableSimulation = true;
				this.TackleTipMass.DisableSimulation = true;
				this.TackleTipMass = verletMass;
				this.tackleObject.Masses[0] = verletMass;
				Vector4f vector4f = Vector4f.Zero;
				vector4f = (verletMass.Position - fishObject.Masses[0].Position).AsPhyVector(ref vector4f);
				abstractFishBody.groundAlign(ref vector4f);
				abstractFishBody.KinematicTranslate(vector4f);
				abstractFishBody.StopMasses();
				mass2 = abstractFishBody.ReplaceFirstMass(verletMass);
				verletMass.KinematicTranslate(vector4f);
			}
			else
			{
				Vector4f vector4f2 = Vector4f.Zero;
				vector4f2 = (this.TackleTipMass.Position - fishObject.Masses[0].Position).AsPhyVector(ref vector4f2);
				abstractFishBody.groundAlign(ref vector4f2);
				abstractFishBody.KinematicTranslate(vector4f2);
				abstractFishBody.StopMasses();
				mass2 = abstractFishBody.ReplaceFirstMass(this.TackleTipMass as VerletMass);
				(this.TackleTipMass as VerletMass).KinematicTranslate(vector4f2);
			}
			if (this.tackleObject is FeederTackleObject)
			{
				FeederTackleObject feederTackleObject = this.tackleObject as FeederTackleObject;
				feederTackleObject.RigidBody.AxialWaterDragEnabled = false;
			}
			base.RemoveMass(mass2);
			if (this.isLureTackle)
			{
				for (int j = 0; j < this.tackleObject.Masses.Count; j++)
				{
					Mass mass3 = this.tackleObject.Masses[j];
					mass3.Buoyancy = 0f;
					mass3.BuoyancySpeedMultiplier = 0f;
				}
			}
			this.TurnLimitsOn(true);
		}

		public void EscapeFish(PhyObject fishObject)
		{
			if (this.HookedFishObject != fishObject)
			{
				return;
			}
			this.HookedFishObject = null;
			this.IsHorizontalStabilizationOn = false;
			if (this.isLureTackle)
			{
				for (int i = 0; i < this.tackleObject.Masses.Count; i++)
				{
					Mass mass = this.tackleObject.Masses[i];
					mass.Buoyancy = this.lureBuoyancy;
					mass.BuoyancySpeedMultiplier = this.lureBuoyancySpeedMultiplier;
				}
				(this.tackleObject as TackleObject).EscapeFish();
			}
			VerletMass verletMass = new VerletMass(this.RodSlot.Sim, this.TackleTipMass.MassValue, this.TackleTipMass.Position, Mass.MassType.Fish)
			{
				Sim = this,
				Buoyancy = 0f,
				IgnoreEnvironment = true,
				Collision = Mass.CollisionType.Full,
				SlidingFrictionFactor = 0.01f,
				StaticFrictionFactor = 0.01f
			};
			verletMass.StopMass();
			base.Masses.Add(verletMass);
			if (this.tackleObject is RigidBodyTackleObject && !(this.tackleObject is FeederTackleObject) && !(this.tackleObject is RigTackleObject))
			{
				Mass mass2 = (this.tackleObject as RigidBodyTackleObject).RigidBody.PriorSpring.Mass1;
				base.RemoveMass(this.TackleTipMass);
				base.RemoveConnection(mass2.NextSpring);
				this.tackleObject.Masses[0].DisableSimulation = false;
				this.tackleObject.Masses[1].DisableSimulation = false;
				base.Connections.Add(this.tackleObject.Springs[0]);
				mass2.NextSpring = this.tackleObject.Springs[0];
				this.tackleObject.Masses[0].Position = this.TackleTipMass.Position;
				this.tackleObject.StopMasses();
				this.TackleTipMass = this.tackleObject.Masses[1];
			}
			else
			{
				this.TackleTipMass.MassValue = 0.001f + this.baitMass;
				this.TackleTipMass.Buoyancy = this.baitBuoyancy;
				this.TackleTipMass.IsRef = false;
				this.TackleTipMass.Motor = Vector3.zero;
				this.TackleTipMass.WaterMotor = Vector3.zero;
				this.TackleTipMass.Collision = Mass.CollisionType.Full;
				this.TackleTipMass.SlidingFrictionFactor = 0.25f;
				this.TackleTipMass.StaticFrictionFactor = 0.3f;
				this.TackleTipMass.Radius = 0.05f;
				this.StopObject(this.tackleObject);
			}
			if (this.tackleObject is FeederTackleObject)
			{
				FeederTackleObject feederTackleObject = this.tackleObject as FeederTackleObject;
				feederTackleObject.RigidBody.AxialWaterDragEnabled = true;
			}
			Mass mass3 = (fishObject as AbstractFishBody).ReplaceFirstMass(verletMass);
			if (mass3.PriorSpring != null)
			{
				mass3.PriorSpring.ImpulseVerletMax2 = 100f;
			}
			if (this.RodSlot.SimThread != null)
			{
				this.RodSlot.SimThread.UpdateLineParams(2E-05f, 500f);
			}
			else
			{
				this.UpdateLineMass(2E-05f);
			}
			this.HorizontalRealignLineMasses();
			this.RefreshObjectArrays(true);
			this.TurnLimitsOn(false);
		}

		public void DestroyFish(PhyObject fishObject)
		{
			if (fishObject == null)
			{
				return;
			}
			if (fishObject.Masses[0] == this.TackleTipMass)
			{
				this.TackleTipMass.MassValue = 0.001f + this.baitMass;
				this.TackleTipMass.Buoyancy = this.baitBuoyancy;
				this.TackleTipMass.IsRef = false;
				this.TackleTipMass.Motor = Vector3.zero;
				this.TackleTipMass.WaterMotor = Vector3.zero;
				this.TackleTipMass.Collision = Mass.CollisionType.Full;
				this.TackleTipMass.SlidingFrictionFactor = 0.25f;
				this.TackleTipMass.StaticFrictionFactor = 0.3f;
				this.TackleTipMass.Radius = 0.05f;
				this.StopObject(this.tackleObject);
				VerletMass verletMass = new VerletMass(this.RodSlot.Sim, this.TackleTipMass.MassValue, this.TackleTipMass.Position, Mass.MassType.Fish)
				{
					Collision = Mass.CollisionType.Full
				};
				fishObject.Masses[0] = verletMass;
			}
			fishObject.Remove();
			if (this.tackleObject != null && this.isLureTackle)
			{
				for (int i = 0; i < this.tackleObject.Masses.Count; i++)
				{
					Mass mass = this.tackleObject.Masses[i];
					mass.Buoyancy = this.lureBuoyancy;
					mass.BuoyancySpeedMultiplier = this.lureBuoyancySpeedMultiplier;
				}
			}
			if (this.HookedFishObject == fishObject)
			{
				if (this.RodSlot.SimThread != null)
				{
					this.RodSlot.SimThread.UpdateLineParams(2E-05f, 500f);
				}
				else
				{
					this.UpdateLineMass(2E-05f);
				}
			}
			this.RefreshObjectArrays(true);
		}

		public void DestroyItem(PhyObject itemObject)
		{
			if (itemObject != null)
			{
				itemObject.Remove();
				this.RefreshObjectArrays(true);
			}
		}

		public void ResetTransformCycle()
		{
			this.RodTipMass.ResetAvgForce();
			this.HandsMass.ResetAvgForce();
			if (this.LineTipMass != null)
			{
				this.LineTipMass.ResetAvgForce();
			}
			this.currentTime = 0f;
			this.initialLineLength = this.CurrentLineLength;
			this.initialTacklePosition = this.CurrentTacklePosition;
			if (!this.RodKinematicSplineMovementEnabled)
			{
				base.Masses[0].Position = this.RodPosition;
				base.Masses[0].Rotation = this.RodRotation;
			}
		}

		protected override void Solve()
		{
			bool flag = false;
			if (this.omittedImpulseCompensation < 5)
			{
				this.omittedImpulseCompensation++;
			}
			else
			{
				this.omittedImpulseCompensation = 0;
				flag = this.lineObject != null && this.RodTipMass != null && this.TackleTipMass != null && this.LineTipMass != null && this.RodSlot.Tackle != null && !this.fishIsShowing && !this.RodSlot.Tackle.ItemIsShowing && this.Player != null && this.Player.State != typeof(PlayerShowFishOut) && this.Player.State != typeof(PlayerShowFishLineOut) && this.Player.State != typeof(PlayerShowFishLineIn);
			}
			Vector3 vector = Vector3.zero;
			if (flag)
			{
				float num = this.RodTipMass.Position4f.Distance(this.LineTipMass.Position4f) + 0.05f;
				if (num > this.CurrentLineLength)
				{
					float num2;
					if (Mathf.Approximately(this.PriorLineLength, this.CurrentLineLength))
					{
						num2 = this.CurrentLineLength;
					}
					else
					{
						num2 = Mathf.Lerp(this.PriorLineLength, this.CurrentLineLength, base.FrameProgress);
					}
					bool flag2 = false;
					Spring priorSpring = this.LineTipMass.PriorSpring;
					Mass lineTipMass = this.LineTipMass;
					num = this.RodTipMass.Position4f.Distance(lineTipMass.Position4f);
					float num3 = 1.005f;
					float num4 = num2;
					num4 *= num3;
					if (flag2)
					{
						num4 += 0.015f;
					}
					if (num - num4 > 0f)
					{
						bool flag3 = this.TackleTipMass.IsKinematic;
						if (!flag3)
						{
							PointOnRigidBody pointOnRigidBody = this.TackleTipMass as PointOnRigidBody;
							if (pointOnRigidBody != null)
							{
								flag3 = pointOnRigidBody.RigidBody.IsKinematic;
							}
						}
						float num5 = 0.05f;
						float num6 = (num - num4) * num5;
						float num7 = lineTipMass.CurrentVelocityLimit * 0.0004f;
						if (!flag3)
						{
							num7 *= 0.15f;
						}
						num4 = num - ((num6 >= num7) ? num7 : num6);
						if (!this.RodTipMass.IsKinematic || !lineTipMass.IsKinematic)
						{
							float num8 = Spring.SatisfyImpulseConstraintMass(priorSpring, this.RodTipMass, lineTipMass, num4, Vector4f.Zero, priorSpring.ImpulseThreshold2 * 6f, flag3);
							vector += (lineTipMass.Position - this.RodTipMass.Position).normalized * num8 * Simulation.InvTimeQuant4f.X;
						}
					}
				}
			}
			if (this.LineTipMass != null && this.LineTipMass.PriorSpring != null)
			{
				Vector3 vector2 = (this.LineTipMass.PriorSpring.Mass2.Position - this.LineTipMass.PriorSpring.Mass1.Position).normalized * this.LineTipMass.PriorSpring.Tension * Simulation.InvTimeQuant4f.X;
				this.LineTipMass.UpdateAvgForce((vector2 + vector).AsPhyVector());
			}
			base.Solve();
			if (this.magnetObject != null)
			{
				this.magnetObject.Connections[0].Solve();
			}
		}

		protected override void Simulate(float dt)
		{
			if (this.simCycleNumber == 0)
			{
				if (this.isLureTackle && this.TackleTipMass != null)
				{
					this.SimulateFishAttackForceImpulse(dt);
				}
				if (this.TackleTipMass != null && this.tackleObject != null)
				{
					this.UpdateTopWaterStrikeSignal();
				}
			}
			base.Simulate(dt);
			this.currentTime += dt;
			this.transformLerpValue = base.FrameProgress;
			if (this.simCycleNumber == 0)
			{
			}
			this.simCycleNumber++;
			if (this.simCycleNumber >= 15)
			{
				this.simCycleNumber = 0;
			}
		}

		private PhyObject syncObject(PhyObject dest, PhyObject source)
		{
			if (source == null)
			{
				return null;
			}
			PhyObject phyObject;
			if ((dest == null || dest.UID != source.UID) && base.DictObjects.TryGetValue(source.UID, out phyObject))
			{
				return phyObject;
			}
			return dest;
		}

		private Mass syncMass(Mass dest, Mass source)
		{
			if (source == null)
			{
				return null;
			}
			Mass mass;
			if ((dest == null || dest.UID != source.UID) && this.DictMasses.TryGetValue(source.UID, out mass))
			{
				return mass;
			}
			return dest;
		}

		private ConnectionBase syncConnection(ConnectionBase dest, ConnectionBase source)
		{
			if (source == null)
			{
				return null;
			}
			ConnectionBase connectionBase;
			if ((dest == null || dest.UID != source.UID) && this.DictConnections.TryGetValue(source.UID, out connectionBase))
			{
				return connectionBase;
			}
			return dest;
		}

		public void Sync(FishingRodSimulation sourceSim)
		{
			this.RodSlot = sourceSim.RodSlot;
			this.lureBuoyancy = sourceSim.lureBuoyancy;
			this.lureBuoyancySpeedMultiplier = sourceSim.lureBuoyancySpeedMultiplier;
			this.IsHorizontalStabilizationOn = sourceSim.IsHorizontalStabilizationOn;
			this.fishSize = sourceSim.fishSize;
			this.fishIsShowing = false;
			if (this.RodSlot != null && this.RodSlot.Tackle != null)
			{
				this.fishIsShowing = this.RodSlot.Tackle.FishIsShowing;
			}
			this.tackleObject = this.syncObject(this.tackleObject, sourceSim.tackleObject) as SpringDrivenObject;
			this.lineObject = this.syncObject(this.lineObject, sourceSim.lineObject) as SpringDrivenObject;
			this.leaderObject = this.syncObject(this.leaderObject, sourceSim.leaderObject) as SpringDrivenObject;
			this.leashObject = this.syncObject(this.leashObject, sourceSim.leashObject) as SpringDrivenObject;
			this.hookObject = this.syncObject(this.hookObject, sourceSim.hookObject) as SpringDrivenObject;
			this.magnetObject = this.syncObject(this.magnetObject, sourceSim.magnetObject);
			this.HookedFishObject = this.syncObject(this.HookedFishObject, sourceSim.HookedFishObject);
			this.sinkerObject = this.syncObject(this.sinkerObject, sourceSim.sinkerObject);
			this.rodObject = this.syncObject(this.rodObject, sourceSim.rodObject) as RodObjectInHands;
			this.TackleTipMass = this.syncMass(this.TackleTipMass, sourceSim.TackleTipMass);
			this.RodTipMass = this.syncMass(this.RodTipMass, sourceSim.RodTipMass);
			this.LineTipMass = this.syncMass(this.LineTipMass, sourceSim.LineTipMass);
			this.LineFakeMass = this.syncMass(this.LineFakeMass, sourceSim.LineFakeMass);
			this.HandsMass = this.syncMass(this.HandsMass, sourceSim.HandsMass);
			this.LineTensionDetectorMass = this.syncMass(this.LineTensionDetectorMass, sourceSim.LineTensionDetectorMass);
			this.LineTipNextSpring = (Spring)this.syncConnection(this.LineTipNextSpring, sourceSim.LineTipNextSpring);
			this.CurrentLineLength = sourceSim.CurrentLineLength;
			this.PriorLineLength = sourceSim.PriorLineLength;
			this.FinalLineLength = sourceSim.FinalLineLength;
			this.FinalTacklePosition = sourceSim.FinalTacklePosition;
			this.tackleImpulseThreshold = sourceSim.tackleImpulseThreshold;
			this.tackleLyingImpulseThreshold = sourceSim.tackleLyingImpulseThreshold;
			this.lineImpulseThreshold = sourceSim.lineImpulseThreshold;
			this.lineLyingImpulseThreshold = sourceSim.lineLyingImpulseThreshold;
		}

		public void ApplyMoveAndRotateToRod(Vector3 rootPosition, Quaternion rootRotation)
		{
			Vector3? vector = this.priorRootPosition;
			if (vector != null)
			{
				Quaternion? quaternion = this.priorRootRotation;
				if (quaternion != null)
				{
					Vector3? vector2 = this.priorRootPosition;
					if (vector2 != null)
					{
						new Vector3?(rootPosition - vector2.GetValueOrDefault());
					}
					else
					{
						Vector3? vector3 = null;
					}
					Quaternion quaternion2 = rootRotation * Quaternion.Inverse(this.priorRootRotation.Value);
				}
			}
			this.priorRootPosition = new Vector3?(rootPosition);
			this.priorRootRotation = new Quaternion?(rootRotation);
		}

		private void CompensateRodRootMoveAndRotation(Vector3 rootPosition, Vector3 rootPositionDelta, Quaternion rootRotationDelta)
		{
			if (this.rodObject == null)
			{
				return;
			}
			Vector3 vector = Vector3.zero;
			if (rootPositionDelta != Vector3.zero || rootRotationDelta != Quaternion.identity)
			{
				for (int i = 1; i < this.rodObject.Masses.Count; i++)
				{
					Mass mass = this.rodObject.Masses[i];
					vector = rootPositionDelta;
					Vector3 vector2 = mass.Position - rootPosition;
					Vector3 vector3 = rootRotationDelta * vector2;
					vector += Vector3.Lerp(Vector3.zero, vector3 - vector2, 0.99f);
					mass.Position += vector;
					mass.Rotation *= rootRotationDelta;
				}
				int num = Mathf.Min((this.HookedFishObject != null) ? (base.Masses.Count - 3) : (base.Masses.Count - 1), this.firstLineMassIndex + 40);
				if (this.TackleMoveCompensationMode == TackleMoveCompensationMode.PitchIdle)
				{
					if (rootPositionDelta.magnitude > 0.05f && rootRotationDelta.eulerAngles.magnitude < 0.01f)
					{
						rootPositionDelta *= 1.2f;
					}
					for (int j = this.firstLineMassIndex; j <= num; j++)
					{
						Mass mass2 = base.Masses[j];
						if (mass2.Position.y < 0.01f || mass2.IsKinematic || mass2 is VerletMass || mass2.IsLying)
						{
							break;
						}
						vector = rootPositionDelta;
						Vector3 vector4 = mass2.Position - rootPosition;
						Vector3 vector5 = rootRotationDelta * vector4;
						vector += Vector3.Lerp(Vector3.zero, vector5 - vector4, 0.99f);
						mass2.Position += vector;
					}
				}
				else if (this.TackleMoveCompensationMode == TackleMoveCompensationMode.TackleCasted)
				{
					for (int k = this.firstLineMassIndex; k <= num; k++)
					{
						Mass mass3 = base.Masses[k];
						if (mass3.Position.y < 0.01f || mass3.IsKinematic || mass3 is VerletMass || mass3.IsLying)
						{
							break;
						}
						base.Masses[k].Position += Vector3.Lerp(Vector3.zero, vector, 0.8f);
					}
				}
			}
		}

		public override void Update(float dt)
		{
			base.Update(dt);
			if (this.IsMain)
			{
				this.UpdateBeforeSync();
				this.UpdateAfterSync();
			}
		}

		public void UpdateBeforeSync()
		{
			if (this.CurrentLineLength != this.FinalLineLength || (this.IsOnPod ^ (this.lineObject.Springs.Count == 1)))
			{
				if (!this.IsOnPod)
				{
					this.UpdateNonUniformLine();
				}
				else
				{
					this.UpdateOneSegmentLine();
				}
			}
			if (this.plannedHorizontalRealignMasses)
			{
				this._horizontalRealignLineMasses();
			}
			if (this.isLureTackle && this.TackleTipMass != null)
			{
				this.UpdateTackleVelocityLimit();
			}
			this.UpdateTackleSpringMaxImpulse();
		}

		public void UpdateAfterSync()
		{
			this.UpdateForces();
		}

		private void UpdateForces()
		{
			this.RodAppliedForce = ((this.RodTipMass == null) ? Vector3.zero : this.RodTipMass.AvgForce);
			this.HandsAppliedForce = ((this.HandsMass == null) ? Vector3.zero : this.HandsMass.AvgForce);
			this.TackleApliedForce = ((this.TackleTipMass == null) ? Vector3.zero : this.TackleTipMass.AvgForce);
		}

		public void CompensateKinematicMassMovement(Mass kinematicLineMass, Vector3 actualDelta)
		{
			int num = base.Masses.IndexOf(kinematicLineMass);
			int num2 = this.firstLineMassIndex - 1;
			int num3 = num - num2;
			int num4 = 1;
			for (int i = num; i > num2; i--)
			{
				Mass mass = base.Masses[i];
				float num5 = (float)num4 / (float)num3;
				mass.Position += Vector3.Lerp(actualDelta, Vector3.zero, num5);
				num4++;
			}
		}

		public float FullLineLength
		{
			get
			{
				if (this.isLureTackle)
				{
					return this.FinalLineLength;
				}
				return this.FinalLineLength + this.bobberSize + this.leaderLength;
			}
		}

		private void AddSegments(int count)
		{
			for (int i = 0; i < count; i++)
			{
				this.AddSegment(0.05f, false);
			}
			this.RefreshObjectArrays(true);
		}

		private float RemoveSegments(int count)
		{
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				num += this.RemoveSegment(0.025f, false);
			}
			this.RefreshObjectArrays(true);
			return num;
		}

		private void UpdateOneSegmentLine()
		{
			if (this.lineObject.Springs.Count > 1)
			{
				this.RemoveSegments(this.lineObject.Springs.Count - 1);
				this.lineObject.Springs[0].SetSpringLengthImmediate(this.FinalLineLength);
			}
			else
			{
				this.lineObject.Springs[0].SpringLength = this.FinalLineLength;
			}
			this.CurrentLineTensionSegmentLength = this.FinalLineLength;
			this.RodSlot.Sim.LineTensionDetectorMass = this.lineObject.Springs[0].Mass2;
		}

		private void UpdateNonUniformLine()
		{
			float num = this.FinalLineLength / 0.05f;
			int count = this.lineObject.Masses.Count;
			int num2;
			float num3;
			if (num <= 1f)
			{
				num2 = Mathf.CeilToInt(num / 1f);
				num3 = 1f - ((float)num2 - num);
			}
			else
			{
				float num4 = num - 1f;
				float num5 = 0.8333333f;
				float num6 = 0.5f - num5 + Mathf.Sqrt((num5 - 0.5f) * (num5 - 0.5f) + 2f * num4 * num5);
				int num7 = Mathf.FloorToInt(num6);
				num3 = num6 - (float)num7;
				num2 = 2 + num7;
			}
			int num8 = 0;
			if (this.lineObject.Springs.Count < num2)
			{
				Vector3 normalized = (this.lineObject.Masses[1].Position - this.lineObject.Masses[0].Position).normalized;
				this.AddSegments(num2 - this.lineObject.Springs.Count);
				if (count < this.lineObject.Masses.Count)
				{
					for (int i = 1; i < this.lineObject.Masses.Count - count + 1; i++)
					{
						float num9;
						if (num8 == 0)
						{
							num9 = num3 * 0.05f;
						}
						else if (num8 <= 1)
						{
							num9 = 0.05f;
						}
						else
						{
							num9 = 0.05f * (1f + ((float)(num8 - 1 - 1) + num3) * 1.2f);
						}
						this.lineObject.Masses[i].PriorSpring.SetSpringLengthImmediate(num9);
						this.lineObject.Masses[i].Position = this.lineObject.Masses[i - 1].Position + num9 * normalized;
						num8++;
					}
				}
			}
			else if (this.lineObject.Springs.Count > num2)
			{
				float num10 = this.RodTipMass.NextSpring.Mass2.NextSpring.SpringLength;
				num10 += this.RemoveSegments(this.lineObject.Springs.Count - num2);
				this.RodTipMass.NextSpring.SetSpringLengthImmediate(num10);
			}
			num8 = 0;
			Spring spring = this.RodTipMass.NextSpring;
			while (spring != null && spring.Mass2.Type == Mass.MassType.Line)
			{
				if (num8 == 0)
				{
					spring.SpringLength = num3 * 0.05f;
				}
				else if (num8 <= 1)
				{
					spring.SpringLength = 0.05f;
				}
				else
				{
					spring.SpringLength = 0.05f * (1f + ((float)(num8 - 1 - 1) + num3) * 1.2f);
				}
				num8++;
				spring = spring.Mass2.NextSpring;
			}
			this.PriorLineLength = this.CurrentLineLength;
			this.CurrentLineLength = 0f;
			this.CurrentLineTensionSegmentLength = 0f;
			for (int j = 0; j < this.lineObject.Springs.Count; j++)
			{
				this.CurrentLineLength += this.lineObject.Springs[j].SpringLength;
				if (this.lineObject.Springs[j].Mass2 == this.LineTensionDetectorMass)
				{
					this.CurrentLineTensionSegmentLength = this.CurrentLineLength;
				}
			}
			float num11 = (float)this.lineObject.Springs.Count * 2E-05f * this.rodImpulseThresholdMult;
			this.lineImpulseThreshold = 0.02f * num11;
			this.lineLyingImpulseThreshold = 0.01f * num11;
		}

		private void SimulateLineLengthChange()
		{
			ObscuredFloat finalLineLength = this.FinalLineLength;
			float num = finalLineLength - this.CurrentLineLength;
			if (this.CurrentLineLength + num < this.MinLineLength)
			{
				num = this.MinLineLength - this.CurrentLineLength;
			}
			if (this.CurrentLineLength + num > this.MaxLineLength)
			{
				num = this.MaxLineLength - this.CurrentLineLength;
			}
			if (num == 0f)
			{
				return;
			}
			float num2 = num * 0.6f;
			float num3 = num * 0.39999998f;
			float num4 = num3 / (float)this.lineObject.Springs.Count;
			this.lineSegmentLength += num4;
			for (int i = 1; i < this.lineObject.Springs.Count; i++)
			{
				if (this.lineObject.Springs[i].Mass1.IsKinematic)
				{
					break;
				}
				this.lineObject.Springs[i].SpringLength = this.lineSegmentLength;
			}
			if (this.firstLineConnection.SpringLength + num2 < 0f)
			{
				float num5 = -(this.firstLineConnection.SpringLength + num2);
				this.RemoveSegment(num5, true);
			}
			else if (this.firstLineConnection.SpringLength + num2 > this.lineSegmentLength)
			{
				float num6 = this.firstLineConnection.SpringLength + num2 - this.lineSegmentLength;
				this.AddSegment(num6, true);
			}
			else
			{
				this.firstLineConnection.SpringLength += num2;
			}
			this.CurrentLineLength = 0f;
			for (int j = 0; j < this.lineObject.Springs.Count; j++)
			{
				this.CurrentLineLength += this.lineObject.Springs[j].SpringLength;
			}
		}

		private void AddSegment(float extendExcess, bool rebuildArrays = true)
		{
			float num = 10f;
			Vector3 vector = this.RodTipMass.Position + (this.firstLineMass.Position - this.RodTipMass.Position).normalized * extendExcess;
			Mass mass = new Mass(this.RodSlot.Sim, this.AdaptiveLineSegmentMass, vector, Mass.MassType.Line)
			{
				Buoyancy = this.firstLineMass.Buoyancy,
				BuoyancySpeedMultiplier = this.firstLineMass.BuoyancySpeedMultiplier,
				Motor = this.firstLineMass.Motor,
				Velocity = this.TackleTipMass.Velocity,
				AirDragConstant = 0.0001f,
				WaterDragConstant = num,
				CurrentVelocityLimit = this.CurrentVelocityLimit
			};
			Spring spring = new Spring(this.RodTipMass, mass, 500f, extendExcess, 0.002f);
			this.RodTipMass.NextSpring = spring;
			this.firstLineConnection.Mass1 = mass;
			mass.NextSpring = this.firstLineConnection;
			mass.PriorSpring = spring;
			this.firstLineConnection.SpringLength = this.lineSegmentLength;
			this.firstLineConnection = spring;
			this.firstLineMass = mass;
			base.Masses.Insert(this.firstLineMassIndex, mass);
			base.Connections.Insert(this.firstLineConnectionIndex, spring);
			this.lineObject.Masses.Insert(1, mass);
			this.lineObject.Springs.Insert(0, spring);
			if (rebuildArrays)
			{
				this.RefreshObjectArrays(true);
			}
		}

		private float RemoveSegment(float shrinkExcess, bool rebuildArrays = true)
		{
			float num = this.lineSegmentLength - shrinkExcess;
			Mass mass = base.Masses[this.firstLineMassIndex + 1];
			Spring spring = (Spring)base.Connections[this.firstLineConnectionIndex + 1];
			spring.Mass1 = this.RodTipMass;
			this.RodTipMass.NextSpring = spring;
			spring.SpringLength = num;
			this.firstLineConnection = spring;
			this.firstLineMass = mass;
			this.PhyActionsListener.MassDestroyed(base.Masses[this.firstLineMassIndex]);
			base.Masses.RemoveAt(this.firstLineMassIndex);
			this.PhyActionsListener.ConnectionDestroyed(base.Connections[this.firstLineConnectionIndex]);
			base.Connections.RemoveAt(this.firstLineConnectionIndex);
			this.lineObject.Masses.RemoveAt(1);
			float springLength = this.lineObject.Springs[0].SpringLength;
			this.lineObject.Springs.RemoveAt(0);
			if (rebuildArrays)
			{
				this.RefreshObjectArrays(true);
			}
			return springLength;
		}

		public void ResetLineToMinLength()
		{
			if (this.CurrentLineLength == this.MinLineLength)
			{
				int num = (int)(this.MinLineLength / 0.05f);
				if (this.lineObject.Springs.Count > num)
				{
					this.RemoveSegments(this.lineObject.Springs.Count - num);
				}
				else if (this.lineObject.Springs.Count < num)
				{
					this.AddSegments(num - this.lineObject.Springs.Count);
				}
				for (int i = 0; i < this.lineObject.Springs.Count; i++)
				{
					Spring spring = this.lineObject.Springs[i];
					spring.SpringLength = 0.05f;
				}
			}
			else
			{
				SpringDrivenObject springDrivenObject = this.BuildLine(this.MinLineLength);
				this.firstLineMass = springDrivenObject.Masses[1];
				this.firstLineConnection = springDrivenObject.Springs[0];
				for (int j = 1; j < this.lineObject.Masses.Count - 1; j++)
				{
					Mass mass = this.lineObject.Masses[j];
					base.RemoveMass(mass);
				}
				for (int k = 0; k < this.lineObject.Springs.Count; k++)
				{
					Spring spring2 = this.lineObject.Springs[k];
					base.RemoveConnection(spring2);
				}
				this.lineObject.Masses.Clear();
				this.lineObject.Springs.Clear();
				this.lineObject.Masses.Add(this.RodTipMass);
				int num2 = this.firstLineMassIndex;
				for (int l = 1; l < springDrivenObject.Masses.Count; l++)
				{
					Mass mass2 = springDrivenObject.Masses[l];
					base.Masses.Insert(num2, mass2);
					this.lineObject.Masses.Add(mass2);
					num2++;
				}
				Spring spring3 = (Spring)base.Connections[this.firstLineConnectionIndex];
				spring3.Mass1 = this.LineTipMass;
				this.LineTipMass.NextSpring = spring3;
				this.RodTipMass.NextSpring = this.firstLineConnection;
				this.lineObject.Masses.Add(spring3.Mass2);
				num2 = this.firstLineConnectionIndex;
				for (int m = 0; m < springDrivenObject.Springs.Count; m++)
				{
					Spring spring4 = springDrivenObject.Springs[m];
					base.Connections.Insert(num2, spring4);
					this.lineObject.Springs.Add(spring4);
					num2++;
				}
				this.CurrentLineLength = this.MinLineLength;
				this.FinalLineLength = this.MinLineLength;
				this.ForwardRealignMasses(num2);
				this.RodSlot.SimThread.GlobalReset();
				this.RefreshObjectArrays(true);
			}
		}

		public void SetLeaderLength(float length)
		{
			if (this.leaderObject == null || this.CurrentTackleType == FishingRodSimulation.TackleType.CarpMethod)
			{
				return;
			}
			this.leaderLength = length;
			float num = length / (float)this.leaderObject.Springs.Count;
			for (int i = 0; i < this.leaderObject.Springs.Count; i++)
			{
				Spring spring = this.leaderObject.Springs[i];
				spring.SpringLength = num;
			}
		}

		private void UpdateTopWaterStrikeSignal()
		{
			if (this.Player == null)
			{
				return;
			}
			Mass mass = null;
			if (this.DictMasses.TryGetValue(this.tackleObject.Masses[0].UID, out mass))
			{
				TopWaterLure topWaterLure = mass as TopWaterLure;
				if (topWaterLure != null)
				{
					bool isPulling = this.Player.IsPulling;
					if (isPulling && !this.wasStriking)
					{
						topWaterLure.Strike = true;
					}
					this.wasStriking = isPulling;
				}
			}
		}

		private void UpdateTackleSpringMaxImpulse()
		{
			if (this.tackleObject == null || this.RodSlot.Tackle == null || this.TackleTipMass == null || this.RodSlot.Tackle.Fish == null)
			{
				return;
			}
			ConnectionBase priorSpring = this.RodSlot.Tackle.Fish.HeadMass.PriorSpring;
			Spring spring = priorSpring as Spring;
			if (spring != null)
			{
				if (this.RodSlot.Tackle.Fish != null)
				{
					spring.ImpulseVerletMax2 = this.RodSlot.Reel.CurrentFrictionForce * 0.0004f;
				}
				else
				{
					spring.ImpulseVerletMax2 = 100f;
				}
			}
		}

		private void UpdateTackleVelocityLimit()
		{
			if (this.tackleObject == null || this.RodSlot.Tackle == null)
			{
				return;
			}
			ConnectionBase priorSpring = this.tackleObject.Masses[0].PriorSpring;
			Spring spring = priorSpring as Spring;
			if (spring == null)
			{
				return;
			}
			if (spring != null)
			{
				spring.ImpulseThreshold2 = 0f;
			}
			MassToRigidBodySpring massToRigidBodySpring = spring as MassToRigidBodySpring;
			float num = 1f;
			if (this.RodSlot.Tackle.IsFlying)
			{
				this.tackleObject.CurrentVelocityLimit = 100f;
			}
			else if (this.tackleObject.Masses[0].Position.y > 0.03f)
			{
				this.tackleObject.CurrentVelocityLimit = 50f;
			}
			else if (this.RodSlot.Tackle.Fish != null)
			{
				this.tackleObject.CurrentVelocityLimit = 100f;
				if (spring != null)
				{
					spring.ImpulseThreshold2 = this.tackleImpulseThreshold + this.lineImpulseThreshold;
				}
			}
			else
			{
				this.tackleObject.CurrentVelocityLimit = 50f;
				if (spring != null)
				{
					spring.ImpulseThreshold2 = ((!this.RodSlot.Tackle.IsLying) ? (this.lineImpulseThreshold + this.tackleImpulseThreshold) : (this.lineLyingImpulseThreshold + this.tackleLyingImpulseThreshold));
					num = Mathf.Min(Mathf.Exp(2f - this.CurrentLineLength) + this.TackleTipMass.Position.y * this.TackleTipMass.Position.y, 1f);
				}
			}
			if (this.RodSlot.Reel.IsReeling || this.wasReeling)
			{
			}
			this.wasReeling = this.RodSlot.Reel.IsReeling;
			if (massToRigidBodySpring != null)
			{
				massToRigidBodySpring.ImpulseVerticalFactor = num;
			}
		}

		private void SimulateFakeLineTension()
		{
			if (this.LineFakeMass == null)
			{
				return;
			}
			bool flag = this.FinalLineLength - this.CurrentLineLength > 0.001f;
			bool flag2 = this.FinalLineLength - this.CurrentLineLength < -0.001f;
			if (this.TackleTipMass.IsLying && flag)
			{
				this.reelOffHackFakeLineTensionLock = true;
			}
			if (!this.TackleTipMass.IsLying && flag2)
			{
				this.reelOffHackFakeLineTensionLock = false;
			}
			if (this.TackleTipMass.MassValue != 0.001f)
			{
				return;
			}
			if (this.CurrentLineLength < 5f)
			{
				this.LineFakeMass.WaterMotor = Vector3.zero;
			}
			else if (this.reelOffHackFakeLineTensionLock)
			{
				this.LineFakeMass.WaterMotor = (this.LineFakeMass.Position - this.RodTipMass.Position).normalized * (float)this.lineObject.Masses.Count * 1.962E-06f;
			}
			else if (this.TackleTipMass.IsLying)
			{
				this.LineFakeMass.WaterMotor = (this.LineFakeMass.Position - this.RodTipMass.Position).normalized * (float)this.lineObject.Masses.Count * 4.9050002E-05f;
			}
			else
			{
				Vector3 vector = (this.LineFakeMass.Position - this.RodTipMass.Position).normalized;
				float num = (float)this.lineObject.Masses.Count * 0.00019620001f;
				if (!flag2)
				{
					if (this.LineFakeMass.Position.y > this.TackleTipMass.Position.y)
					{
						vector += Vector3.down;
					}
					else
					{
						vector.y = 0f;
						vector.Normalize();
					}
				}
				this.LineFakeMass.WaterMotor = vector * num;
			}
		}

		public void UpdateLineMass(float massValue)
		{
			for (int i = 0; i < this.lineObject.Masses.Count; i++)
			{
				Mass mass = this.lineObject.Masses[i];
				if (mass.Type == Mass.MassType.Line)
				{
					mass.MassValue = massValue;
				}
			}
			if (this.leaderObject != null)
			{
				for (int j = 0; j < this.leaderObject.Masses.Count; j++)
				{
					Mass mass2 = this.leaderObject.Masses[j];
					if (mass2.Type == Mass.MassType.Leader)
					{
						mass2.MassValue = massValue;
					}
				}
			}
		}

		private void UpdateLineBuoyancyForLure()
		{
			if (this.lineObject == null)
			{
				return;
			}
			int num = 0;
			int num2 = this.lineObject.Masses.Count / 2;
			float num3 = this.lureBuoyancy;
			float num4 = this.lureMass * this.lureBuoyancySpeedMultiplier / (2E-05f * (float)this.lineObject.Masses.Count);
			if (this.HookedFishObject != null)
			{
				num3 = 0f;
				num4 = 0f;
			}
			for (int i = 0; i < this.lineObject.Masses.Count; i++)
			{
				Mass mass = this.lineObject.Masses[i];
				if (mass.Type == Mass.MassType.Line)
				{
					if (num > num2)
					{
						mass.Buoyancy = num3;
						mass.BuoyancySpeedMultiplier = num4 * (float)(num - num2) / (float)(this.lineObject.Masses.Count - num2);
					}
					else
					{
						mass.Buoyancy = 0.02f;
						mass.BuoyancySpeedMultiplier = 0f;
					}
					num++;
				}
			}
		}

		public override void RefreshObjectArrays(bool updateDict = true)
		{
			base.RefreshObjectArrays(updateDict);
		}

		private void SimulateFishAttackForceImpulse(float dt)
		{
			if (this.TackleTipMass.MassValue == 0.001f)
			{
				return;
			}
			if (!this.HasFishAttackForceImpulse)
			{
				return;
			}
			if (this.HasFishAttackForceImpulse && !this.hadFishAttackForceImpulse)
			{
				this.hadFishAttackForceImpulse = true;
				this.fishAttackImpulseTime = 0f;
			}
			this.fishAttackImpulseTime += dt;
			if (this.fishAttackImpulseTime > 0.3f)
			{
				this.HasFishAttackForceImpulse = false;
				this.hadFishAttackForceImpulse = false;
				this.LineFakeMass.WaterMotor = Vector3.zero;
				return;
			}
			this.LineFakeMass.WaterMotor = (this.LineFakeMass.Position - this.RodTipMass.Position).normalized * this.FishAttackImpulseForce;
		}

		public void TurnLimitsOff()
		{
			this.LimitsState = 0;
			this.CurrentVelocityLimit = 100f;
			if (this.PhyActionsListener != null)
			{
				this.PhyActionsListener.VelocityLimitChanged(this.CurrentVelocityLimit);
			}
		}

		public void TurnLimitsOn(bool withFish = false)
		{
			if (withFish)
			{
				this.LimitsState = 2;
				this.CurrentVelocityLimit = 100f;
				if (this.PhyActionsListener != null)
				{
					this.PhyActionsListener.VelocityLimitChanged(this.CurrentVelocityLimit);
				}
			}
			else
			{
				this.LimitsState = 1;
				this.CurrentVelocityLimit = 50f;
				if (this.PhyActionsListener != null)
				{
					this.PhyActionsListener.VelocityLimitChanged(this.CurrentVelocityLimit);
				}
			}
		}

		private void CheckAgainstTackleJumpOut()
		{
			if (!this.isLureTackle)
			{
				return;
			}
			if (this.TackleTipMass.MassValue != 0.001f)
			{
				return;
			}
			if (this.CurrentLineLength < this.rodLength)
			{
				return;
			}
			if (this.tackleObject.Masses.Count != 3)
			{
				return;
			}
			this.FixMassVelocity(this.tackleObject.Masses[0]);
			this.FixMassVelocity(this.tackleObject.Masses[1]);
			this.FixMassVelocity(this.tackleObject.Masses[2]);
		}

		private void FixMassVelocity(Mass m)
		{
			if (m.Velocity.y > 0f && m.Position.y > -0.1f && m.Velocity.magnitude > 45f)
			{
				m.Velocity = Vector3.zero;
			}
		}

		private void SimulateTackleFly()
		{
			this.CurrentTacklePosition = Vector3.Lerp(this.initialTacklePosition, this.FinalTacklePosition, 1f);
			Mass mass = this.tackleObject.Masses[0];
			Vector3 vector = this.CurrentTacklePosition - mass.Position;
			mass.Position = this.CurrentTacklePosition;
			float num = 0f;
			for (int i = 0; i < this.tackleObject.Masses.Count; i++)
			{
				Mass mass2 = this.tackleObject.Masses[i];
				if (mass2 != mass)
				{
					num += ((mass2.PriorSpring != null) ? mass2.PriorSpring.SpringLength : 0f);
					mass2.Position = this.CurrentTacklePosition - vector.normalized * num;
					mass2.Velocity = mass.Velocity;
				}
			}
		}

		public void RealignLineAfterThrow()
		{
			Mass mass = this.lineObject.Masses[this.lineObject.Masses.Count - 1];
			Mass mass2 = mass;
			for (int i = this.lineObject.Masses.Count - 3; i > 0; i--)
			{
				Mass mass3 = this.lineObject.Masses[i];
				Spring spring = this.lineObject.Springs[i - 1];
				Vector3 vector = mass2.Position - mass3.Position;
				float magnitude = vector.magnitude;
				Vector3 normalized = vector.normalized;
				Vector3 vector2 = normalized * (magnitude - spring.SpringLength);
				mass3.Position += vector2;
				mass2 = mass3;
			}
			float springLength = this.lineObject.Springs[1].SpringLength;
			float num = Vector3.Distance(this.lineObject.Masses[0].Position, this.lineObject.Masses[1].Position) - springLength;
			if (num > springLength)
			{
				int num2 = (int)(num / springLength);
				this.AddSegments(num2);
				this.RealignLineAfterThrow();
			}
		}

		private void ShiftPosition(Mass m, float factor)
		{
			Vector3 vector = this.RodTipMass.Position + (m.Position - this.RodTipMass.Position) * factor;
			float num = Math3d.GetGroundHight(vector);
			num += m.Radius;
			if (num > vector.y)
			{
				vector.y = num;
			}
			m.Position = vector;
		}

		public void RealignLineAfterHitch(bool isFeeder = false)
		{
			if (this.TackleTipMass == null || this.RodTipMass == null || this.RodTipMass.NextSpring == null)
			{
				return;
			}
			float num = this.FinalLineLength / (this.TackleTipMass.Position - this.RodTipMass.Position).magnitude;
			if (num >= 1f)
			{
				return;
			}
			Mass mass = this.RodTipMass.NextSpring.Mass2;
			while (mass != null)
			{
				this.ShiftPosition(mass, num);
				if (mass.NextSpring == null)
				{
					mass = null;
				}
				else
				{
					mass = mass.NextSpring.Mass2;
				}
			}
			if (isFeeder)
			{
				if (this.leaderObject != null)
				{
					foreach (Mass mass2 in this.leaderObject.Masses)
					{
						if (mass2.Type == Mass.MassType.Leader)
						{
							this.ShiftPosition(mass2, num);
						}
					}
				}
				if (this.hookObject != null)
				{
					foreach (Mass mass3 in this.hookObject.Masses)
					{
						this.ShiftPosition(mass3, num);
					}
				}
			}
		}

		public bool plannedHorizontalRealignMasses { get; private set; }

		public void HorizontalRealignLineMasses()
		{
			this.plannedHorizontalRealignMasses = true;
		}

		private void _horizontalRealignLineMasses()
		{
			Vector3 vector = Vector3.zero;
			this.rodObject.Masses[0].Rotation = this.RodRotation;
			this.rodObject.Masses[0].Position = this.RodPosition;
			for (int i = 0; i < this.rodObject.Connections.Count; i++)
			{
				ConnectionBase connectionBase = base.Connections[i];
				if (connectionBase is Bend)
				{
					Bend bend = (Bend)connectionBase;
					bend.Mass2.Rotation = this.RodRotation;
					bend.Mass2.Position = bend.UnbentPosition;
					bend.Mass2.StopMass();
					vector = bend.Mass2.Position;
				}
			}
			HashSet<int> hashSet = new HashSet<int>();
			hashSet.Add(this.RodTipMass.UID);
			for (Spring spring = this.RodTipMass.NextSpring; spring != null; spring = spring.Mass2.NextSpring)
			{
				if (hashSet.Contains(spring.Mass2.UID))
				{
					break;
				}
				hashSet.Add(spring.Mass2.UID);
				Vector3 normalized = (spring.Mass2.Position - spring.Mass1.Position).normalized;
				spring.Mass1.Position = vector;
				if (spring.Mass1.Position.y < spring.Mass1.GroundHeight)
				{
					spring.Mass1.Position += Vector3.up * (spring.Mass1.GroundHeight - spring.Mass1.Position.y);
				}
				vector += normalized * spring.SpringLength;
				spring.Mass2.Position = vector;
				if (spring.Mass2.Position.y < spring.Mass2.GroundHeight)
				{
					spring.Mass2.Position += Vector3.up * (spring.Mass2.GroundHeight - spring.Mass2.Position.y);
				}
				spring.Mass2.StopMass();
			}
			this.plannedHorizontalRealignMasses = false;
		}

		public void ForwardRealignMasses(int startIndex)
		{
			float num = 0f;
			for (int i = startIndex; i < base.Connections.Count; i++)
			{
				Spring spring = base.Connections[i] as Spring;
				if (spring != null)
				{
					num += spring.Mass2.MassValue;
				}
			}
			for (int j = startIndex; j < base.Connections.Count; j++)
			{
				Spring spring2 = base.Connections[j] as Spring;
				MassToRigidBodySpring massToRigidBodySpring = spring2 as MassToRigidBodySpring;
				if (spring2 != null)
				{
					if (massToRigidBodySpring == null)
					{
						spring2.Mass2.Position = spring2.Mass1.Position + Vector3.down * spring2.SpringLength;
					}
					else
					{
						massToRigidBodySpring.RigidBody.Rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
						massToRigidBodySpring.RigidBody.Position = spring2.Mass1.Position;
						massToRigidBodySpring.RigidBody.Torque = Vector4f.Zero;
						massToRigidBodySpring.RigidBody.AngularVelocity = Vector4f.Zero;
						massToRigidBodySpring.RigidBody.StopMass();
					}
					spring2.Mass2.Force = Vector3.zero;
					spring2.Mass2.Velocity = Vector3.zero;
					spring2.Mass2.Motor = Vector3.zero;
					spring2.Mass2.WaterMotor = Vector3.zero;
					num -= spring2.Mass2.MassValue;
				}
			}
			for (int k = 0; k < base.Masses.Count; k++)
			{
				PointOnRigidBody pointOnRigidBody = base.Masses[k] as PointOnRigidBody;
				if (pointOnRigidBody != null)
				{
					pointOnRigidBody.Simulate();
				}
			}
		}

		public void StopAllBodies()
		{
			for (int i = 0; i < base.Masses.Count; i++)
			{
				Mass mass = base.Masses[i];
				mass.Velocity = Vector3.zero;
			}
		}

		public void StopObject(PhyObject obj)
		{
			for (int i = 0; i < obj.Masses.Count; i++)
			{
				Mass mass = obj.Masses[i];
				mass.Velocity = Vector3.zero;
				mass.Motor = Vector3.zero;
				mass.WaterMotor = Vector3.zero;
			}
		}

		public override string ToString()
		{
			return string.Format("isLureTackle={0}, RodMovesCompensationEnabled={1} \r\n", (!this.isLureTackle) ? "N" : "Y", Enum.GetName(typeof(TackleMoveCompensationMode), this.TackleMoveCompensationMode));
		}

		public void RealignWholeModel()
		{
			for (int i = 0; i < base.Connections.Count; i++)
			{
				ConnectionBase connectionBase = base.Connections[i];
				if (connectionBase is Bend)
				{
					Bend bend = (Bend)connectionBase;
					bend.Mass2.Position = bend.UnbentPosition;
					bend.Mass2.Rotation = bend.Mass1.Rotation;
					bend.Mass2.Velocity = Vector3.zero;
				}
				if (connectionBase is Spring)
				{
					Spring spring = (Spring)connectionBase;
					spring.Mass2.Position = spring.Mass1.Position + Vector3.down * spring.SpringLength;
					spring.Mass2.Velocity = Vector3.zero;
				}
			}
			this.priorRootPosition = null;
			this.priorRootRotation = null;
		}

		public const float RodSpringConstant = 5000f;

		public bool MoveCompensationEnabled = true;

		public const float RodRotationCompensation = 0.99f;

		public static readonly Vector4f RodRotationCompensation4f = new Vector4f(0.99f);

		public const float RodTranslateCompensation = 0.8f;

		public static readonly Vector4f RodTranslateCompensation4f = new Vector4f(0.8f);

		public const float TackleMoveCompensationInPitch = 1.2f;

		public const float TackleMoveCompensationInCasted = 0.8f;

		public const int MaxCompensatedMasses = 40;

		public const float ConstraintStretchMultiplier = 1.5f;

		public const float MinConstraintStretch = 0.05f;

		public const float CompensateMinY = 0.5f;

		public const float CompensateMaxY = 1f;

		public const float CompensateMaxDistanceSqr = 12f;

		public const float MinLineLengthAfterAutoReel = 1f;

		public const float LineSpringConstant = 500f;

		public const float LineSpringConstantFishHooked = 1000f;

		public const float LineFrictionConstant = 0.002f;

		public const float InitialLineSegmentLength = 0.05f;

		public const float InitialLeaderSegmentLength = 0.05f;

		public const float LineSegmentMass = 2E-05f;

		public const float LineSegmentMassFishHooked = 8E-05f;

		public const float LineSegmentMassHitch = 0.0001f;

		public const float LineWaterDragConstant = 10f;

		public const float LineAirDragConstant = 0.0001f;

		public const float LureWaterDragConstant = 10f;

		public float MinLineLength = 2f;

		public float MaxLineLength = 100f;

		public const float LodFraction = 0.6f;

		public const float NormalLineBuoyancy = 0.02f;

		public const float SinkBuoyancy = -1f;

		public const float SinkerLength = 0.1f;

		public const float SinkerDistance = 0.04f;

		public const float SinkerMassQuota1 = 0.65f;

		public const float SinkerMassQuota2 = 0.35f;

		public const float BobberMinBuoyancy = 3f;

		public const float BobberAirDrag = 0.005f;

		public const float LineUnstrainedNoiseAmplitude = 0f;

		public const float LineUnstrainedNoiseFrequency = 4f;

		public const float LineGravityOverrideRatio = 1f;

		public const int NonUniformLineFixedSegments = 1;

		public const float NonUniformLineProgressionFactor = 1.2f;

		public const float LineImpulseThreshold = 0.02f;

		public const float LineLyingImpulseThreshold = 0.01f;

		public const float LureImpulseThreshold = 0.02f;

		public const float LyingLureImpulseThreshold = 0.002f;

		public const float BobberImpulseThreshold = 0.005f;

		public const float TopWaterImpulseThresholdFactor = 4f;

		public const float RigidLureTorsionalFriction = 0.0001f;

		public const float LineMaxStretchFactor = 1.005f;

		public const float LineCompensationPerStepRelativeLimit = 0.05f;

		public const float LineCompensationPerStepAbsoluteLimit = 0.15f;

		public const int OmitImpulseCompensation = 5;

		public const float LeaderLengthFactor = 0.25f;

		public const float MaxLeaderLength = 0.32f;

		public const float MinLeaderLength = 0.05f;

		public const float SwivelMass = 0.005f;

		public const float SwivelSize = 0.015f;

		public const float FishFrictionConstant = 0.2f;

		public const float RepultionConstant = 25f;

		public const float AttractionConstant = 25f;

		public const float FishWaterHeight = -0.15f;

		public const float FishBuoyancy = 0f;

		public const float FishSinkBuoyancy = -0.5f;

		public const float FishFloatBuoyancy = 0.5f;

		public const float TackleFrictionConstant = 0.002f;

		public const float HookMass = 0.001f;

		public const float DefaultTackleRadius = 0.05f;

		public Vector3 RodPosition;

		public Quaternion RodRotation;

		public Quaternion RodRotationDelta = Quaternion.identity;

		public Vector3 prevRodPosition;

		public Quaternion prevRodRotation;

		private float rodLength;

		private float rodImpulseThresholdMult;

		private float currentTime;

		private float transformLerpValue;

		private float totalTime;

		public float CurrentLineTensionSegmentLength;

		public float CurrentLineLength;

		public float PriorLineLength;

		public ObscuredFloat FinalLineLength;

		private float initialLineLength;

		private int firstLineMassIndex;

		private int firstLineConnectionIndex;

		private Mass firstLineMass;

		private Spring firstLineConnection;

		private float lineSegmentLength;

		private float leaderLength;

		private Vector3 initialTacklePosition;

		public Vector3 CurrentTacklePosition;

		public Vector3 FinalTacklePosition;

		private float tackleImpulseThreshold;

		private float tackleLyingImpulseThreshold;

		private float lineImpulseThreshold;

		private float lineLyingImpulseThreshold;

		private float lureMass;

		private float bobberSize;

		private bool reelOffHackFakeLineTensionLock;

		public bool IsHorizontalStabilizationOn;

		private float fishSize;

		private bool fishIsShowing;

		public RodObjectInHands rodObject;

		private SpringDrivenObject lineObject;

		private SpringDrivenObject leaderObject;

		private SpringDrivenObject leashObject;

		private SpringDrivenObject tackleObject;

		private SpringDrivenObject hookObject;

		private PhyObject sinkerObject;

		private PhyObject magnetObject;

		public PhyObject HookedFishObject;

		public bool RodKinematicSplineMovementEnabled = true;

		public bool RodTransformResetFlag;

		public KinematicSpline RodKinematicSplineMovement;

		public double SimCycleLength;

		public int LimitsState;

		public float CurrentVelocityLimit;

		private const float AdaptiveBuoLowMass = 0.004f;

		private const float AdaptiveBuoLowBuo = -0.42f;

		private const float AdaptiveBuoHighMass = 0.042f;

		private const float AdaptiveBuoHighBuo = -1.1f;

		private const float AdaptiveBuoA = -17.894737f;

		private const float AdaptiveBuoB = -0.34842104f;

		private const float HookLineFactor = 0.6f;

		private const float LeashLength = 0.25f;

		protected const int AutoreelFrameSteps = 30;

		private float AutoTensionsMaxSpeed = 2f;

		private float AutoTensionsSpeedFactor = 10f;

		private float AutoTensionsBias = 0.025f;

		private float AutoTensionsEpsillon = 0.002f;

		public TackleMoveCompensationMode TackleMoveCompensationMode;

		private int omittedImpulseCompensation;

		private int simCycleNumber;

		private const int SparceCalcRate = 15;

		private bool wasStriking;

		private Vector3? priorRootPosition;

		private Quaternion? priorRootRotation;

		private Vector3 groundCacheProcreationRootPosition;

		private Vector3 groundCacheProcreationTacklePosition;

		private int groundCacheProcreationRootCounter;

		private int groundCacheProcreationTackleCounter;

		private float lineStrain;

		private bool wasReeling;

		private float lineLengthDebt;

		private const float FakeLineTensionCompensation = 0.00019620001f;

		private const float BottomFakeLineTensionCompensation = 4.9050002E-05f;

		private const float ReelOffHackFakeLineTensionComnpensation = 1.962E-06f;

		private const float MinLineLengthForTension = 5f;

		public bool HasFishAttackForceImpulse;

		public float FishAttackImpulseForce;

		private bool hadFishAttackForceImpulse;

		private const float FishAttackImpulseLength = 0.3f;

		private float fishAttackImpulseTime;

		private const float LureVelocityLimit = 45f;

		public enum TackleType
		{
			None,
			Float,
			Lure,
			Wobbler,
			Topwater,
			Feeder,
			CarpClassic,
			CarpPVAStick,
			CarpPVABag,
			Spod,
			CarpMethod
		}
	}
}
