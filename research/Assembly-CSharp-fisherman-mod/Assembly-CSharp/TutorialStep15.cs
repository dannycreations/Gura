using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialStep15 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		GameFactory.Message.IgnoreMessages = false;
		base.MapMenuGlow.SetActive(true);
		base.MenuCanvas.sortingOrder = 2;
		base.MapToggle.interactable = true;
		ControlsController.ControlsActions.BlockInput(null);
		Selectable[] array = Object.FindObjectsOfType<Selectable>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!this.SelectablesForSkip.Contains(array[i]))
			{
				ToggleStateChanges component = array[i].gameObject.GetComponent<ToggleStateChanges>();
				ChangeColor changeColor = array[i].gameObject.GetComponent<ChangeColor>();
				if (component != null)
				{
					component.enabled = false;
				}
				if (changeColor != null)
				{
					changeColor.enabled = false;
				}
				changeColor = array[i].gameObject.GetComponent<ChangeColorOther>();
				if (changeColor != null)
				{
					changeColor.enabled = false;
				}
				array[i].interactable = false;
			}
		}
		base.MapToggle.interactable = true;
		EventSystem.current.SetSelectedGameObject(base.MapToggle.gameObject);
		ToggleStateChanges component2 = base.MapToggle.gameObject.GetComponent<ToggleStateChanges>();
		if (component2 != null)
		{
			component2.enabled = true;
		}
	}

	public override void StepUpdate()
	{
		base.StepUpdate();
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != base.MapToggle.gameObject)
		{
			UINavigation.SetSelectedGameObject(base.MapToggle.gameObject);
		}
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		if (ShowLocationInfo.Instance != null)
		{
			ShowLocationInfo.Instance.GetComponentsInChildren<HintElementId>().FirstOrDefault((HintElementId x) => x.GetElementId() == "LM_MAP_GoFishing").GetComponent<Button>()
				.interactable = false;
		}
		base.MapMenuGlow.SetActive(false);
		base.MenuCanvas.sortingOrder = 0;
		base.MapToggle.interactable = false;
		ControlsController.ControlsActions.UnBlockInput();
	}
}
