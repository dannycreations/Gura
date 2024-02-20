using System;

namespace ObjectModel
{
	public class InteractiveObject
	{
		public int ObjectId { get; set; }

		public string Name { get; set; }

		public int PondId { get; set; }

		public InteractiveObjectConfig Config { get; set; }

		public string BeforeMessage { get; set; }

		public string IteractedMessage { get; set; }

		public string AfterMessage { get; set; }

		public DateTime ShowStart { get; set; }

		public DateTime ShowEnd { get; set; }

		public DateTime InteractionStart { get; set; }

		public DateTime InteractionEnd { get; set; }

		public string Asset { get; set; }
	}
}
