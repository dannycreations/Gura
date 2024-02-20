using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public class WindyMaps : ProbabilityMap
	{
		public WindyMaps(int id, string name, Vector2f position2D)
			: base(id, name, position2D)
		{
			this._maps = new List<WindyMaps.Record>();
		}

		public WindyMaps()
		{
		}

		[JsonProperty]
		public override string Type
		{
			get
			{
				return "WindyMaps";
			}
		}

		public void AddMap(int mapId, List<WindDirection> winds)
		{
			this._maps.Add(new WindyMaps.Record(mapId, winds));
		}

		public override float GetProbability(Vector2f pos, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			int num = this.FindMap(wind);
			return this._maps[num].Map.GetProbability(pos, progress, wind, isInversed, rangeMin, rangeMax);
		}

		private int FindMap(WindDirection wind)
		{
			int num = this._maps.FindIndex((WindyMaps.Record r) => r.Winds.Contains(wind));
			if (num != -1)
			{
				return num;
			}
			num = this._maps.FindIndex((WindyMaps.Record r) => r.Winds.Count == 0);
			if (num != -1)
			{
				return num;
			}
			return 0;
		}

		public override void FinishInitialization(Pond pond)
		{
			for (int i = 0; i < this._maps.Count; i++)
			{
				this._maps[i].FinishInitialization(pond);
			}
		}

		[JsonProperty]
		private List<WindyMaps.Record> _maps;

		public class Record
		{
			public Record(int mapId, List<WindDirection> winds)
			{
				this.MapId = mapId;
				this.Winds = new List<WindDirection>(winds);
				this.Map = null;
			}

			[JsonProperty]
			public int MapId { get; private set; }

			[JsonIgnore]
			public ProbabilityMap Map { get; private set; }

			[JsonProperty]
			public List<WindDirection> Winds { get; private set; }

			public void FinishInitialization(Pond pond)
			{
				this.Map = pond.FindMap(this.MapId);
			}
		}
	}
}
