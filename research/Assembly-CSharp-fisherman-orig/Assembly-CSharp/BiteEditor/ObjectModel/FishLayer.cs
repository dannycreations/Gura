using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiteEditor.ObjectModel
{
	public class FishLayer
	{
		public FishLayer(string unityName, Depth depth, List<FishForm> fishForms, int mapId, float mapModifier, int timeChartId)
		{
			this.UnityName = unityName;
			this.Depth = depth;
			this.Forms = new List<FishForm>(fishForms);
			this._timeChartId = timeChartId;
			this._mapId = mapId;
			this._mapModifier = mapModifier;
		}

		public FishLayer()
		{
		}

		[JsonProperty]
		public string UnityName { get; private set; }

		[JsonProperty]
		public Depth Depth { get; private set; }

		[JsonProperty]
		public List<FishForm> Forms { get; private set; }

		[JsonIgnore]
		public float MapModifier
		{
			get
			{
				return this._mapModifier;
			}
		}

		[JsonIgnore]
		public TimeChart TimeChart { get; private set; }

		[JsonIgnore]
		public ProbabilityMap Map { get; private set; }

		public void FinishInitialization(Pond pond)
		{
			this.TimeChart = pond.FindChart(this._timeChartId);
			this.Map = pond.FindMap(this._mapId);
		}

		[JsonProperty]
		private int _timeChartId;

		[JsonProperty]
		private int _mapId;

		[JsonProperty]
		private float _mapModifier;
	}
}
