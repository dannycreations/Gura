using System;

namespace ObjectModel
{
	public class MetadataForUserCompetitionSearch
	{
		public MetadataForUserCompetitionSearch.FishCategory[] FishCategories { get; set; }

		public class FishCategory
		{
			public int CategoryId { get; set; }

			public string Name { get; set; }

			public override string ToString()
			{
				return string.Format("{0} '{1}'", this.CategoryId, this.Name);
			}
		}
	}
}
