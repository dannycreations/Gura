using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PremiumMoneyPackInit : PremiumAccountInit
{
	public override void Init(StoreProduct product)
	{
		this._product = product;
		this.Silvers.text = product.Gold.Value.ToString("D");
		base.GetComponent<ShowInfoPanel>().ProductInfo = product;
		this.ImageLdbl.Image = this.Image;
		this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", product.ImageBID));
		if (product.HasActiveDiscount(PhotonConnectionFactory.Instance.ServerUtcNow))
		{
			this.DiscountPriceInit(product);
		}
		else
		{
			this.NormalPriceInit(product);
		}
	}

	private void NormalPriceInit(StoreProduct product)
	{
		this.Image.transform.parent.Find("DiscountImage").GetComponent<Image>().overrideSprite = ResourcesHelpers.GetTransparentSprite();
		this.Price.color = Color.white;
		if (string.IsNullOrEmpty(product.PriceString))
		{
			this.Price.text = string.Format("{0} {1}", product.Price.ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
		}
		else
		{
			this.Price.text = product.PriceString;
		}
	}

	private void DiscountPriceInit(StoreProduct product)
	{
		if (product.DiscountImageBID != null)
		{
			this.DiscountImageLdbl.Image = this.Image.transform.parent.Find("DiscountImage").GetComponent<Image>();
			this.DiscountImageLdbl.Load(string.Format("Textures/Inventory/{0}", product.DiscountImageBID));
		}
		this.Price.color = new Color(0.72156864f, 0.6117647f, 0f, 1f);
		if (string.IsNullOrEmpty(product.PriceString))
		{
			this.Price.text = string.Format("{0} {1}", product.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow).ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
		}
		else
		{
			this.Price.text = product.PriceString;
		}
	}

	public new void BuyAccount()
	{
		PhotonConnectionFactory.Instance.CaptureActionInStats("PrShopOldBuyAction", this._product.ProductId.ToString(), ShowInfoPanel.InfoPanelType.MoneyPack.ToString(), null);
		BuyProductManager.InitiateProductPurchase(this._product);
	}

	public Text Silvers;
}
