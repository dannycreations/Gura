using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class PremiumPassInit : MonoBehaviour
{
	public void Init(List<StoreProduct> passes)
	{
		this._passes = passes;
		List<StoreProduct> list = passes ?? passes.ToList<StoreProduct>();
		bool flag = false;
		StoreProduct storeProduct = new StoreProduct();
		for (int i = 0; i < list.Count - 1; i++)
		{
			if (!flag)
			{
				flag = list[i].HasActiveDiscount(PhotonConnectionFactory.Instance.ServerUtcNow);
			}
			if (i == 0)
			{
				storeProduct = list[i];
			}
			if (list[i].Price < storeProduct.Price)
			{
				storeProduct = list[i];
			}
		}
		base.GetComponent<ShowInfoPanel>().ProductInfo = storeProduct;
		this.ImageLdbl.Image = this.Image;
		this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", storeProduct.ImageBID));
		if (flag)
		{
			this.DiscountPriceInit(storeProduct);
		}
		else
		{
			this.NormalPriceInit(storeProduct);
		}
	}

	private void NormalPriceInit(StoreProduct product)
	{
		this.DiscountImage.overrideSprite = ResourcesHelpers.GetTransparentSprite();
		this.Price.color = Color.white;
		if (string.IsNullOrEmpty(product.PriceString))
		{
			this.Price.text = string.Format("{0} {1} {2}", ScriptLocalization.Get("FromCaption"), product.Price.ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
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
			this.DiscountImageLdbl.Image = this.DiscountImage;
			this.DiscountImageLdbl.Load(string.Format("Textures/Inventory/{0}", product.DiscountImageBID));
		}
		if (product.HasActiveDiscount(PhotonConnectionFactory.Instance.ServerUtcNow))
		{
			this.Price.color = new Color(0.72156864f, 0.6117647f, 0f, 1f);
		}
		if (string.IsNullOrEmpty(product.PriceString))
		{
			this.Price.text = string.Format("{0} {1}", product.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow).ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
		}
		else
		{
			this.Price.text = product.PriceString;
		}
	}

	public void BuyClick()
	{
		if ((StaticUserData.ClientType == ClientTypes.SteamWindows || StaticUserData.ClientType == ClientTypes.SteamLinux || StaticUserData.ClientType == ClientTypes.SteamOsX) && !SteamUtils.IsOverlayEnabled())
		{
			MessageBox messageBox = this.helpers.ShowMessage(null, ScriptLocalization.Get("SteamOverlayNotFoundCaption"), ScriptLocalization.Get("SteamOverlayNotFoundMessage"), false, false, false, null);
			messageBox.GetComponent<EventAction>().ActionCalled += delegate(object sender, EventArgs args)
			{
				messageBox.Close();
			};
			return;
		}
		this._messageBox = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.buyPassSelectionPrefab).GetComponent<MessageBox>();
		this._messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this._messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this._messageBox.GetComponent<BuyPassSelectionInit>().Init(this._passes);
	}

	public Text Price;

	public Image Image;

	public Image DiscountImage;

	private ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private ResourcesHelpers.AsyncLoadableImage DiscountImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private MessageBox _messageBox;

	private List<StoreProduct> _passes;

	private MenuHelpers helpers = new MenuHelpers();
}
