using System;

namespace ObjectModel
{
	public class ChangeResidenceInfo
	{
		public int StateId { get; set; }

		public int NewStateId { get; set; }

		public int Amount { get; set; }

		public string Currency { get; set; }

		public string NewStateName { get; set; }
	}
}
