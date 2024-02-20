using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class MissionOnClient
	{
		[JsonProperty]
		public int MissionId { get; set; }

		[JsonProperty]
		public MissionGroupOnClient Group { get; set; }

		[JsonProperty]
		public string Code { get; set; }

		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public string Description { get; set; }

		[JsonProperty]
		public int? ThumbnailBID { get; set; }

		[JsonProperty]
		public bool IsStarted { get; set; }

		[JsonProperty]
		public bool IsCompleted { get; set; }

		[JsonProperty]
		public bool IsFailed { get; set; }

		[JsonProperty]
		public bool IsArchived { get; set; }

		[JsonProperty]
		public bool IsMultiStart { get; set; }

		[JsonProperty]
		public string StartedFrom { get; set; }

		[JsonProperty]
		public List<MissionTaskOnClient> Tasks { get; set; }

		[JsonProperty]
		public bool IsActiveMission { get; set; }

		[JsonProperty]
		public DateTime StartTime { get; set; }

		[JsonProperty]
		public DateTime LastProgressTime { get; set; }

		[JsonProperty]
		public int? TimeToComplete { get; set; }

		[JsonProperty]
		public TimeSpan? TimeRemain { get; set; }

		[JsonProperty]
		public Reward Reward { get; set; }

		[JsonProperty]
		public long LastUpdatedHash { get; set; }

		[JsonProperty]
		public int OrderId { get; set; }
	}
}
