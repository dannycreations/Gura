using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public class MissionClientConfiguration
	{
		public int MissionId { get; set; }

		public Dictionary<string, HintMessage> VisualHints { get; set; }
	}
}
