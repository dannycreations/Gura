using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectModel
{
	public interface IBuyItemsResource : IEnumerable<int[]>, IEnumerable
	{
		bool OnlyGlobalShop { get; set; }

		float? MinLoad { get; set; }

		float? MaxLoad { get; set; }

		int? LineLength { get; set; }

		float? LineMinLoad { get; set; }

		float? LineMaxLoad { get; set; }

		float? LineMinThickness { get; set; }

		float? LineMaxThickness { get; set; }

		int? LeaderLength { get; set; }

		float? LeaderMinLoad { get; set; }

		float? LeaderMaxLoad { get; set; }

		float? LeaderMinThickness { get; set; }

		float? LeaderMaxThickness { get; set; }

		float? ChumWeight { get; set; }

		int GetAssemblingSlot(MissionsContext context);
	}
}
