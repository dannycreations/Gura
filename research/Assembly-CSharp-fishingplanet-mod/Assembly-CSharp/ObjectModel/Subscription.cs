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
	}
}
