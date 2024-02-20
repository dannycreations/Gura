using System;
using Newtonsoft.Json;
using UnityEngine;

namespace BiteEditor
{
	public struct SimpleCurve
	{
		public SimpleCurve(float[] values, float minTime, float maxTime)
		{
			this._values = new float[values.Length];
			values.CopyTo(this._values, 0);
			this._minTime = minTime;
			this._maxTime = maxTime;
		}

		public SimpleCurve(AnimationCurve curve, int pointsCount)
		{
			this._values = new float[pointsCount];
			this._minTime = curve.keys[0].time;
			this._maxTime = curve.keys[curve.length - 1].time;
			float num = (this._maxTime - this._minTime) / (float)(pointsCount - 1);
			for (int i = 0; i < pointsCount; i++)
			{
				this._values[i] = curve.Evaluate(this._minTime + (float)i * num);
			}
		}

		public float Evaluate(float time)
		{
			if (time < this._minTime || time > this._maxTime)
			{
				return 0f;
			}
			float num = (time - this._minTime) / (this._maxTime - this._minTime);
			int num2 = (int)((float)(this._values.Length - 1) * num + 0.5f);
			return this._values[num2];
		}

		[JsonProperty]
		private readonly float[] _values;

		[JsonProperty]
		private readonly float _minTime;

		[JsonProperty]
		private readonly float _maxTime;
	}
}
