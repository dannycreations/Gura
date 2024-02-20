using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using Boats;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ShopDetailedInfo : MonoBehaviour
{
	private void OnEnable()
	{
		CacheLibrary.MapCache.OnGetBoatDescs += this.GetBoatDesc;
		this.Clear();
	}

	private void OnDisable()
	{
		if (this.subscribedForFishes)
		{
			this.subscribedForFishes = false;
			CacheLibrary.MapCache.OnFishes -= this.FillFishRestrictions;
		}
		CacheLibrary.MapCache.OnGetBoatDescs -= this.GetBoatDesc;
	}

	public void Show(InventoryItem item)
	{
		this.Clear();
		this._currentInventoryItem = item;
		if (Viewer3DArguments.HasArgs(item.ItemSubType, item.ItemId) && this.ButtonPreview != null)
		{
			this.ButtonPreview.SetActive(true);
		}
		this.ItemImgLdbl.Image = this.ItemImage;
		this.ItemImgLdbl.Load((item.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", item.ThumbnailBID));
		this.BrandImageLdbl.Image = this.BrandImage;
		if (item.Brand != null)
		{
			this.BrandImageLdbl.Load(string.Format("Textures/Inventory/{0}", item.Brand.LogoBID.ToString()));
		}
		this.Name.text = string.Format("<b>{0}</b> {1}", (item.Brand == null) ? string.Empty : item.BrandName, item.Name);
		this.Params.text = InventoryParamsHelper.Split((!(item is Reel)) ? item.Params : InventoryParamsHelper.FixReelParams(item.Params));
		this.Desc.text = InventoryParamsHelper.ParseDesc(this._currentInventoryItem);
		this.DescScroll.value = 1f;
		if (item.Technologies != null)
		{
			for (int i = 0; i < item.Technologies.Length; i++)
			{
				Transform transform = this.TechologiesPanel.transform.Find("TechnologyImage" + i);
				if (transform != null)
				{
					if (this.images.Count <= i)
					{
						this.images.Add(new ResourcesHelpers.AsyncLoadableImage());
					}
					this.images[i].Image = transform.GetComponent<Image>();
					this.images[i].Load(string.Format("Textures/Inventory/{0}", item.Technologies[i].LogoBID));
					PopupInfo component = transform.GetComponent<PopupInfo>();
					if (component != null && !string.IsNullOrEmpty(item.Technologies[i].Desc))
					{
						component.enabled = true;
						component.PopupValue = item.Technologies[i].Desc;
					}
				}
			}
		}
		List<ListOfCompatibility.ConstraintType> list = null;
		if (ListOfCompatibility.ItemsConstraints.TryGetValue(item.ItemSubType, out list))
		{
			for (int j = 0; j < this.Constraints.Length; j++)
			{
				this.Constraints[j].gameObject.SetActive(true);
				if (list.Contains((ListOfCompatibility.ConstraintType)j))
				{
					this.Constraints[j].color = this.greenColor;
					this.Constraints[j].transform.GetComponentInChildren<Text>().color = this.textEnabledColor;
				}
				else
				{
					this.Constraints[j].color = this.grayColor;
					this.Constraints[j].transform.GetComponentInChildren<Text>().color = this.textDisabledColor;
				}
			}
		}
	}

	public void Show(StoreProduct item)
	{
		this.Clear();
		this.ItemImgLdbl.Image = this.ItemImage;
		this.ItemImgLdbl.Load((item.ImageBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", item.ImageBID));
		this.Name.text = item.Name;
		this.Desc.text = item.Desc;
		this.DescScroll.value = 1f;
		for (int i = 0; i < 4; i++)
		{
			Transform transform = this.TechologiesPanel.transform.Find("TechnologyImage" + i);
			if (transform != null)
			{
				transform.gameObject.SetActive(false);
			}
		}
	}

	public void Show(ShopLicense item, int term)
	{
		if (this.lastShowLicenseId != item.LicenseId)
		{
			this.curentLicense = item;
			this.Clear();
			this.lastShowLicenseId = item.LicenseId;
			this.ItemImgLdbl.Image = this.ItemImage;
			this.ItemImgLdbl.Load((item.LogoBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", item.LogoBID));
			this.Name.text = item.Name;
			this.PriceLabel.text = ScriptLocalization.Get("PriceCaption");
			this.DurationNote.text = ScriptLocalization.Get("DurationNote");
			this.LicenseRequirements.text = ScriptLocalization.Get("LevelRequirements") + '\n' + item.OriginalMinLevel;
			List<int> list = new List<int>(item.TakeFish.Length + item.FreeFish.Length);
			list.AddRange(item.FreeFish.Select((FishLicenseConstraint x) => x.FishId));
			list.AddRange(item.TakeFish.Select((FishLicenseConstraint x) => x.FishId));
			if (!this.subscribedForFishes)
			{
				this.subscribedForFishes = true;
				CacheLibrary.MapCache.OnFishes += this.FillFishRestrictions;
			}
			CacheLibrary.MapCache.GetFishes(list.ToArray());
		}
		if (this.ButtonPreview != null)
		{
			this.ButtonPreview.SetActive(false);
		}
		this._currentInventoryItem = null;
		this.LicenseDuration.text = ScriptLocalization.Get("LicenseDuration") + '\n';
		switch (term)
		{
		case 0:
			this.LicenseDuration.text = this.LicenseDuration.text + ScriptLocalization.Get("UnlimLicenseShopCaption").Replace(":", string.Empty) + '*';
			break;
		case 1:
			this.LicenseDuration.text = this.LicenseDuration.text + ScriptLocalization.Get("DayLicenseShopCaption").Replace(":", string.Empty) + '*';
			break;
		default:
			if (term == 30)
			{
				this.LicenseDuration.text = this.LicenseDuration.text + ScriptLocalization.Get("MonthLicenseShopCaption").Replace(":", string.Empty) + '*';
			}
			break;
		case 3:
			this.LicenseDuration.text = this.LicenseDuration.text + ScriptLocalization.Get("Days3LicenseShopCaption").Replace(":", string.Empty) + '*';
			break;
		case 7:
			this.LicenseDuration.text = this.LicenseDuration.text + ScriptLocalization.Get("WeekLicenseShopCaption").Replace(":", string.Empty) + '*';
			break;
		}
		this.Expires.text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ScriptLocalization.Get("WillExpireCaption")) + '\n' + ((term != 0) ? MeasuringSystemManager.DateTimeString(DateTime.Now.AddDays((double)term)) : ScriptLocalization.Get("Never"));
		this._priceGo.SetActive(true);
		ShopLicenseCost shopLicenseCost = item.Costs.FirstOrDefault((ShopLicenseCost x) => x.Term == term);
		Text priceIcoRetail = this._priceIcoRetail;
		string text = ((!(shopLicenseCost.Currency != "GC")) ? "\ue62c" : "\ue62b");
		this.PriceIcon.text = text;
		priceIcoRetail.text = text;
		Text priceRetail = this._priceRetail;
		text = shopLicenseCost.NotResidentCost.ToString();
		this.PriceValue.text = text;
		priceRetail.text = text;
		this._priceGoRetail.SetActive(!this._priceGo.activeSelf);
	}

	public void ShowPreview()
	{
		if (this._currentInventoryItem != null && Viewer3DArguments.HasArgs(this._currentInventoryItem.ItemSubType, this._currentInventoryItem.ItemId))
		{
			Viewer3DArguments.ViewArgs args = Viewer3DArguments.GetArgs(this._currentInventoryItem.ItemSubType, new int?(this._currentInventoryItem.ItemId));
			string text = string.Format("<b>{0}</b>", (this._currentInventoryItem.Brand == null) ? string.Empty : this._currentInventoryItem.BrandName) + ((this._currentInventoryItem.Brand == null) ? string.Empty : " ") + this._currentInventoryItem.Name;
			ModelInfo modelInfo = new ModelInfo
			{
				Title = text,
				Info = this.Params.text,
				ItemSubType = this._currentInventoryItem.ItemSubType,
				Technologies = this._currentInventoryItem.Technologies,
				Brand = this._currentInventoryItem.Brand
			};
			string text2 = this._currentInventoryItem.Asset;
			if (this._currentInventoryItem is Boat)
			{
				BoatsFactory component = Resources.Load<GameObject>("Boats/_boatsFactory").GetComponent<BoatsFactory>();
				BoatFactoryData data = component.GetData(component.GetID(this._currentInventoryItem.Asset, (this._currentInventoryItem as Boat).Material));
				text2 = data.Prefab3p;
				modelInfo.Materials = data.Materials;
			}
			base.GetComponent<LoadPreviewScene>().LoadScene(text2, modelInfo, args);
		}
	}

	private void GetBoatDesc(object sender, GlobalMapBoatDescCacheEventArgs e)
	{
		if (CacheLibrary.MapCache.CachedPonds == null || this.curentLicense == null)
		{
			return;
		}
		Pond licensePond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.State != null && x.State.StateId == this.curentLicense.StateId);
		bool flag = licensePond != null && e.Items.Any((BoatDesc x) => x.Prices != null && x.Prices.Any((BoatPriceDesc p) => p.PondId == licensePond.PondId));
		if (flag && this.lastShowLicenseId != -1)
		{
			IEnumerable<ShopLicense> enumerable = CacheLibrary.MapCache.AllLicenses.Where((ShopLicense x) => x.StateId == this.curentLicense.StateId);
			string text = string.Empty;
			if (enumerable.First<ShopLicense>().LicenseId == this.curentLicense.LicenseId)
			{
				text = string.Format(ScriptLocalization.Get("RowBoatBasicLicenseText"), "\n");
			}
			else
			{
				text = string.Format(ScriptLocalization.Get("RowBoatAdvancedLicenseText"), "\n");
			}
			this.Fishes.text = this.Fishes.text + "\n\n" + text;
		}
	}

	private static string SetPrice(ShopLicense item, ShopLicenseCost s)
	{
		if (PhotonConnectionFactory.Instance.Profile.HomeState == item.StateId)
		{
			if (s.DiscountResidentCost > 0f)
			{
				return string.Format("<color=red>{0}</color> (<color=grey>{1}</color>)", s.DiscountResidentCost.ToString(), s.ResidentCost.ToString());
			}
			return s.ResidentCost.ToString();
		}
		else
		{
			if (s.DiscountNonResidentCost > 0f)
			{
				return string.Format("<color=red>{0}</color> (<color=grey>{1}</color>)", s.DiscountNonResidentCost.ToString(), s.NotResidentCost.ToString());
			}
			return s.NotResidentCost.ToString();
		}
	}

	private void FillFishRestrictions(object sender, GlobalMapFishCacheEventArgs e)
	{
		IEnumerable<ShopLicense> enumerable = CacheLibrary.MapCache.AllLicenses.Where((ShopLicense x) => x.StateId == this.curentLicense.StateId);
		string text = string.Empty;
		if (enumerable.First<ShopLicense>().LicenseId == this.curentLicense.LicenseId)
		{
			text = string.Format(ScriptLocalization.Get("NightCatchBasic"), "\n");
		}
		else
		{
			text = string.Format(ScriptLocalization.Get("NightCatchAdvanced"), "\n");
		}
		if (this.lastShowLicenseId != -1)
		{
			this.Fishes.text = string.Concat(new string[]
			{
				"<b>",
				ScriptLocalization.Get("Release").ToUpper(),
				"</b>\n",
				FishHelper.FillFishes(this.curentLicense.FreeFish, e.Items),
				"\n\n<b>",
				ScriptLocalization.Get("Take").ToUpper(),
				"</b>\n",
				FishHelper.FillFishes(this.curentLicense.TakeFish, e.Items),
				"\n\n",
				text
			});
		}
		CacheLibrary.MapCache.GetBoatDescs();
	}

	public void Clear()
	{
		if (this.ButtonPreview != null)
		{
			this.ButtonPreview.SetActive(false);
		}
		if (this._priceGo != null)
		{
			this._priceGo.SetActive(false);
		}
		if (this._priceGoRetail != null)
		{
			this._priceGoRetail.SetActive(false);
		}
		this.lastShowLicenseId = -1;
		this.ItemImage.overrideSprite = ResourcesHelpers.GetTransparentSprite();
		this.BrandImage.overrideSprite = ResourcesHelpers.GetTransparentSprite();
		this.Name.text = string.Empty;
		this.Params.text = string.Empty;
		this.Desc.text = string.Empty;
		this.LicenseRequirements.text = string.Empty;
		this.LicenseDuration.text = string.Empty;
		this.Expires.text = string.Empty;
		this.PriceLabel.text = string.Empty;
		this.PriceValue.text = string.Empty;
		this.PriceIcon.text = string.Empty;
		this.DurationNote.text = string.Empty;
		this.Fishes.text = string.Empty;
		for (int i = 0; i < this.Constraints.Length; i++)
		{
			this.Constraints[i].gameObject.SetActive(false);
		}
		for (int j = 0; j < this.TechologiesPanel.transform.childCount; j++)
		{
			this.TechologiesPanel.transform.GetChild(j).GetComponent<Image>().overrideSprite = ResourcesHelpers.GetTransparentSprite();
		}
	}

	[SerializeField]
	private Text _priceRetail;

	[SerializeField]
	private Text _priceIcoRetail;

	[SerializeField]
	private GameObject _priceGoRetail;

	[SerializeField]
	private GameObject _priceGo;

	public Image ItemImage;

	private ResourcesHelpers.AsyncLoadableImage ItemImgLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text Name;

	public Image BrandImage;

	private ResourcesHelpers.AsyncLoadableImage BrandImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public GameObject TechologiesPanel;

	public Text Params;

	public Text Desc;

	public Scrollbar DescScroll;

	public Image[] Constraints;

	public GameObject ButtonPreview;

	public Text LicenseRequirements;

	public Text LicenseDuration;

	public Text Expires;

	public Text PriceLabel;

	public Text PriceValue;

	public Text PriceIcon;

	public Text DurationNote;

	public Text Fishes;

	private Color grayColor = new Color(0.68235296f, 0.68235296f, 0.68235296f, 1f);

	private Color greenColor = new Color(0.41568628f, 0.6117647f, 0.42745098f, 1f);

	private Color textEnabledColor = new Color(0.96862745f, 0.96862745f, 0.96862745f, 1f);

	private Color textDisabledColor = new Color(0.8627451f, 0.8627451f, 0.8627451f, 1f);

	private int lastShowLicenseId = -1;

	private ShopLicense curentLicense;

	private InventoryItem _currentInventoryItem;

	private bool subscribedForFishes;

	private List<ResourcesHelpers.AsyncLoadableImage> images = new List<ResourcesHelpers.AsyncLoadableImage>();
}
