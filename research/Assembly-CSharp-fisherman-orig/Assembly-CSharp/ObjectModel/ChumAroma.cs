using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(0)]
	public class ChumAroma : ChumIngredient
	{
		public List<FishTypeAttractivity> AromatizerFishTypeAttractivity { get; set; }

		public List<FishTypeAttractivity> AromatizerFishDenial { get; set; }

		public float AromatizerFeederRadius { get; set; }

		public float AromatizerManualFeedRadius { get; set; }

		public float AromatizerEffectiveTime { get; set; }

		public float AromatizerDecayTime { get; set; }

		public float AromatizerSpawnTime { get; set; }

		public float AromatizerUsageTime { get; set; }

		public float AttractMaxEffectConcentration { get; set; }

		public float AttractMaxPositiveConcentration { get; set; }

		public float AttractMaxNegativeConcentration { get; set; }

		public float DetractMaxEffectConcentration { get; set; }

		public float DetractMaxPositiveConcentration { get; set; }

		public float DetractMaxNegativeConcentration { get; set; }
	}
}
