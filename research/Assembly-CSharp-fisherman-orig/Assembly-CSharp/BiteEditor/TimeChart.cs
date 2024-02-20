using System;
using UnityEngine;

namespace BiteEditor
{
	public class TimeChart : MonoBehaviour
	{
		public float Evaluate(int minutes)
		{
			float num = Settings.MinutesFromFirstHourToHours(minutes);
			return (num < 21f) ? this._dayCurve.Chart.Evaluate(num) : this._nightCurve.Chart.Evaluate(num);
		}

		public int ExportId;

		[SerializeField]
		private DayChart _dayCurve;

		[SerializeField]
		private NightChart _nightCurve;

		[SerializeField]
		private float _minRnd;

		[SerializeField]
		private float _maxRnd;
	}
}
