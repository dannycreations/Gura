using System;
using Assets.Scripts.Common.Managers;
using ObjectModel;
using UnityEngine;

public abstract class PlayerStateBase : AvatarStateBase
{
	protected float AnimationSpeedK
	{
		get
		{
			return (float)((!base.Player.FastUseRodPod) ? 1 : 10);
		}
	}

	public Rod1stBehaviour Rod
	{
		get
		{
			return base.Player.Rod;
		}
	}

	protected bool IsChumLoadingRequired
	{
		get
		{
			bool flag = false;
			if (StaticUserData.RodInHand.RodTemplate.IsChumFishingTemplate())
			{
				Chum[] array = FeederHelper.FindPreparedChumActiveRodAll();
				foreach (Chum chum in array)
				{
					if ((!chum.HasWeight || !chum.WasFilled) && !chum.CancelRequested && !chum.BeginFillRequested && !chum.FinishFillRequested)
					{
						if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanBeginFillChum(chum))
						{
							return false;
						}
						flag = true;
					}
				}
			}
			return flag;
		}
	}

	protected bool IsHandChumLoadingRequired
	{
		get
		{
			if (base.Player.IsHandThrowMode)
			{
				Chum chum = FeederHelper.FindPreparedChumInHand();
				if (chum != null && (!chum.HasWeight || !chum.WasFilled || chum.WasThrown) && !chum.CancelRequested && !chum.BeginFillRequested && !chum.FinishFillRequested && PhotonConnectionFactory.Instance.Profile.Inventory.CanBeginFillChum(chum))
				{
					return true;
				}
			}
			return false;
		}
	}

	protected bool ChumClearingRequired
	{
		get
		{
			if (StaticUserData.RodInHand.RodTemplate.IsChumFishingTemplate())
			{
				Chum[] array = FeederHelper.FindPreparedChumActiveRodAll();
				foreach (Chum chum in array)
				{
					if (!chum.HasWeight && !chum.CancelRequested && !chum.BeginFillRequested && !chum.FinishFillRequested && !PhotonConnectionFactory.Instance.Profile.Inventory.CanBeginFillChum(chum) && (PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError == "Can't fill chum - unavailable mix" || PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError == "Can't fill chum - not enough mix amount"))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	protected bool IsAnimationFinished
	{
		get
		{
			return this.AnimationState == null || (this.AnimationState.speed > 0f && this.AnimationState.time >= this.AnimationState.length) || (this.AnimationState.speed < 0f && this.AnimationState.time < 0f);
		}
	}

	protected bool IsOpenMapRequest
	{
		get
		{
			return (!base.Player.IsSailing && ControlsController.ControlsActions.OpenMap.WasClicked && (SettingsManager.InputType != InputModuleManager.InputType.GamePad || (SettingsManager.InputType == InputModuleManager.InputType.GamePad && (InteractiveObjectPanelHandler.Instance == null || !InteractiveObjectPanelHandler.Instance.IsPanelShowing())))) || (base.Player.IsSailing && base.Player.CurrentBoat.IsOpenMap);
		}
	}

	protected bool IsDrawRodPodRequest { get; set; }

	protected float calcAnimationWeight(float angle, float force, float maxLoad)
	{
		float num = (Time.time - base.Player.throwFinishTime) * 0.7f;
		float num2 = Mathf.Lerp(0f, 1f, num);
		float handRotation = base.Player.GetHandRotation();
		float num3 = Math.Abs(angle);
		float num4 = num3 * num3 * num3 * num3;
		float num5 = num4 * handRotation * 17.5f;
		return (num4 * 10f + num5) * num2;
	}

	protected void PlaySound(PlayerStateBase.Sounds sound)
	{
		if ((int)((byte)sound) < PlayerStateBase._sounds.Length)
		{
			RandomSounds.PlaySoundAtPoint(PlayerStateBase._sounds[(int)((byte)sound)], base.Player.transform.position, 0.125f * GlobalConsts.BgVolume, false);
		}
	}

	protected void PlayOpenReelSound()
	{
		if (base.Player.ReelType != ReelTypes.Baitcasting)
		{
			this.PlaySound(PlayerStateBase.Sounds.OpenReel);
		}
	}

	protected virtual AnimationState PlayHandAnimation(string name, float animSpeed = 1f)
	{
		return base.Player.PlayHandAnimation(name, animSpeed, 0f, 0f);
	}

	protected void PlayTransitAnimation(string clipName, byte transitionSignal, float animSpeed = 1f)
	{
		this._transitAnimation = this.PlayHandAnimation(clipName, animSpeed);
		this._transitionSignal = transitionSignal;
	}

	protected virtual void OnTransitAnimationFinished(byte signal)
	{
		this._transitAnimation = null;
	}

	protected bool UpdateTransitAnimation()
	{
		if (this._transitAnimation == null)
		{
			return false;
		}
		if ((this._transitAnimation.speed > 0f && this._transitAnimation.time >= this._transitAnimation.length) || (this._transitAnimation.speed < 0f && this._transitAnimation.time <= 0f))
		{
			this.OnTransitAnimationFinished(this._transitionSignal);
			return true;
		}
		return false;
	}

	public virtual bool CantOpenInventory
	{
		get
		{
			return true;
		}
	}

	protected virtual void OnCantOpenMenu(MainMenuPage page)
	{
		if ((this is FeederLoadingIdle || this is HandLoadingIdle) && ControlsController.ControlsActions.CancelRename.WasReleasedMandatory)
		{
			GameFactory.Message.ShowChumFillingCancelled();
		}
		else if (base.Player.IsTackleThrown)
		{
			GameFactory.Message.ShowCantOpenMenuWhenCasted();
		}
	}

	protected virtual void ShowCantUseRodStand(bool competition)
	{
		GameFactory.Message.ShowCantUseRodStand(competition);
	}

	protected override void OnPreUpdate()
	{
		MainMenuPage menuPage = ControlsController.ControlsActions.GetMenuPage();
		if (menuPage != MainMenuPage.None)
		{
			if (StaticUserData.IS_IN_TUTORIAL && menuPage == MainMenuPage.Latest)
			{
				return;
			}
			if (this.CantOpenInventory || base.Player.CantOpenInventory || CursorManager.IsModalWindow())
			{
				this.OnCantOpenMenu(menuPage);
			}
			else if (KeysHandlerAction.HelpShown)
			{
				KeysHandlerAction.EscapeHandler(true);
			}
			else
			{
				this.OpenMenuPage(menuPage);
			}
		}
	}

	protected void ProcessSavedMenuPage()
	{
		if (PlayerStateBase._savedPage != MainMenuPage.None)
		{
			this.OpenMenuPage(PlayerStateBase._savedPage);
			PlayerStateBase._savedPage = MainMenuPage.None;
		}
	}

	protected void OpenMenuPage(MainMenuPage page)
	{
		if (page == MainMenuPage.Latest)
		{
			page = PlayerStateBase._lastPage;
		}
		if (page == MainMenuPage.Inventory)
		{
			KeysHandlerAction.InventoryHandler();
			PlayerStateBase._lastPage = page;
		}
		else if (page == MainMenuPage.LocalMap)
		{
			KeysHandlerAction.MapHandler();
			PlayerStateBase._lastPage = page;
		}
	}

	public void PlayDrawInAnimation(float blendTime)
	{
		PlayerController player = base.Player;
		string drawInAnimationName = this.GetDrawInAnimationName();
		float num = 1f;
		this.AnimationState = player.PlayAnimation(drawInAnimationName, num, 1f, blendTime);
	}

	public string GetDrawInAnimationName()
	{
		if (base.Player.IsPitching)
		{
			ReelTypes reelType = base.Player.ReelType;
			if (reelType == ReelTypes.Spinning)
			{
				return "DrawToIdlePitch";
			}
			if (reelType == ReelTypes.Baitcasting)
			{
				return "BaitDrawToIdlePitch";
			}
		}
		else
		{
			ReelTypes reelType2 = base.Player.ReelType;
			if (reelType2 == ReelTypes.Spinning)
			{
				return "OpenDraw";
			}
			if (reelType2 == ReelTypes.Baitcasting)
			{
				return "BaitDraw";
			}
		}
		Debug.LogError("GetDrawInAnimationName returns none: " + base.Player.ReelType);
		return string.Empty;
	}

	protected const float HOLD_ENOUGH_DELAY = 0.15f;

	protected AnimationState _transitAnimation;

	protected byte _transitionSignal;

	protected static string[] _sounds = new string[] { "Sounds/Actions/sfx_accurate_cast_mode", "Sounds/Actions/sfx_inaccurate_cast_mode", "Sounds/Actions/Reel/sfx_reel_open", "Sounds/Actions/Reel/sfx_reel_close", "Sounds/Actions/sfx_rod_draw", "Sounds/Actions/sfx_rod_hide", "Sounds/Actions/sfx_tablet_draw", "Sounds/Actions/sfx_tablet_hide", "Sounds/Actions/sfx_cast" };

	protected static MainMenuPage _savedPage = MainMenuPage.None;

	protected static MainMenuPage _lastPage = MainMenuPage.LocalMap;

	protected enum Sounds
	{
		AccurateCast,
		InaccurateCast,
		OpenReel,
		CloseReel,
		RodDrawIn,
		RodDrawOut,
		TabletDrawIn,
		TabletDrawOut,
		RodCast
	}
}
