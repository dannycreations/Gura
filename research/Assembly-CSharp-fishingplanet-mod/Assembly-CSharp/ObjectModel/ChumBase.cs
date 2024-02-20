using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(0)]
	public class ChumBase : ChumIngredient
	{
		public ChumBase()
		{
			this.BaseEffectiveTemperatureRange = new EffectiveTemperatureRange();
			this.BaseChumFragmentation = new ChumFragmentation();
		}

		public List<FishTypeAttractivity> BaseFishTypeAttractivity { get; set; }

		public int BaseViscosity { get; set; }

		public EffectiveTemperatureRange BaseEffectiveTemperatureRange { get; set; }

		public ChumFragmentation BaseChumFragmentation { get; set; }

		public float BaseEffectiveTime { get; set; }

		public float BaseDecayTime { get; set; }

		public float BaseSpawnTime { get; set; }

		public float BaseUsageTime { get; set; }

		[JsonConfig]
		public string Color { get; set; }
	}
}
