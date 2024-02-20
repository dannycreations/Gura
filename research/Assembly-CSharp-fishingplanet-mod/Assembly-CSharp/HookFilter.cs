using System;
using System.Collections.Generic;
using ObjectModel;

public class HookFilter : BaseHookFilter
{
	internal override void Init()
	{
		base.Init();
		base.AddSingle<ItemSubTypes>(1, this.HooksTypes, "ItemSubType", "TypeHookFilter", true);
	}

	protected readonly Dictionary<string, ItemSubTypes> HooksTypes = new Dictionary<string, ItemSubTypes>
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
		}
	};
}
