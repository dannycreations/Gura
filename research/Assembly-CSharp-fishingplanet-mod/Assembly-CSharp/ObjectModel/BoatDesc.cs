using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class BoatDesc
	{
		public int BoatId { get; set; }

		public int BoatCategoryId { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public string Params { get; set; }

		public int ThumbnailBID { get; set; }

		public BoatPriceDesc[] Prices { get; set; }

		public string Asset { get; set; }

		[JsonProperty(ItemTypeNameHandling = 3)]
		public List<InventoryItem> BoatInventory { get; set; }

		[JsonIgnore]
		public Boat BoatModel
		{
			get
			{
				return this.BoatInventory.OfType<Boat>().First<Boat>();
			}
		}
	}
}
