using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class MotorBoat : Boat
	{
		public float FuelCapacity { get; set; }

		public float FuelConsumptionFactor { get; set; }

		public float MinThrustVelocity { get; set; }

		public float MaxThrustVelocity { get; set; }

		public float MaxThrustGainTime { get; set; }

		public float PropellerDragFactor { get; set; }

		public float AcceleratedForceMultiplier { get; set; }

		public float WaterResistance { get; set; }

		public float TurnFactor { get; set; }

		public float TurnAngle { get; set; }

		public float RollFactor { get; set; }

		public float RollAngle { get; set; }

		public float HighSpeed { get; set; }

		public float ReverseFactor { get; set; }

		public float IdleRevolutions { get; set; }

		public float MinRevolutions { get; set; }

		public float MaxRevolutions { get; set; }

		public float AccelerationRevolutions { get; set; }

		public float ThrustVelocity1 { get; set; }

		public float ThrustVelocity2 { get; set; }

		public float ThrustVelocity3 { get; set; }

		public float GlidingOnSpeed { get; set; }

		public float GlidingOffSpeed { get; set; }

		public float GlidingAcceleration { get; set; }

		public float LiftFactor { get; set; }

		public float TangageFactor { get; set; }

		public float TrollingMaxRevs { get; set; }

		[JsonIgnore]
		public override AllowedEquipment[] AllowedEquipment
		{
			get
			{
				return new AllowedEquipment[]
				{
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.BoatMotor),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.BoatFuel),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.BoatOar),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.BoatFishBox),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.BoatLight),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.BoatAnchor),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.EchoSounder),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.SounderBattery),
						MaxItems = 1
					}
				};
			}
		}
	}
}
