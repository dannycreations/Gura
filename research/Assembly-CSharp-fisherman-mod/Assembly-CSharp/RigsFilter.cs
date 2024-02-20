using System;
using System.Collections.Generic;
using ObjectModel;

public class RigsFilter : ConcreteRigFilter
{
	internal override void Init()
	{
		base.Init<Leader>(false);
		base.AddSingle<ItemSubTypes>(this.RigsTypes, "ItemSubType", "TypeLineFilter", true);
		base.AddRange<float>(this.Diameters, "Thickness", "DiameterLineFilter", true);
		base.AddRange<float>(this.MaxLoads, "MaxLoad", "PoundTestLineFilter", true);
	}

	protected readonly Dictionary<string, ItemSubTypes> RigsTypes = new Dictionary<string, ItemSubTypes>
	{
		{
			"UGC_CarolinaRigsCaption",
			ItemSubTypes.CarolinaRig
		},
		{
			"UGC_TexasRigsCaption",
			ItemSubTypes.TexasRig
		},
		{
			"UGC_ThreewayRigsCaption",
			ItemSubTypes.ThreewayRig
		}
	};
}
