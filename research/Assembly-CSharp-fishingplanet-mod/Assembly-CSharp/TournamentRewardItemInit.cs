using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TournamentRewardItemInit : MonoBehaviour
{
	public void Init(InventoryItem item)
	{
		this.Image.gameObject.SetActive(true);
		this.ImageLdbl.Image = this.Image;
		this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", item.ThumbnailBID));
		this.TitleText.text = item.Name;
	}

	public void Init(StoreProduct product)
	{
		if (product.ImageBID != null)
		{
			this.Image.gameObject.SetActive(true);
			this.ImageLdbl.Image = this.Image;
			this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", product.ImageBID));
		}
		this.TitleText.text = product.Name;
	}

	public void Init(double? money, string currency)
	{
		this.CurrencyIcon.gameObject.SetActive(true);
		this.TitleText.text = money.ToString();
		this.CurrencyIcon.text = MeasuringSystemManager.GetCurrencyIcon(currency);
	}

	public Text TitleText;

	public Image Image;

	private ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text CurrencyIcon;
}
