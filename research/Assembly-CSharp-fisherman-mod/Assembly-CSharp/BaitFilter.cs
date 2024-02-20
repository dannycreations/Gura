using System;
using System.Collections.Generic;
using ObjectModel;

public class BaitFilter : ConcreteBaitFilter
{
	internal override void Init()
	{
		base.Init();
		base.AddSingle<ItemSubTypes>(this.BaitType, "ItemSubType", "TypeFilter", true);
	}

	protected readonly Dictionary<string, ItemSubTypes> BaitType = new Dictionary<string, ItemSubTypes>
	{
		{
			"CommonBaitsFilter",
			ItemSubTypes.CommonBait
		},
		{
			"InsectsWormBaitsFilter",
			ItemSubTypes.InsectsWormBait
		},
		{
			"FreshBaitsFilter",
			ItemSubTypes.FreshBait
		},
		{
			"CarpbaitsMenu",
			ItemSubTypes.BoilBait
		}
	};
}
