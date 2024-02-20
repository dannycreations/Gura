using System;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class FirstScreenInit : MonoBehaviour
{
	private void Awake()
	{
		if (this._copyrightText != null)
		{
			this._copyrightText.text = string.Format("© 2013-{0} Fishing Planet LLC. All rights reserved.", DateTime.Now.Year);
		}
	}

	private void OnEnable()
	{
		Time.timeScale = 1f;
		this._waitTimeout = 0f;
		this.state = FirstScreenInit.SignInState.Engaging;
		this.IconOfPress.gameObject.SetActive(true);
		GameFactory.Clear(true);
		this._isXboxNetworkAvailability = true;
		this.PressButton.gameObject.SetActive(true);
		this.IconOfPress.gameObject.SetActive(false);
		UiEffectsUtil.TextPulsation(this.IconOfPress.gameObject, Color.white, 2f, 0.2f, true);
		UiEffectsUtil.TextPulsation(this.PressText.gameObject, Color.white, 2f, 0.2f, true);
		UiEffectsUtil.ImagePulsation(this.PressButton.gameObject, Color.white, 2f, 0.2f, true);
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
		this._waitTimeout += Time.deltaTime;
		if (this.state == FirstScreenInit.SignInState.Engaging && this._waitTimeout > 0.5f && this._isXboxNetworkAvailability && (FirstScreenInit.autoSkip || InputManager.AnyKeyIsPressed || ControlsController.ControlsActions.GetMouseButtonDownMandatory(0) || ControlsController.ControlsActions.GetMouseButtonDownMandatory(1) || InputManager.ActiveDevice.GetControl(InputControlType.Action1).WasPressed || (GlobalConsts.IsDebugLoading && this._waitTimeout > 2f)))
		{
			if (!SteamManager.Initialized)
			{
				if (!this._isSteamNotRunShowing)
				{
					this._isSteamNotRunShowing = true;
					LogHelper.Error("Steam not initialized !", new object[0]);
					UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("SteamNotRun"), true, delegate
					{
						this._isSteamNotRunShowing = false;
					}, true);
				}
				return;
			}
			if (FirstScreenInit.autoSkip)
			{
				FirstScreenInit.autoSkip = false;
			}
			this.state = FirstScreenInit.SignInState.RequestingUser;
			SceneController.CallAction(ScenesList.Empty, SceneStatuses.GoToStart, this, null);
		}
	}

	[SerializeField]
	private ActiveUserIndication _activeUserIndication;

	[SerializeField]
	private Text _copyrightText;

	public Text IconOfPress;

	public Text PressText;

	public Button PressButton;

	public Image Logo;

	public Sprite RetailLogoSprite;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;

	private float _waitTimeout;

	public static bool autoSkip;

	private bool _isXboxNetworkAvailability;

	private bool _isSteamNotRunShowing;

	private FirstScreenInit.SignInState state;

	private enum SignInState
	{
		Engaging,
		RequestingUser
	}
}
