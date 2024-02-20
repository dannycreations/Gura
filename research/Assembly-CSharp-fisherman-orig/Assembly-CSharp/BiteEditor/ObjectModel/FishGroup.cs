using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiteEditor.ObjectModel
{
	public class FishGroup : IFishGroup
	{
		public FishGroup(int id, string unityName, FishGroup.Record[] fish)
		{
			this.Id = id;
			this.UnityName = unityName;
			this.Fish = new FishGroup.Record[fish.Length];
			fish.CopyTo(this.Fish, 0);
		}

		public FishGroup()
		{
		}

		[JsonProperty]
		public int Id { get; private set; }

		[JsonProperty]
		public string UnityName { get; private set; }

		[JsonProperty]
		public FishGroup.Record[] Fish { get; private set; }

		public class Record
		{
			public FishName FishName;

			public List<FishForm> FishForms;
		}
	}
}
