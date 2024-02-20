using System;
using System.Collections.Generic;
using ObjectModel;

public class ToolsFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init<InventoryItem>(false);
		base.AddSingle<ItemSubTypes>(this.SubTypes, "ItemSubType", "TypeJigBaitFilter", true);
	}

	protected readonly Dictionary<string, ItemSubTypes> SubTypes = new Dictionary<string, ItemSubTypes>
	{
		{
			"KeepnetFilter",
			ItemSubTypes.Keepnet
		},
		{
			"StringerFilter",
			ItemSubTypes.Stringer
		},
		{
			"RodCaseFilter",
			ItemSubTypes.RodCase
		},
		{
			"WaistcoatFilter",
			ItemSubTypes.Waistcoat
		},
		{
			"TackleBoxFilter",
			ItemSubTypes.LuresBox
		},
		{
			"RodStandsCaption",
			ItemSubTypes.RodStand
		},
		{
			"ShopHatFilterCaption",
			ItemSubTypes.Hat
		}
	};
}
