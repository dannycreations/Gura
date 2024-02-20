using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ChumRecipes : List<Chum>
	{
		[JsonProperty(ItemTypeNameHandling = 3)]
		public Chum[] Items
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
