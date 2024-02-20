using System;
using UnityEngine;

namespace Boats
{
	public class EngineSettings : MonoBehaviour
	{
		public float Weight;

		public float MaxThrustVelocity;

		public float MinThrustVelocity;

		public float MaxThrustGainTime;

		public float PropellerDragFactor;

		public float AcceleratedForceMultiplier = 2f;

		public float TurnAngle;

		public float RollFactor;

		public float TangageFactor;

		public float LiftFactor;

		public float TurnFactor;

		public float GlidingOnSpeed;

		public float GlidingOffSpeed;

		public float GlidingAcceleration;

		public float RollAngle;

		public float HighSpeed;

		public float RotationSpeed = 45f;

		public float RotationRange;

		public float OilCapacity;

		public float FuelCapacity;

		public float ElectricCapacity;

		public Transform ThrustPivot;

		public Transform Barycenter;

		public Transform HorizontalRotationPivot;

		public Transform VerticalRotationPivot;

		public Transform SoundPosition;

		public float EngineStoppedPitch = 40f;

		public float EngineReadyPitch;

		public Transform TrottleRotationPivot;

		public float MaxTrottleRoll = 90f;

		public Transform HandleRotationPivot;

		public Transform PropellerPivot;

		public float PropellerMaxRPM = 600f;

		public AnimationsBank AnimationsBank;

		public float[] IgnitionAttemptSucessPrc = new float[] { 0.1f, 0.3f, 0.5f };

		public int IgnitionAttemptsCount = 4;

		public AnimationCurve IgnitionForwardSpeed;

		public float IgnitionForwardAnimK = 1f;

		public AnimationCurve IgnitionBackwardSpeed;

		public float IgnitionBackwardAnimK = 1f;

		public float BeatEngineAnimK = 1.5f;

		public string WaterSplashPrefab = "2D/Splashes/pMotorSplash";

		public float WaterSplashSizeMin = 1f;

		public float WaterSplashSizeMax = 2f;

		public float WaterSplashRateMin = 5f;

		public float WaterSplashRateMax = 10f;

		public float WaterSplashPositionSpreadMin = 0.1f;

		public float WaterSplashPositionSpreadMax = 0.2f;

		public float WaterSplashDuration = 2f;

		public float WaterDisturbanceRadius = 1f;

		public float EngineModelNominalMaxAngularSpeed = 650f;

		public float EngineModelNominalMinAngularSpeed = 115f;

		public float EngineModelInputLoadMultiplier = 3f;

		public float EngineModelIdleFuelRate = 0.3f;

		public float EngineModelMaxFuelRate = 1.5f;
	}
}
