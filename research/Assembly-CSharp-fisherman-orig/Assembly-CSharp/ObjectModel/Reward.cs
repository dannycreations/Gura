using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class Reward
	{
		public int RewardId { get; set; }

		public string Name { get; set; }

		public int Experience { get; set; }

		public double? Money1 { get; set; }

		public string Currency1 { get; set; }

		public double? Money2 { get; set; }

		public string Currency2 { get; set; }

		public int? SkillId { get; set; }

		public string Items { get; set; }

		public string Products { get; set; }

		public string Licenses { get; set; }

		public List<RewardInventoryItemBrief> ItemsBrief { get; set; }

		public static double GetSumOfRewards(IList<Reward> rewards, string currency = "SC")
		{
			double num = 0.0;
			for (int i = 0; i < rewards.Count; i++)
			{
				if (rewards[i].Money1 != null && rewards[i].Currency1 == currency)
				{
					num += rewards[i].Money1.Value;
				}
				if (rewards[i].Money2 != null && rewards[i].Currency2 == currency)
				{
					num += rewards[i].Money2.Value;
				}
			}
			return num;
		}

		public ItemReward[] GetItemRewards()
		{
			if (string.IsNullOrEmpty(this.Items))
			{
				return null;
			}
			return JsonConvert.DeserializeObject<ItemReward[]>(this.Items, SerializationHelper.JsonSerializerSettings);
		}

		public ProductReward[] GetProductRewards()
		{
			if (string.IsNullOrEmpty(this.Products))
			{
				return null;
			}
			return JsonConvert.DeserializeObject<ProductReward[]>(this.Products, SerializationHelper.JsonSerializerSettings);
		}

		public LicenseRef[] GetLicenseRewards()
		{
			if (string.IsNullOrEmpty(this.Licenses))
			{
				return null;
			}
			return JsonConvert.DeserializeObject<LicenseRef[]>(this.Licenses, SerializationHelper.JsonSerializerSettings);
		}
	}
}
