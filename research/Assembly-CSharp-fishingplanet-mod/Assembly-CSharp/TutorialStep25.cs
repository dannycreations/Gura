using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialStep25 : TutorialStep
{
	protected override void Start()
	{
		this.ShortControlIcon = (this.BigControlIcon = InputControlType.Action2);
		PhotonConnectionFactory.Instance.OnTutorialFinished += this.OnTutorialFinished;
		if (this.MapPanel == null && ShowLocationInfo.Instance != null)
		{
			this.InitMap();
		}
		if (this.BackToLobbyClickInstance != null && !this.BackToLobbySubscribed)
		{
			this.BackToLobbySubscribed = true;
			this.BackToLobbyClickInstance.ShowingMessageBox += this.BackToLobbyClickInstance_ShowingMessageBox;
		}
	}

	protected override void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnTutorialFinished -= this.OnTutorialFinished;
		if (this.BackToLobbySubscribed)
		{
			this.BackToLobbyClickInstance.ShowingMessageBox -= this.BackToLobbyClickInstance_ShowingMessageBox;
			this.BackToLobbySubscribed = false;
		}
	}

	private void BackToLobbyClickInstance_ShowingMessageBox(object sender, EventArgs e)
	{
		base.MapMenuGlow.SetActive(false);
		if (this.BackToLobbySubscribed)
		{
			this.BackToLobbyClickInstance.ShowingMessageBox -= this.BackToLobbyClickInstance_ShowingMessageBox;
			this.BackToLobbySubscribed = false;
		}
	}

	private void InitMap()
	{
		this.BackToLobbyClickInstance = ShowLocationInfo.Instance.GetComponentsInChildren<HintElementId>().FirstOrDefault((HintElementId x) => x.GetElementId() == "LM_MAP_Leave").GetComponent<BackToLobbyClick>();
		this._btnLeaveGlow = this.BackToLobbyClickInstance.transform.Find("Panel").gameObject;
		this.MapPanel = ShowLocationInfo.Instance.GetComponentInChildren<SetLocationsOnGlobalMap>().MapTexture.transform;
		this.FishingButton = ShowLocationInfo.Instance.GetComponentsInChildren<HintElementId>().FirstOrDefault((HintElementId x) => x.GetElementId() == "LM_MAP_GoFishing").GetComponent<Button>();
		this._btnLeave = ShowLocationInfo.Instance.GetComponentsInChildren<HintElementId>().FirstOrDefault((HintElementId x) => x.GetElementId() == "LM_MAP_Leave").GetComponent<Button>();
	}

	public override void DoStartAction()
	{
		base.DoStartAction();
		if (this.MapPanel == null)
		{
			this.InitMap();
		}
		this.FishingButton.interactable = false;
		this.SetInteractable(this.MapPanel, false, false);
		Selectable[] componentsInChildren = this.MapPanel.GetComponentsInChildren<Selectable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!this.SelectablesForSkip.Contains(componentsInChildren[i]))
			{
				ToggleStateChanges component = componentsInChildren[i].gameObject.GetComponent<ToggleStateChanges>();
				ChangeColor changeColor = componentsInChildren[i].gameObject.GetComponent<ChangeColor>();
				if (component != null)
				{
					component.enabled = false;
				}
				if (changeColor != null)
				{
					changeColor.enabled = false;
				}
				changeColor = componentsInChildren[i].gameObject.GetComponent<ChangeColorOther>();
				if (changeColor != null)
				{
					changeColor.enabled = false;
				}
				componentsInChildren[i].interactable = false;
			}
		}
		DashboardTabSetter.Instance.SetAllTogglesInteractable(false);
		base.MapToggle.interactable = true;
		base.MenuCanvas.sortingOrder = 2;
		this._btnLeave.interactable = true;
		EventSystem.current.SetSelectedGameObject(this._btnLeave.gameObject);
		base.MenuCanvas.GetComponent<ActivityState>().OnStart += this.LocalDashboard_OnStart;
		ControlsController.ControlsActions.BlockInput(null);
	}

	private void SetInteractable(Transform t, bool flag, bool buttonAlso)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);
			Toggle component = child.GetComponent<Toggle>();
			if (component != null)
			{
				component.interactable = flag;
			}
			if (buttonAlso)
			{
				Button component2 = child.GetComponent<Button>();
				if (component2 != null)
				{
					component2.interactable = flag;
				}
			}
		}
	}

	private void LocalDashboard_OnStart()
	{
		base.Invoke("InvokeLocalMapMenuGlow", 0.5f);
	}

	private void InvokeLocalMapMenuGlow()
	{
		this._btnLeaveGlow.SetActive(true);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		if (base.MapToggle == null || this.MapPanel == null)
		{
			this.InitMap();
		}
		if (this.BackToLobbySubscribed)
		{
			this.BackToLobbyClickInstance.ShowingMessageBox -= this.BackToLobbyClickInstance_ShowingMessageBox;
			this.BackToLobbySubscribed = false;
		}
		this._btnLeaveGlow.SetActive(false);
		base.MenuCanvas.sortingOrder = 0;
		this._btnLeave.interactable = false;
		ControlsController.ControlsActions.UnBlockInput();
	}

	public override void StepUpdate()
	{
		if (this._btnLeave == null || this.MapPanel == null)
		{
			this.InitMap();
		}
		if (MessageBoxList.Instance.currentMessage != null)
		{
			this.TimeShowingFullMessage = 0f;
		}
		if (this.BackToLobbyClickInstance.MessageBox != null)
		{
			this.BackToLobbyClickInstance.MessageBox.GetComponent<MessageBox>().cancelButtonText.transform.parent.gameObject.GetComponent<Button>().interactable = false;
		}
	}

	private void OnTutorialFinished()
	{
		TravelSlides.Count = -1;
		ControlsController.ControlsActions.ResetAndUnblock();
		PhotonConnectionFactory.Instance.RequestProfile();
	}

	public BackToLobbyClick BackToLobbyClickInstance;

	private bool BackToLobbySubscribed;

	public Transform MapPanel;

	public Button FishingButton;

	private MenuHelpers _helper = new MenuHelpers();

	private Button _btnLeave;

	private GameObject _btnLeaveGlow;
}
