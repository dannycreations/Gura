using System;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeysInterceptor : MonoBehaviour
{
	private void Start()
	{
		LoadPreviewScene.OnLoad += this.LoadPreviewScene_OnLoad;
	}

	private void OnDestroy()
	{
		LoadPreviewScene.OnLoad -= this.LoadPreviewScene_OnLoad;
	}

	internal void Update()
	{
		if (ControlsController.ControlsActions == null || GameFactory.Player == null)
		{
			return;
		}
		if (ControlsController.ControlsActions.OpenMap.WasClicked)
		{
			KeysHandlerAction.InGameMapHandler();
		}
		else if (GameFactory.Player.Reel != null)
		{
			if (ControlsController.ControlsActions.FastestReelMode.IsPressed)
			{
				GameFactory.Player.Reel.SetHighSpeedMode();
			}
			else
			{
				GameFactory.Player.Reel.SetNormalSpeedMode();
			}
		}
		if ((ControlsController.ControlsActions.LineLeashInc.WasReleased || ControlsController.ControlsActions.LineLeashDec.WasReleased) && !ControlsController.ControlsActions.RodPanel.IsPressed)
		{
			this._startChangeLineLeashedTime = 0f;
		}
		if (KeysInterceptor.IsEnterText || ScreenKeyboard.IsOpened)
		{
			return;
		}
		if (KeysHandlerAction.IsMenuActive && !KeyBindingsList.panelActive && ControlsController.ControlsActions.GetMenuPage() != MainMenuPage.None)
		{
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			if (currentSelectedGameObject == null || currentSelectedGameObject.GetComponent<InputField>() == null)
			{
				KeysHandlerAction.EscapeHandler(this._is3DPreviewActive);
			}
		}
		else if (ControlsController.ControlsActions.Help.WasPressedMandatory && !StaticUserData.IS_IN_TUTORIAL)
		{
			if (!GameFactory.Player.IsReplayMode)
			{
				KeysHandlerAction.HelpHandler(this);
			}
		}
		else if (ControlsController.ControlsActions.HelpGamepad.WasLongPressed)
		{
			KeysHandlerAction.HelpGamepadHandler(true);
		}
		else if (ControlsController.ControlsActions.HelpGamepad.WasReleased)
		{
			KeysHandlerAction.HelpGamepadHandler(false);
		}
		else if ((GameFactory.Player.State == typeof(PlayerIdle) || GameFactory.Player.State == typeof(PlayerIdlePitch)) && !ShowHudElements.Instance.IsAngleMeterActive() && !ControlsController.ControlsActions.RodPanel.IsPressed)
		{
			if (ControlsController.ControlsActions.LineLeashInc.WasPressed)
			{
				this.ChangeLineLeash(true);
			}
			else if (ControlsController.ControlsActions.LineLeashDec.WasPressed)
			{
				this.ChangeLineLeash(false);
			}
			else if (ControlsController.ControlsActions.LineLeashInc.IsPressed)
			{
				this._startChangeLineLeashedTime += Time.deltaTime;
				if (this._startChangeLineLeashedTime > 0.5f)
				{
					this.ChangeLineLeash(true);
				}
			}
			else if (ControlsController.ControlsActions.LineLeashDec.IsPressed)
			{
				this._startChangeLineLeashedTime += Time.deltaTime;
				if (this._startChangeLineLeashedTime > 0.5f)
				{
					this.ChangeLineLeash(false);
				}
			}
		}
	}

	private void ChangeLineLeash(bool increase = true)
	{
		Rod1stBehaviour rod = GameFactory.Player.Rod;
		if (rod == null)
		{
			return;
		}
		Rod rod2 = rod.AssembledRod.Rod;
		Leader leader = rod.AssembledRod.Leader;
		bool flag = leader != null && leader.ItemSubType.IsUncuttableLeader();
		float num = ((!flag) ? 0f : MeasuringSystemManager.LineLeashLength(leader.LeaderLength));
		RodTemplate rodTemplate = PhotonConnectionFactory.Instance.Profile.Inventory.GetRodTemplate(rod2);
		if (KeysInterceptor._pondHelpers.PondControllerList.Game3DPond.activeSelf && rod2.Storage == StoragePlaces.Hands && rodTemplate == RodTemplate.Float)
		{
			int num2 = Mathf.RoundToInt(num + MeasuringSystemManager.LineLeashLength(rod2.LeaderLength)) + ((!increase) ? (-1) : 1);
			if (((float)num2 >= num + MeasuringSystemManager.LineLeashMinLength || increase) && ((float)num2 <= num + MeasuringSystemManager.LineLeashMaxLength || !increase))
			{
				PhotonConnectionFactory.Instance.Profile.Inventory.SetLeaderLength(rod2, MeasuringSystemManager.LineLeashBackLength((float)num2 - num));
				PhotonConnectionFactory.Instance.SetLeaderLength(rod2);
				GameFactory.Message.ShowLeashLineChanged(num2, null);
			}
		}
	}

	private void LoadPreviewScene_OnLoad(bool isActive)
	{
		this._is3DPreviewActive = isActive;
	}

	private static PondHelpers _pondHelpers = new PondHelpers();

	public static bool IsEnterText = false;

	private float _startChangeLineLeashedTime;

	private bool _is3DPreviewActive;
}
