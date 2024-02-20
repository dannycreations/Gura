using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectModel
{
	public class MissionDollContext : List<InventoryItem>
	{
		public MissionsContext Context { get; set; }

		public RodCase RodCase
		{
			get
			{
				return this.OfType<RodCase>().FirstOrDefault<RodCase>() ?? new RodCase();
			}
		}

		public Hat Hat
		{
			get
			{
				return this.OfType<Hat>().FirstOrDefault<Hat>() ?? new Hat();
			}
		}
	}
}
