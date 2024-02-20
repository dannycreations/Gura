using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class InventoryRodSetups : List<RodSetup>
	{
		[JsonProperty(ItemTypeNameHandling = 3)]
		public RodSetup[] Items
		{
			get
			{
				return base.ToArray();
			}
			set
			{
				base.Clear();
				base.AddRange(value);
			}
		}
	}
}
