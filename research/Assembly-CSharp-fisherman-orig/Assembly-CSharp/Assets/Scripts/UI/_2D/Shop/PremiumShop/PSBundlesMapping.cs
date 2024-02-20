using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop
{
	public class PSBundlesMapping
	{
		public static int GetBundleId(int productId)
		{
			foreach (KeyValuePair<int, List<int>> keyValuePair in PSBundlesMapping._bundlesMapping)
			{
				if (keyValuePair.Value.Contains(productId))
				{
					return keyValuePair.Key;
				}
			}
			return 0;
		}

		public static List<int> GetBundleProducts(int bundleId)
		{
			return (!PSBundlesMapping._bundlesMapping.ContainsKey(bundleId)) ? new List<int>() : PSBundlesMapping._bundlesMapping[bundleId];
		}

		public static List<int> AllBundles
		{
			get
			{
				return PSBundlesMapping._bundlesMapping.Keys.ToList<int>();
			}
		}

		private static readonly Dictionary<int, List<int>> _bundlesMapping = new Dictionary<int, List<int>>();

		public static readonly IList<int> BundleRemoveIfAllItemsBought = new ReadOnlyCollection<int>(new List<int> { 8980, 8990, 9000 });
	}
}
