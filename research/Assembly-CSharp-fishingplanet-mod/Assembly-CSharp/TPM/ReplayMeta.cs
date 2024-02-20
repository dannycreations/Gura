using System;
using Newtonsoft.Json;

namespace TPM
{
	public class ReplayMeta
	{
		[JsonProperty]
		public int PondId { get; set; }

		[JsonProperty]
		public int Hour { get; set; }

		[JsonProperty]
		public string Precipitation { get; set; }

		[JsonProperty]
		public string Sky { get; set; }

		[JsonProperty]
		public CameraFrames CameraData { get; set; }

		[JsonProperty]
		public ReplayMeta.KeyFramesModes KeyFramesMode { get; set; }

		public enum KeyFramesModes
		{
			Linear,
			Bezier
		}
	}
}
