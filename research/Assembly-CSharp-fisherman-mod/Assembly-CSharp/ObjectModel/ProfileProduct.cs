using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ProfileProduct
	{
		public int ProductId { get; set; }

		public int TypeId { get; set; }

		public int? Term { get; set; }

		public string Name { get; set; }

		public int? Silver { get; set; }

		public int? Gold { get; set; }

		public double Price { get; set; }

		public int? ImageBID { get; set; }

		public InventoryItem[] Items { get; set; }

		public PlayerLicense[] Licenses { get; set; }

		public RodSetup[] RodSetups { get; set; }

		public double? ExpMultiplier { get; set; }

		public double? MoneyMultiplier { get; set; }

		public string ForeignProductId { get; set; }

		public int[] PondsUnlocked { get; set; }

		public int[] PaidPondsUnlocked { get; set; }

		public int? AccessibleLevel { get; set; }

		public int? InventoryExt { get; set; }

		public int? RodSetupExt { get; set; }

		public int? BuoyExt { get; set; }

		public int? ChumRecipesExt { get; set; }

		public int PlatformId { get; set; }

		[JsonIgnore]
		public bool HasSubscription
		{
			get
			{
				return this.ExpMultiplier != null && this.MoneyMultiplier != null && this.Term != null;
			}
		}

		public override string ToString()
		{
			return string.Format("#{0} {1} ", this.ProductId, this.Name);
		}
	}
}
