using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialStep17 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		if (this.GlowPanel == null)
		{
			HintElementId hintElementId = ShowLocationInfo.Instance.GetComponentsInChildren<HintElementId>().FirstOrDefault((HintElementId x) => x.GetElementId() == "LM_MAP_GoFishing");
			if (hintElementId != null && EventSystem.current != null && !EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(hintElementId.gameObject);
			}
			if (hintElementId != null)
			{
				this.GlowPanel = hintElementId.transform.Find("Panel").gameObject;
			}
		}
		this.GlowPanel.SetActive(true);
		this.GlowPanel.transform.parent.SetAsLastSibling();
		ControlsController.ControlsActions.BlockInput(null);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
		this.GlowPanel.SetActive(false);
		Toggle[] componentsInChildren = base.MenuCanvas.GetComponentsInChildren<Toggle>();
		foreach (Toggle toggle in componentsInChildren)
		{
			toggle.interactable = true;
		}
	}

	public GameObject GlowPanel;
}
