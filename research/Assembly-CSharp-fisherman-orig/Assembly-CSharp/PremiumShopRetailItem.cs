using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Helpers;
using DG.Tweening;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PremiumShopRetailItem : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<StoreProduct> OnMoveComplete = delegate(StoreProduct product)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<StoreProduct> OnSelect = delegate(StoreProduct product)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<StoreProduct> OnBuy = delegate(StoreProduct product)
	{
	};

	public static float Offset { get; private set; }

	public string Currency
	{
		get
		{
			return this._currency.text;
		}
	}

	public string Price
	{
		get
		{
			return this._price.text;
		}
	}

	public int ProductId
	{
		get
		{
			return (this._product != null) ? this._product.ProductId : 0;
		}
	}

	public StoreProduct Product
	{
		get
		{
			return this._product;
		}
	}

	private void Awake()
	{
		this._active.SetActive(false);
		this._notActive.SetActive(false);
		if (PremiumShopRetailItem.Offset <= 0f)
		{
			PremiumShopRetailItem.Offset = this._rt.rect.width + 32f;
		}
	}

	public void Init(StoreProduct p)
	{
		this._product = p;
		this._imageLdbl.Load(this._product.PremShopImageBID, this._image, "Textures/Inventory/{0}");
		if (this.IsPurchased(p))
		{
			this._btnBuy.gameObject.SetActive(false);
			this._purchased.SetActive(true);
			Text priceActive = this._priceActive;
			string text = string.Empty;
			this._currencyActive.text = text;
			text = text;
			this._currency.text = text;
			text = text;
			this._price.text = text;
			priceActive.text = text;
		}
		else if (this._product.ProductCurrency == "USD")
		{
			Text priceActive2 = this._priceActive;
			string text = (string.IsNullOrEmpty(this._product.PriceString) ? string.Format("{0} {1}", (!PremiumItemBase.HasValue(this._product.Price)) ? "0" : this._product.Price.ToString("N2"), (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency)) ? PhotonConnectionFactory.Instance.Profile.PaymentCurrency : "USD") : this._product.PriceString);
			this._price.text = text;
			priceActive2.text = text;
			Text currency = this._currency;
			text = string.Empty;
			this._currencyActive.text = text;
			currency.text = text;
		}
		else
		{
			Text priceActive3 = this._priceActive;
			string text = ((!PremiumItemBase.HasValue(this._product.Price)) ? "0" : this._product.Price.ToString("N2"));
			this._price.text = text;
			priceActive3.text = text;
			Text currency2 = this._currency;
			text = MeasuringSystemManager.GetCurrencyIcon(this._product.ProductCurrency);
			this._currencyActive.text = text;
			currency2.text = text;
		}
		this.DeSelect();
	}

	public void Move(float x, float t)
	{
		Vector2 vector;
		vector..ctor(x, this._rt.anchoredPosition.y);
		if (t <= 0f)
		{
			this._rt.anchoredPosition = vector;
		}
		else
		{
			TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._rt, vector, t, false), 6), delegate
			{
				this.OnMoveComplete(this._product);
			});
		}
	}

	public void Remove()
	{
		Object.Destroy(base.gameObject);
	}

	public void Buy()
	{
		if (this.IsPurchased(this._product))
		{
			return;
		}
		this.CaptureActionInStats("PrShopBuyAction");
		this.OnBuy(this._product);
	}

	public void Click()
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
		{
			this.Buy();
		}
	}

	public void Select()
	{
		this.CaptureActionInStats("PrShopClickAction");
		this._active.SetActive(true);
		this._notActive.SetActive(false);
		TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(this._rt, this._selectedScale, 0.05f), 17), delegate
		{
			this.OnSelect(this._product);
		});
	}

	public void DeSelect()
	{
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(this._rt, Vector3.one, 0.05f), 27);
		this._active.SetActive(false);
		this._notActive.SetActive(true);
	}

	private void CaptureActionInStats(string action)
	{
		PhotonConnectionFactory.Instance.CaptureActionInStats(action, this._product.ProductId.ToString(), PremiumShopCategories.FishingPacks.ToString(), PremiumShopMainPageHandler.ProductCategories.Dlc.ToString());
	}

	private bool IsPurchased(StoreProduct p)
	{
		return false;
	}

	[SerializeField]
	private Text _debugId;

	[SerializeField]
	private Image _image;

	private ResourcesHelpers.AsyncLoadableImage _imageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private Text _priceActive;

	[SerializeField]
	private Text _currencyActive;

	[SerializeField]
	private Text _price;

	[SerializeField]
	private Text _currency;

	[SerializeField]
	private GameObject _active;

	[SerializeField]
	private GameObject _notActive;

	[SerializeField]
	private GameObject _purchased;

	[SerializeField]
	private RectTransform _rt;

	[SerializeField]
	private BorderedButton _btnBuy;

	[SerializeField]
	private Button _btnSelect;

	public const float XdistBetween = 32f;

	private StoreProduct _product;

	private const float ScaleSpeed = 0.05f;

	private readonly Vector3 _selectedScale = new Vector3(1.0125f, 1.04f, 1f);
}
