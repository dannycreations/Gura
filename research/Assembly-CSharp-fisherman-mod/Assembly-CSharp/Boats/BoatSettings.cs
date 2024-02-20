using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boats
{
	[ExecuteInEditMode]
	public class BoatSettings : MonoBehaviour
	{
		public MeshRenderer[] Renderers;

		public float Weight;

		public float WaterNoise;

		public float LateralResistance;

		public float LongitudalResistance;

		public float DynamicTangageStabilizer;

		public float DynamicYawStabilizer;

		public float DynamicRollStabilizer;

		public float BowLength;

		public float MiddleLength;

		public float SternLength;

		public float Width;

		public float Height;

		public float BowWidth;

		public float BottomWidth;

		public FloatingBodyType BodyType;

		public BoatShaderParametersController.BoatMaskType MaskType;

		public Animation FakeLegs;

		public Transform Barycenter;

		public Transform VolumeBodyAnchor;

		public Transform DriverPivot;

		public Transform AnglerPivot;

		public Transform DriverShadowPivot;

		public Transform AnglerShadowPivot;

		public Transform AnchorFrontPivot;

		public Transform AnchorBackPivot;

		public Transform OarAnchor;

		public Transform[] WaterDisturbers;

		public byte TicksBetweenDisturbs = 3;

		public float DisturbanceRadius = 0.5f;

		public float DisturbanceMaxForceAtSpeed = 5f;

		public WaterDisturbForce DisturbanceMaxForce = WaterDisturbForce.Medium;

		public float AnchorDepth;

		public EngineMountSettings[] engines;

		public PaddleSettings paddle;

		public float FakeTurnsF = 6f;

		public float FakeMovementF = 3f;

		public float AnchoredForceK = 0.7f;

		public float StartRollBackWhenRoll = 80f;

		public float AnchoredResistenceForce = 50f;

		public float VelocityToChangeRowingAnimSpeed = 2f;

		public float VelocityWithMaxRowingAnimSpeed = 6f;

		public float MaxRowingAnimSpeedK = 2f;

		[Space(5f)]
		public Transform[] FishFinderPivots;

		public Transform SpeedometerArrow;

		public Vector2[] SpeedometerScale;

		public Transform TachometerArrow;

		public Vector2[] TachometerScale;

		public List<EngineObject.WaterSplashEmitter> SplashEmitters;

		public XZCurveFromT AnglerToDriverToCurve;
	}
}
