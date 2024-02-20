using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class HintMessageTranslationOnClient
	{
		[JsonProperty]
		public string Code { get; set; }

		[JsonProperty]
		public string Title { get; set; }

		[JsonProperty]
		public string Description { get; set; }

		[JsonProperty]
		public string Tooltip { get; set; }

		[JsonProperty]
		public int? ImageId { get; set; }
	}
}
