using System;

namespace ObjectModel
{
	public class FishRef
	{
		public long Id { get; set; }

		public int FishId { get; set; }

		public Guid InstanceId { get; set; }

		public float Score { get; set; }

		public float Weight { get; set; }

		public float Length { get; set; }

		public float? Experience { get; set; }
	}
}
