using System;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WhatsNewInit : MonoBehaviour
{
	public bool HasActualProduct
	{
		get
		{
			return this._currentItem != null && this._currentItem.ProductId != null && this._currentItem.ProductId > 0;
		}
	}

	public bool HasUrl
	{
		get
		{
			return !string.IsNullOrEmpty(this._url);
		}
	}

	public StoreProduct Product
	{
		get
		{
			return this._product;
		}
	}

	internal void Init(WhatsNewItem item, Button buyButton)
	{
		this._buyButton = buyButton;
		this._currentItem = item;
		this._endTimeValue = this._currentItem.End;
		if (this._endTimeValue == null)
		{
			this.TimerValue.gameObject.SetActive(false);
		}
		if (this._currentItem.ProductId != null && this._currentItem.ProductId > 0)
		{
			this._hasProduct = true;
			this._product = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == this._currentItem.ProductId);
			if (this._product == null)
			{
				string text = string.Format("WhatsNewInit: Product #{0} is absent in client product cache for WN #{1}", this._currentItem.ProductId, this._currentItem.ItemId);
				Debug.LogErrorFormat(text, new object[0]);
				PhotonConnectionFactory.Instance.PinError(text, null);
				return;
			}
			this._url = this._product.ExternalShopLink;
		}
		else
		{
			this._hasProduct = !string.IsNullOrEmpty(item.OfferLinkText);
			this._url = item.OfferLinkText;
		}
		this.ActionArea.interactable = this._hasProduct;
		if (this._currentItem.BackgroundBID != null)
		{
			this.ImagePanelLoadable.Image = this.ImagePanel;
			this.ImagePanelLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentItem.BackgroundBID));
		}
		this.ImagePanel.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1207f, 766f);
		this.DescValue.rectTransform.sizeDelta = new Vector2(this._currentItem.Config.TextScale.X, this._currentItem.Config.TextScale.Y);
		this.DescValue.text = string.Format(this._currentItem.Text, "<b><color=yellow>", "</color></b>", "\n");
		this.DescValue.transform.localPosition = new Vector3(this._currentItem.Config.TextPosition.X, this._currentItem.Config.TextPosition.Y, 0f);
		this.DescValue.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Config.TextAlignment);
		this.DescValue.color = this.ToColor(this._currentItem.Config.TextFontColor);
		if (PhotonConnectionFactory.Instance.Profile.LanguageId == 12 || PhotonConnectionFactory.Instance.Profile.LanguageId == 13)
		{
			this.DescValue.resizeTextForBestFit = false;
			this.DescValue.fontSize = this._currentItem.Config.TextMaxFontSize;
		}
		else
		{
			this.DescValue.resizeTextForBestFit = true;
			this.DescValue.resizeTextMaxSize = this._currentItem.Config.TextMaxFontSize;
		}
		this.ActionArea.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(this._currentItem.Config.OfferLinkScale.X, this._currentItem.Config.OfferLinkScale.Y);
		this.ActionArea.transform.GetComponent<RectTransform>().localPosition = new Vector3(this._currentItem.Config.OfferLinkPosition.X, this._currentItem.Config.OfferLinkPosition.Y, 0f);
		if (this._currentItem.OfferImageBID != null)
		{
			this.OfferImagePanel.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(this._currentItem.Config.OfferImageScale.X, this._currentItem.Config.OfferImageScale.Y);
			this.OfferImagePanel.transform.GetComponent<RectTransform>().localPosition = new Vector3(this._currentItem.Config.OfferImagePosition.X, this._currentItem.Config.OfferImagePosition.Y, 0f);
			this.OfferImagePanelLoadable.Image = this.OfferImagePanel;
			this.OfferImagePanelLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentItem.OfferImageBID));
		}
		this.OfferTitle.text = this._currentItem.OfferTitle;
		this.OfferTitle.rectTransform.sizeDelta = new Vector2(this._currentItem.Config.OfferTitleScale.X, this._currentItem.Config.OfferTitleScale.Y);
		this.OfferTitle.rectTransform.localPosition = new Vector3(this._currentItem.Config.OfferTitlePosition.X, this._currentItem.Config.OfferTitlePosition.Y, 0f);
		this.OfferTitle.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Config.OfferTitleAlignment);
		this.OfferTitle.color = this.ToColor(this._currentItem.Config.OfferTitleFontColor);
		this.OfferTitle.resizeTextMaxSize = this._currentItem.Config.OfferTitleMaxFontSize;
		this.TimerValue.rectTransform.sizeDelta = new Vector2(this._currentItem.Config.TimerScale.X, this._currentItem.Config.TimerScale.Y);
		this.TimerValue.rectTransform.localPosition = new Vector3(this._currentItem.Config.TimerPosition.X, this._currentItem.Config.TimerPosition.Y, 0f);
		this.TimerValue.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Config.TimerAlignment);
		this.TimerValue.color = this.ToColor(this._currentItem.Config.TimerFontColor);
		this.TimerValue.resizeTextMaxSize = this._currentItem.Config.TimerMaxFontSize;
		if (!string.IsNullOrEmpty(this._currentItem.Config.TimerStyle))
		{
			string timerStyle = this._currentItem.Config.TimerStyle;
			if (timerStyle != null)
			{
				if (!(timerStyle == "Icon"))
				{
					if (timerStyle == "NoIcon")
					{
						this.TimerValue.transform.Find("Icon").gameObject.SetActive(false);
					}
				}
				else
				{
					this.TimerValue.transform.Find("Icon").gameObject.SetActive(true);
				}
			}
		}
		this.Title.rectTransform.sizeDelta = new Vector2(this._currentItem.Config.TitleScale.X, this._currentItem.Config.TitleScale.Y);
		this.Title.rectTransform.localPosition = new Vector3(this._currentItem.Config.TitlePosition.X, this._currentItem.Config.TitlePosition.Y, 0f);
		this.Title.text = this._currentItem.Title;
		this.Title.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Config.TitleAlignment);
		this.Title.color = this.ToColor(this._currentItem.Config.TitleFontColor);
		this.Title.resizeTextMaxSize = this._currentItem.Config.TitleMaxFontSize;
	}

	private void OnEnable()
	{
		base.GetComponent<AlphaFade>().ShowFinished += this.WhatsNewInit_ShowFinished;
		base.GetComponent<AlphaFade>().HideFinished += this.WhatsNewInit_HideFinished;
	}

	public void Hide()
	{
		this._buyButton.onClick.RemoveAllListeners();
	}

	public void Show()
	{
		this._buyButton.onClick.AddListener(new UnityAction(this.DoAction));
		this._buyButton.interactable = this._hasProduct;
		PhotonConnectionFactory.Instance.CaptureActionInStats("WNShow", this._currentItem.ItemId.ToString(), null, null);
	}

	private void WhatsNewInit_HideFinished(object sender, EventArgsAlphaFade e)
	{
		base.GetComponent<AlphaFade>().HideFinished -= this.WhatsNewInit_HideFinished;
	}

	private void WhatsNewInit_ShowFinished(object sender, EventArgs e)
	{
		base.GetComponent<AlphaFade>().ShowFinished -= this.WhatsNewInit_ShowFinished;
	}

	private void OnDestroy()
	{
		base.GetComponent<AlphaFade>().ShowFinished -= this.WhatsNewInit_ShowFinished;
		base.GetComponent<AlphaFade>().HideFinished -= this.WhatsNewInit_HideFinished;
	}

	public void DoAction()
	{
		StoreProduct product = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == this._currentItem.ProductId);
		if (product == null)
		{
			string text = string.Format("WhatsNewInit: Product #{0} is absent in client product cache for WN #{1}", this._currentItem.ProductId, this._currentItem.ItemId);
			Debug.LogErrorFormat(text, new object[0]);
			PhotonConnectionFactory.Instance.PinError(text, null);
			return;
		}
		if (product.TypeId == 9 && product.ProductCurrency == "GC")
		{
			string text2 = product.GetPrice(TimeHelper.UtcTime()).ToString("N0");
			string currencyIcon = MeasuringSystemManager.GetCurrencyIcon(product.ProductCurrency);
			string text3 = string.Format("{0} {1} ", UgcConsts.GetYellowTan(currencyIcon), UgcConsts.GetYellowTan(text2));
			string text4 = string.Format(ScriptLocalization.Get("ProductBuyConfirm"), UgcConsts.GetYellowTan(product.Name) + '\n', text3);
			UIHelper.ShowYesNo(text4, delegate
			{
				this.InitiateProductPurchase(product);
			}, null, "YesCaption", null, "NoCaption", null, null, null);
		}
		else
		{
			this.InitiateProductPurchase(product);
		}
	}

	private void Update()
	{
		if (this._endTimeValue != null)
		{
			this.TimerValue.text = this._endTimeValue.Value.GetTimeFinishInValue(true);
		}
	}

	public void SelectedOffer()
	{
		if (this._currentItem.OfferHoverImageBID != null)
		{
			this.OfferImagePanelLoadable.Image = this.OfferImagePanel;
			this.OfferImagePanelLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentItem.OfferHoverImageBID));
		}
	}

	public void UnSelectedOffer()
	{
		if (this._currentItem.OfferImageBID != null)
		{
			this.OfferImagePanelLoadable.Image = this.OfferImagePanel;
			this.OfferImagePanelLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentItem.OfferImageBID));
		}
	}

	public Color32 ToColor(int HexVal)
	{
		byte b = (byte)((HexVal >> 16) & 255);
		byte b2 = (byte)((HexVal >> 8) & 255);
		byte b3 = (byte)(HexVal & 255);
		return new Color32(b, b2, b3, byte.MaxValue);
	}

	private void InitiateProductPurchase(StoreProduct product)
	{
		BuyProductManager.InitiateProductPurchase(product);
		PhotonConnectionFactory.Instance.CaptureActionInStats("WNClick", this._currentItem.ItemId.ToString(), null, null);
		PhotonConnectionFactory.Instance.WhatsNewClicked(this._currentItem);
	}

	public Button ActionArea;

	public Text TimerValue;

	public Text DescValue;

	public Text Title;

	public Image ImagePanel;

	private ResourcesHelpers.AsyncLoadableImage ImagePanelLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Image OfferImagePanel;

	private ResourcesHelpers.AsyncLoadableImage OfferImagePanelLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Text OfferTitle;

	private string _url;

	private bool _hasProduct;

	private DateTime? _endTimeValue;

	private WhatsNewItem _currentItem;

	private Button _buyButton;

	private StoreProduct _product;
}
