using System;
using System.Collections.Generic;
using ObjectModel;

public class TerminalTackleFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		base.AddSingle<ItemSubTypes>(this.TackleTypes, "ItemSubType", "TypeTerminalTackleFilter", true);
		this.FilterType = typeof(InventoryItem);
	}

	protected readonly Dictionary<string, ItemSubTypes> TackleTypes = new Dictionary<string, ItemSubTypes>
	{
		{
			"SimpleHook",
			ItemSubTypes.SimpleHook
		},
		{
			"BarblessHook",
			ItemSubTypes.BarblessHook
		},
		{
			"CarpHook",
			ItemSubTypes.CarpHook
		},
		{
			"OffsetHooksCaption",
			ItemSubTypes.OffsetHook
		},
		{
			"BobberFilter",
			ItemSubTypes.Bobber
		},
		{
			"CommonBellsCaption",
			ItemSubTypes.CommonBell
		},
		{
			"SinkerFilter",
			ItemSubTypes.Sinker
		},
		{
			"BulletSinkersCaption",
			ItemSubTypes.SpinningSinker
		},
		{
			"DropSinkersCaption",
			ItemSubTypes.DropSinker
		},
		{
			"CageFeedersCaption",
			ItemSubTypes.CageFeeder
		},
		{
			"FlatFeedersCaption",
			ItemSubTypes.FlatFeeder
		},
		{
			"PVACaption",
			ItemSubTypes.PvaFeeder
		},
		{
			"SpodFeedersCaption",
			ItemSubTypes.SpodFeeder
		},
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
