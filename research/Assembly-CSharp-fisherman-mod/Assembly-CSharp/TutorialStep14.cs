using System;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStep14 : TutorialStep
{
	protected override void Update()
	{
		base.Update();
		if (this._tutorialGlow == null && TutorialController.CurrentStep == this.Name)
		{
			this.StartAction();
		}
		if (this._buyBtn != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Inventory != null)
		{
			if (PhotonConnectionFactory.Instance.Profile.Inventory.Any((InventoryItem x) => x.ItemSubType == ItemSubTypes.Keepnet || x.ItemSubType == ItemSubTypes.Stringer))
			{
				this._buyBtn.enabled = false;
			}
		}
	}

	public override void DoStartAction()
	{
		base.DoStartAction();
		this.StartAction();
	}

	private void StartAction()
	{
		this.CompleteMessage(false);
		if (this._isEnded)
		{
			return;
		}
		foreach (Selectable selectable in Object.FindObjectsOfType<Selectable>())
		{
			this.Disable<ToggleStateChanges>(selectable);
			this.Disable<ChangeColor>(selectable);
			this.Disable<ChangeColorOther>(selectable);
			selectable.interactable = false;
		}
		ShopMainPageHandler shop = TutorialStep.Shop;
		shop.SortInventoryItems(1426);
		Transform transform = shop.FindBuyBtn(1426);
		transform.GetComponent<Selectable>().interactable = true;
		this._buyBtn = transform.GetComponent<Button>();
		this._buyBtn.onClick.AddListener(delegate
		{
			this._buyBtn.interactable = false;
		});
		this._tutorialGlow = transform.Find("TutorialGlow").gameObject;
		this._tutorialGlow.SetActive(true);
		shop.SetActivePanels(false);
		shop.SetHeaderMenuPaused(true);
		ControlsController.ControlsActions.BlockInput(null);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		this._tutorialGlow.SetActive(false);
		ControlsController.ControlsActions.UnBlockInput();
		this.CompleteMessage(true);
		TutorialStep.Shop.SetHeaderMenuPaused(false);
	}

	protected virtual void Disable<T>(Selectable sel) where T : Behaviour
	{
		T component = sel.gameObject.GetComponent<T>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	protected virtual void CompleteMessage(bool flag)
	{
		if (this.ShopCompleteMessage == null)
		{
			this.ShopCompleteMessage = InfoMessageController.Instance.GetComponent<CompleteMessage>();
		}
		this.ShopCompleteMessage.enabled = flag;
	}

	public CompleteMessage ShopCompleteMessage;

	private GameObject _tutorialGlow;

	private bool _isEnded;

	private Button _buyBtn;

	public const int FishHutXsItemId = 1426;
}
