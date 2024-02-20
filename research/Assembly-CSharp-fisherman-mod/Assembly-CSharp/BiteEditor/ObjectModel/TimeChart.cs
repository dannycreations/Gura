using System;
using Newtonsoft.Json;

namespace BiteEditor.ObjectModel
{
	public class TimeChart
	{
		public TimeChart(int id, string unityName, SimpleCurve dayChart, SimpleCurve nightChart, float minRnd, float maxRnd)
		{
			this.Id = id;
			this.UnityName = unityName;
			this._dayChart = dayChart;
			this._nightChart = nightChart;
			this._minRnd = minRnd;
			this._maxRnd = maxRnd;
		}

		public TimeChart()
		{
		}

		[JsonProperty]
		public int Id { get; private set; }

		[JsonProperty]
		public string UnityName { get; private set; }

		[JsonIgnore]
		public float MinRnd
		{
			get
			{
				return this._minRnd;
			}
		}

		[JsonIgnore]
		public float MaxRnd
		{
			get
			{
				return this._maxRnd;
			}
		}

		public float Evaluate(int minutes)
		{
			float num = Settings.MinutesFromFirstHourToHours(minutes);
			return (num < 21f) ? this._dayChart.Evaluate(num) : this._nightChart.Evaluate(num);
		}

		[JsonProperty]
		private SimpleCurve _dayChart;

		[JsonProperty]
		private SimpleCurve _nightChart;

		[JsonProperty]
		private float _minRnd;

		[JsonProperty]
		private float _maxRnd;
	}
}
