using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public class ChumPortion
	{
		public Guid Id { get; set; }

		public int Viscosity { get; set; }

		public float ZoneHeight { get; set; }

		public float ZoneRadius { get; set; }

		public List<FishTypeAttractivity> FishTypeAttractivity { get; set; }

		public ChumFragmentation Fragmentation { get; set; }

		public EffectiveTemperatureRange EffectiveTemperatureRange { get; set; }

		public float SpawnTime { get; set; }

		public float EffectiveTime { get; set; }

		public float DecayTime { get; set; }

		public static float ChumMinEffectiveness = 1.3f;
	}
}
