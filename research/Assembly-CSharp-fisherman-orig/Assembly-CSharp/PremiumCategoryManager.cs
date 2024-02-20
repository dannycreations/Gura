using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PremiumCategoryManager : MainPageItem, PremiumShopMainPageHandler.IPremiumCategory
{
	public bool IsInited
	{
		get
		{
			return this.Inited;
		}
	}

	private void Awake()
	{
		this.ImgHeaderLoadable.Image = this.ImageHeader;
		this.HkPressRed = base.GetComponent<HotkeyPressRedirect>();
		this.GlG = this.ItemsContainer.GetComponent<GridLayoutGroup>();
	}

	public void Init<T>(List<PremiumCategoryManager.PremiumItem> products) where T : PremiumItemBase
	{
		this.Inited = true;
		this.Clear();
		List<PremiumShopMainPageHandler.ProductCategories> list = Enum.GetValues(typeof(PremiumShopMainPageHandler.ProductCategories)).Cast<PremiumShopMainPageHandler.ProductCategories>().ToList<PremiumShopMainPageHandler.ProductCategories>();
		Dictionary<PremiumShopMainPageHandler.ProductCategories, int> dictionary = new Dictionary<PremiumShopMainPageHandler.ProductCategories, int>();
		Dictionary<PremiumShopMainPageHandler.ProductCategories, int> dictionary2 = new Dictionary<PremiumShopMainPageHandler.ProductCategories, int>();
		for (int i = 0; i < list.Count; i++)
		{
			PremiumShopMainPageHandler.ProductCategories cat = list[i];
			dictionary2[cat] = products.Count((PremiumCategoryManager.PremiumItem p) => p.Category == cat && p.UiCategory == PremiumShopCategories.FishingPacks);
			dictionary[cat] = products.Count((PremiumCategoryManager.PremiumItem p) => p.Category == cat && p.UiCategory == PremiumShopCategories.Sales);
		}
		for (int j = 0; j < products.Count; j++)
		{
			PremiumCategoryManager.PremiumItem premiumItem = products[j];
			GameObject go;
			T o;
			if (j < this.GameObjects.Count)
			{
				go = this.GameObjects[j];
				o = go.GetComponent<T>();
			}
			else
			{
				GameObject gameObject = this.ItemPrefab;
				if ((premiumItem.UiCategory == PremiumShopCategories.Sales && dictionary[premiumItem.Category] < 3) || (premiumItem.UiCategory == PremiumShopCategories.FishingPacks && dictionary2[premiumItem.Category] < 3))
				{
					gameObject = this.ItemWnPrefab;
				}
				go = GUITools.AddChild(this.ItemsContainer, gameObject);
				this.GameObjects.Add(go);
				o = go.GetComponent<T>();
				int i1 = j;
				o.OnDestroyEv += delegate
				{
					go.SetActive(false);
					this.SetFirstActive<T>(false);
				};
				o.OnSelectedEv += delegate
				{
					if (this.ImageHeader != null)
					{
						this.ImgHeaderLoadable.Load(o.GetImageHeaderPath());
					}
					if (this.TextHeader != null)
					{
						this.TextHeader.text = o.GetDescription();
					}
					for (int k = 0; k < this.GameObjects.Count; k++)
					{
						if (k != i1)
						{
							T component = this.GameObjects[k].GetComponent<T>();
							component.Deselect();
						}
					}
				};
			}
			o.Init(premiumItem);
			go.SetActive(true);
		}
		this.SetFirstActive<T>(true);
		if (this.HkPressRed != null)
		{
			this.HkPressRed.StartListenForHotkeys();
		}
	}

	public virtual void BuyAction()
	{
		GameObject gameObject = this.GameObjects.FirstOrDefault((GameObject p) => p.activeSelf && p.GetComponent<PremiumItemBase>().IsSelected);
		if (gameObject != null)
		{
			gameObject.GetComponent<PremiumItemBase>().BuyAction();
		}
	}

	protected virtual void Clear()
	{
		this.GameObjects.ForEach(new Action<GameObject>(Object.Destroy));
		this.GameObjects.Clear();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.GameObjects.ForEach(new Action<GameObject>(Object.Destroy));
		this.LeftMenuGameObjects.ForEach(new Action<GameObject>(Object.Destroy));
	}

	protected virtual void SetFirstActive<T>(bool isPreheatSelectables) where T : PremiumItemBase
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (isPreheatSelectables)
			{
				this.ContentNavigation.PreheatSelectables();
			}
			this.ContentNavigation.SetFirstActive();
		}
		else if (this.GameObjects.Count > 0)
		{
			T component = this.GameObjects.First((GameObject p) => p.activeSelf).GetComponent<T>();
			component.Select(false);
		}
	}

	public void InitCategories(List<ProductCategory> listCategories)
	{
		this.LeftMenuGameObjects.ForEach(new Action<GameObject>(Object.Destroy));
		this.LeftMenuGameObjects.Clear();
		if (this.LeftMenuItemsContainer != null)
		{
			for (int i = 0; i < listCategories.Count; i++)
			{
				int catId = listCategories[i].CategoryId;
				string text = listCategories[i].Name.ToUpper();
				GameObject gameObject = GUITools.AddChild(this.LeftMenuItemsContainer, this.LeftMenuItemPrefab);
				for (int j = 0; j < gameObject.transform.childCount; j++)
				{
					Transform child = gameObject.transform.GetChild(j);
					Text component = child.GetComponent<Text>();
					if (component != null)
					{
						component.text = text;
					}
				}
				Toggle tgl = gameObject.GetComponent<Toggle>();
				tgl.group = this.LeftMenuToggleGroup;
				ToggleStateChanges tglStateChg = gameObject.GetComponent<ToggleStateChanges>();
				tglStateChg.OnSelected += delegate(Toggle t, bool v)
				{
					this.UpdateCategories(v, catId, t, listCategories);
					if (!v)
					{
						tglStateChg.OnNormal();
					}
				};
				tgl.onValueChanged.AddListener(delegate(bool v)
				{
					this.UpdateCategories(v, catId, tgl, listCategories);
				});
				this.LeftMenuGameObjects.Add(gameObject);
			}
			if (this.LeftMenuGameObjects.Count > 0)
			{
				PlayButtonEffect.SetToogleOn(true, this.LeftMenuGameObjects[0].GetComponent<Toggle>());
			}
		}
	}

	private void UpdateCategories(bool v, int catId, Toggle tgl, List<ProductCategory> listCategories)
	{
		if (v && listCategories.Count > 1)
		{
			for (int i = 0; i < this.GameObjects.Count; i++)
			{
				PremiumItemBase component = this.GameObjects[i].GetComponent<PremiumItemBase>();
				component.gameObject.SetActive(component.Item.Category == (PremiumShopMainPageHandler.ProductCategories)catId);
			}
			if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
			{
				this.ContentNavigation.PreheatSelectables();
				this.ContentNavigation.SetFirstActiveForce();
				PremiumItemBase premiumItemBase = ((!(this.ContentNavigation.CurrentSelected != null)) ? null : this.ContentNavigation.CurrentSelected.GetComponent<PremiumItemBase>());
				if (premiumItemBase != null)
				{
					premiumItemBase.Select(false);
				}
			}
			else if (this.GameObjects.Count > 0)
			{
				this.GameObjects.First((GameObject p) => p.activeSelf).GetComponent<PremiumItemBase>().Select(false);
			}
		}
		if (v && this.GlG != null)
		{
			this.UpdateGridLayoutGroup();
		}
		tgl.transform.Find("NameNormal").gameObject.SetActive(!v);
		tgl.transform.Find("NameActive").gameObject.SetActive(v);
	}

	private void UpdateGridLayoutGroup()
	{
		int num = this.GameObjects.Count((GameObject p) => p.activeSelf);
		this.GlG.spacing = ((num != 2) ? this.SpacingBig : this.SpacingSmall);
		if (num == 1 || num == 2)
		{
			this.GlG.constraintCount = num;
			this.GlG.cellSize = this.CellSizeSmall;
			this.GlG.padding.top = this.PaddingTopSmall;
		}
		else
		{
			this.GlG.constraintCount = 4;
			this.GlG.cellSize = this.CellSizeBig;
			this.GlG.padding.top = this.PaddingTopBig;
		}
	}

	[SerializeField]
	protected GameObject LeftMenuItemPrefab;

	[SerializeField]
	protected GameObject LeftMenuItemsContainer;

	[SerializeField]
	protected ToggleGroup LeftMenuToggleGroup;

	protected List<GameObject> LeftMenuGameObjects = new List<GameObject>();

	[Space(10f)]
	[SerializeField]
	protected Image ImageHeader;

	protected ResourcesHelpers.AsyncLoadableImage ImgHeaderLoadable = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	protected TextMeshProUGUI TextHeader;

	[SerializeField]
	protected GameObject ItemPrefab;

	[SerializeField]
	protected GameObject ItemWnPrefab;

	[SerializeField]
	protected GameObject ItemsContainer;

	[SerializeField]
	protected UINavigation ContentNavigation;

	[Space(10f)]
	[SerializeField]
	protected Vector2 SpacingSmall = new Vector2(83f, 24f);

	[SerializeField]
	protected Vector2 SpacingBig = new Vector2(26.92f, 24f);

	[SerializeField]
	protected Vector2 CellSizeSmall = new Vector2(420f, 570f);

	[SerializeField]
	protected Vector2 CellSizeBig = new Vector2(232f, 410f);

	[SerializeField]
	protected int PaddingTopSmall = 102;

	[SerializeField]
	protected int PaddingTopBig = 35;

	protected bool Inited;

	protected HotkeyPressRedirect HkPressRed;

	protected List<GameObject> GameObjects = new List<GameObject>();

	protected GridLayoutGroup GlG;

	public class PremiumItem : ICloneable
	{
		public StoreProduct Product { get; set; }

		public OfferClient Offer { get; set; }

		public WhatsNewItem WhatsNew { get; set; }

		public PremiumShopCategories UiCategory { get; set; }

		public PremiumShopMainPageHandler.ProductCategories Category { get; set; }

		public object Clone()
		{
			return new PremiumCategoryManager.PremiumItem
			{
				Product = this.Product,
				Offer = this.Offer,
				Category = this.Category,
				UiCategory = this.UiCategory,
				WhatsNew = this.WhatsNew
			};
		}
	}
}
