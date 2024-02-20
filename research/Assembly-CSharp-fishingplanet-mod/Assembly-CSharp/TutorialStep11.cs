using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStep11 : TutorialStep
{
	protected override void Update()
	{
		base.Update();
		if (TutorialController.CurrentStep == this.Name)
		{
			if (this._tutorialGlow == null)
			{
				this.StartAction();
			}
			else
			{
				InventoryItemComponent component = this._tutorialGlow.transform.parent.GetComponent<InventoryItemComponent>();
				if (component != null && component.InventoryItem.ItemId == 434)
				{
					UINavigation.SetSelectedGameObject(this._tutorialGlow.transform.parent.parent.gameObject);
				}
				else
				{
					UINavigation.SetSelectedGameObject(null);
				}
			}
		}
	}

	public override void DoStartAction()
	{
		base.DoStartAction();
		this.StartAction();
		UINavigation[] array = Object.FindObjectsOfType<UINavigation>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
	}

	private void StartAction()
	{
		if (this._isEnded)
		{
			return;
		}
		InventoryInit component = MenuHelpers.Instance.MenuPrefabsList.inventoryForm.GetComponent<InventoryInit>();
		if (this.Content == null)
		{
			this.Content = component.TutorialBaitContent;
		}
		if (this.TerminalContent == null)
		{
			this.TerminalContent = component.TutorialTerminalContent;
		}
		if (this.LineContent == null)
		{
			this.LineContent = component.TutorialLineContent;
		}
		if (this.BlackoutParent == null)
		{
			this.BlackoutParent = component.TutorialBlackoutParent;
		}
		if (this.DollDisableItems[0] == null)
		{
			this.DollDisableItems = new List<DragMe>(component.TutorialDollDisableItems);
		}
		if (this.BaitGlowPanel == null)
		{
			this.BaitGlowPanel = component.TutorialBaitGlowPanel;
		}
		this.CreateBlackout();
		component.SRIA.UiNavigation.enabled = false;
		GameObject gameObject = null;
		for (int i = 0; i < this.Content.transform.childCount; i++)
		{
			InventoryItemComponent component2 = this.Content.transform.GetChild(i).GetComponent<InventoryItemComponent>();
			DragMe component3 = this.Content.transform.GetChild(i).GetComponent<DragMe>();
			if (component2 != null && component2.InventoryItem.ItemId == 434)
			{
				HotkeyPressRedirect component4 = this.Content.transform.GetChild(i).GetComponent<HotkeyPressRedirect>();
				component4.StartListenForHotkeys();
				gameObject = this.Content.transform.GetChild(i).Find("Equip").gameObject;
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					this._tutorialGlow.SetActive(false);
					CanvasGroup component6 = this.Content.GetComponent<CanvasGroup>();
					if (component6 != null)
					{
						component6.interactable = false;
					}
				});
				this._tutorialGlow = this.Content.transform.GetChild(i).Find("TutorialGlow").gameObject;
				this._tutorialGlow.SetActive(true);
				this._bckpParent = this._tutorialGlow.transform.parent.parent as RectTransform;
				this._tutorialGlow.transform.parent.SetParent(this.BlackoutParent, true);
				this._tutorialGlow.transform.parent.SetAsLastSibling();
				this._glowBackPanel = this.CreateImage(this._tutorialGlow.transform.parent, this._tutorialGlow.GetComponent<RectTransform>(), new Color32(50, 51, 56, byte.MaxValue), true);
			}
			else if (component3 != null)
			{
				component3.enabled = false;
			}
		}
		foreach (DragMe dragMe in this.DollDisableItems)
		{
			dragMe.enabled = false;
		}
		this._glowBackBait = this.CreateImage(this.BaitGlowPanel.transform.parent, this.BaitGlowPanel.GetComponent<RectTransform>(), new Color32(50, 51, 56, byte.MaxValue), true);
		this._baitParent = this.BaitGlowPanel.transform.parent.transform.parent.gameObject;
		this._baitParentSizeSetter = this._baitParent.GetComponent<ContentPreferredSizeSetter>();
		if (this._baitParentSizeSetter != null)
		{
			this._baitParentSizeSetter.SetHeightOverride(new float?((this.BaitGlowPanel.transform.parent as RectTransform).rect.height));
		}
		this.BaitGlowPanel.SetActive(true);
		this.BaitGlowPanel.transform.parent.SetParent(this.BlackoutParent, true);
		this.BaitGlowPanel.transform.parent.SetAsLastSibling();
		this.BaitGlowPanel.transform.parent.GetComponentInChildren<DeEquip>().enabled = false;
		Selectable[] array = Object.FindObjectsOfType<Selectable>();
		for (int j = 0; j < array.Length; j++)
		{
			if (!this.SelectablesForSkip.Contains(array[j]))
			{
				if (array[j].gameObject != this._tutorialGlow.transform.parent && array[j].gameObject != this.BaitGlowPanel && array[j].gameObject != gameObject)
				{
					ToggleStateChanges component5 = array[j].gameObject.GetComponent<ToggleStateChanges>();
					ChangeColor changeColor = array[j].gameObject.GetComponent<ChangeColor>();
					if (component5 != null)
					{
						component5.enabled = false;
					}
					if (changeColor != null)
					{
						changeColor.enabled = false;
					}
					changeColor = array[j].gameObject.GetComponent<ChangeColorOther>();
					if (changeColor != null)
					{
						changeColor.enabled = false;
					}
					array[j].interactable = false;
				}
			}
		}
		ControlsController.ControlsActions.BlockInput(null);
	}

	public override void DoEndAction()
	{
		CanvasGroup component = this.Content.GetComponent<CanvasGroup>();
		if (component != null)
		{
			component.interactable = false;
		}
		base.DoEndAction();
		this._isEnded = true;
		Object.Destroy(this._glowBackBait);
		Object.Destroy(this._glowBackPanel);
		this._tutorialGlow.transform.parent.SetParent(this._bckpParent);
		this._tutorialGlow.SetActive(false);
		this.BaitGlowPanel.transform.parent.SetParent(this._baitParent.transform, true);
		if (this._baitParentSizeSetter != null)
		{
			this._baitParentSizeSetter.SetHeightOverride(null);
		}
		this.BaitGlowPanel.SetActive(false);
		ControlsController.ControlsActions.UnBlockInput();
		UINavigation.SetSelectedGameObject(null);
	}

	protected override void OnInventoryUpdated()
	{
		if (TutorialController.CurrentStep != this.Name)
		{
			return;
		}
		for (int i = 0; i < this.Content.transform.childCount; i++)
		{
			DragMe component = this.Content.transform.GetChild(i).GetComponent<DragMe>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	private void CreateBlackout()
	{
		if (this.BlackoutParent == null)
		{
			this.BlackoutParent = MenuHelpers.Instance.MenuPrefabsList.inventoryForm.transform.GetChild(0);
		}
		Image image = new GameObject("BlackoutMask").AddComponent<Image>();
		image.sprite = this.BlackoutImage;
		image.raycastTarget = true;
		image.color = new Color32(0, 0, 0, 198);
		this._blackoutTransform = image.GetComponent<RectTransform>();
		this._blackoutTransform.SetParent(this.BlackoutParent);
		this._blackoutTransform.SetAsLastSibling();
		this._blackoutTransform.anchorMin = Vector2.zero;
		this._blackoutTransform.anchorMax = Vector2.one;
		this._blackoutTransform.pivot = new Vector2(0.5f, 0.5f);
		this._blackoutTransform.offsetMin = Vector2.zero;
		this._blackoutTransform.offsetMax = Vector2.zero;
		this._blackoutTransform.localPosition = Vector3.zero;
		this._blackoutTransform.localScale = Vector3.one * 2f;
		this.BlackoutParent.parent.GetComponent<Canvas>().sortingOrder = 4;
	}

	private Image CreateImage(Transform parent, RectTransform copyFrom, Color32 color, bool setSiblingFirst = true)
	{
		Image image = new GameObject("Blackout").AddComponent<Image>();
		image.color = color;
		RectTransform component = image.GetComponent<RectTransform>();
		component.SetParent(parent);
		if (setSiblingFirst)
		{
			component.SetAsFirstSibling();
		}
		else
		{
			component.SetAsLastSibling();
		}
		component.localPosition = copyFrom.position;
		component.localScale = copyFrom.localScale;
		component.anchorMin = copyFrom.anchorMin;
		component.anchorMax = copyFrom.anchorMax;
		component.pivot = copyFrom.pivot;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		return image;
	}

	public GameObject Content;

	public GameObject TerminalContent;

	public GameObject LineContent;

	public List<DragMe> DollDisableItems;

	public GameObject BaitGlowPanel;

	public GameObject TopMenuPanel;

	public GameObject TopMenuLeftPanel;

	public GameObject TopMenuRightPanel;

	public Transform BlackoutParent;

	public Sprite BlackoutImage;

	private GameObject _tutorialGlow;

	private GameObject _baitParent;

	private Image _glowBackBait;

	private Image _glowBackPanel;

	private bool _isEnded;

	private RectTransform _blackoutTransform;

	private RectTransform _bckpParent;

	private ContentPreferredSizeSetter _baitParentSizeSetter;
}
