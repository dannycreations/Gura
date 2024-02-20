using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PremiumMoneyPackInfoPanelInit : MonoBehaviour
{
	public void Init(StoreProduct product)
	{
		this.Credits.text = product.Gold.Value.ToString("f0");
		this.Desc.text = string.Format(product.Desc, "\n", "\t");
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
		this.Price.gameObject.SetActive(true);
		this.Price.color = this.Credits.color;
		this.NormalPrice.transform.parent.gameObject.gameObject.SetActive(false);
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
		if (product.DiscountPrice == null)
		{
			this.Price.gameObject.SetActive(true);
			this.Price.color = new Color(0.72156864f, 0.6117647f, 0f, 1f);
			this.NormalPrice.transform.parent.gameObject.gameObject.SetActive(false);
			if (string.IsNullOrEmpty(product.PriceString))
			{
				this.Price.text = string.Format("{0} {1}", product.Price.ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
			}
			else
			{
				this.Price.text = product.PriceString;
			}
		}
		else
		{
			this.Price.gameObject.SetActive(false);
			this.NormalPrice.transform.parent.gameObject.gameObject.SetActive(true);
			this.DiscountPrice.text = string.Format("{0} {1}", product.DiscountPrice.Value.ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
			this.NormalPrice.text = string.Format("{0} {1}", product.Price.ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
			RectTransform component = this.NormalPrice.transform.Find("Image").GetComponent<RectTransform>();
			component.rect.Set(component.rect.x, component.rect.y, this.NormalPrice.preferredWidth, component.rect.height);
		}
	}

	public Text Credits;

	public Text Price;

	public Text NormalPrice;

	public Text DiscountPrice;

	public Text Desc;
}
