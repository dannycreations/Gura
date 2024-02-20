using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class MissionTaskOnClient
	{
		[JsonProperty]
		public int MissionId { get; set; }

		[JsonProperty]
		public int TaskId { get; set; }

		[JsonProperty]
		public string Code { get; set; }

		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public string Description { get; set; }

		[JsonProperty]
		public int? ThumbnailBID { get; set; }

		[JsonProperty]
		public bool IsDynamic { get; set; }

		[JsonProperty]
		public bool IsCompleted { get; set; }

		[JsonProperty]
		public bool IsHidden { get; set; }

		[JsonProperty]
		public string Progress { get; set; }

		[JsonProperty]
		public int OrderId { get; set; }

		[JsonProperty]
		public bool IsHiddenWhenCompleted { get; set; }
	}
}
