using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public class Pond : ICloneable
	{
		public Pond(string name, HeightMap heightMap, SplatMap splatMap, FlowMap flowMap, bool isUsedToGenerateFish)
		{
			this.Name = name;
			this._isUsedToGenerateFish = isUsedToGenerateFish;
			this._weathers = new Dictionary<string, Weather>();
			this._heightMap = heightMap;
			this._splatMap = splatMap;
			this._flowMap = flowMap;
			this._fish = new Dictionary<FishName, FishDescription>();
		}

		[JsonProperty]
		public string Name { get; private set; }

		[JsonIgnore]
		public bool IsUsedToGenerateFish
		{
			get
			{
				return this._isUsedToGenerateFish;
			}
		}

		[JsonIgnore]
		public SplatMap SplatMap
		{
			get
			{
				return this._splatMap;
			}
		}

		[JsonIgnore]
		public FlowMap FlowMap
		{
			get
			{
				return this._flowMap;
			}
		}

		public object Clone()
		{
			Pond pond = (Pond)base.MemberwiseClone();
			pond._weathers = this._weathers.ToDictionary((KeyValuePair<string, Weather> p) => p.Key, (KeyValuePair<string, Weather> p) => p.Value);
			pond._fishGroups = this._fishGroups.ToArray<FishGroup>();
			pond._timeCharts = this._timeCharts.ToArray<TimeChart>();
			pond._maps = this._maps.ToArray<ProbabilityMap>();
			pond._curves = this._curves.ToArray<Curve>();
			pond._fish = this._fish.ToDictionary((KeyValuePair<FishName, FishDescription> p) => p.Key, (KeyValuePair<FishName, FishDescription> p) => p.Value);
			if (this._heightMap != null)
			{
				pond._heightMap = (HeightMap)this._heightMap.Clone();
			}
			if (this._splatMap != null)
			{
				pond._splatMap = (SplatMap)this._splatMap.Clone();
			}
			if (this._flowMap != null)
			{
				pond._flowMap = (FlowMap)this._flowMap.Clone();
			}
			return pond;
		}

		public void FinishInitialization()
		{
			this._rnd = new Random();
			string imagesFolder = Settings.GetImagesFolder(this.Name);
			if (!Directory.Exists(imagesFolder))
			{
				throw new InvalidOperationException(string.Format("Can't find folder with images {0}", imagesFolder));
			}
			foreach (ProbabilityMap probabilityMap in this._maps)
			{
				probabilityMap.FinishInitialization(this);
			}
			foreach (Weather weather in this._weathers.Values)
			{
				weather.FinishInitialization(this);
			}
			this._heightMap.FinishInitialization(this.Name);
			this._splatMap.FinishInitialization(this.Name);
			if (this._flowMap != null)
			{
				this._flowMap.FinishInitialization(this.Name);
			}
		}

		public void AddWeather(Weather weather)
		{
			this._weathers[weather.UnityName] = weather;
		}

		public void AddFishGroups(IEnumerable<FishGroup> groups)
		{
			this._fishGroups = groups.ToArray<FishGroup>();
		}

		public void AddTimeCharts(IEnumerable<TimeChart> charts)
		{
			this._timeCharts = charts.ToArray<TimeChart>();
		}

		public void AddMaps(IEnumerable<ProbabilityMap> maps)
		{
			this._maps = maps.ToArray<ProbabilityMap>();
		}

		public void AddCurves(IEnumerable<Curve> curves)
		{
			this._curves = curves.ToArray<Curve>();
		}

		public void AddFish(FishName name)
		{
			if (!this._fish.ContainsKey(name))
			{
				this._fish[name] = new FishDescription(name);
				return;
			}
			throw new InvalidOperationException(string.Format("Duplicated fish description for {0}", name));
		}

		public void AddFishForm(FishName name, FishForm form, float minWeight, float maxWeight, float attractorsModifier, bool isDetractorEnabled, float detractionDuration, DetractionType detractionType, float detraction, float r100, float r0)
		{
			this._fish[name].AddForm(form, minWeight, maxWeight, attractorsModifier, isDetractorEnabled, detractionDuration, detractionType, detraction, r100, r0);
		}

		public ProbabilityMap FindMap(int id)
		{
			return (id < 0 || id >= this._maps.Length) ? null : this._maps[id];
		}

		public TimeChart FindChart(int id)
		{
			return (id < 0 || id >= this._timeCharts.Length) ? null : this._timeCharts[id];
		}

		public Curve FindCurve(int id)
		{
			return (id < 0 || id >= this._curves.Length) ? null : this._curves[id];
		}

		public FishGroup FindFishGroup(int id)
		{
			return (id < 0 || id >= this._fishGroups.Length) ? null : this._fishGroups[id];
		}

		public bool TestFishForm(FishName name, FishForm form)
		{
			return this._fish.ContainsKey(name) && this._fish[name].TestForm(form);
		}

		public float GetBottomHeight(Vector3f pos)
		{
			return this._heightMap.GetBottomHeight(pos);
		}

		public Vector3f GetFlowVector(Vector3f pos)
		{
			return (this._flowMap == null) ? Vector3f.Zero : this._flowMap.GetFlowVector(pos);
		}

		[JsonProperty]
		private Dictionary<string, Weather> _weathers;

		[JsonProperty]
		private FishGroup[] _fishGroups;

		[JsonProperty]
		private TimeChart[] _timeCharts;

		[JsonProperty]
		private ProbabilityMap[] _maps;

		[JsonProperty]
		private Curve[] _curves;

		[JsonProperty]
		private HeightMap _heightMap;

		[JsonProperty]
		private SplatMap _splatMap;

		[JsonProperty]
		private FlowMap _flowMap;

		[JsonProperty]
		private Dictionary<FishName, FishDescription> _fish;

		[JsonProperty]
		private bool _isUsedToGenerateFish;

		[JsonIgnore]
		private Random _rnd;
	}
}
