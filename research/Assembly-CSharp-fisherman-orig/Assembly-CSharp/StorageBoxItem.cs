using System;
using System.Diagnostics;
using System.Globalization;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class StorageBoxItem : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private event EventHandler<EventArgs> OnChoose = delegate
	{
	};

	private void Awake()
	{
		this.BuyButton.gameObject.SetActive(false);
	}

	public void Selected()
	{
		this.OnToggleValueChanged(true);
	}

	public void SetHelp(bool set)
	{
		this.OnToggleValueChanged(set);
	}

	public void ShowDetails()
	{
		if (this.OnChoose != null)
		{
			this.OnChoose(this, null);
		}
	}

	public void OnToggleValueChanged(bool isOn)
	{
		if (isOn)
		{
			this.ShowDetails();
			if (this.ElementInputListener == null)
			{
				this.ElementInputListener = base.GetComponent<HotkeyPressRedirect>();
			}
			this.ElementInputListener.StartListenForHotkeys();
		}
		else
		{
			this.OnDeselect();
		}
	}

	public void OnDeselect()
	{
		if (this.ElementInputListener == null)
		{
			this.ElementInputListener = base.GetComponent<HotkeyPressRedirect>();
		}
		this.ElementInputListener.StopListenForHotKeys();
	}

	public void FillData(EventHandler<EventArgs> onChoose, bool isBuyBtnActive)
	{
		this.BuyButton.gameObject.SetActive(isBuyBtnActive);
		this.SaveIcon.SetActive(false);
		this.Title.text = this.Product.Name;
		if (onChoose != null)
		{
			this.OnChoose = onChoose;
		}
		this.Currency = this.Product.ProductCurrency;
		bool flag = this.Product.ProductCurrency == "GC";
		bool flag2 = this.Product.HasActiveDiscount(PhotonConnectionFactory.Instance.ServerUtcNow) && this.Product.DiscountPrice != null;
		this.Cost = this.Product.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow);
		this.PriceCurrency.text = (flag ? "\ue62c" : "\ue62b");
		this.PriceCurrency.color = (flag ? new Color(0.72156864f, 0.6117647f, 0f, 1f) : new Color(0.24313726f, 0.25490198f, 0.28627452f, 1f));
		this.PriceValue.text = this.Cost.ToString(CultureInfo.InvariantCulture);
		if (this.Product.InventoryExt != null)
		{
			this.SlotsAmount.text = "+" + this.Product.InventoryExt.Value;
			this.SlotsIcon.text = "\ue746";
		}
		else if (this.Product.RodSetupExt != null)
		{
			this.SlotsAmount.text = "+" + this.Product.RodSetupExt.Value;
			this.SlotsIcon.text = "\ue749";
		}
		else if (this.Product.BuoyExt != null)
		{
			this.SlotsAmount.text = "+" + this.Product.BuoyExt.Value;
			this.SlotsIcon.text = "\ue745";
		}
		else
		{
			this.SlotsAmount.transform.parent.gameObject.SetActive(false);
		}
		this.SaveIcon.SetActive(flag2);
		this.OldCostText.gameObject.SetActive(flag2);
		if (flag2)
		{
			Text saveAmount = this.SaveAmount;
			string text = "-{0} %";
			double? discountPrice = this.Product.DiscountPrice;
			double? num = ((discountPrice == null) ? null : new double?(this.Product.Price - discountPrice.GetValueOrDefault()));
			double? num2 = ((num == null) ? null : new double?(100.0 * num.GetValueOrDefault()));
			saveAmount.text = string.Format(text, (num2 == null) ? null : new double?(num2.GetValueOrDefault() / this.Product.Price));
			this.OldCostText.text = this.Product.Price.ToString(CultureInfo.InvariantCulture);
		}
		if (this.Product.ImageBID != null)
		{
			this.thumbnailLdbl.Image = this.Thumbnail;
			this.thumbnailLdbl.Load(string.Format("Textures/Inventory/{0}", this.Product.ImageBID.Value.ToString(CultureInfo.InvariantCulture)));
		}
		if (this.Product.TypeId == 5 && this.Product.InventoryExt != null && PhotonConnectionFactory.Instance.Profile.Inventory.CurrentInventoryCapacity + this.Product.InventoryExt.Value > Inventory.MaxInventoryCapacity)
		{
			this.BuyButton.enabled = false;
		}
		if (this.Product.TypeId == 6 && this.Product.RodSetupExt != null && PhotonConnectionFactory.Instance.Profile.Inventory.CurrentRodSetupCapacity + this.Product.RodSetupExt.Value > Inventory.MaxRodSetupCapacity)
		{
			this.BuyButton.enabled = false;
		}
		if (this.Product.TypeId == 7 && this.Product.BuoyExt != null && PhotonConnectionFactory.Instance.Profile.Inventory.CurrentBuoyCapacity + this.Product.BuoyExt.Value > Inventory.MaxBuoyCapacity)
		{
			this.BuyButton.enabled = false;
		}
	}

	public void Buy()
	{
		LogHelper.Log("___kocha Buy ProductId:{0} Name:{1}", new object[]
		{
			this.Product.ProductId,
			this.Product.Name
		});
		this.BuyButton.onClick.Invoke();
	}

	public BorderedButton Button;

	public Text Title;

	public Text PriceCurrency;

	public Text PriceValue;

	public Text OldCostText;

	public Text SlotsAmount;

	public Text SlotsIcon;

	public Text SaveAmount;

	public GameObject SaveIcon;

	public StoreProduct Product;

	public Image Thumbnail;

	private ResourcesHelpers.AsyncLoadableImage thumbnailLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Toggle Toggle;

	public Button BuyButton;

	[HideInInspector]
	public double Cost;

	[HideInInspector]
	public string Currency;

	public HotkeyPressRedirect ElementInputListener;
}
