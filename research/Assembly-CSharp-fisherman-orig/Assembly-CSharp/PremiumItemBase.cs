using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.PlayerProfile;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PremiumItemBase : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnDestroyEv = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSelectedEv = delegate
	{
	};

	public int ProductId
	{
		get
		{
			return (this.Product == null) ? (-1) : this.Product.ProductId;
		}
	}

	public PremiumCategoryManager.PremiumItem Item
	{
		get
		{
			return this.ProductItem;
		}
	}

	protected StoreProduct Product
	{
		get
		{
			return (this.ProductItem == null) ? null : this.ProductItem.Product;
		}
	}

	protected virtual void Awake()
	{
		this.HkPressRed = base.GetComponent<HotkeyPressRedirect>();
	}

	protected virtual void OnDestroy()
	{
		BuyProductManager.Dispose();
	}

	protected virtual void Update()
	{
		if (this.HasRemain)
		{
			if (this.TimeRemain.TotalSeconds > 0.0)
			{
				this.TimeRemain = TimeSpan.FromSeconds(this.TimeRemain.TotalSeconds - (double)Time.deltaTime);
			}
			else if (this.RemainGo() == null)
			{
				this.CallDestroyEv();
			}
			else if (this.RemainGo().activeSelf)
			{
				this.SetTimeRemain(this.TimeRemain);
			}
		}
		if (this.HasDiscountTime)
		{
			if (this.TimeDiscount.TotalSeconds > 0.0)
			{
				this.TimeDiscount = TimeSpan.FromSeconds(this.TimeDiscount.TotalSeconds - (double)Time.deltaTime);
			}
			else
			{
				this.InitMain();
			}
		}
	}

	protected virtual GameObject RemainGo()
	{
		return null;
	}

	public virtual string GetImageHeaderPath()
	{
		return null;
	}

	public virtual string GetDescription()
	{
		return (this.Product == null) ? string.Empty : string.Format(this.Product.Desc, "\n", "\t");
	}

	public virtual void Init(PremiumCategoryManager.PremiumItem p)
	{
		this.ProductItem = p;
		this.ImageLdbl.Image = this.Image;
		this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", this.Product.ImageBID));
		this.SetCurrency();
		this.SetPrice();
		this.InitMain();
	}

	public virtual bool IsSelected
	{
		get
		{
			return this.BuyPanelGo.activeSelf;
		}
	}

	public virtual void Select()
	{
		this.Select(true);
	}

	public virtual void Select(bool isCaptureAction)
	{
		if (this.IsSelected)
		{
			return;
		}
		if (isCaptureAction)
		{
			this.CaptureActionInStats("PrShopClickAction");
		}
		this.BuyPanelGo.SetActive(true);
		if (this.IsScale())
		{
			ShortcutExtensions.DOKill(base.transform, false);
			TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(base.transform, this.GetSelectedScale, 0.05f), 17);
		}
		if (this.IsChangeColor())
		{
			this.ImageBg.color = this.SelectedColor;
		}
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad && this.HkPressRed != null)
		{
			this.HkPressRed.StartListenForHotkeys();
		}
		this.OnSelected();
	}

	public virtual void Deselect()
	{
		if (!this.IsSelected)
		{
			return;
		}
		this.BuyPanelGo.SetActive(false);
		if (this.IsScale())
		{
			ShortcutExtensions.DOKill(base.transform, false);
			TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(base.transform, Vector3.one, 0.05f), 27);
		}
		if (this.IsChangeColor())
		{
			this.ImageBg.color = this.NormalColor;
		}
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad && this.HkPressRed != null)
		{
			this.HkPressRed.StopListenForHotKeys();
		}
	}

	public virtual void BuyAction()
	{
		this.CaptureActionInStats("PrShopBuyAction");
		BuyProductManager.InitiateProductPurchase(this.Product);
	}

	public static bool HasActiveDiscount(StoreProduct p)
	{
		return p.HasActiveDiscount(TimeHelper.UtcTime());
	}

	public static bool HasDiscount(StoreProduct p)
	{
		return p.DiscountPrice != null && PremiumItemBase.HasValue(p.DiscountPrice.Value);
	}

	public static bool HasValue(double value)
	{
		return !double.IsNaN(value) && !double.IsInfinity(value);
	}

	protected virtual void SetTimeRemain(TimeSpan timeRemain)
	{
		this.TimeRemain = timeRemain;
		this.RemainGo().SetActive(this.TimeRemain.TotalSeconds > 0.0);
		if (this.HasRemain && !this.RemainGo().activeSelf)
		{
			this.CallDestroyEv();
		}
	}

	protected virtual void InitMain()
	{
		bool flag = PremiumItemBase.HasDiscount(this.Product);
		bool flag2 = ((this.Product.DiscountStart == null || this.Product.DiscountEnd == null) ? flag : (flag && PremiumItemBase.HasActiveDiscount(this.Product)));
		this.HasDiscountTime = flag && PremiumItemBase.HasActiveDiscount(this.Product);
		if (this.HasDiscountTime && this.Product.DiscountEnd != null)
		{
			this.TimeDiscount = this.Product.DiscountEnd.Value - TimeHelper.UtcTime();
		}
		this.PriceGo.SetActive(!flag2);
		this.PriceDiscountGo.SetActive(flag2);
		Text currency = this.Currency;
		string text = PlayerProfileHelper.PaymentCurrency;
		this.CurrencyDiscount.text = text;
		currency.text = text;
		TMP_Text priceWithoutDiscount = this.PriceWithoutDiscount;
		text = this.Product.Price.ToString("N2");
		this.Price.text = text;
		priceWithoutDiscount.text = text;
		if (this.CurrencyWithoutDiscount != null)
		{
			this.CurrencyWithoutDiscount.text = this.Currency.text;
		}
		else
		{
			this.PriceWithoutDiscount.text = string.Format("{0} {1}", this.Price.text, this.Currency.text);
		}
		if (flag2)
		{
			double? discountPrice = this.Product.DiscountPrice;
			double? num = ((discountPrice == null) ? null : new double?(this.Product.Price - discountPrice.GetValueOrDefault()));
			double num2 = ((num == null) ? 0.0 : num.Value) * 100.0 / this.Product.Price;
			this.DiscountPrc.text = string.Format("-{0}%", (int)num2);
			Text priceDiscount = this.PriceDiscount;
			double? discountPrice2 = this.Product.DiscountPrice;
			priceDiscount.text = ((discountPrice2 == null) ? 0.0 : discountPrice2.Value).ToString("N2");
		}
	}

	protected virtual void SetCurrency()
	{
		this.CurrencyName.text = ScriptLocalization.Get("PremiumAccountCaption").ToUpper();
		if (this.Product.Term != null)
		{
			this.CurrencyIco.text = this.Product.Term.Value.ToString();
		}
	}

	protected virtual void SetPrice()
	{
		if (this.Product.Term != null)
		{
			this.Silvers.text = DateTimeExtensions.GetDaysLocalization(this.Product.Term.Value);
		}
	}

	protected virtual Vector3 GetSelectedScale
	{
		get
		{
			return this._selectedScale;
		}
	}

	protected virtual bool IsScale()
	{
		return true;
	}

	protected virtual bool IsChangeColor()
	{
		return true;
	}

	protected virtual BuyProductManager.UrlTypes UrlType()
	{
		return BuyProductManager.UrlTypes.BuySubscription;
	}

	protected virtual void OnSelected()
	{
		this.OnSelectedEv();
	}

	protected virtual void CallDestroyEv()
	{
		this.OnDestroyEv();
	}

	protected virtual void CaptureActionInStats(string action)
	{
		PhotonConnectionFactory.Instance.CaptureActionInStats(action, this.ProductId.ToString(), this.ProductItem.UiCategory.ToString(), this.ProductItem.Category.ToString());
	}

	[SerializeField]
	protected Text DebugId;

	[SerializeField]
	protected GameObject BuyPanelGo;

	[SerializeField]
	protected GameObject PriceGo;

	[SerializeField]
	protected GameObject PriceDiscountGo;

	[SerializeField]
	protected Text PriceDiscount;

	[SerializeField]
	protected Text Currency;

	[SerializeField]
	protected Text CurrencyDiscount;

	[SerializeField]
	protected Text DiscountPrc;

	[SerializeField]
	protected TextMeshProUGUI PriceWithoutDiscount;

	[SerializeField]
	protected TextMeshProUGUI CurrencyWithoutDiscount;

	[SerializeField]
	protected Text CurrencyIco;

	[SerializeField]
	protected Text CurrencyName;

	[SerializeField]
	protected Image ImageBg;

	[SerializeField]
	protected Text Silvers;

	[SerializeField]
	protected Text Price;

	[SerializeField]
	protected Image Image;

	protected ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	protected HotkeyPressRedirect HkPressRed;

	protected PremiumCategoryManager.PremiumItem ProductItem;

	protected const float ScaleSpeed = 0.05f;

	protected readonly Color SelectedColor = new Color(1f, 0.99607843f, 0.972549f);

	protected readonly Color NormalColor = new Color(1f, 0.99215686f, 0.92156863f);

	protected TimeSpan TimeRemain = TimeSpan.Zero;

	protected bool HasRemain;

	protected TimeSpan TimeDiscount = TimeSpan.Zero;

	protected bool HasDiscountTime;

	private readonly Vector3 _selectedScale = new Vector3(1.05f, 1.05f, 1f);

	public const string PrShopBuyActionTag = "PrShopBuyAction";

	public const string PrShopClickActionTag = "PrShopClickAction";

	public const string PrShopOldBuyActionTag = "PrShopOldBuyAction";

	public const string PrShopOldClickActionTag = "PrShopOldClickAction";
}
