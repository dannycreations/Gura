using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BiteEditor.ObjectModel
{
	public class Fish
	{
		public Fish(string unityName, int fishGroupId, FishName fishName)
		{
			this._unityName = unityName;
			this._name = fishName;
			this._fishGroupId = fishGroupId;
			this.Layers = new List<FishLayer>();
		}

		public Fish()
		{
		}

		[JsonProperty]
		public List<FishLayer> Layers { get; private set; }

		public void FinishInitialization(Pond pond)
		{
			this._fishGroup = pond.FindFishGroup(this._fishGroupId);
			for (int i = 0; i < this.Layers.Count; i++)
			{
				this.Layers[i].FinishInitialization(pond);
			}
		}

		public FishName[] GetAffectedFish(IEnumerable<FishName> fish)
		{
			if (this._fishGroup == null)
			{
				FishName[] array;
				if (fish.Contains(this._name))
				{
					(array = new FishName[1])[0] = this._name;
				}
				else
				{
					array = new FishName[0];
				}
				return array;
			}
			return fish.Where((FishName f) => this._fishGroup.Fish.Any((FishGroup.Record r) => r.FishName == f)).ToArray<FishName>();
		}

		[JsonProperty]
		private string _unityName;

		[JsonProperty]
		private FishName _name;

		[JsonProperty]
		private int _fishGroupId;

		[JsonIgnore]
		private FishGroup _fishGroup;
	}
}
