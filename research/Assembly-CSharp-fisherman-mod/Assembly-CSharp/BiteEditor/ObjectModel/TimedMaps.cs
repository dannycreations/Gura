using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public class TimedMaps : ProbabilityMap
	{
		public TimedMaps(int id, string name, Vector2f position2D)
			: base(id, name, position2D)
		{
			this._maps = new List<TimedMaps.Record>();
		}

		public TimedMaps()
		{
		}

		[JsonProperty]
		public override string Type
		{
			get
			{
				return "TimedMaps";
			}
		}

		public override void FinishInitialization(Pond pond)
		{
			for (int i = 0; i < this._maps.Count; i++)
			{
				this._maps[i].FinishInitialization(pond);
			}
		}

		public void AddMap(int mapId, int till)
		{
			this._maps.Add(new TimedMaps.Record(mapId, till));
		}

		public override float GetProbability(Vector2f pos, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			int num = this.FindMap(progress);
			return this._maps[num].Map.GetProbability(pos, progress, wind, isInversed, rangeMin, rangeMax);
		}

		private int FindMap(int progress)
		{
			int num = this._maps.FindIndex((TimedMaps.Record r) => r.Till * 60 > progress);
			if (num > 0)
			{
				return num - 1;
			}
			if (num == -1)
			{
				return this._maps.Count - 1;
			}
			return 0;
		}

		[JsonProperty]
		private List<TimedMaps.Record> _maps;

		public class Record
		{
			public Record(int mapId, int till)
			{
				this.MapId = mapId;
				this.Till = till;
				this.Map = null;
			}

			[JsonProperty]
			public int MapId { get; private set; }

			[JsonIgnore]
			public ProbabilityMap Map { get; private set; }

			[JsonProperty]
			public int Till { get; private set; }

			public void FinishInitialization(Pond pond)
			{
				this.Map = pond.FindMap(this.MapId);
			}
		}
	}
}
