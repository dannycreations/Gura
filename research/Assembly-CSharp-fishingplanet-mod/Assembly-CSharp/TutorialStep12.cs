using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialStep12 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		if (this.VisibleTransfrom == null)
		{
			this.VisibleTransfrom = MenuHelpers.Instance.MenuPrefabsList.inventoryForm.transform.GetChild(0);
		}
		this.OnInventoryUpdated();
		base.ShopMenuGlow.SetActive(true);
		this._shopTransform = base.ShopToggle.transform.parent;
		this._blackoutImage = this.CreateImage(base.ShopMenuGlow.transform.parent, base.ShopMenuGlow.GetComponent<RectTransform>(), new Color32(60, 60, 60, 240), true);
		base.ShopToggle.transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
		base.ShopToggle.transform.SetParent(this.VisibleTransfrom);
		base.ShopToggle.transform.SetAsLastSibling();
		base.MenuCanvas.sortingOrder = 2;
		base.ShopToggle.interactable = true;
		UINavigation.SetSelectedGameObject(base.ShopToggle.gameObject);
		ControlsController.ControlsActions.BlockInput(null);
		TutorialStep.Shop.SetActiveButtons(false);
		this._initPosition = base.ShopToggle.transform.position;
	}

	public override void StepUpdate()
	{
		base.StepUpdate();
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != base.ShopToggle.gameObject)
		{
			UINavigation.SetSelectedGameObject(base.ShopToggle.gameObject);
		}
		if (this._initPosition != base.ShopToggle.transform.position && this._shopTransform != null)
		{
			Object.Destroy(this._blackoutImage);
			base.ShopToggle.transform.position = this._initPosition;
			base.ShopToggle.transform.SetParent(this._shopTransform, true);
			base.ShopToggle.transform.localScale = Vector3.one;
		}
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		Object.Destroy(this._blackoutImage);
		base.ShopToggle.transform.position = this._initPosition;
		base.ShopToggle.transform.SetParent(this._shopTransform, true);
		base.ShopToggle.transform.localScale = Vector3.one;
		base.ShopMenuGlow.SetActive(false);
		base.MenuCanvas.sortingOrder = 0;
		base.ShopToggle.isOn = false;
		base.ShopToggle.interactable = false;
		ControlsController.ControlsActions.UnBlockInput();
	}

	protected override void OnInventoryUpdated()
	{
		if (TutorialController.CurrentStep != this.Name)
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
			for (int i = 0; i < this.Content.transform.childCount; i++)
			{
				DragMe component2 = this.Content.transform.GetChild(i).GetComponent<DragMe>();
				if (component2 != null)
				{
					component2.enabled = false;
				}
			}
		}
		Selectable[] array = Object.FindObjectsOfType<Selectable>();
		for (int j = 0; j < array.Length; j++)
		{
			if (!this.SelectablesForSkip.Contains(array[j]))
			{
				if (array[j].gameObject != base.ShopToggle.transform.parent)
				{
					ToggleStateChanges component3 = array[j].gameObject.GetComponent<ToggleStateChanges>();
					ChangeColor changeColor = array[j].gameObject.GetComponent<ChangeColor>();
					if (component3 != null)
					{
						component3.enabled = false;
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
		component.offsetMin = new Vector2(4f, 5f);
		component.offsetMax = new Vector2(-4f, -5f);
		return image;
	}

	public GameObject Content;

	public GameObject TerminalContent;

	public GameObject LineContent;

	public Transform VisibleTransfrom;

	private Transform _shopTransform;

	private Image _blackoutImage;

	private Vector3 _initPosition;
}
