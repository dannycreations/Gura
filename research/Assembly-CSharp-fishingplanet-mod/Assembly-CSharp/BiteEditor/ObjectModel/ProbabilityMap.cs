using System;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public abstract class ProbabilityMap
	{
		public ProbabilityMap(int id, string name, Vector2f position2D)
		{
			this.Id = id;
			this.UnityName = name;
			this.Position2D = position2D;
		}

		public ProbabilityMap()
		{
		}

		[JsonProperty]
		public abstract string Type { get; }

		[JsonProperty]
		public int Id { get; private set; }

		[JsonProperty]
		public string UnityName { get; private set; }

		[JsonProperty]
		public Vector2f Position2D { get; private set; }

		public abstract float GetProbability(Vector2f pos, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f);

		public abstract void FinishInitialization(Pond pond);
	}
}
