using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PremShopItemInfo : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<StoreProduct, PremiumShopMainHandler.ProductCategories> OnBuy = delegate(StoreProduct product, PremiumShopMainHandler.ProductCategories cat)
	{
	};

	protected override void Awake()
	{
		base.Awake();
		this._handlerSRIA.OnInited += delegate
		{
			this._handlerSRIA.UpdateData(this._modelSRIA);
		};
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.StopAllCoroutines();
	}

	public void Init(StoreProduct product, PremiumShopMainHandler.ProductCategories cat, bool isFastBuy, string priceStr, List<StoreProduct> pondPasses)
	{
		PremShopItemInfo.<Init>c__AnonStorey2 <Init>c__AnonStorey = new PremShopItemInfo.<Init>c__AnonStorey2();
		<Init>c__AnonStorey.$this = this;
		this._isFastBuy = isFastBuy;
		this._cat = cat;
		this._product = product;
		this._imageLdbl.Load(this._product.PremShopImageBID, this._img, "Textures/Inventory/{0}");
		this._description.text = this._product.Desc;
		this._title.text = Regex.Replace(this._product.Name, "<.*?>", string.Empty);
		bool flag = cat == PremiumShopMainHandler.ProductCategories.PondPasses || product.TypeId == 4;
		if (flag)
		{
			try
			{
				this._title.text = this._title.text.Substring(0, this._title.text.IndexOf('(')).TrimEnd(new char[0]);
			}
			catch (Exception ex)
			{
				LogHelper.Error("PremShopItemInfo:Init - error parsing product name ProductId:{0} cat:{1}", new object[] { product.ProductId, cat, ex.Message });
			}
			this._pondPassesContent.SetActive(true);
			for (int m = 0; m < pondPasses.Count; m++)
			{
				StoreProduct p2 = pondPasses[m];
				PremShopItemHandler item = GUITools.AddChild(this._pondPassesContent, this._pondPassesPrefab).GetComponent<PremShopItemHandler>();
				item.Init(p2, this._pondPassesToggleGroup, null, cat, true, true);
				item.OnActive += delegate(bool b)
				{
					<Init>c__AnonStorey.$this._product = p2;
					<Init>c__AnonStorey.$this.UpdateOkBtnText(item.PriceStr);
				};
				this._productsPondPasses.Add(item);
			}
			this.UpdateOkBtnText(this._productsPondPasses[0].PriceStr);
			this.Alpha.ShowFinished += delegate(object sender, EventArgs args)
			{
				if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
				{
					<Init>c__AnonStorey.$this.StartCoroutine(<Init>c__AnonStorey.$this.SelectFirstPondPass());
				}
			};
		}
		else if (isFastBuy)
		{
			this._okBtnValue.text = ScriptLocalization.Get("OkButton").ToUpper();
		}
		else
		{
			this.UpdateOkBtnText(priceStr);
		}
		PremShopItemInfo.<Init>c__AnonStorey2 <Init>c__AnonStorey3 = <Init>c__AnonStorey;
		int? buoyExt = this._product.BuoyExt;
		<Init>c__AnonStorey3.buoyExt = ((buoyExt == null) ? 0 : buoyExt.Value);
		int? rodSetupExt = this._product.RodSetupExt;
		int num = ((rodSetupExt == null) ? 0 : rodSetupExt.Value);
		PremShopItemInfo.<Init>c__AnonStorey2 <Init>c__AnonStorey4 = <Init>c__AnonStorey;
		int? inventoryExt = this._product.InventoryExt;
		<Init>c__AnonStorey4.inventoryExt = ((inventoryExt == null) ? 0 : inventoryExt.Value);
		int? chumRecipesExt = this._product.ChumRecipesExt;
		int num2 = ((chumRecipesExt == null) ? 0 : chumRecipesExt.Value);
		int[] array = this._product.PondsUnlocked ?? new int[0];
		int? silver = this._product.Silver;
		int num3 = ((silver == null) ? 0 : silver.Value);
		int? gold = this._product.Gold;
		int num4 = ((gold == null) ? 0 : gold.Value);
		ProductInventoryItemBrief[] array2 = this._product.Items ?? new ProductInventoryItemBrief[0];
		LicenseRef[] array3 = this._product.Licenses ?? new LicenseRef[0];
		double? expMultiplier = this._product.ExpMultiplier;
		double num5 = ((expMultiplier == null) ? 0.0 : expMultiplier.Value);
		double? moneyMultiplier = this._product.MoneyMultiplier;
		double num6 = ((moneyMultiplier == null) ? 0.0 : moneyMultiplier.Value);
		PremShopItemInfo.<Init>c__AnonStorey2 <Init>c__AnonStorey5 = <Init>c__AnonStorey;
		int? term = this._product.Term;
		<Init>c__AnonStorey5.term = ((term == null) ? 0 : term.Value);
		LogHelper.Log("___kocha PSH >>>>>> Details \nProductId:{0} \nbuoyExt:{1} \nrodSetupExt:{2} \ninventoryExt:{3} \npondsUnlocked:{4} \nsilver:{5} \ngold:{6} \nitems:{7} \nlicensees:{8} \nexpMultiplier:{9} \nmoneyMultiplier:{10} \nterm:{11} \nchumRecipesExt:{12}", new object[]
		{
			this._product.ProductId,
			<Init>c__AnonStorey.buoyExt,
			num,
			<Init>c__AnonStorey.inventoryExt,
			array.Length,
			num3,
			num4,
			array2.Length,
			array3.Length,
			num5,
			num6,
			<Init>c__AnonStorey.term,
			num2
		});
		string text = string.Format("{0} {1}", <Init>c__AnonStorey.term, DateTimeExtensions.GetDaysLocalization(<Init>c__AnonStorey.term));
		List<StoreProduct> products = CacheLibrary.ProductCache.Products;
		if (<Init>c__AnonStorey.term > 0 && num6 > 0.0 && num5 > 0.0)
		{
			this.AddHeader("PremiumAccountCaption", true, 24);
			this.AddEmptyHeader();
			List<StoreProduct> list = products.Where((StoreProduct p) => p.TypeId == 3 && p.Term != null).ToList<StoreProduct>();
			StoreProduct storeProduct = list.Aggregate((StoreProduct x, StoreProduct y) => (Math.Abs(x.Term.Value - <Init>c__AnonStorey.term) >= Math.Abs(y.Term.Value - <Init>c__AnonStorey.term)) ? y : x);
			string text2 = Regex.Replace(storeProduct.Name, "<.*?>", string.Empty);
			text2 = string.Format("{0} ({1})", this.RemoveNumbers(text2), text);
			this._modelSRIA.Add(new PremiumShopItemModelIcoSmall
			{
				Name = text2,
				Ico = UgcConsts.GetYellowTan("\ue645")
			});
		}
		bool flag2 = num4 > 0;
		bool flag3 = num3 > 0;
		if (flag2 || flag3)
		{
			this.AddHeader(string.Format("{0}, {1}", ScriptLocalization.Get("MoneySortCaption").ToUpper(), ScriptLocalization.Get("GoldsButtonPopup").ToUpper()), false, 24);
			string text3 = string.Empty;
			string text4 = string.Empty;
			if (flag2)
			{
				text4 = string.Format("{0}: {1}", ScriptLocalization.Get("GoldsButtonPopup"), num4);
				text3 = MeasuringSystemManager.GetCurrencyIcon("GC");
			}
			if (flag3)
			{
				string text5 = string.Format("{0}: {1}", ScriptLocalization.Get("MoneySortCaption"), num3);
				string currencyIcon = MeasuringSystemManager.GetCurrencyIcon("SC");
				if (flag2)
				{
					text3 = string.Format("{0} {1}", text3, currencyIcon);
					text4 = string.Format("{0}\n{1}", text4, text5);
					text3 = string.Format("<size=40>{0}", text3);
				}
				else
				{
					text4 = text5;
					text3 = currencyIcon;
				}
			}
			this.AddEmptyHeader();
			this._modelSRIA.Add(new PremiumShopItemModelIcoSmall
			{
				Name = text4,
				Ico = UgcConsts.GetYellowTan(text3)
			});
		}
		if (array2.Length > 0)
		{
			HashSet<int> itemsNotInParentItemStorage = new HashSet<int>();
			string text6 = StoragePlaces.ParentItem.ToString();
			Dictionary<ItemTypes, List<PremiumShopBaseModel>> dictionary = new Dictionary<ItemTypes, List<PremiumShopBaseModel>>();
			Dictionary<ItemTypes, List<PremiumShopBaseModel>> dictionary2 = new Dictionary<ItemTypes, List<PremiumShopBaseModel>>();
			foreach (ProductInventoryItemBrief productInventoryItemBrief in array2)
			{
				InventoryItemBrief inventoryItemBriefById = CacheLibrary.MapCache.GetInventoryItemBriefById(productInventoryItemBrief.ItemId);
				if (inventoryItemBriefById != null)
				{
					string text7 = string.Empty;
					if (productInventoryItemBrief.Count > 1 && (productInventoryItemBrief.Length == null || inventoryItemBriefById.ItemType == ItemTypes.Leader))
					{
						text7 = string.Format("{0}", productInventoryItemBrief.Count);
					}
					string text8 = (string.IsNullOrEmpty(text7) ? inventoryItemBriefById.Name : string.Format("{0} (x{1})", inventoryItemBriefById.Name, text7));
					Vector4 zero = Vector4.zero;
					if (inventoryItemBriefById.ItemType == ItemTypes.Rod)
					{
						text8 = string.Format("{0}\n{1}", ItemSubTypesLocalization.Localize(inventoryItemBriefById.ItemSubType, true), text8);
						zero..ctor(0f, -15f, 0f, 0f);
					}
					if (productInventoryItemBrief.Storage != text6)
					{
						itemsNotInParentItemStorage.Add(productInventoryItemBrief.ItemId);
						this.AddInventoryItem(dictionary, inventoryItemBriefById, text8, zero);
					}
					else
					{
						this.AddInventoryItem(dictionary2, inventoryItemBriefById, text8, zero);
					}
				}
			}
			for (int k = 0; k < this.SortetItems.Count; k++)
			{
				ItemTypes iType = this.SortetItems[k];
				if (dictionary.ContainsKey(iType) || dictionary2.ContainsKey(iType))
				{
					InventoryCategory inventoryCategory = StaticUserData.AllCategories.FirstOrDefault((InventoryCategory p) => p.CategoryId == (int)iType);
					this.AddHeader(inventoryCategory.Name.ToUpper(), false, 24);
					this.AddEmptyHeader();
					if (dictionary.ContainsKey(iType))
					{
						this._modelSRIA.AddRange(dictionary[iType]);
						if (dictionary2.ContainsKey(iType))
						{
							this._modelSRIA.AddRange(dictionary2[iType].Where((PremiumShopBaseModel p) => !itemsNotInParentItemStorage.Contains(p.ItemId)));
						}
					}
					else if (dictionary2.ContainsKey(iType))
					{
						this._modelSRIA.AddRange(dictionary2[iType]);
					}
				}
			}
		}
		if (<Init>c__AnonStorey.buoyExt > 0)
		{
			this.AddHeader(ProductTypeFilter.GetLocForProductType(7), true, 24);
			this.AddEmptyHeader();
			List<StoreProduct> list2 = products.Where((StoreProduct p) => p.TypeId == 7).ToList<StoreProduct>();
			StoreProduct storeProduct = list2.Aggregate(delegate(StoreProduct x, StoreProduct y)
			{
				string value = Regex.Match(x.Name, "\\d+").Value;
				string value2 = Regex.Match(y.Name, "\\d+").Value;
				int num7 = int.Parse(value);
				int num8 = int.Parse(value2);
				return (Math.Abs(num7 - <Init>c__AnonStorey.buoyExt) >= Math.Abs(num8 - <Init>c__AnonStorey.buoyExt)) ? y : x;
			});
			string text9 = string.Format("{0} ({1})", this.RemoveNumbers(storeProduct.Name), <Init>c__AnonStorey.buoyExt);
			this._modelSRIA.Add(new PremiumShopItemModelIcoSmall
			{
				Name = text9,
				Ico = "\ue745"
			});
		}
		if (<Init>c__AnonStorey.inventoryExt > 0)
		{
			this.AddHeader(ProductTypeFilter.GetLocForProductType(5), true, 24);
			this.AddEmptyHeader();
			List<StoreProduct> list3 = products.Where((StoreProduct p) => p.TypeId == 5 && p.InventoryExt != null).ToList<StoreProduct>();
			StoreProduct storeProduct = list3.Aggregate((StoreProduct x, StoreProduct y) => (Math.Abs(x.InventoryExt.Value - <Init>c__AnonStorey.inventoryExt) >= Math.Abs(y.InventoryExt.Value - <Init>c__AnonStorey.inventoryExt)) ? y : x);
			string text10 = string.Format("{0} ({1})", this.RemoveNumbers(storeProduct.Name), <Init>c__AnonStorey.inventoryExt);
			this._modelSRIA.Add(new PremiumShopItemModelIcoSmall
			{
				Name = text10,
				Ico = "\ue746"
			});
		}
		if (num > 0)
		{
			this.AddHeader(ProductTypeFilter.GetLocForProductType(6), true, 24);
			this.AddEmptyHeader();
			StoreProduct storeProduct = products.FirstOrDefault((StoreProduct p) => p.TypeId == 6);
			string text11 = string.Format("{0} ({1})", storeProduct.Name, num);
			this._modelSRIA.Add(new PremiumShopItemModelIcoSmall
			{
				Name = text11,
				Ico = "\ue749"
			});
		}
		if (num2 > 0)
		{
			this.AddHeader("PremShop_Recipe", true, 24);
			StoreProduct storeProduct = products.FirstOrDefault((StoreProduct p) => p.TypeId == 8);
			string text12 = string.Format("{0} ({1})", storeProduct.Name, num2);
			this._modelSRIA.Add(new PremiumShopItemModelIcoSmall
			{
				Name = text12,
				Ico = "\ue723"
			});
			this.AddEmptyHeader();
		}
		if (array.Length > 0)
		{
			PremShopItemInfo.<Init>c__AnonStorey5 <Init>c__AnonStorey8 = new PremShopItemInfo.<Init>c__AnonStorey5();
			this.AddHeader("PremShop_Pondpass", true, 24);
			this.AddEmptyHeader();
			<Init>c__AnonStorey8.ponds = array.ToList<int>();
			int i;
			for (i = 0; i < <Init>c__AnonStorey8.ponds.Count; i++)
			{
				Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == <Init>c__AnonStorey8.ponds[i]);
				this._modelSRIA.Add(new PremiumShopItemModelIcoSmall
				{
					Name = ((!flag) ? string.Format("{0} - {1} ({2})", pond.Name, pond.State.Name, text) : string.Format("{0} - {1}", pond.Name, pond.State.Name)),
					Ico = "\ue725"
				});
			}
		}
		if (array3.Length > 0)
		{
			this.AddHeader("LicensesMenu", true, 24);
			this.AddEmptyHeader();
			string[] array4 = new string[array3.Length];
			for (int l = 0; l < array3.Length; l++)
			{
				LicenseRef lr = array3[l];
				ShopLicense shopLicense = CacheLibrary.MapCache.AllLicenses.FirstOrDefault((ShopLicense p) => p.LicenseId == lr.LicenseId);
				if (flag)
				{
					array4[l] = shopLicense.Name;
				}
				else
				{
					array4[l] = string.Format("{0} ({1})", shopLicense.Name, (lr.Term <= 0) ? text : string.Format("{0} {1}", lr.Term, DateTimeExtensions.GetDaysLocalization(lr.Term)));
				}
				this._modelSRIA.Add(new PremiumShopItemModelIcoSmall
				{
					Name = array4[l],
					Ico = "\ue63a"
				});
			}
		}
	}

	protected override void AcceptActionCalled()
	{
		if (!this._isFastBuy)
		{
			this.OnBuy(this._product, this._cat);
		}
	}

	private void AddHeader(string header, bool getLoc = true, int height = 24)
	{
		if (this._modelSRIA.Count > 0)
		{
			this._modelSRIA.Add(new HeaderPremiumShopGroupModel
			{
				Height = height,
				Name = string.Empty
			});
		}
		this._modelSRIA.Add(new HeaderPremiumShopGroupModel
		{
			Height = height,
			Name = ((!getLoc) ? header : ScriptLocalization.Get(header).ToUpper())
		});
	}

	private void AddEmptyHeader()
	{
		this._modelSRIA.Add(new HeaderPremiumShopGroupModel
		{
			Height = 3,
			Name = string.Empty
		});
	}

	private string RemoveNumbers(string s)
	{
		char[] array = (from n in s.ToCharArray()
			where !char.IsDigit(n)
			select n).ToArray<char>();
		string text = new string(array);
		text = text.TrimStart(new char[0]);
		return text.TrimEnd(new char[0]);
	}

	private void AddInventoryItem(Dictionary<ItemTypes, List<PremiumShopBaseModel>> iiSria, InventoryItemBrief iiBrief, string itemName, Vector4 margin)
	{
		if (!iiSria.ContainsKey(iiBrief.ItemType))
		{
			iiSria[iiBrief.ItemType] = new List<PremiumShopBaseModel>();
		}
		iiSria[iiBrief.ItemType].Add(new PremiumShopItemModel
		{
			ItemId = iiBrief.ItemId,
			Name = itemName,
			Description = iiBrief.Params,
			ImageBID = iiBrief.ThumbnailBID,
			Margin = margin
		});
	}

	private IEnumerator SelectFirstPondPass()
	{
		yield return null;
		this._productsPondPasses[0].Select();
		UINavigation.SetSelectedGameObject(this._productsPondPasses[0].gameObject);
		yield break;
	}

	private void UpdateOkBtnText(string priceStr)
	{
		this._okBtnValue.text = string.Format("{0} {1}", ScriptLocalization.Get("PremShop_BuyFor"), priceStr);
	}

	[SerializeField]
	private GameObject _pondPassesContent;

	[SerializeField]
	private GameObject _pondPassesPrefab;

	[SerializeField]
	private ToggleGroup _pondPassesToggleGroup;

	[Space(5f)]
	[SerializeField]
	private PremiumShopHandlerSRIA _handlerSRIA;

	[Space(15f)]
	[SerializeField]
	private TextMeshProUGUI _title;

	[SerializeField]
	private TextMeshProUGUI _description;

	[SerializeField]
	private Image _img;

	[SerializeField]
	private Text _okBtnValue;

	private ResourcesHelpers.AsyncLoadableImage _imageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private StoreProduct _product;

	private readonly List<PremShopItemHandler> _productsPondPasses = new List<PremShopItemHandler>();

	private PremiumShopMainHandler.ProductCategories _cat;

	private bool _isFastBuy;

	private List<PremiumShopBaseModel> _modelSRIA = new List<PremiumShopBaseModel>();

	private const int HeaderHeightSmall = 3;

	private const int HeaderHeightBig = 24;

	private readonly IList<ItemTypes> SortetItems = new ReadOnlyCollection<ItemTypes>(new List<ItemTypes>
	{
		ItemTypes.Rod,
		ItemTypes.Reel,
		ItemTypes.Outfit,
		ItemTypes.Misc,
		ItemTypes.Boat,
		ItemTypes.Line,
		ItemTypes.Leader,
		ItemTypes.TerminalTackle,
		ItemTypes.Bobber,
		ItemTypes.Sinker,
		ItemTypes.Feeder,
		ItemTypes.Bell,
		ItemTypes.Hook,
		ItemTypes.JigHead,
		ItemTypes.JigBait,
		ItemTypes.Lure,
		ItemTypes.Bait,
		ItemTypes.Chum,
		ItemTypes.ChumBase,
		ItemTypes.ChumAroma,
		ItemTypes.ChumParticle,
		ItemTypes.Tool
	});
}
