using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PremiumShopMenuItemHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public bool IsLast { get; private set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnActive = delegate(bool b)
	{
	};

	public Selectable Sel
	{
		get
		{
			return this._tgl;
		}
	}

	public ProductCategory Category { get; private set; }

	public List<StoreProduct> Products { get; private set; }

	public Dictionary<int, DateTime?> LifetimeRemain { get; private set; }

	public bool IsActive
	{
		get
		{
			return this._categoryName.fontStyle == 1;
		}
	}

	private void Update()
	{
		if (this.Products != null)
		{
			this._curUpdateTime += Time.deltaTime;
			if (this._curUpdateTime >= 10f)
			{
				this.UpdateDiscount();
				this._curUpdateTime = 0f;
			}
		}
	}

	public void Init(ProductCategory productCategory, List<StoreProduct> products, ToggleGroup tg)
	{
		this.Category = productCategory;
		this._isLeftDiscountImg = this.Category.CategoryId == 1 || this.Category.CategoryId == 4;
		this._tgl.group = tg;
		this._tgl.onValueChanged.AddListener(delegate(bool b)
		{
			this._categoryName.fontStyle = ((!b) ? 0 : 1);
			this._categoryName.color = ((!b) ? this.Gray : Color.white);
			this._imSelectedRect.color = ((!b) ? Color.clear : this.Gray);
			this._imSelectedBg.color = ((!b) ? Color.clear : new Color(1f, 1f, 1f, 0.11f));
			this.OnActive(b);
		});
		this._categoryName.text = productCategory.Name.ToUpper();
		this.SetProducts(products);
	}

	public void SetProducts(List<StoreProduct> products)
	{
		this._productsPondPassesDict = new Dictionary<int, List<StoreProduct>>();
		if (this.Category.CategoryId != 13)
		{
			if (!products.Any((StoreProduct p) => p.TypeId == 4))
			{
				this.Products = products;
				goto IL_16E;
			}
		}
		this.Products = new List<StoreProduct>();
		for (int i = 0; i < products.Count; i++)
		{
			StoreProduct p = products[i];
			if (p.TypeId == 4)
			{
				if (p.PondsUnlocked != null && p.PondsUnlocked.Length > 0)
				{
					int pondUnlocked = p.PondsUnlocked[0];
					if (!this._productsPondPassesDict.ContainsKey(pondUnlocked) || !this._productsPondPassesDict[pondUnlocked].Any((StoreProduct el) => el.ProductId == p.ProductId))
					{
						List<StoreProduct> list = products.Where((StoreProduct el) => el.TypeId == 4 && el.PondsUnlocked != null && el.PondsUnlocked.Length > 0 && el.PondsUnlocked[0] == pondUnlocked).ToList<StoreProduct>();
						this.Products.Add(list[0]);
						this._productsPondPassesDict[pondUnlocked] = list;
					}
				}
			}
			else
			{
				this.Products.Add(p);
			}
		}
		IL_16E:
		this.UpdateNew();
		this.UpdateDiscount();
	}

	public List<StoreProduct> GetProductsPondPasses(StoreProduct product)
	{
		if (product.PondsUnlocked != null && product.PondsUnlocked.Length > 0)
		{
			int num = product.PondsUnlocked[0];
			if (this._productsPondPassesDict.ContainsKey(num))
			{
				return this._productsPondPassesDict[num];
			}
		}
		return null;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this._imMouseOver.color = new Color(0f, 0f, 0f, 0.23f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this._imMouseOver.color = Color.clear;
	}

	public void Select()
	{
		UINavigation.SetSelectedGameObject(base.gameObject);
		PlayButtonEffect.SetToogleOn(true, this._tgl);
	}

	public void DeSelect()
	{
		PlayButtonEffect.SetToogleOn(false, this._tgl);
	}

	public void SetLifetimeRemain(Dictionary<int, DateTime?> lifetimeRemain)
	{
		this.LifetimeRemain = lifetimeRemain;
	}

	public void SetLast()
	{
		this.IsLast = true;
		Selectable tgl = this._tgl;
		Navigation navigation = default(Navigation);
		navigation.mode = 4;
		navigation.selectOnUp = this._tgl.navigation.selectOnUp;
		navigation.selectOnDown = null;
		navigation.selectOnLeft = this._tgl.navigation.selectOnLeft;
		navigation.selectOnRight = this._tgl.navigation.selectOnRight;
		tgl.navigation = navigation;
	}

	public void StopShowNew()
	{
		if (!this._isLeftDiscountImg)
		{
			this._discountImageRight.gameObject.SetActive(false);
			(from p in this.Products
				where p.IsNew
				select p.ProductId.ToString()).ToList<string>().ForEach(delegate(string productId)
			{
				ObscuredPrefs.SetBool(string.Format("PREM_SHOP_NEW_PRODUCT_{0}_{1}", this.Category.CategoryId, productId), true);
			});
		}
	}

	public void StopShowNew(int productId)
	{
		(from p in this.Products
			where p.IsNew && p.ProductId == productId
			select p.ProductId.ToString()).ToList<string>().ForEach(delegate(string pId)
		{
			ObscuredPrefs.SetBool(string.Format("PREM_SHOP_NEW_PRODUCT_{0}_{1}", this.Category.CategoryId, pId), true);
		});
		this.UpdateNew();
	}

	public int HideProduct(int productId)
	{
		return this.Products.RemoveAll((StoreProduct p) => p.ProductId == productId);
	}

	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	private void UpdateNew()
	{
		if (!this._isLeftDiscountImg)
		{
			this._discountImageRight.gameObject.SetActive((from p in this.Products
				where p.IsNew
				select p.ProductId.ToString()).ToList<string>().Any((string p) => !ObscuredPrefs.HasKey(string.Format("PREM_SHOP_NEW_PRODUCT_{0}_{1}", this.Category.CategoryId, p))));
		}
	}

	private void UpdateDiscount()
	{
		bool flag = this.Products.Any((StoreProduct p) => p.HasActiveDiscount(TimeHelper.UtcTime()) && p.DiscountPrice != null);
		if (!flag && this.LifetimeRemain != null)
		{
			flag = this.LifetimeRemain.Any((KeyValuePair<int, DateTime?> p) => p.Value != null && p.Value.Value > TimeHelper.UtcTime());
		}
		this._discountImageLeft.gameObject.SetActive(flag && this._isLeftDiscountImg);
	}

	[SerializeField]
	private TextMeshProUGUI _categoryName;

	[SerializeField]
	private RectTransform _discountImageLeft;

	[SerializeField]
	private RectTransform _discountImageRight;

	[SerializeField]
	private Toggle _tgl;

	[SerializeField]
	private Image _imSelectedBg;

	[SerializeField]
	private Image _imSelectedRect;

	[SerializeField]
	private Image _imMouseOver;

	public readonly Color Gray = new Color(0.8901961f, 0.8901961f, 0.8901961f);

	private Dictionary<int, List<StoreProduct>> _productsPondPassesDict = new Dictionary<int, List<StoreProduct>>();

	private bool _isLeftDiscountImg;

	private float _curUpdateTime = 10f;

	private const float UpdateTime = 10f;

	private const string NewProductPrefix = "PREM_SHOP_NEW_PRODUCT_{0}_{1}";
}
