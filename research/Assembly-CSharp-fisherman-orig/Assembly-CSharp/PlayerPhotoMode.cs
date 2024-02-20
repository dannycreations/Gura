using System;
using System.Collections.Generic;
using ObjectModel;
using TPM;
using UnityEngine;

public class PlayerPhotoMode : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
	}

	protected override void onEnter()
	{
		Transform cam = base.Player.CameraController.Camera.transform;
		this._lastCameraParent = cam.parent;
		this._lastCameraPosition = cam.localPosition;
		this._lastCameraRotation = cam.localRotation;
		ShowHudElements.Instance.SetCatchedFishWindowActivity(false);
		CursorManager.HideCursor();
		base.Player.SetLabelsVisibility(false);
		base.Player.IsPhotoModeRequested = false;
		this._baker = PlayerController.GetBaker();
		this._modeFsm = new TFSM<PlayerPhotoMode.PhotoModeState, PlayerPhotoMode.PhotoModeSignal>("PhotoMode", PlayerPhotoMode.PhotoModeState.None);
		this._modeFsm.AddState(PlayerPhotoMode.PhotoModeState.None, null, null, null, 0f, PlayerPhotoMode.PhotoModeSignal.Start, null, null);
		this._modeFsm.AddState(PlayerPhotoMode.PhotoModeState.InitialFadeIn, delegate
		{
			BlackScreenHandler.Show(false, null);
			this._wasMovementEnabled = this.Player.FPController.enabled;
			this.Player.FPController.enabled = false;
		}, null, null, 0.5f, PlayerPhotoMode.PhotoModeSignal.ReadyToCreatePlayer, null, null);
		this._modeFsm.AddState(PlayerPhotoMode.PhotoModeState.PlayerCreating, delegate
		{
			this._modelRoot = this.Player.Create3DPlayerFromProfile("PhotoModeModel", "p3dv", new MeshBakersController.ModelReadyDelegate(this.BakerOnModelCreated));
			cam.parent = null;
		}, null, null, 0f, PlayerPhotoMode.PhotoModeSignal.Start, null, null);
		this._modeFsm.AddState(PlayerPhotoMode.PhotoModeState.Idle, null, null, null, 0f, PlayerPhotoMode.PhotoModeSignal.Start, null, null);
		this._modeFsm.AddState(PlayerPhotoMode.PhotoModeState.Leave, delegate
		{
			BlackScreenHandler.Show(false, null);
			this._unbakeAt = Time.time + 0.5f;
		}, null, null, 0f, PlayerPhotoMode.PhotoModeSignal.Start, null, null);
		this._modeFsm.AddTransition(PlayerPhotoMode.PhotoModeState.None, PlayerPhotoMode.PhotoModeState.InitialFadeIn, PlayerPhotoMode.PhotoModeSignal.Start, null);
		this._modeFsm.AddTransition(PlayerPhotoMode.PhotoModeState.InitialFadeIn, PlayerPhotoMode.PhotoModeState.PlayerCreating, PlayerPhotoMode.PhotoModeSignal.ReadyToCreatePlayer, null);
		this._modeFsm.AddTransition(PlayerPhotoMode.PhotoModeState.PlayerCreating, PlayerPhotoMode.PhotoModeState.Idle, PlayerPhotoMode.PhotoModeSignal.PlayerReady, null);
		this._modeFsm.AddTransition(PlayerPhotoMode.PhotoModeState.Idle, PlayerPhotoMode.PhotoModeState.Leave, PlayerPhotoMode.PhotoModeSignal.Finishing, null);
		this._modeFsm.AddTransition(PlayerPhotoMode.PhotoModeState.Leave, PlayerPhotoMode.PhotoModeState.None, PlayerPhotoMode.PhotoModeSignal.Finished, null);
		this._modeFsm.SendSignal(PlayerPhotoMode.PhotoModeSignal.Start);
	}

	private void BakerOnModelCreated(string objId, Dictionary<CustomizedParts, TPMModelLayerSettings> parts, TPMCharacterModel modelSettings, SkinnedMeshRenderer bakedRenderer)
	{
		if (objId == "p3dv")
		{
			ShowHudElements.Instance.gameObject.SetActive(false);
			this._baker.EModelReady -= this.BakerOnModelCreated;
			GameObject gameObject = Object.Instantiate<GameObject>(TPMCharacterCustomization.Instance.MainPlayerPrefab);
			gameObject.name = this._modelRoot.name;
			this._modelRoot.transform.parent = gameObject.transform;
			this._modelRoot = gameObject;
			HandsViewController component = gameObject.GetComponent<HandsViewController>();
			Player player = new Player
			{
				TpmCharacterModel = modelSettings
			};
			component.Initialize(null, player, parts, bakedRenderer, false, true, base.Player.CameraController.Camera.transform, false);
			base.Player.Set3DViewMode();
			this._cantSitdown = base.Player.SetupPhotoMode(component);
			this._nextUpdateAt = Time.time + 0.1f;
			this._hideBlackScreenAfterBakingAt = Time.time + 1f;
			this._modeFsm.SendSignal(PlayerPhotoMode.PhotoModeSignal.PlayerReady);
		}
	}

	private void BakerOnModelUnbaked(string objID)
	{
		if (objID == "p3dv")
		{
			this._baker.EModelUnbaked -= this.BakerOnModelUnbaked;
			ShowHudElements.Instance.HidePhotomodeHUD();
			ShowHudElements.Instance.gameObject.SetActive(true);
			Object.Destroy(this._modelRoot);
			base.Player.Switch3DViewVisibility(true);
			base.Player.FPController.enabled = this._wasMovementEnabled;
			BlackScreenHandler.HideFast();
			this._modeFsm.SendSignal(PlayerPhotoMode.PhotoModeSignal.Finished);
			Transform transform = base.Player.CameraController.Camera.transform;
			transform.parent = this._lastCameraParent;
			transform.localRotation = Quaternion.identity;
			transform.localPosition = this._lastCameraPosition;
		}
	}

	protected override Type onUpdate()
	{
		this._modeFsm.Update();
		PlayerPhotoMode.PhotoModeState curStateID = this._modeFsm.CurStateID;
		if (curStateID != PlayerPhotoMode.PhotoModeState.Idle)
		{
			if (curStateID != PlayerPhotoMode.PhotoModeState.Leave)
			{
				if (curStateID == PlayerPhotoMode.PhotoModeState.None)
				{
					base.Player.LeavePhotoMode();
					return (!base.Player.Tackle.Fish.IsBig) ? typeof(PlayerShowFishLineIdle) : typeof(PlayerShowFishIdle);
				}
			}
			else if (this._unbakeAt > 0f && this._unbakeAt < Time.time)
			{
				this._unbakeAt = -1f;
				this._baker.EModelUnbaked += this.BakerOnModelUnbaked;
				this._baker.AskToDeleteModelParts("p3dv");
			}
		}
		else
		{
			if (ControlsController.ControlsActions.FlashlightVisibility.WasPressed)
			{
				base.Player.SwitchPhotoModeFlashlight();
			}
			if (ControlsController.ControlsActions.PHMCloseUI.WasReleased || ControlsController.ControlsActions.UICancel)
			{
				this._modeFsm.SendSignal(PlayerPhotoMode.PhotoModeSignal.Finishing);
			}
			else
			{
				if (this._hideBlackScreenAfterBakingAt > 0f && this._hideBlackScreenAfterBakingAt < Time.time)
				{
					ShowHudElements.Instance.ShowPhotomodeHUD(this._cantSitdown);
					BlackScreenHandler.Hide();
					this._hideBlackScreenAfterBakingAt = -1f;
				}
				if (Time.time > this._nextUpdateAt)
				{
					this._nextUpdateAt = Time.time + 0.1f;
					base.Player.UpdatePhotoModeRequest();
				}
			}
		}
		return null;
	}

	private const string MODEL_NAME = "PhotoModeModel";

	private const string MODEL_ID = "p3dv";

	private const float HIDE_BLACK_SCREEN_AFTER_BAKING_DELAY = 1f;

	private TFSM<PlayerPhotoMode.PhotoModeState, PlayerPhotoMode.PhotoModeSignal> _modeFsm;

	private float _nextUpdateAt = -1f;

	private float _hideBlackScreenAfterBakingAt = -1f;

	private MeshBakersController _baker;

	private GameObject _modelRoot;

	private float _unbakeAt = -1f;

	private bool _wasMovementEnabled;

	private bool _cantSitdown;

	private Transform _lastCameraParent;

	private Vector3 _lastCameraPosition;

	private Quaternion _lastCameraRotation;

	private enum PhotoModeState
	{
		None,
		InitialFadeIn,
		PlayerCreating,
		Idle,
		Leave
	}

	private enum PhotoModeSignal
	{
		Start,
		ReadyToCreatePlayer,
		PlayerReady,
		Finishing,
		Finished
	}
}
