using System;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TargetAdInit : MonoBehaviour
{
	public bool IsBuyButtonActive
	{
		get
		{
			return this._currentItem.ProductId != null && this._currentItem.ProductId > 0;
		}
	}

	public StoreProduct Product
	{
		get
		{
			if (this._currentItem == null || this._currentItem.ProductId == null)
			{
				return null;
			}
			return CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == this._currentItem.ProductId);
		}
	}

	public void SetActive(bool isActive, Button btn)
	{
		this._isActive = isActive;
		if (this._taMission != null)
		{
			this._taMission.SetActive(isActive);
		}
		if (isActive)
		{
			this.BuyButton = btn;
			this.BuyButton.onClick.AddListener(new UnityAction(this.DoAction));
			this.BuyButton.interactable = this.IsBuyButtonActive;
		}
		else if (this.BuyButton != null)
		{
			this.BuyButton.onClick.RemoveListener(new UnityAction(this.DoAction));
			this.BuyButton = null;
		}
	}

	public void Init(TargetedAdSlide item)
	{
		this._currentItem = item;
		this._endTimeValue = this._currentItem.End;
		if (this._endTimeValue == null)
		{
			this.TimerValue.gameObject.SetActive(false);
		}
		this.ActionArea.interactable = this.IsBuyButtonActive;
		this.ImagePanelLoadable.Image = this.ImagePanel;
		this.ImagePanelLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentItem.BackgroundBID));
		this.ImagePanel.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1207f, 766f);
		this.DescValue.text = string.Format(this._currentItem.Text, "<b><color=#FFC609FF>", "</color></b>", "\n");
		this.DescValue.rectTransform.sizeDelta = new Vector2(this._currentItem.Design.TextLayout.Scale.X, this._currentItem.Design.TextLayout.Scale.Y);
		this.DescValue.rectTransform.localPosition = new Vector3(this._currentItem.Design.TextLayout.Position.X, this._currentItem.Design.TextLayout.Position.Y, 0f);
		this.DescValue.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Design.TextLayout.Alignment);
		this.DescValue.color = this.ToColor(this._currentItem.Design.TextLayout.FontColor);
		this.DescValue.resizeTextMaxSize = this._currentItem.Design.TextLayout.MaxFontSize;
		this.ActionArea.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(this._currentItem.Design.OfferLinkLayout.Scale.X, this._currentItem.Design.OfferLinkLayout.Scale.Y);
		this.ActionArea.transform.GetComponent<RectTransform>().localPosition = new Vector3(this._currentItem.Design.OfferLinkLayout.Position.X, this._currentItem.Design.OfferLinkLayout.Position.Y, 0f);
		if (this._currentItem.OfferImageBID != null)
		{
			this.OfferImagePanel.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(this._currentItem.Design.OfferImageLayout.Scale.X, this._currentItem.Design.OfferImageLayout.Scale.Y);
			this.OfferImagePanel.transform.GetComponent<RectTransform>().localPosition = new Vector3(this._currentItem.Design.OfferImageLayout.Position.X, this._currentItem.Design.OfferImageLayout.Position.Y, 0f);
			this.OfferImagePanelLoadable.Image = this.OfferImagePanel;
			this.OfferImagePanelLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentItem.OfferImageBID));
		}
		this.OfferTitle.text = this._currentItem.OfferTitle;
		this.OfferTitle.rectTransform.sizeDelta = new Vector2(this._currentItem.Design.OfferTitleLayout.Scale.X, this._currentItem.Design.OfferTitleLayout.Scale.Y);
		this.OfferTitle.rectTransform.localPosition = new Vector3(this._currentItem.Design.OfferTitleLayout.Position.X, this._currentItem.Design.OfferTitleLayout.Position.Y, 0f);
		this.OfferTitle.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Design.OfferTitleLayout.Alignment);
		this.OfferTitle.color = this.ToColor(this._currentItem.Design.OfferTitleLayout.FontColor);
		this.OfferTitle.resizeTextMaxSize = this._currentItem.Design.OfferTitleLayout.MaxFontSize;
		this.TimerValue.rectTransform.sizeDelta = new Vector2(this._currentItem.Design.TimerLayout.Scale.X, this._currentItem.Design.TimerLayout.Scale.Y);
		this.TimerValue.rectTransform.localPosition = new Vector3(this._currentItem.Design.TimerLayout.Position.X, this._currentItem.Design.TimerLayout.Position.Y, 0f);
		this.TimerValue.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Design.TimerLayout.Alignment);
		this.TimerValue.color = this.ToColor(this._currentItem.Design.TimerLayout.FontColor);
		this.TimerValue.resizeTextMaxSize = this._currentItem.Design.TimerLayout.MaxFontSize;
		if (!string.IsNullOrEmpty(this._currentItem.Design.TimerStyle))
		{
			string timerStyle = this._currentItem.Design.TimerStyle;
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
		this.Title.rectTransform.sizeDelta = new Vector2(this._currentItem.Design.TitleLayout.Scale.X, this._currentItem.Design.TitleLayout.Scale.Y);
		this.Title.rectTransform.localPosition = new Vector3(this._currentItem.Design.TitleLayout.Position.X, this._currentItem.Design.TitleLayout.Position.Y, 0f);
		this.Title.text = this._currentItem.Title;
		this.Title.alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Design.TitleLayout.Alignment);
		this.Title.color = this.ToColor(this._currentItem.Design.TitleLayout.FontColor);
		this.Title.resizeTextMaxSize = this._currentItem.Design.TitleLayout.MaxFontSize;
		if (item.AdConfig.MissionId != null)
		{
			if (ClientMissionsManager.Instance.CurrentTrackedMission != null)
			{
				this.InitMisisonPanel(ClientMissionsManager.Instance.CurrentTrackedMission);
			}
			else
			{
				ClientMissionsManager.Instance.TrackedMissionUpdated += this.OnTrackedMissionUpdated;
			}
		}
	}

	private void OnEnable()
	{
		AlphaFade component = base.GetComponent<AlphaFade>();
		component.ShowFinished += this.TargetAdInit_ShowFinished;
		component.HideFinished += this.TargetAdInit_HideFinished;
	}

	private void TargetAdInit_HideFinished(object sender, EventArgsAlphaFade e)
	{
		base.GetComponent<AlphaFade>().HideFinished -= this.TargetAdInit_HideFinished;
	}

	private void TargetAdInit_ShowFinished(object sender, EventArgs e)
	{
		base.GetComponent<AlphaFade>().ShowFinished -= this.TargetAdInit_ShowFinished;
		PhotonConnectionFactory.Instance.CaptureActionInStats("TAShow", this._currentItem.ItemId.ToString(), this._currentItem.DesignId.ToString(), null);
	}

	private void OnDestroy()
	{
		ClientMissionsManager.Instance.TrackedMissionUpdated -= this.OnTrackedMissionUpdated;
		AlphaFade component = base.GetComponent<AlphaFade>();
		component.ShowFinished -= this.TargetAdInit_ShowFinished;
		component.HideFinished -= this.TargetAdInit_HideFinished;
	}

	public void DoAction()
	{
		if (this._currentItem.ProductId == null)
		{
			string text = string.Format("TargetAdInit: TA #{0} do not have product set", this._currentItem.ItemId);
			Debug.LogErrorFormat(text, new object[0]);
			PhotonConnectionFactory.Instance.PinError(text, null);
			return;
		}
		StoreProduct product = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == this._currentItem.ProductId);
		if (product == null)
		{
			string text2 = string.Format("TargetAdInit: Product #{0} is absent in client product cache for TA #{1}", this._currentItem.ProductId, this._currentItem.ItemId);
			Debug.LogErrorFormat(text2, new object[0]);
			PhotonConnectionFactory.Instance.PinError(text2, null);
			return;
		}
		if (product.TypeId == 9 && product.ProductCurrency == "GC")
		{
			string text3 = product.GetPrice(TimeHelper.UtcTime()).ToString("N0");
			string currencyIcon = MeasuringSystemManager.GetCurrencyIcon(product.ProductCurrency);
			string text4 = string.Format("{0} {1} ", UgcConsts.GetYellowTan(currencyIcon), UgcConsts.GetYellowTan(text3));
			string text5 = string.Format(ScriptLocalization.Get("ProductBuyConfirm"), UgcConsts.GetYellowTan(product.Name) + '\n', text4);
			UIHelper.ShowYesNo(text5, delegate
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
			this.OfferImagePanelLoadable.Load(this._currentItem.OfferHoverImageBID, this.OfferImagePanel, "Textures/Inventory/{0}");
		}
	}

	public void UnSelectedOffer()
	{
		if (this._currentItem.OfferImageBID != null)
		{
			this.OfferImagePanelLoadable.Load(this._currentItem.OfferImageBID, this.OfferImagePanel, "Textures/Inventory/{0}");
		}
	}

	public Color32 ToColor(int HexVal)
	{
		byte b = (byte)((HexVal >> 16) & 255);
		byte b2 = (byte)((HexVal >> 8) & 255);
		byte b3 = (byte)(HexVal & 255);
		return new Color32(b, b2, b3, byte.MaxValue);
	}

	private void InitMisisonPanel(MissionOnClient mission)
	{
		if (mission != null && mission.MissionId == this._currentItem.AdConfig.MissionId)
		{
			GameObject gameObject = GUITools.AddChild(this.ActionArea.gameObject, this._missionPrefab);
			RectTransform component = gameObject.GetComponent<RectTransform>();
			if (this._currentItem.Design.MissionLayout.Position != null)
			{
				component.localPosition = new Vector3(this._currentItem.Design.MissionLayout.Position.X, this._currentItem.Design.MissionLayout.Position.Y);
			}
			else if (!string.IsNullOrEmpty(this._currentItem.Design.MissionLayout.Alignment))
			{
				Rect rect = this.ActionArea.GetComponent<RectTransform>().rect;
				float width = component.rect.width;
				float height = component.rect.height;
				float num = -rect.width / 2f + width / 2f;
				float num2 = rect.width / 2f - width / 2f;
				float num3 = rect.height / 2f - height / 2f;
				float num4 = -rect.height / 2f + height / 2f;
				switch ((TextAnchor)Enum.Parse(typeof(TextAnchor), this._currentItem.Design.MissionLayout.Alignment))
				{
				case 0:
					component.localPosition = new Vector3(num, num3);
					break;
				case 1:
					component.localPosition = new Vector3(0f, num3);
					break;
				case 2:
					component.localPosition = new Vector3(num2, num3);
					break;
				case 3:
					component.localPosition = new Vector3(num, 0f);
					break;
				case 4:
					component.localPosition = Vector3.zero;
					break;
				case 5:
					component.localPosition = new Vector3(num2, 0f);
					break;
				case 6:
					component.localPosition = new Vector3(num, num4);
					break;
				case 7:
					component.localPosition = new Vector3(0f, num4);
					break;
				case 8:
					component.localPosition = new Vector3(num2, num4);
					break;
				}
			}
			this._taMission = gameObject.GetComponent<TargetAdMission>();
			this._taMission.Init(mission, this._currentItem.AdConfig.MissionId, this._currentItem.AdConfig.TaskId);
			if (this._isActive)
			{
				this._taMission.SetActive(true);
			}
		}
	}

	private void OnTrackedMissionUpdated(MissionOnClient m)
	{
		ClientMissionsManager.Instance.TrackedMissionUpdated -= this.OnTrackedMissionUpdated;
		this.InitMisisonPanel(m);
	}

	private void InitiateProductPurchase(StoreProduct product)
	{
		BuyProductManager.InitiateProductPurchase(product);
		PhotonConnectionFactory.Instance.CaptureActionInStats("TAClick", this._currentItem.ItemId.ToString(), this._currentItem.DesignId.ToString(), null);
	}

	[SerializeField]
	private GameObject _missionPrefab;

	public Button ActionArea;

	public Text TimerValue;

	public Text DescValue;

	public Text Title;

	public Image ImagePanel;

	private ResourcesHelpers.AsyncLoadableImage ImagePanelLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Image OfferImagePanel;

	private ResourcesHelpers.AsyncLoadableImage OfferImagePanelLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Text OfferTitle;

	[HideInInspector]
	public Text ConsoleHelpText;

	[HideInInspector]
	public Button BuyButton;

	private DateTime? _endTimeValue;

	private TargetedAdSlide _currentItem;

	private TargetAdMission _taMission;

	private bool _isActive;
}
