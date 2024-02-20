using System;
using BiteEditor;
using Newtonsoft.Json;

namespace TPM
{
	public class ReplaysSetings
	{
		[JsonProperty]
		public string LoadFrom { get; set; }

		[JsonProperty]
		public ReplaysSetings.PlayerCustomization Player { get; set; }

		[JsonProperty]
		public ReplaysSetings.Fish GenerateFish { get; set; }

		public class Fish
		{
			[JsonProperty]
			public FishName Name { get; set; }

			[JsonProperty]
			public FishForm Form { get; set; }

			[JsonProperty]
			public int RodSlot { get; set; }

			[JsonProperty]
			public float Weight { get; set; }

			[JsonProperty]
			public float Delay { get; set; }

			[JsonProperty]
			public bool IsAutohook { get; set; }
		}

		public class PlayerCustomization
		{
			[JsonProperty]
			public string Head { get; set; }

			[JsonProperty]
			public string Hair { get; set; }

			[JsonProperty]
			public string Pants { get; set; }

			[JsonProperty]
			public string Hat { get; set; }

			[JsonProperty]
			public string Shirt { get; set; }

			[JsonProperty]
			public string Shoes { get; set; }

			[JsonProperty]
			public int SkinR { get; set; }

			[JsonProperty]
			public int SkinG { get; set; }

			[JsonProperty]
			public int SkinB { get; set; }
		}
	}
}
