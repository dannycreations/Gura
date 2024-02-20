using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using ObjectModel.Common;
using UnityEngine;
using UnityEngine.UI;

public class RefferalDeliveryInit : MonoBehaviour
{
	public void Init(bool isInitial, bool isInvited, Amount amount1, Amount amount2, List<InventoryItem> items, ProductReward[] products)
	{
		string text = ((!isInitial) ? (isInvited ? "LevelRewardInvitee" : "LevelRewardInvited") : (isInvited ? "FirstRewardInvitee" : "FirstRewardInvited"));
		this.Message.text = ScriptLocalization.Get(text);
		int i = 0;
		if (amount1 != null && amount1.Value > 0)
		{
			this.Previews[i].transform.Find("Icon").GetComponent<Text>().text = ((!(amount1.Currency == "SC")) ? "\ue62c" : "\ue62b");
			this.Previews[i].transform.Find("Name").GetComponent<Text>().text = amount1.Value.ToString();
			i++;
		}
		if (amount2 != null && amount2.Value > 0)
		{
			this.Previews[i].transform.Find("Icon").GetComponent<Text>().text = ((!(amount2.Currency == "SC")) ? "\ue62c" : "\ue62b");
			this.Previews[i].transform.Find("Name").GetComponent<Text>().text = amount2.Value.ToString();
			i++;
		}
		int num = 0;
		if (items != null)
		{
			int k;
			for (k = 0; k < items.Count; k++)
			{
				if (this.images.Count <= num)
				{
					this.images.Add(new ResourcesHelpers.AsyncLoadableImage());
				}
				this.images[num].Image = this.Previews[k + i];
				this.images[num++].Load(string.Format("Textures/Inventory/{0}", items[k].ThumbnailBID));
				this.Previews[k + i].transform.Find("Name").GetComponent<Text>().text = items[k].Name;
			}
			i += k;
		}
		if (products != null)
		{
			int j;
			for (j = 0; j < products.Length; j++)
			{
				StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == products[j].ProductId);
				if (storeProduct != null)
				{
					if (this.images.Count <= num)
					{
						this.images.Add(new ResourcesHelpers.AsyncLoadableImage());
					}
					this.images[num].Image = this.Previews[j + i];
					this.images[num++].Load(string.Format("Textures/Inventory/{0}", storeProduct.ImageBID));
					this.Previews[j + i].transform.Find("Name").GetComponent<Text>().text = storeProduct.Name;
				}
			}
			i += j;
		}
		while (i < this.Previews.Length)
		{
			this.Previews[i].gameObject.SetActive(false);
			i++;
		}
	}

	public Text Message;

	public Image[] Previews;

	private List<ResourcesHelpers.AsyncLoadableImage> images = new List<ResourcesHelpers.AsyncLoadableImage>();
}
