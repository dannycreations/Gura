using System;
using UnityEngine;

namespace UltraReal
{
	[Serializable]
	public class AnimationCurveProperty
	{
		public float EvaluateStep(float delta)
		{
			if (this._startTime == 0f && this.randomStartTime)
			{
				this._startTime = Random.Range(0.01f, 1f);
			}
			this._currentTime += delta;
			return this.flickerCurve.Evaluate((this._currentTime / this.timeLength + this._startTime) % 1f);
		}

		public AnimationCurve flickerCurve;

		public float timeLength = 1f;

		public bool randomStartTime;

		private float _startTime;

		private float _currentTime;
	}
}
