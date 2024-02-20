using System;

namespace ObjectModel
{
	public class FishBrief
	{
		public int FishId { get; set; }

		public string CodeName { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public int CategoryId { get; set; }

		public int? ThumbnailBID { get; set; }
	}
}
