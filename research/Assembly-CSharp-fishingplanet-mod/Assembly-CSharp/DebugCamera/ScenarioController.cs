using System;
using System.Diagnostics;
using UnityEngine;

namespace DebugCamera
{
	public class ScenarioController
	{
		public ScenarioController(ScenarioSettings settings, int index)
		{
			this._settings = settings;
			this._scenarioIndex = index;
			this._startAt = Time.realtimeSinceStartup + settings.Delay;
			this._finishAt = this._startAt + settings.Duration;
			this._curYaw = settings.StartYaw;
			this._curPitch = settings.StartPitch;
			this._yawCurveK = this.CalcCurveK(settings.YawSpeedCurve, settings.StartYaw, settings.FinishYaw);
			this._pitchCurveK = this.CalcCurveK(settings.PitchSpeedCurve, settings.StartPitch, settings.FinishPitch);
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action EFinished = delegate
		{
		};

		public float CurYaw
		{
			get
			{
				return this._curYaw;
			}
		}

		public float CurPitch
		{
			get
			{
				return this._curPitch;
			}
		}

		public int ScenarioIndex
		{
			get
			{
				return this._scenarioIndex;
			}
		}

		public float CalcCurveK(AnimationCurve curve, float startValue, float finishValue)
		{
			float num = finishValue - startValue;
			float num2 = 0.01f;
			float num3 = 0f;
			for (float num4 = 0f; num4 < 1f - num2; num4 += num2)
			{
				num3 += (curve.Evaluate(num4) + curve.Evaluate(num4 + num2)) * 0.5f * num2;
			}
			return num / num3 / this._settings.Duration;
		}

		public void Update()
		{
			if (this._startAt < Time.realtimeSinceStartup)
			{
				if (this._finishAt < Time.realtimeSinceStartup)
				{
					this._curYaw = this._settings.FinishYaw;
					this._curPitch = this._settings.FinishPitch;
					this.EFinished();
				}
				else
				{
					float num = (Time.realtimeSinceStartup - this._startAt) / this._settings.Duration;
					this._curYaw += this._settings.YawSpeedCurve.Evaluate(num) * this._yawCurveK * Time.unscaledDeltaTime;
					this._curPitch += this._settings.PitchSpeedCurve.Evaluate(num) * this._pitchCurveK * Time.unscaledDeltaTime;
				}
			}
		}

		private readonly ScenarioSettings _settings;

		private float _curYaw;

		private float _curPitch;

		private float _startAt;

		private float _finishAt;

		private int _scenarioIndex;

		private float _yawCurveK;

		private float _pitchCurveK;
	}
}
