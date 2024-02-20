using System;
using UnityEngine;

namespace Boats
{
	public class PaddleSettings : MonoBehaviour
	{
		public float OarTipMass;

		public float RowingVelocity;

		public float FastSpeedK;

		public StaminaSettings StaminaSettings;

		public float OarArea;

		public float OarLength;

		public Transform LeftPaddlePushPoint;

		public Transform RightPaddlePushPoint;

		public float YawToStartHandsRotation = 40f;

		public float HandsMaxYaw = 10f;

		public float MaxForceFromHandsRotation = 1.5f;

		public Transform[] Disturbers;

		public string SplashParticlePath = "2D/Splashes/pSplash_universal";

		public float SplashSize = 3f;

		public float DisturbenceRadius = 0.2f;

		public WaterDisturbForce DisturbenceForce = WaterDisturbForce.XSmall;

		public byte TicksBetweenDisturbs = 5;
	}
}
