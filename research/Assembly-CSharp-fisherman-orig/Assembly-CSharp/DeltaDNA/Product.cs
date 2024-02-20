using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public class Product<T> : Params where T : Product<T>
	{
		public T SetRealCurrency(string type, int amount)
		{
			if (string.IsNullOrEmpty(type))
			{
				throw new ArgumentException("Type cannot be null or empty");
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "realCurrencyType", type },
				{ "realCurrencyAmount", amount }
			};
			base.AddParam("realCurrency", dictionary);
			return (T)((object)this);
		}

		public T AddVirtualCurrency(string name, string type, int amount)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Name cannot be null or empty");
			}
			if (string.IsNullOrEmpty(type))
			{
				throw new ArgumentException("Type cannot be null or empty");
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object> { 
			{
				"virtualCurrency",
				new Dictionary<string, object>
				{
					{ "virtualCurrencyName", name },
					{ "virtualCurrencyType", type },
					{ "virtualCurrencyAmount", amount }
				}
			} };
			if (base.GetParam("virtualCurrencies") == null)
			{
				this.virtualCurrencies = new List<Dictionary<string, object>>();
				base.AddParam("virtualCurrencies", this.virtualCurrencies);
			}
			this.virtualCurrencies.Add(dictionary);
			return (T)((object)this);
		}

		public T AddItem(string name, string type, int amount)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Name cannot be null or empty");
			}
			if (string.IsNullOrEmpty(type))
			{
				throw new ArgumentException("Type cannot be null or empty");
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object> { 
			{
				"item",
				new Dictionary<string, object>
				{
					{ "itemName", name },
					{ "itemType", type },
					{ "itemAmount", amount }
				}
			} };
			if (base.GetParam("items") == null)
			{
				this.items = new List<Dictionary<string, object>>();
				base.AddParam("items", this.items);
			}
			this.items.Add(dictionary);
			return (T)((object)this);
		}

		private List<Dictionary<string, object>> virtualCurrencies;

		private List<Dictionary<string, object>> items;
	}
}
