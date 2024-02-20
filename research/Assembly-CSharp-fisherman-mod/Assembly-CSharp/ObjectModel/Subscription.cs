using System;

namespace ObjectModel
{
	public class Subscription
	{
		public int ProductId { get; set; }

		public string EntitlementId { get; set; }

		public bool IsPremium { get; set; }

		public bool IsUnlock { get; set; }

		public int PondId { get; set; }

		public bool IsPaidUnlock { get; set; }

		public int Silver { get; set; }

		public int Gold { get; set; }

		public int[] Licenses { get; set; }
	}
}
