using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class LevelShow : MonoBehaviour
{
	private void Awake()
	{
		if (this._PremiumGo != null)
		{
			this._PremiumGo.SetActive(false);
		}
		if (this._PremiumTglColorMgr != null)
		{
			this._premTglColorMgrNormalTextColor = this._PremiumTglColorMgr.NormalTextColor;
		}
		if (this._PremiumTglColorMgr != null)
		{
			this._premTglColorMgrActiveTextColor = this._PremiumTglColorMgr.ActiveTextColor;
		}
	}

	private void Start()
	{
		PhotonConnectionFactory.Instance.OnProductDelivered += this.Instance_OnProductDelivered;
		PhotonConnectionFactory.Instance.OnSubscriptionEnded += this.Instance_OnSubscriptionEnded;
		PhotonConnectionFactory.Instance.OnLevelGained += this.Instance_OnLevelGained;
		PhotonConnectionFactory.Instance.OnExpGained += this.Instance_OnExpGained;
		PhotonConnectionFactory.Instance.OnNameChanged += this.Instance_OnNameChanged;
		ScreenManager.Instance.OnScreenChanged += this.Instance_OnScreenChanged;
		this.UpdatePremColors();
		this.ReInit();
		List<int> productsIdsInCategories = new List<int>();
		CacheLibrary.ProductCache.ProductCategories.ForEach(delegate(ProductCategory c)
		{
			productsIdsInCategories.AddRange(c.ProductIds);
		});
		CacheLibrary.ProductCache.Products.ForEach(delegate(StoreProduct p)
		{
			if (productsIdsInCategories.Contains(p.ProductId) && PremiumItemBase.HasValue(p.Price) && ((p.HasActiveDiscount(TimeHelper.UtcTime()) && p.DiscountPrice != null) || p.IsNew))
			{
				this._productsWithDiscount.Add(p);
			}
		});
		this.UpdateProductsDiscount();
	}

	private void Update()
	{
		this._productsDiscountCurrentTime += Time.deltaTime;
		if (this._productsDiscountCurrentTime > 3f)
		{
			this._productsDiscountCurrentTime = 0f;
			this.UpdateProductsDiscount();
		}
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.Instance_OnProductDelivered;
		PhotonConnectionFactory.Instance.OnSubscriptionEnded -= this.Instance_OnSubscriptionEnded;
		PhotonConnectionFactory.Instance.OnLevelGained -= this.Instance_OnLevelGained;
		PhotonConnectionFactory.Instance.OnExpGained -= this.Instance_OnExpGained;
		PhotonConnectionFactory.Instance.OnNameChanged -= this.Instance_OnNameChanged;
		ScreenManager.Instance.OnScreenChanged -= this.Instance_OnScreenChanged;
	}

	private void UpdatePremColors()
	{
		Color greyColor = this._greyColor;
		if (this.IconColorSelecting != null)
		{
			this.IconColorSelecting.UseColor[1] = greyColor;
		}
	}

	private void Instance_OnLevelGained(LevelInfo lvl)
	{
		Profile profile = this.Profile;
		if (profile == null)
		{
			return;
		}
		if (this._level != profile.Level)
		{
			this._level = profile.Level;
			this.LevelText.text = "\ue62d " + this._level;
		}
		if (this._rank != profile.Rank)
		{
			this._rank = profile.Rank;
			this._rankText.text = "\ue727 " + this._rank;
		}
		this._rankText.gameObject.SetActive(this._rank > 0);
		this._layoutRank.padding.left = ((this._rank <= 0) ? 14 : (-14));
		this.Instance_OnExpGained(0, 0);
	}

	private void Instance_OnExpGained(int arg1, int arg2)
	{
		Profile profile = this.Profile;
		if (profile != null)
		{
			this.LevelProgress.value = PlayerStatisticsInit.GetProgress(profile);
		}
	}

	private Profile Profile
	{
		get
		{
			return (PhotonConnectionFactory.Instance == null) ? null : PhotonConnectionFactory.Instance.Profile;
		}
	}

	private void Instance_OnNameChanged()
	{
		Profile profile = this.Profile;
		if (profile != null && this.playerNameLabel != null && (this.playerNameLabel.text == string.Empty || this.playerNameLabel.text != profile.Name))
		{
			this.playerNameLabel.text = profile.Name;
		}
	}

	private void Instance_OnScreenChanged(GameScreenType curScreen)
	{
		if ((!ScreenManager.Game3DScreens.Contains(curScreen) && ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreenPrev)) || this._screen == GameScreenType.Undefined)
		{
			this.ReInit();
		}
		this._screen = curScreen;
	}

	private void Instance_OnSubscriptionEnded()
	{
		Profile profile = this.Profile;
		if (profile != null && this._isPremium != profile.HasPremium)
		{
			this.UpdatePremColors();
			this._isPremium = profile.HasPremium;
			if (this.PremiumIcon != null)
			{
				this.PremiumIcon.color = ((!this._isPremium) ? this._premTglColorMgrNormalTextColor : this._premiumColor);
			}
			if (this._PremiumTglColorMgr != null)
			{
				this._PremiumTglColorMgr.NormalTextColor = ((!this._isPremium) ? this._premTglColorMgrNormalTextColor : this._premiumColor);
				this._PremiumTglColorMgr.ActiveTextColor = ((!this._isPremium) ? this._premTglColorMgrActiveTextColor : this._premiumColor);
			}
		}
	}

	private void Instance_OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		this.Instance_OnSubscriptionEnded();
	}

	private void ReInit()
	{
		this.Instance_OnExpGained(0, 0);
		this.Instance_OnNameChanged();
		this.Instance_OnLevelGained(null);
		this.Instance_OnSubscriptionEnded();
	}

	private void UpdateProductsDiscount()
	{
		GameObject gameObject = this._premShopMenuDiscountImg.gameObject;
		bool flag;
		if (DashboardTabSetter.IsNewPremShopEnabled)
		{
			flag = this._productsWithDiscount.Any((StoreProduct p) => p.HasActiveDiscount(TimeHelper.UtcTime()) || p.IsNew);
		}
		else
		{
			flag = false;
		}
		gameObject.SetActive(flag);
	}

	[SerializeField]
	private Text _rankText;

	[SerializeField]
	private HorizontalLayoutGroup _layoutRank;

	[SerializeField]
	private GameObject _PremiumGo;

	[SerializeField]
	private ToggleColorTransitionChanges _PremiumTglColorMgr;

	[SerializeField]
	private Image _premShopMenuDiscountImg;

	public Text LevelText;

	public Text playerNameLabel;

	public Slider LevelProgress;

	public Text PremiumIcon;

	public ChangeColorOther IconColorSelecting;

	public Button PremiumButton;

	public bool IsNewCanvas;

	private int _level;

	private int _rank;

	private bool _isPremium;

	private Color _greyColor = new Color(0.4745098f, 0.47843137f, 0.49411765f);

	private Color _premiumColor = new Color32(byte.MaxValue, 223, 0, byte.MaxValue);

	private GameScreenType _screen;

	private const int LayoutRankPaddingLeft = 14;

	private Color _premTglColorMgrNormalTextColor = Color.white;

	private Color _premTglColorMgrActiveTextColor = Color.white;

	private readonly List<StoreProduct> _productsWithDiscount = new List<StoreProduct>();

	private float _productsDiscountCurrentTime;

	private const float ProductsDiscountUpdateTime = 3f;
}
