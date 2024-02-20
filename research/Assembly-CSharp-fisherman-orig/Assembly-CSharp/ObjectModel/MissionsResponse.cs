using System;

namespace ObjectModel
{
	public class MissionsResponse
	{
		public string PrototypeMessagesToSend { get; set; }

		public string MessagesToSend { get; set; }

		public string ProgressMessages { get; set; }

		public string TaskTrackingStopMessages { get; set; }

		public string TaskTrackingStartMessages { get; set; }

		public string CompleteMessages { get; set; }

		public int? ActiveMissionId { get; set; }
	}
}
