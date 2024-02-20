using System;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowDetailedInfo : MonoBehaviour
{
	public bool Expanded
	{
		get
		{
			return this._isShow;
		}
		set
		{
			if (this._isShow != value)
			{
				this.ignore = true;
				this.DetailedPanel.SetActive(value);
				this.InfoToggle.isOn = value;
				this._isShow = value;
			}
		}
	}

	private void Start()
	{
		if (this.SellButton != null && this.RemoveButton != null && this.RepaireButton != null)
		{
			if (this._inventoryItem != null)
			{
				this.CheckCanSell();
			}
			this._repaireButton = this.RepaireButton.GetComponent<Button>();
		}
	}

	private void OnDestroy()
	{
		this.UnsubscribeDestroyItem(false);
	}

	internal virtual void OnEnable()
	{
		this._rect = base.GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (((this.RemoveButton != null && this.RemoveButton.activeInHierarchy) || (this.SellButton != null && this.SellButton.activeInHierarchy)) && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			if (ControlsController.ControlsActions.UISellRemove.IsPressedMandatory)
			{
				this.SellRemoveProgress.fillAmount = Mathf.Min(1f, this.sellRemoveTime);
				this.SellRemoveProgress.color = Color.Lerp(Color.white, Color.red, this.sellRemoveTime * this.sellRemoveTime);
				this.sellRemoveTime += Time.deltaTime;
			}
			else
			{
				this.ResetSellProgress();
			}
		}
		else if (this.SellRemoveProgress != null && this.SellRemoveProgress.fillAmount > 0f)
		{
			this.ResetSellProgress();
		}
	}

	public void InfoClick()
	{
		if (this.ignore)
		{
			this.ignore = false;
			return;
		}
		if (this.InfoToggle.isOn)
		{
			this.Show();
		}
		else
		{
			this.Hide();
		}
	}

	public void Init(InventoryItem ii)
	{
		bool flag = ii is Chum;
		if (this.MoveButton != null)
		{
			this.MoveButton.interactable = !PondHelper.IsOnPond;
		}
		this._inventoryItem = ii;
		if (this.SellButton != null && this.RemoveButton != null && this.RepaireButton != null)
		{
			this.CheckCanSell();
			if (StaticUserData.CurrentPond != null || !this._inventoryItem.IsRepairable || flag)
			{
				this.RepaireButton.SetActive(false);
			}
			else
			{
				this.RepaireButton.SetActive(true);
				ShowDetailedInfo.PreviewRepairData previewRepairData = this.PreviewRepair();
				if (this._repaireButton == null && this.RepaireButton != null)
				{
					this._repaireButton = this.RepaireButton.GetComponent<Button>();
				}
				if (this._repaireButton != null)
				{
					this._repaireButton.interactable = previewRepairData.Damage > 0;
				}
			}
		}
		if (flag && ((Chum)ii).IsExpired)
		{
			this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", "sour-foodball"), this.Image);
		}
		else
		{
			this.ImageLdbl.Load(this._inventoryItem.ThumbnailBID, this.Image, "Textures/Inventory/{0}");
		}
		this.Params.text = InventoryParamsHelper.Split(InventoryParamsHelper.ParseParamsInfo(this._inventoryItem, true));
		this.Description.text = InventoryParamsHelper.ParseDesc(this._inventoryItem);
	}

	protected virtual void Show()
	{
		this.DetailedPanel.SetActive(true);
		this._isShow = true;
		if (this.sizeChangesHandler != null)
		{
			bool flag = this.sizeChangesHandler.HandleSizeChangeRequest(this._rect, this.ExpandedSize);
			if (flag)
			{
				this.sizeChangesHandler.OnExpandedStateChanged(this._rect, this._isShow);
			}
			else
			{
				this.DetailedPanel.SetActive(false);
				this._isShow = false;
			}
		}
		else if (this._layoutElement != null)
		{
			this._layoutElement.preferredHeight = this.ExpandedSize;
		}
	}

	protected virtual void Hide()
	{
		if (!this._isShow)
		{
			return;
		}
		this.DetailedPanel.SetActive(false);
		this._isShow = false;
		if (this.sizeChangesHandler == null && this._layoutElement != null)
		{
			this._layoutElement.preferredHeight = this.NonExpandedSize;
		}
		else if (this.sizeChangesHandler != null)
		{
			bool flag = this.sizeChangesHandler.HandleSizeChangeRequest(this._rect, this.NonExpandedSize);
			if (flag)
			{
				this.sizeChangesHandler.OnExpandedStateChanged(this._rect, this._isShow);
			}
			else
			{
				this.DetailedPanel.SetActive(true);
				this._isShow = true;
			}
		}
	}

	public virtual void RemoveClick()
	{
		UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get("RemoveItemMessage"), this._inventoryItem.Name), delegate
		{
			this.DestroyItem();
		}, null, "YesCaption", null, "NoCaption", null, null, null);
	}

	public virtual void SellClick()
	{
		string empty = string.Empty;
		Action action = delegate
		{
			this.WriteAnalytics(this._inventoryItem.SellPrice.Currency, this._inventoryItem.SellPrice.Value, this._inventoryItem);
			this.DestroyItem();
		};
		string text = null;
		GameObject messageBoxPrefab = this.MessageBoxPrefab;
		UIHelper.ShowYesNo(empty, action, text, "YesCaption", null, "NoCaption", messageBoxPrefab, null, null).GetComponent<MessageBoxSellItem>().Init(string.Format(ScriptLocalization.Get("SellConfirmationText"), "\n", this._inventoryItem.Name), this._inventoryItem.SellPrice.Currency, this._inventoryItem.SellPrice.Value.ToString());
	}

	public virtual void RepairClick()
	{
		ShowDetailedInfo.PreviewRepairData r = this.PreviewRepair();
		int cost = (int)r.Cost;
		if ((r.Currency == "SC" && PhotonConnectionFactory.Instance.Profile.SilverCoins >= (double)r.Cost) || (r.Currency == "GC" && PhotonConnectionFactory.Instance.Profile.GoldCoins >= (double)r.Cost))
		{
			UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get("RepaireItemMessage"), cost, MeasuringSystemManager.GetCurrencyIcon(r.Currency), "\n"), delegate
			{
				this.RepaireButton.GetComponent<Button>().interactable = false;
				PhotonConnectionFactory.Instance.RepairItem(this._inventoryItem);
				this.WriteAnalytics(r.Currency, cost, null);
			}, null, "YesCaption", null, "NoCaption", null, null, null);
		}
		else
		{
			this.helpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("RepaireHaventMoney"), cost, MeasuringSystemManager.GetCurrencyIcon(r.Currency), "\n"), false, true, true, null);
		}
	}

	private void CheckCanSell()
	{
		this.RemoveButton.SetActive(!this._inventoryItem.CanSell);
		this.SellButton.SetActive(this._inventoryItem.CanSell);
	}

	private void ResetSellProgress()
	{
		this.SellRemoveProgress.fillAmount = (this.sellRemoveTime = 0f);
		this.SellRemoveProgress.color = Color.white;
	}

	public void ShowFullDescription()
	{
		MessageBox messageBoxDesc = this.helpers.ShowFullDescriptionMessage(InventoryParamsHelper.ParseDesc(this._inventoryItem), false);
		messageBoxDesc.GetComponent<EventAction>().ActionCalled += delegate(object obj, EventArgs args)
		{
			messageBoxDesc.Close();
		};
	}

	private ShowDetailedInfo.PreviewRepairData PreviewRepair()
	{
		int num;
		float num2;
		string text;
		PhotonConnectionFactory.Instance.Profile.Inventory.PreviewRepair(this._inventoryItem, out num, out num2, out text);
		return new ShowDetailedInfo.PreviewRepairData(num, num2, text);
	}

	private void WriteAnalytics(string currency, int cost, InventoryItem ii)
	{
		if (currency == "SC")
		{
			if (ii != null)
			{
				AnalyticsFacade.WriteEarnedSilver(ii, cost);
			}
			else
			{
				AnalyticsFacade.WriteSpentSilver("Repair", cost, 1);
			}
		}
		else if (currency == "GC")
		{
			if (ii != null)
			{
				AnalyticsFacade.WriteEarnedGold(ii, cost);
			}
			else
			{
				AnalyticsFacade.WriteSpentGold("Repair", cost, 1);
			}
		}
	}

	private void DestroyItem()
	{
		UIHelper.Waiting(true, null);
		PhotonConnectionFactory.Instance.OnTransactionFailed += this.Instance_OnTransactionFailed;
		PhotonConnectionFactory.Instance.OnItemDestroyed += this.Instance_OnItemDestroyed;
		PhotonConnectionFactory.Instance.DestroyItem(this._inventoryItem);
	}

	private void Instance_OnTransactionFailed(TransactionFailure err)
	{
		this.UnsubscribeDestroyItem(true);
	}

	private void Instance_OnItemDestroyed(InventoryItem ii)
	{
		UIAudioSourceListener.Instance.Successfull();
		this.UnsubscribeDestroyItem(true);
	}

	private void UnsubscribeDestroyItem(bool hideWaiting)
	{
		PhotonConnectionFactory.Instance.OnTransactionFailed -= this.Instance_OnTransactionFailed;
		PhotonConnectionFactory.Instance.OnItemDestroyed -= this.Instance_OnItemDestroyed;
		if (hideWaiting)
		{
			UIHelper.Waiting(false, null);
		}
	}

	protected bool _isShow;

	private bool ignore;

	public float HeightDetailedPanel = 300f;

	public float NonExpandedSize = 80f;

	public float ExpandedSize = 380f;

	public GameObject DetailedPanel;

	public GameObject SellButton;

	public GameObject RemoveButton;

	public Selectable MoveButton;

	public GameObject MessageBoxPrefab;

	public Text Description;

	public Text Params;

	public Image Image;

	private ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Toggle InfoToggle;

	public GameObject RepaireButton;

	public Image SellRemoveProgress;

	private InventoryItem _inventoryItem;

	private MenuHelpers helpers = new MenuHelpers();

	private RectTransform _rect;

	private Button _repaireButton;

	private bool _isActive;

	[SerializeField]
	private LayoutElement _layoutElement;

	public ShowDetailedInfo.ISizeChangesHandler sizeChangesHandler;

	private float sellRemoveTime;

	private class PreviewRepairData
	{
		public PreviewRepairData(int damage, float cost, string currency)
		{
			this.Damage = damage;
			this.Cost = cost;
			this.Currency = currency;
		}

		public int Damage { get; private set; }

		public float Cost { get; private set; }

		public string Currency { get; private set; }
	}

	public interface ISizeChangesHandler
	{
		bool HandleSizeChangeRequest(RectTransform rt, float newSize);

		void OnExpandedStateChanged(RectTransform rt, bool expanded);
	}
}
