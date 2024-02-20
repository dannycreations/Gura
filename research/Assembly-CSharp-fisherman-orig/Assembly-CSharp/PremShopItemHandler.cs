using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Assets.Scripts.UI._2D.Helpers;
using Coffee.UIExtensions;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PremShopItemHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBack = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnActive = delegate(bool b)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<StoreProduct, PremiumShopMainHandler.ProductCategories> OnClick = delegate(StoreProduct p, PremiumShopMainHandler.ProductCategories c)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<StoreProduct, PremiumShopMainHandler.ProductCategories> OnClickDetails = delegate(StoreProduct p, PremiumShopMainHandler.ProductCategories c)
	{
	};

	public StoreProduct Product { get; protected set; }

	public string PriceStr
	{
		get
		{
			if (this.Product.HasActiveDiscount(TimeHelper.UtcTime()))
			{
				double num;
				if (this.Product.DiscountPrice != null)
				{
					num = this.Product.DiscountPrice.Value;
				}
				else
				{
					num = this.Product.Price;
				}
				return string.Format("<b><color=#BB2200ff>{0}</color></b> {1}", num.ToString("N2"), this.ProductCurrency);
			}
			return Regex.Replace(this.PriceValue.text, "<.*?>", string.Empty);
		}
	}

	public bool IsActive
	{
		get
		{
			return (!(this.Parent == null)) ? (this.Cg.alpha > 0f) : base.gameObject.activeSelf;
		}
	}

	public GameObject Parent { get; private set; }

	public PremShopItemHandler LinkedItem { get; private set; }

	private void Awake()
	{
		this.Hkpr.SetPausedFromScript(true);
		this.Hkpr.StopListenForHotKeys();
		this.Btn.onClick.AddListener(delegate
		{
			this.OnClick(this.Product, this._category);
		});
		this.BtnDetails.onClick.AddListener(delegate
		{
			this.OnClickDetails(this.Product, this._category);
		});
		this.BtnBack.onClick.AddListener(delegate
		{
			this.OnBack();
		});
		this.Tgl.onValueChanged.AddListener(delegate(bool b)
		{
			this.Hkpr.SetPausedFromScript(!b);
			if (b)
			{
				this.Hkpr.StartListenForHotkeys();
			}
			else
			{
				this.Hkpr.StopListenForHotKeys();
			}
			if (b)
			{
				this.Select(true);
			}
			else if (!this._isMouseOver)
			{
				this.Select(false);
			}
			this.OnActive(b);
		});
	}

	private void Update()
	{
		if (this._endDiscountTime != null)
		{
			double? endDiscountTime = this._endDiscountTime;
			this._endDiscountTime = ((endDiscountTime == null) ? null : new double?(endDiscountTime.GetValueOrDefault() - (double)Time.deltaTime));
			if (this._endDiscountTime <= 0.0)
			{
				this._endDiscountTime = null;
				this.UpdateDiscount();
			}
		}
		if (this._lifetimeRemain != null)
		{
			this.HotValue.text = this.GetLifetimeRemainFormated(this._lifetimeRemain.Value - TimeHelper.UtcTime());
		}
	}

	public void Init(StoreProduct p, ToggleGroup tg, DateTime? lifetime, PremiumShopMainHandler.ProductCategories cat, bool isFastBuy, bool openFromDetails = false)
	{
		this.OnActive = delegate(bool b)
		{
		};
		this.OnClick = delegate(StoreProduct pr, PremiumShopMainHandler.ProductCategories c)
		{
		};
		this.OnClickDetails = delegate(StoreProduct pr, PremiumShopMainHandler.ProductCategories c)
		{
		};
		this.OnBack = delegate
		{
		};
		this.Tgl.group = tg;
		this._openFromDetails = openFromDetails;
		this._lifetimeRemain = lifetime;
		this.Hot.SetActive(this._lifetimeRemain != null);
		this.Product = p;
		this._category = cat;
		this._isFastBuy = isFastBuy;
		this.BtnDetails.interactable = this._isFastBuy;
		bool flag = ChangeLanguage.GetCurrentLanguage.Lang == CustomLanguages.Chinese || ChangeLanguage.GetCurrentLanguage.Lang == CustomLanguages.ChineseTraditional;
		this.Name.text = p.Name;
		if (p.TypeId == 4)
		{
			if (this._openFromDetails)
			{
				this.Name.text = string.Format("<size=30>\ue725 <b>{0} {1}</size>", p.Term, DateTimeExtensions.GetDaysLocalization(p.Term.Value));
			}
			else
			{
				this.Name.text = string.Format("<size=30>\ue725 </size><size=20><color=#CCCCCCFF>{0}</color></size> <b>{1} {2}", ScriptLocalization.Get("FromCaption"), p.Term, DateTimeExtensions.GetDaysLocalization(p.Term.Value));
			}
		}
		else if (p.TypeId == 1)
		{
			try
			{
				int num = this.Name.text.IndexOfAny(PremShopItemHandler.Numbers);
				int num2 = this.Name.text.LastIndexOfAny(PremShopItemHandler.Numbers);
				string text = this.Name.text.Substring(num, num2 + 1);
				string text2 = this.Name.text.Substring(num2 + 1);
				if (!string.IsNullOrEmpty(p.PromoText))
				{
					if (flag)
					{
						this.Name.text = string.Format("<color=#FFC60DFF><b>{0}</b></color>{1}", text, text2);
					}
					else
					{
						this.Name.text = string.Format("<b><color=#FFC60DFF>{0}</color>{1}</b>", text, text2);
					}
				}
				else if (flag)
				{
					this.Name.text = string.Format("<b>{0}</b>{1}", text, text2);
				}
				else
				{
					this.Name.text = string.Format("<b>{0}{1}</b>", text, text2);
				}
				this.Name.text = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon("GC"), this.Name.text);
			}
			catch (Exception ex)
			{
				LogHelper.Error("Parsing error 'Name' for ProductId:{0} TypeId:MoneyPack Message:{1}", new object[] { p.ProductId, ex.Message });
			}
		}
		else if (p.TypeId == 3)
		{
			try
			{
				if (flag)
				{
					int num3 = this.Name.text.IndexOfAny("天月".ToCharArray());
					this.Name.text = string.Format("<b>{0}</b>\n{1}", this.Name.text.Substring(0, num3 + 1), this.Name.text.Substring(num3 + 1));
				}
				else
				{
					char[] array = this.Name.text.ToCharArray();
					List<char> list = new List<char>();
					List<char> list2 = new List<char>();
					int num4 = 0;
					foreach (char c2 in array)
					{
						bool flag2 = char.IsWhiteSpace(c2);
						if (flag2)
						{
							num4++;
						}
						if (num4 >= 2)
						{
							list2.Add(c2);
						}
						else
						{
							list.Add(c2);
						}
					}
					this.Name.text = string.Format("<b>{0}</b>\n{1}", new string(list.ToArray()), new string(list2.ToArray()).TrimStart(new char[0]));
				}
			}
			catch (Exception ex2)
			{
				LogHelper.Error("Parsing error 'Name' for ProductId:{0} TypeId:PremiumAccount Message:{1}", new object[] { p.ProductId, ex2.Message });
			}
		}
		else
		{
			this.Name.text = string.Format("<b>{0}</b>", p.Name);
		}
		if (!string.IsNullOrEmpty(p.PromoText))
		{
			bool flag3 = MeasuringSystemManager.ContainsIconFromWeb(p.PromoText);
			if (flag3)
			{
				this.PromoMoneyValue.text = MeasuringSystemManager.GetIconFromWeb(p.PromoText, (!(this.Parent == null)) ? 26 : 53);
			}
			else
			{
				this.PromoNoMoneyValue.text = p.PromoText;
			}
			this.PromoMoney.SetActive(flag3);
			this.PromoNoMoney.SetActive(!flag3);
		}
		else
		{
			this.PromoMoney.SetActive(false);
			this.PromoNoMoney.SetActive(false);
		}
		this.ImageLdbl.Load(this.Product.PremShopImageBID, this.Img, "Textures/Inventory/{0}");
		this.UpdateDiscount();
		ClientDebugHelper.Log(ProfileFlag.ProductDelivery, string.Format("PremShopItemHandler - Price:{0} PriceString:{1} DiscountPrice:{2} ProductId:{3} ProductCurrency:{4}", new object[] { p.Price, p.PriceString, p.DiscountPrice, p.ProductId, p.ProductCurrency }));
	}

	public void SetNavigation(Selectable left)
	{
		Selectable tgl = this.Tgl;
		Navigation navigation = default(Navigation);
		navigation.mode = 4;
		navigation.selectOnUp = this.Tgl.navigation.selectOnUp;
		navigation.selectOnDown = null;
		navigation.selectOnLeft = left;
		navigation.selectOnRight = this.Tgl.navigation.selectOnRight;
		tgl.navigation = navigation;
	}

	public void SetParent(GameObject parent)
	{
		this.Parent = parent;
	}

	public void SetActive(bool flag)
	{
		if (this.Parent == null)
		{
			base.gameObject.SetActive(flag);
		}
		else
		{
			this.Cg.alpha = ((!flag) ? 0f : 1f);
			this.Cg.interactable = flag;
		}
	}

	public void SetParentActive(bool flag)
	{
		this.Parent.SetActive(flag);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this._isMouseOver = true;
		this.Select(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this._isMouseOver = false;
		if (!this.Tgl.isOn)
		{
			this.Select(false);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (this._isFastBuy && eventData.button == 1)
		{
			this.OnClickDetails(this.Product, this._category);
		}
	}

	public void SetLinked(PremShopItemHandler item)
	{
		this.LinkedItem = item;
	}

	public void Select()
	{
		this.Tgl.isOn = true;
	}

	private void Select(bool flag)
	{
		this.Shiny.enabled = flag;
		this.ImSelectRect.color = ((!flag) ? Color.clear : Color.white);
		this.ImSelectPrice.color = ((!flag) ? new Color(0.14117648f, 0.14117648f, 0.14509805f, 0.8f) : new Color(0.8156863f, 0.38431373f, 0.15686275f));
		if (this._endDiscountTime != null)
		{
			this.UpdatePriceIfDiscount(flag);
		}
	}

	private void UpdateDiscount()
	{
		string productPrice = this.ProductPrice;
		string productCurrency = this.ProductCurrency;
		bool flag = this.Product.HasActiveDiscount(TimeHelper.UtcTime());
		this._endDiscountTime = ((!flag) ? null : new double?((double)((float)this.Product.DiscountEnd.Value.Subtract(TimeHelper.UtcTime()).TotalSeconds)));
		this.ImageDiscount.gameObject.SetActive(flag && this.Product.TypeId == 4 && !this._openFromDetails);
		if (this.Product.TypeId == 4 && !this._openFromDetails)
		{
			if (flag)
			{
				this.UpdatePriceIfDiscountPondPass(this.Tgl.isOn || this._isMouseOver);
			}
			else
			{
				this.PriceValue.text = string.Format("<size=24>{0}</size> <b><color=#e3e3e3FF>{1}</color></b> {2}", ScriptLocalization.Get("FromCaption"), productPrice, productCurrency);
			}
			this.DPrice.SetActive(false);
			this.Price.SetActive(true);
		}
		else
		{
			if (flag)
			{
				bool flag2 = this.Product.DiscountPrice != null;
				if (flag2)
				{
					this.DPriceDiscountValue.text = string.Format("<b>{0}</b> {1}", productPrice, productCurrency);
					this.DDiscountValue.text = string.Format("-{0}%", (int)((this.Product.Price - this.Product.DiscountPrice.Value) * 100.0 / this.Product.Price));
				}
				else
				{
					this.ImageDiscountWithPrcLdbl.Load(this.Product.DiscountImageBID, this.ImageDiscountWithPrc, "Textures/Inventory/{0}");
				}
				this.DPriceDiscountValue.color = new Color(0.8f, 0.8f, 0.8f, (!flag2) ? 0f : 1f);
				this.DDiscountValue.transform.parent.gameObject.SetActive(flag2);
				this.ImageDiscountWithPrc.gameObject.SetActive(!flag2);
				this.UpdatePriceIfDiscount(this.Tgl.isOn || this._isMouseOver);
			}
			else
			{
				this.PriceValue.text = string.Format("<b><color=#e3e3e3ff>{0}</color></b> {1}", productPrice, productCurrency);
			}
			this.DPrice.SetActive(flag);
			this.Price.SetActive(!flag);
		}
	}

	private string ProductPrice
	{
		get
		{
			if (!string.IsNullOrEmpty(this.Product.PriceString))
			{
				return this.Product.PriceString;
			}
			return (!PremiumItemBase.HasValue(this.Product.Price)) ? "0" : this.Product.Price.ToString("N2");
		}
	}

	private string ProductCurrency
	{
		get
		{
			if (!(this.Product.ProductCurrency == "USD"))
			{
				return MeasuringSystemManager.GetCurrencyIcon(this.Product.ProductCurrency);
			}
			if (string.IsNullOrEmpty(this.Product.PriceString))
			{
				return string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.PaymentCurrency) ? "USD" : PhotonConnectionFactory.Instance.Profile.PaymentCurrency;
			}
			return string.Empty;
		}
	}

	private void UpdatePriceIfDiscount(bool isSelected)
	{
		if (this.Product.TypeId == 4 && !this._openFromDetails)
		{
			this.UpdatePriceIfDiscountPondPass(isSelected);
		}
		else
		{
			double? discountPrice = this.Product.DiscountPrice;
			double num = ((discountPrice == null) ? this.Product.Price : discountPrice.Value);
			string text = ((!isSelected && string.IsNullOrEmpty(this.Product.PriceString)) ? "#f03a3aff" : "#f3d462ff");
			this.DPriceValue.text = string.Format("<b><color={2}>{0}</color></b> {1}", num.ToString("N2"), this.ProductCurrency, text);
		}
	}

	private void UpdatePriceIfDiscountPondPass(bool isSelected)
	{
		double? discountPrice = this.Product.DiscountPrice;
		double num = ((discountPrice == null) ? 0.0 : discountPrice.Value);
		string text = ((!isSelected && string.IsNullOrEmpty(this.Product.PriceString)) ? "#f03a3aff" : "#f3d462ff");
		this.PriceValue.text = string.Format("<size=24>{0}</size> <b><color={3}>{1}</color></b> {2}", new object[]
		{
			ScriptLocalization.Get("FromCaption"),
			num.ToString("N2"),
			this.ProductCurrency,
			text
		});
	}

	private string GetLifetimeRemainFormated(TimeSpan ts)
	{
		string text = ((ts.TotalSeconds < 0.0) ? "00:00" : string.Format("{0}:{1}", ts.Hours.ToString("D2"), ts.Minutes.ToString("D2")));
		if (ts.Days <= 0)
		{
			return string.Format("<b>{0}:{1}</b>", text, (ts.TotalSeconds < 0.0) ? "00" : ts.Seconds.ToString("D2"));
		}
		int num = ts.Days;
		if (ts.Seconds > 0)
		{
			num++;
		}
		return string.Format("<b>{0}</b> {1} <b>{2}</b>", num, DateTimeExtensions.GetDaysLocalization(num), text);
	}

	[SerializeField]
	protected UIShiny Shiny;

	[SerializeField]
	protected GameObject DPrice;

	[SerializeField]
	protected TextMeshProUGUI DPriceValue;

	[SerializeField]
	protected TextMeshProUGUI DPriceDiscountValue;

	[SerializeField]
	protected TextMeshProUGUI DDiscountValue;

	[SerializeField]
	protected GameObject Price;

	[SerializeField]
	protected TextMeshProUGUI PriceValue;

	[SerializeField]
	protected GameObject Hot;

	[SerializeField]
	protected TextMeshProUGUI HotValue;

	[SerializeField]
	protected GameObject PromoMoney;

	[SerializeField]
	protected TextMeshProUGUI PromoMoneyValue;

	[SerializeField]
	protected GameObject PromoNoMoney;

	[SerializeField]
	protected TextMeshProUGUI PromoNoMoneyValue;

	[SerializeField]
	protected TextMeshProUGUI Name;

	[SerializeField]
	protected Image Img;

	[SerializeField]
	protected Toggle Tgl;

	[SerializeField]
	protected BorderedButton Btn;

	[SerializeField]
	protected BorderedButton BtnDetails;

	[SerializeField]
	protected BorderedButton BtnBack;

	[SerializeField]
	protected CanvasGroup Cg;

	[SerializeField]
	protected RectTransform Rt;

	[SerializeField]
	protected Image ImSelectRect;

	[SerializeField]
	protected Image ImSelectPrice;

	[SerializeField]
	protected HotkeyPressRedirect Hkpr;

	[SerializeField]
	protected Image ImageDiscount;

	[SerializeField]
	protected Image ImageDiscountWithPrc;

	public static readonly char[] Numbers = "0123456789".ToCharArray();

	protected bool _isFastBuy;

	protected PremiumShopMainHandler.ProductCategories _category;

	protected ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	protected ResourcesHelpers.AsyncLoadableImage ImageDiscountWithPrcLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private double? _endDiscountTime;

	private bool _isMouseOver;

	private DateTime? _lifetimeRemain;

	private bool _openFromDetails;
}
