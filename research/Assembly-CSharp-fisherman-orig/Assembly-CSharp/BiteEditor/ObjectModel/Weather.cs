using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiteEditor.ObjectModel
{
	public class Weather
	{
		public Weather(string unityName)
		{
			this._unityName = unityName;
			this.Fish = new List<Fish>();
			this._attractors = new List<AttractorsGroup>();
		}

		public Weather()
		{
		}

		[JsonIgnore]
		public string UnityName
		{
			get
			{
				return this._unityName;
			}
		}

		[JsonProperty]
		public List<Fish> Fish { get; private set; }

		public void FinishInitialization(Pond pond)
		{
			this._pond = pond;
			for (int i = 0; i < this.Fish.Count; i++)
			{
				this.Fish[i].FinishInitialization(pond);
			}
			for (int j = 0; j < this._attractors.Count; j++)
			{
				this._attractors[j].FinishInitialization(pond);
			}
		}

		public void AddAttractorGroup(AttractorsGroup group)
		{
			this._attractors.Add(group);
		}

		[JsonProperty]
		private string _unityName;

		[JsonProperty]
		private List<AttractorsGroup> _attractors;

		[JsonIgnore]
		private Pond _pond;
	}
}
