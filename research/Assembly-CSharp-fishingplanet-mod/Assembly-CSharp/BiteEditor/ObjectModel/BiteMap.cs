using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public class BiteMap : ProbabilityMap
	{
		public BiteMap(int id, string name, Vector2f position2D, float modifier)
			: base(id, name, position2D)
		{
			this._patches = new List<BiteMapPatch>();
			this._modifier = modifier;
		}

		public BiteMap()
		{
		}

		[JsonProperty]
		public override string Type
		{
			get
			{
				return "BiteMap";
			}
		}

		public void AddPatch(BiteMapPatch patch)
		{
			this._patches.Add(patch);
		}

		public override void FinishInitialization(Pond pond)
		{
			for (int i = 0; i < this._patches.Count; i++)
			{
				this._patches[i].FinishInitialization(pond);
			}
		}

		public override float GetProbability(Vector2f pos, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			byte b = this._patches[0].GetProbability(pos);
			for (int i = 0; i < this._patches.Count; i++)
			{
				byte probability = this._patches[i].GetProbability(pos);
				if (probability > 0)
				{
					b = ((probability != 1) ? probability : 0);
					break;
				}
			}
			float num = (float)b / 255f;
			if (isInversed)
			{
				return (num * rangeMin + (1f - num) * rangeMax) * this._modifier;
			}
			return ((1f - num) * rangeMin + num * rangeMax) * this._modifier;
		}

		[JsonProperty]
		private List<BiteMapPatch> _patches;

		[JsonProperty]
		protected float _modifier = 1f;
	}
}
