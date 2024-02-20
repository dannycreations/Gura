using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialStep16 : TutorialStep
{
	private void InitMap()
	{
		this.MapPanel = ShowLocationInfo.Instance.GetComponentInChildren<SetLocationsOnGlobalMap>();
		this.FishingButton = ShowLocationInfo.Instance.GetComponentsInChildren<HintElementId>().FirstOrDefault((HintElementId x) => x.GetElementId() == "LM_MAP_GoFishing").GetComponent<Button>();
		this.PondCanvas = ShowLocationInfo.Instance.GetComponentInParent<ActivityState>();
	}

	public void EnableArrow()
	{
		Debug.Log("Enable arrow called: " + Time.time);
		if (this.FishingButton.interactable)
		{
			return;
		}
		if (this.MapPanel == null)
		{
			this.InitMap();
		}
		int childCount = this.MapPanel.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = this.MapPanel.transform.GetChild(i);
			LocationPin component = child.GetComponent<LocationPin>();
			if (child.gameObject.activeInHierarchy && component != null && component.RLDA.CurrentLocationBrief.LocationId == 10121)
			{
				this._arrow = component.TutorialArrow;
				this._arrow.SetActive(true);
				EventSystem.current.SetSelectedGameObject(child.gameObject);
				component.ColorTransitions.Disable();
			}
		}
		Debug.Log("Enable arrow ends: " + Time.time);
	}

	public override void DoStartAction()
	{
		Debug.Log(" DoStartAction called: " + Time.time);
		base.DoStartAction();
		if (this.MapPanel == null)
		{
			this.InitMap();
		}
		int childCount = this.MapPanel.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = this.MapPanel.transform.GetChild(i);
			LocationPin component = child.GetComponent<LocationPin>();
			if (child.gameObject.activeInHierarchy && !(component == null))
			{
				component.DoubleClick.enabled = false;
				if (component.RLDA.CurrentLocationBrief.LocationId != 10121)
				{
					component.DoubleClick.enabled = false;
					component.Toggle.interactable = false;
					component.ColorTransitions.Disable();
				}
				else
				{
					this.pondToggle = component.Toggle;
					this.pondToggle.interactable = true;
				}
			}
		}
		Selectable[] array = Object.FindObjectsOfType<Selectable>();
		for (int j = 0; j < array.Length; j++)
		{
			if (!this.SelectablesForSkip.Contains(array[j]))
			{
				if (!(array[j] == this.pondToggle))
				{
					ToggleStateChanges component2 = array[j].gameObject.GetComponent<ToggleStateChanges>();
					ChangeColor changeColor = array[j].gameObject.GetComponent<ChangeColor>();
					if (component2 != null)
					{
						component2.enabled = false;
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
		base.Invoke("EnableArrow", 0.75f);
		ControlsController.ControlsActions.BlockInput(null);
		this.FishingButton.interactable = false;
		Debug.Log("Do Start Action ends: " + Time.time);
	}

	public override void DoEndAction()
	{
		Debug.Log(" DoEnd Action called: " + Time.time);
		base.DoEndAction();
		if (this.MapPanel == null)
		{
			this.InitMap();
		}
		int childCount = this.MapPanel.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = this.MapPanel.transform.GetChild(i);
			Toggle component = child.GetComponent<Toggle>();
			if (child.gameObject.activeInHierarchy && component != null)
			{
				component.interactable = false;
			}
		}
		if (this.pondToggle != null && this.pondToggle.interactable)
		{
			this.pondToggle.interactable = false;
		}
		if (this._arrow != null)
		{
			this._arrow.SetActive(false);
		}
		ControlsController.ControlsActions.UnBlockInput();
		this.FishingButton.interactable = true;
		Debug.Log(" DoEnd Action ends: " + Time.time);
	}

	public SetLocationsOnGlobalMap MapPanel;

	public Button FishingButton;

	public ActivityState PondCanvas;

	private GameObject _arrow;

	private Toggle pondToggle;
}
