using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(0)]
	public class ChumParticle : ChumIngredient
	{
		public List<ParticleFishInfluence> BiteInfluence { get; set; }

		public float ParticleFishSizeMod { get; set; }

		public float ParticleUsageTime { get; set; }

		public float LifetimeModifier { get; set; }

		public float AttractMaxEffectConcentration { get; set; }

		public float AttractMaxPositiveConcentration { get; set; }

		public float AttractMaxNegativeConcentration { get; set; }

		public float DetractMaxEffectConcentration { get; set; }

		public float DetractMaxPositiveConcentration { get; set; }

		public float DetractMaxNegativeConcentration { get; set; }
	}
}
