using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class BuyPassSelectionInit : MonoBehaviour
{
	public void Init(List<StoreProduct> passes)
	{
		this._passes = passes;
		StoreProduct storeProduct = passes.First((StoreProduct x) => x.Term == 3);
		this.Day3Name.text = storeProduct.Name;
		if (storeProduct.HasActiveDiscount(PhotonConnectionFactory.Instance.ServerUtcNow))
		{
			this.DiscountPriceInit(storeProduct, this.Day3Price);
		}
		else
		{
			this.NormalPriceInit(storeProduct, this.Day3Price);
		}
		StoreProduct storeProduct2 = passes.First((StoreProduct x) => x.Term == 7);
		this.Day7Name.text = storeProduct2.Name;
		if (storeProduct2.HasActiveDiscount(PhotonConnectionFactory.Instance.ServerUtcNow))
		{
			this.DiscountPriceInit(storeProduct2, this.Day7Price);
		}
		else
		{
			this.NormalPriceInit(storeProduct2, this.Day7Price);
		}
		StoreProduct storeProduct3 = passes.First((StoreProduct x) => x.Term == 30);
		this.Day30Name.text = storeProduct3.Name;
		if (storeProduct3.HasActiveDiscount(PhotonConnectionFactory.Instance.ServerUtcNow))
		{
			this.DiscountPriceInit(storeProduct3, this.Day30Price);
		}
		else
		{
			this.NormalPriceInit(storeProduct3, this.Day30Price);
		}
		this.Day3Toggle.isOn = true;
	}

	public void ApplyClick()
	{
		int[] pondIds = this._passes[0].PondsUnlocked;
		List<Pond> list = CacheLibrary.MapCache.CachedPonds.Where((Pond x) => pondIds.Contains(x.PondId)).ToList<Pond>();
		if (list.Count > 0 && list[0].MinLevel <= PhotonConnectionFactory.Instance.Profile.Level)
		{
			if (list[0].MinLevelExpiration == null)
			{
				MessageBox messageBox2 = this.helpers.ShowMessageSelectable(null, string.Empty, string.Format(ScriptLocalization.Get("HavePondAccessPermanent"), "\n"), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, true);
				messageBox2.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object e, EventArgs obj)
				{
					messageBox2.Close();
				};
				messageBox2.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object e, EventArgs obj)
				{
					messageBox2.Close();
					this.BuyAction();
				};
			}
			else
			{
				MessageBox messageBox = this.helpers.ShowMessageSelectable(null, string.Empty, string.Format(ScriptLocalization.Get("HavePondAccess"), "\n", MeasuringSystemManager.DateTimeString(list[0].MinLevelExpiration.Value.ToLocalTime())), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, true);
				messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object e, EventArgs obj)
				{
					messageBox.Close();
				};
				messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object e, EventArgs obj)
				{
					messageBox.Close();
					this.BuyAction();
				};
			}
		}
		else
		{
			this.BuyAction();
		}
	}

	private void BuyAction()
	{
		StoreProduct storeProduct = null;
		if (this.Day3Toggle.isOn)
		{
			storeProduct = this._passes.First((StoreProduct x) => x.Term == 3);
		}
		if (this.Day7Toggle.isOn)
		{
			storeProduct = this._passes.First((StoreProduct x) => x.Term == 7);
		}
		if (this.Day30Toggle.isOn)
		{
			storeProduct = this._passes.First((StoreProduct x) => x.Term == 30);
		}
		if (storeProduct == null)
		{
			Debug.LogError("No selected pass on buy action!");
			return;
		}
		PhotonConnectionFactory.Instance.CaptureActionInStats("PrShopOldBuyAction", storeProduct.ProductId.ToString(), ShowInfoPanel.InfoPanelType.PassPack.ToString(), null);
		BuyProductManager.InitiateProductPurchase(storeProduct);
		this.CloseClick();
	}

	public void CloseClick()
	{
		base.GetComponent<MessageBox>().Close();
	}

	private void NormalPriceInit(StoreProduct product, Text text)
	{
		text.color = Color.white;
		if (string.IsNullOrEmpty(product.PriceString))
		{
			text.text = string.Format("{0} {1}", product.Price.ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
		}
		else
		{
			text.text = product.PriceString;
		}
	}

	private void DiscountPriceInit(StoreProduct product, Text text)
	{
		text.color = new Color(0.72156864f, 0.6117647f, 0f, 1f);
		if (string.IsNullOrEmpty(product.PriceString))
		{
			text.text = string.Format("{0} {1}", product.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow).ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD");
		}
		else
		{
			text.text = product.PriceString;
		}
	}

	public Toggle Day1Toggle;

	public Toggle Day3Toggle;

	public Toggle Day7Toggle;

	public Toggle Day30Toggle;

	[Space(20f)]
	public Text Day1Name;

	public Text Day1Price;

	[Space(10f)]
	public Text Day3Name;

	public Text Day3Price;

	[Space(10f)]
	public Text Day7Name;

	public Text Day7Price;

	[Space(10f)]
	public Text Day30Name;

	public Text Day30Price;

	private List<StoreProduct> _passes;

	private MenuHelpers helpers = new MenuHelpers();
}
