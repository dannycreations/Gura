using System;
using UnityEngine;

namespace DebugCamera
{
	[Serializable]
	public class ScenarioSettings
	{
		public float StartYaw;

		public float FinishYaw;

		public float StartPitch;

		public float FinishPitch;

		public float Duration;

		public float Delay;

		public AnimationCurve YawSpeedCurve;

		public AnimationCurve PitchSpeedCurve;
	}
}
