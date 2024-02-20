using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PremiumShopRetailHandler : ActivityStateControlled
{
	private void Awake()
	{
		for (int i = 0; i < this._fullInfoPos0.Length; i++)
		{
			GameObject gameObject = GUITools.AddChild(this._itemsFullInfoParent, this._itemFullInfoPrefab);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(this._fullInfoPos0[i], 0f);
			this._itemsFullInfo.Add(gameObject.GetComponent<PremiumRetailItemFullInfo>());
		}
		this._btnLeft.onClick.AddListener(delegate
		{
			this.Scroll(1);
		});
		this._btnRight.onClick.AddListener(delegate
		{
			this.Scroll(-1);
		});
		TMP_Text component = this._arrowLeft.GetComponent<TextMeshProUGUI>();
		string text = "\ue625";
		this._arrowRightFullInfo.GetComponent<TextMeshProUGUI>().text = text;
		text = text;
		this._arrowLeftFullInfo.GetComponent<TextMeshProUGUI>().text = text;
		text = text;
		this._arrowRight.GetComponent<TextMeshProUGUI>().text = text;
		component.text = text;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.StopAllCoroutines();
	}

	protected override void SetHelp()
	{
		base.SetHelp();
		if (!PremiumShopMainPageHandler.IsProductListAvailable)
		{
			return;
		}
		int showCount = this.ShowCount;
		if (showCount < 2)
		{
			this.SetShowCount(showCount + 1);
			UIHelper.ShowMessage(ScriptLocalization.Get("InformationTournamentCaption"), ScriptLocalization.Get("RetailPacksInfoMsg"), true, null, false);
		}
		ProductCategory productCategory = CacheLibrary.ProductCache.ProductCategories.FirstOrDefault((ProductCategory p) => p.CategoryId == 10);
		if (productCategory != null)
		{
			List<int> productsIds = productCategory.ProductIds.ToList<int>();
			List<StoreProduct> list = (from p in CacheLibrary.ProductCache.Products
				where productsIds.Contains(p.ProductId)
				orderby productsIds.IndexOf(p.ProductId)
				select p).ToList<StoreProduct>();
			if (this._items.Count == 0)
			{
				this._index = 0;
				for (int i = 0; i < list.Count; i++)
				{
					GameObject gameObject = GUITools.AddChild(this._itemsParent, this._itemPrefab);
					PremiumShopRetailItem el = gameObject.GetComponent<PremiumShopRetailItem>();
					float num = -560f + (float)i * PremiumShopRetailItem.Offset;
					this._cachedX.Add(num);
					el.Move(num, 0f);
					el.Init(list[i]);
					int i1 = i;
					el.OnSelect += delegate(StoreProduct p)
					{
						if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
						{
							this._selectedIndexPrev = this._selectedIndex;
							this._selectedIndex = i1;
							this.UpdatePosItems();
						}
					};
					el.OnBuy += delegate(StoreProduct p)
					{
						this._curProduct = p;
						this.OpenFullInfo(p, el);
					};
					this._items.Add(el);
				}
				int num2 = list.Count - 5;
				for (int j = 1; j <= num2; j++)
				{
					this._cachedX.Insert(0, -560f - (float)j * PremiumShopRetailItem.Offset);
					this._index++;
				}
				this._index0 = this._index;
			}
			this.ResetView();
		}
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	protected override void HideHelp()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		base.HideHelp();
	}

	public void Scroll(int value)
	{
		int num = this._index + value;
		if (num <= this._items.Count - 5 && num >= 0)
		{
			this._index = num;
			this.MovePosItems(null, 0.25f);
			this.UpdateScrollBtnsInteractable();
		}
	}

	public void MoveToProductPondPaid(int pondId)
	{
		this._initialPondId = new int?(pondId);
	}

	private void UpdateScrollBtns()
	{
		bool flag = InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse && this._items.Count > 5;
		this._btnLeft.gameObject.SetActive(flag);
		this._btnRight.gameObject.SetActive(flag);
	}

	private void UpdateScrollBtnsInteractable()
	{
		this._btnRight.interactable = this._index - 1 >= 0;
		this._btnLeft.interactable = this._index + 1 <= this._items.Count - 5;
		this._arrowLeft.color = ((!this._btnLeft.interactable) ? this.ArrowDisabled : this.ArrowNormal);
		this._arrowRight.color = ((!this._btnRight.interactable) ? this.ArrowDisabled : this.ArrowNormal);
	}

	private void ResetView()
	{
		this._index = this._index0;
		this._needMove = (this._moving = false);
		this._selectedIndexPrev = (this._selectedIndex = -1);
		int i = this._index;
		this._items.ForEach(delegate(PremiumShopRetailItem p)
		{
			p.Move(this._cachedX[i++], 0f);
		});
		this.SelectFirst();
		this.UpdateScrollBtns();
		this.UpdateScrollBtnsInteractable();
		this.UpdateScrollBtnsInteractableFullInfo();
		this.UpdateScrollBtnsFullInfo();
	}

	private void UpdatePosItems()
	{
		if (this._items.Count > 5)
		{
			if (this._selectedIndex > this._selectedIndexPrev)
			{
				if (this._selectedIndex > 2 && this._selectedIndex < this._items.Count - 2)
				{
					this._index--;
					this.MovePosItems(null, 0.25f);
				}
			}
			else if (this._selectedIndex < this._selectedIndexPrev && this._selectedIndex > 1 && this._selectedIndex < this._items.Count - 3)
			{
				this._index++;
				this.MovePosItems(null, 0.25f);
			}
		}
	}

	private void MovePosItems(Action completeFn = null, float animTime = 0.25f)
	{
		if (this._moving)
		{
			this._needMove = true;
			return;
		}
		Action fn = completeFn;
		this._moving = true;
		int count = this._items.Count;
		int i = this._index;
		this._items.ForEach(delegate(PremiumShopRetailItem p)
		{
			p.OnMoveComplete += delegate(StoreProduct prod)
			{
				count--;
				if (count <= 0)
				{
					this._moving = false;
					if (this._needMove)
					{
						this._needMove = false;
						this.MovePosItems(null, 0.25f);
					}
					else if (fn != null)
					{
						fn();
						fn = null;
					}
				}
			};
			p.Move(this._cachedX[i++], animTime);
		});
	}

	private void Clear()
	{
		this._items.ForEach(delegate(PremiumShopRetailItem p)
		{
			p.Remove();
		});
		this._items.Clear();
	}

	private void OnInputTypeChanged(InputModuleManager.InputType it)
	{
		this.ResetView();
	}

	private void SelectFirst()
	{
		int num = 0;
		if (this._initialPondId != null)
		{
			int num2 = this._items.FindIndex((PremiumShopRetailItem p) => p.Product != null && p.Product.PaidPondsUnlocked != null && p.Product.PaidPondsUnlocked.Contains(this._initialPondId.Value));
			if (num2 != -1)
			{
				num = num2;
			}
			this._initialPondId = null;
		}
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this._nav.SetConcreteActiveForce(this._items[num].gameObject);
			UINavigation.SetSelectedGameObject(this._items[num].gameObject);
		}
		else if (this._items.Count > 0)
		{
			UINavigation.SetSelectedGameObject(this._items[num].gameObject);
		}
		if (num > 0)
		{
			base.StartCoroutine(this.MoveToIndex(num));
		}
	}

	private IEnumerator MoveToIndex(int i)
	{
		yield return null;
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this._selectedIndexPrev = this._selectedIndex;
			this._selectedIndex = i;
		}
		int elemNumber = i + 1;
		if (elemNumber > 5)
		{
			this._index = this._index0 - (elemNumber - 5);
			this.MovePosItems(null, 0.25f);
			if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
			{
				this.UpdateScrollBtnsInteractable();
			}
		}
		yield break;
	}

	public void ScrollFullInfo(int value)
	{
		LogHelper.Log("___kocha LOL value:{0}", new object[] { value });
		int num = this._selectedIndexFullInfo + value;
		if (num == this._selectedIndexFullInfo)
		{
			return;
		}
		this._selectedIndexFullInfo += value;
		this.ScrollFullInfo(0.25f);
	}

	public void CloseFullInfo()
	{
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOFade(this._fullInfo, 0f, 0.2f), delegate
		{
			this._fullInfo.gameObject.SetActive(false);
			this._nav.enabled = true;
			if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
			{
				PremiumShopRetailItem premiumShopRetailItem = this._items.FirstOrDefault((PremiumShopRetailItem p) => p.ProductId == this._curProduct.ProductId);
				if (premiumShopRetailItem != null)
				{
					this._nav.SetConcreteActiveForce(premiumShopRetailItem.gameObject);
					UINavigation.SetSelectedGameObject(premiumShopRetailItem.gameObject);
				}
			}
		});
	}

	public void BuyFullInfo()
	{
		if (this._curProduct == null)
		{
			return;
		}
		if (this._curProduct.ProductCurrency == "SC" || this._curProduct.ProductCurrency == "GC")
		{
			string text = this._curProduct.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow).ToString("N2");
			string currencyIcon = MeasuringSystemManager.GetCurrencyIcon(this._curProduct.ProductCurrency);
			string text2 = string.Format("{0} {1} ", UgcConsts.GetYellowTan(currencyIcon), UgcConsts.GetYellowTan(text));
			string text3 = string.Format(ScriptLocalization.Get("ProductBuyConfirm"), UgcConsts.GetYellowTan(this._curProduct.Name) + '\n', text2);
			UIHelper.ShowYesNo(text3, delegate
			{
				this._buyClick.BuyProduct(this._curProduct);
			}, null, "YesCaption", null, "NoCaption", null, null, null);
			return;
		}
		BuyProductManager.InitiateProductPurchase(this._curProduct);
	}

	private void UpdateScrollBtnsInteractableFullInfo()
	{
		this._btnLeftFullInfo.interactable = this._selectedIndexFullInfo > 0;
		Selectable btnRightFullInfo = this._btnRightFullInfo;
		bool flag;
		if (this._itemsFullInfo.Count - this._selectedIndexFullInfo > 1)
		{
			flag = this._itemsFullInfo.Count((PremiumRetailItemFullInfo p) => p.IsActive) > 1;
		}
		else
		{
			flag = false;
		}
		btnRightFullInfo.interactable = flag;
		this._arrowLeftFullInfo.color = ((!this._btnLeftFullInfo.interactable) ? this.ArrowDisabled : this.ArrowNormal);
		this._arrowRightFullInfo.color = ((!this._btnRightFullInfo.interactable) ? this.ArrowDisabled : this.ArrowNormal);
	}

	private void UpdateScrollBtnsFullInfo()
	{
		bool flag;
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
		{
			flag = this._itemsFullInfo.Count((PremiumRetailItemFullInfo p) => p.IsActive) > 1;
		}
		else
		{
			flag = false;
		}
		bool flag2 = flag;
		CanvasGroup component = this._btnRightFullInfo.GetComponent<CanvasGroup>();
		float num = ((!flag2) ? 0f : 1f);
		this._btnLeftFullInfo.GetComponent<CanvasGroup>().alpha = num;
		component.alpha = num;
	}

	private void OpenFullInfo(StoreProduct p, PremiumShopRetailItem el)
	{
		this._selectedIndexFullInfo = 0;
		this.ScrollFullInfo(0f);
		if (p.PromotionImageBID == null || p.ItemListImageBID == null)
		{
			PremiumRetailItemFullInfo premiumRetailItemFullInfo = this._itemsFullInfo[0];
			int? promotionImageBID = p.PromotionImageBID;
			premiumRetailItemFullInfo.Init((promotionImageBID == null) ? p.ItemListImageBID : promotionImageBID, el.Currency, el.Price);
			this._itemsFullInfo[1].SetActive(false);
		}
		else
		{
			this._itemsFullInfo[0].Init(p.PromotionImageBID, el.Currency, el.Price);
			this._itemsFullInfo[1].Init(p.ItemListImageBID, el.Currency, el.Price);
			this._itemsFullInfo[1].SetActive(true);
		}
		this.UpdateScrollBtnsInteractableFullInfo();
		this.UpdateScrollBtnsFullInfo();
		this._fullInfo.gameObject.SetActive(true);
		ShortcutExtensions.DOFade(this._fullInfo, 1f, 0.2f);
		this._nav.enabled = false;
	}

	private void ScrollFullInfo(float animTime)
	{
		this.UpdateScrollBtnsInteractableFullInfo();
		this.UpdateScrollBtnsFullInfo();
		for (int i = 0; i < this._itemsFullInfo.Count; i++)
		{
			this._itemsFullInfo[i].DoKill(false);
			this._itemsFullInfo[i].Select(i == this._selectedIndexFullInfo);
		}
		float num = this._fullInfoPos0[1] - this._fullInfoPos0[0];
		if (this._selectedIndexFullInfo == 0)
		{
			this._itemsFullInfo[0].MoveX(0f, animTime);
			this._itemsFullInfo[1].MoveX(num, animTime);
		}
		else
		{
			this._itemsFullInfo[0].MoveX(-num, animTime);
			this._itemsFullInfo[1].MoveX(0f, animTime);
		}
	}

	private int ShowCount
	{
		get
		{
			return PlayerPrefs.GetInt("ShowCountTagPremiumShopRetail", 0);
		}
	}

	private void SetShowCount(int v)
	{
		PlayerPrefs.SetInt("ShowCountTagPremiumShopRetail", v);
	}

	[SerializeField]
	private BuyClick _buyClick;

	[SerializeField]
	private Graphic _arrowLeft;

	[SerializeField]
	private Graphic _arrowRight;

	[SerializeField]
	private Button _btnLeft;

	[SerializeField]
	private Button _btnRight;

	[SerializeField]
	private GameObject _itemPrefab;

	[SerializeField]
	private GameObject _itemsParent;

	[SerializeField]
	private UINavigation _nav;

	[Space(8f)]
	[SerializeField]
	private Graphic _arrowLeftFullInfo;

	[SerializeField]
	private Graphic _arrowRightFullInfo;

	[SerializeField]
	private Button _btnLeftFullInfo;

	[SerializeField]
	private Button _btnRightFullInfo;

	[SerializeField]
	private CanvasGroup _fullInfo;

	[SerializeField]
	private GameObject _itemFullInfoPrefab;

	[SerializeField]
	private GameObject _itemsFullInfoParent;

	private int _selectedIndexFullInfo;

	private readonly float[] _fullInfoPos0 = new float[] { 0f, 1243f };

	private List<PremiumRetailItemFullInfo> _itemsFullInfo = new List<PremiumRetailItemFullInfo>();

	private const int ShowCountMax = 2;

	private const string ShowCountTag = "ShowCountTagPremiumShopRetail";

	private const float X0 = -560f;

	private const int MaxItemsWithoutScroll = 5;

	private const float AnimTime = 0.25f;

	private readonly Color ArrowNormal = new Color(0.73333335f, 0.73333335f, 0.73333335f);

	private readonly Color ArrowDisabled = new Color(0.26666668f, 0.26666668f, 0.26666668f);

	private List<PremiumShopRetailItem> _items = new List<PremiumShopRetailItem>();

	private int _selectedIndex = -1;

	private int _selectedIndexPrev = -1;

	private int _index = -1;

	private int _index0 = -1;

	private bool _needMove;

	private bool _moving;

	private List<float> _cachedX = new List<float>();

	private StoreProduct _curProduct;

	private const bool IsOpenFullInfoOnBuyClick = true;

	private int? _initialPondId;
}
