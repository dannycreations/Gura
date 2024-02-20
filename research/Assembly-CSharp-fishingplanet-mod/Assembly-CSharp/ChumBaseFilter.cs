using System;
using System.Collections.Generic;
using ObjectModel;

public class ChumBaseFilter : BaseChumBaseFilter
{
	internal override void Init()
	{
		base.Init();
		base.AddSingle<ItemSubTypes>(this.SubTypes, "ItemSubType", "TypeBoatsFilter", true);
	}

	protected readonly Dictionary<string, ItemSubTypes> SubTypes = new Dictionary<string, ItemSubTypes>
	{
		{
			"CarpbaitsMenu",
			ItemSubTypes.ChumCarpbaits
		},
		{
			"FeederBasesCaption",
			ItemSubTypes.ChumGroundbaits
		},
		{
			"MethodBasesCaption",
			ItemSubTypes.ChumMethodMix
		}
	};
}
