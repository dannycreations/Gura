using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UnityEngine;

public class ControlsActions : PlayerActionSet
{
	public ControlsActions()
	{
		for (int i = 1; i <= this.Rods.Length; i++)
		{
			this.Rods[i - 1] = this.CreateCustomPlayerAction("Rod" + i, string.Empty, ControlsActionsCategories.Misc);
		}
		for (int j = 1; j <= this.RodStandSlots.Length; j++)
		{
			this.RodStandSlots[j - 1] = this.CreateCustomPlayerAction("RodStandSlot" + j, string.Empty, ControlsActionsCategories.Misc);
		}
		this.RodPod = this.CreateCustomPlayerAction("RodPod", "RodStandsCaption", ControlsActionsCategories.Fishing);
		this.EmptyHands = this.CreateCustomPlayerAction("EmptyHands", "EmptyHandsCaption", ControlsActionsCategories.Fishing);
		this.HandThrow = this.CreateCustomPlayerAction("HandThrow", "HandCastCaption", ControlsActionsCategories.Fishing);
		ControlsActions._isBlockedAxis = false;
		this.IsBlockedKeyboardInput = false;
		this.ExcludeInputList = new List<string>();
		this.IgnoreControlTypes = new Dictionary<int, int>();
		this.Fire1 = this.CreateCustomPlayerAction("Fire1", string.Empty, ControlsActionsCategories.Fishing);
		this.Fire2 = this.CreateCustomPlayerAction("Fire2", string.Empty, ControlsActionsCategories.Fishing);
		this.AddReelClip = this.CreateCustomPlayerAction("AddReelClip", string.Empty, ControlsActionsCategories.Fishing);
		this.AddReelClipGamePadPart1 = this.CreateCustomPlayerAction("AddReelClipGamePadPart1", string.Empty, ControlsActionsCategories.Misc);
		this.AddReelClipGamePadPart2 = this.CreateCustomPlayerAction("AddReelClipGamePadPart2", string.Empty, ControlsActionsCategories.Misc);
		this.Left = this.CreateCustomPlayerAction("MoveLeft", string.Empty, ControlsActionsCategories.Movement);
		this.Right = this.CreateCustomPlayerAction("MoveRight", string.Empty, ControlsActionsCategories.Movement);
		this.Up = this.CreateCustomPlayerAction("MoveUp", string.Empty, ControlsActionsCategories.Movement);
		this.Down = this.CreateCustomPlayerAction("MoveDown", string.Empty, ControlsActionsCategories.Movement);
		this.Jump = this.CreateCustomPlayerAction("Jump", string.Empty, ControlsActionsCategories.Movement);
		this.LookLeft = this.CreateCustomPlayerAction("LookLeft", string.Empty, ControlsActionsCategories.Misc);
		this.LookRight = this.CreateCustomPlayerAction("LookRight", string.Empty, ControlsActionsCategories.Misc);
		this.LookUp = this.CreateCustomPlayerAction("LookUp", string.Empty, ControlsActionsCategories.Misc);
		this.LookDown = this.CreateCustomPlayerAction("LookDown", string.Empty, ControlsActionsCategories.Misc);
		this.PhotoModeLookLeft = this.CreateCustomPlayerAction("PhotoModeLookLeft", string.Empty, ControlsActionsCategories.Misc);
		this.PhotoModeLookRight = this.CreateCustomPlayerAction("PhotoModeLookRight", string.Empty, ControlsActionsCategories.Misc);
		this.PhotoModeLookUp = this.CreateCustomPlayerAction("PhotoModeLookUp", string.Empty, ControlsActionsCategories.Misc);
		this.PhotoModeLookDown = this.CreateCustomPlayerAction("PhotoModeLookDown", string.Empty, ControlsActionsCategories.Misc);
		this.PHMMoveFishDescriptionLeft = this.CreateCustomPlayerAction("PHMMoveFishDescriptionLeft", string.Empty, ControlsActionsCategories.Misc);
		this.PHMMoveFishDescriptionRight = this.CreateCustomPlayerAction("PHMMoveFishDescriptionRight", string.Empty, ControlsActionsCategories.Misc);
		this.PHMMoveFishDescriptionUp = this.CreateCustomPlayerAction("PHMMoveFishDescriptionUp", string.Empty, ControlsActionsCategories.Misc);
		this.PHMMoveFishDescriptionDown = this.CreateCustomPlayerAction("PHMMoveFishDescriptionDown", string.Empty, ControlsActionsCategories.Misc);
		this.IncFriction = this.CreateCustomPlayerAction("IncFriction", string.Empty, ControlsActionsCategories.Fishing);
		this.DecFriction = this.CreateCustomPlayerAction("DecFriction", string.Empty, ControlsActionsCategories.Fishing);
		this.IncSpeed = this.CreateCustomPlayerAction("IncSpeed", string.Empty, ControlsActionsCategories.Fishing);
		this.DecSpeed = this.CreateCustomPlayerAction("DecSpeed", string.Empty, ControlsActionsCategories.Fishing);
		this.LineLeashInc = this.CreateCustomPlayerAction("LineLeashInc", string.Empty, ControlsActionsCategories.Fishing);
		this.LineLeashDec = this.CreateCustomPlayerAction("LineLeashDec", string.Empty, ControlsActionsCategories.Fishing);
		this.NextHours = this.CreateCustomPlayerAction("NextHours", string.Empty, ControlsActionsCategories.Misc);
		this.Chat = this.CreateCustomPlayerAction("Chat", string.Empty, ControlsActionsCategories.Misc);
		this.ChatGamePad = this.CreateCustomPlayerAction("ChatGamePad", string.Empty, ControlsActionsCategories.Misc);
		this.ChatScrollUp = this.CreateCustomPlayerAction("ChatScrollUp", string.Empty, ControlsActionsCategories.Misc);
		this.ChatScrollDown = this.CreateCustomPlayerAction("ChatScrollDown", string.Empty, ControlsActionsCategories.Misc);
		this.ChatChangeTabToRight = this.CreateCustomPlayerAction("ChatChangeTabToRight", string.Empty, ControlsActionsCategories.Misc);
		this.ChatChangeTabToLeft = this.CreateCustomPlayerAction("ChatChangeTabToLeft", string.Empty, ControlsActionsCategories.Misc);
		this.ShowKeepnetIn3D = this.CreateCustomPlayerAction("ShowKeepnetIn3D", string.Empty, ControlsActionsCategories.Misc);
		this.ShowHud = this.CreateCustomPlayerAction("ShowHud", string.Empty, ControlsActionsCategories.Misc);
		this.PitchMode = this.CreateCustomPlayerAction("PitchMode", string.Empty, ControlsActionsCategories.Fishing);
		this.Help = this.CreateCustomPlayerAction("Help", string.Empty, ControlsActionsCategories.Misc);
		this.HelpGamepad = this.CreateCustomPlayerAction("HelpGamepad", string.Empty, ControlsActionsCategories.Misc);
		this.RunHotkey = this.CreateCustomPlayerAction("RunHotkey", "LeftStickHoldHint", ControlsActionsCategories.Movement);
		this.Map = this.CreateCustomPlayerAction("Map", string.Empty, ControlsActionsCategories.Misc);
		this.Inventory = this.CreateCustomPlayerAction("Inventory", string.Empty, ControlsActionsCategories.Misc);
		this.InventoryAdditional = this.CreateCustomPlayerAction("InventoryAdditional", string.Empty, ControlsActionsCategories.Misc);
		this.FastestReelMode = this.CreateCustomPlayerAction("FastestReelMode", string.Empty, ControlsActionsCategories.Fishing);
		this.BreakLine = this.CreateCustomPlayerAction("BreakLine", string.Empty, ControlsActionsCategories.Fishing);
		this.GetTool = this.CreateCustomPlayerAction("GetTool", string.Empty, ControlsActionsCategories.Misc);
		this.NextTool = this.CreateCustomPlayerAction("NextTool", string.Empty, ControlsActionsCategories.Misc);
		this.PrevTool = this.CreateCustomPlayerAction("PrevTool", string.Empty, ControlsActionsCategories.Misc);
		this.InteractObject = this.CreateCustomPlayerAction("InteractObject", string.Empty, ControlsActionsCategories.Misc);
		this.ForceMouselook = this.CreateCustomPlayerAction("ForceMouselook", string.Empty, ControlsActionsCategories.Misc);
		this.ShowCursor = this.CreateCustomPlayerAction("ShowCursor", string.Empty, ControlsActionsCategories.Misc);
		this.LurePanel = this.CreateCustomPlayerAction("LurePanel", "RightBumperHint", ControlsActionsCategories.Misc);
		this.RodPanel = this.CreateCustomPlayerAction("RodPanel", "LeftBumperHint", ControlsActionsCategories.Misc);
		this.CameraZoom = this.CreateCustomPlayerAction("CameraZoom", string.Empty, ControlsActionsCategories.Fishing);
		this.UseAnchor = this.CreateCustomPlayerAction("UseAnchor", "KeyMappingUseAnchor", ControlsActionsCategories.Boating);
		this.StartStopBoatEngine = this.CreateCustomPlayerAction("StartStopBoatEngine", string.Empty, ControlsActionsCategories.Boating);
		this.IgnitionForward = this.CreateCustomPlayerAction("IgnitionForward", string.Empty, ControlsActionsCategories.Misc);
		this.StartFishing = this.CreateCustomPlayerAction("StartFishing", "KeyMappingStartFishing", ControlsActionsCategories.Boating);
		this.DropDownUp = this.CreateCustomPlayerAction("DropDownUp", string.Empty, ControlsActionsCategories.Misc);
		this.DropDownDown = this.CreateCustomPlayerAction("DropDownDown", string.Empty, ControlsActionsCategories.Misc);
		this.UpLure = this.CreateCustomPlayerAction("UpLure", string.Empty, ControlsActionsCategories.Misc);
		this.DownLure = this.CreateCustomPlayerAction("DownLure", string.Empty, ControlsActionsCategories.Misc);
		this.UpRod = this.CreateCustomPlayerAction("UpRod", string.Empty, ControlsActionsCategories.Misc);
		this.DownRod = this.CreateCustomPlayerAction("DownRod", string.Empty, ControlsActionsCategories.Misc);
		this.UpRodAdditional = this.CreateCustomPlayerAction("UpRodAdditional", string.Empty, ControlsActionsCategories.Misc);
		this.DownRodAdditional = this.CreateCustomPlayerAction("DownRodAdditional", string.Empty, ControlsActionsCategories.Misc);
		this.UpLureAdditional = this.CreateCustomPlayerAction("UpLureAdditional", string.Empty, ControlsActionsCategories.Misc);
		this.DownLureAdditional = this.CreateCustomPlayerAction("DownLureAdditional", string.Empty, ControlsActionsCategories.Misc);
		this.Space = this.CreateCustomPlayerAction("Space", string.Empty, ControlsActionsCategories.Misc);
		this.UISubmit = this.CreateCustomPlayerAction("UISubmit", string.Empty, ControlsActionsCategories.Misc);
		this.UICancel = this.CreateCustomPlayerAction("UICancel", string.Empty, ControlsActionsCategories.Misc);
		this.MapAdditional = this.CreateCustomPlayerAction("MapAdditional", string.Empty, ControlsActionsCategories.Misc);
		this.ResetToDefault = this.CreateCustomPlayerAction("ResetToDefault", string.Empty, ControlsActionsCategories.Misc);
		this.NextRod = this.CreateCustomPlayerAction("NextRod", string.Empty, ControlsActionsCategories.Misc);
		this.PreviousRod = this.CreateCustomPlayerAction("PreviousRod", string.Empty, ControlsActionsCategories.Misc);
		this.TrackShow = this.CreateCustomPlayerAction("TrackShow", string.Empty, ControlsActionsCategories.Misc);
		this.FishLine = this.CreateCustomPlayerAction("FishLine", string.Empty, ControlsActionsCategories.Misc);
		this.FishMouth = this.CreateCustomPlayerAction("FishMouth", string.Empty, ControlsActionsCategories.Misc);
		this.ChangeWeather = this.CreateCustomPlayerAction("ChangeWeather", string.Empty, ControlsActionsCategories.Misc);
		this.Move = this.CreateCustomTwoAxisPlayerAction(this.Left, this.Right, this.Down, this.Up, false);
		this.Looks = this.CreateCustomTwoAxisPlayerAction(this.LookLeft, this.LookRight, this.LookDown, this.LookUp, false);
		this.PhotoModeLooks = this.CreateCustomTwoAxisPlayerAction(this.PhotoModeLookLeft, this.PhotoModeLookRight, this.PhotoModeLookDown, this.PhotoModeLookUp, true);
		this.PHMMoveFishDescription = this.CreateCustomTwoAxisPlayerAction(this.PHMMoveFishDescriptionLeft, this.PHMMoveFishDescriptionRight, this.PHMMoveFishDescriptionDown, this.PHMMoveFishDescriptionUp, true);
		this.PHMChangeModeUpUI = this.CreateCustomPlayerAction("PHMChangeModeUpUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMChangeModeDownUI = this.CreateCustomPlayerAction("PHMChangeModeDownUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOffsetLeftUI = this.CreateCustomPlayerAction("PHMOffsetLeftUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOffsetRightUI = this.CreateCustomPlayerAction("PHMOffsetRightUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOffsetDownUI = this.CreateCustomPlayerAction("PHMOffsetDownUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOffsetUpUI = this.CreateCustomPlayerAction("PHMOffsetUpUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMLabelOffsetLeftUI = this.CreateCustomPlayerAction("PHMLabelOffsetLeftUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMLabelOffsetRightUI = this.CreateCustomPlayerAction("PHMLabelOffsetRightUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMLabelOffsetDownUI = this.CreateCustomPlayerAction("PHMLabelOffsetDownUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMLabelOffsetUpUI = this.CreateCustomPlayerAction("PHMLabelOffsetUpUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOrbitLeftUI = this.CreateCustomPlayerAction("PHMOrbitLeftUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOrbitRightUI = this.CreateCustomPlayerAction("PHMOrbitRightUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOrbitDownUI = this.CreateCustomPlayerAction("PHMOrbitDownUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOrbitUpUI = this.CreateCustomPlayerAction("PHMOrbitUpUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMDollyZoomInUI = this.CreateCustomPlayerAction("PHMDollyZoomInUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMDollyZoomOutUI = this.CreateCustomPlayerAction("PHMDollyZoomOutUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMDollyZoomInByScrollUI = this.CreateCustomPlayerAction("PHMDollyZoomInByScrollUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMDollyZoomOutByScrollUI = this.CreateCustomPlayerAction("PHMDollyZoomOutByScrollUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMRollRightUI = this.CreateCustomPlayerAction("PHMRollRightUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMRollLeftUI = this.CreateCustomPlayerAction("PHMRollLeftUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMFoVIncUI = this.CreateCustomPlayerAction("PHMFoVIncUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMFoVDecUI = this.CreateCustomPlayerAction("PHMFoVDecUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMFoVIncByScrollUI = this.CreateCustomPlayerAction("PHMFoVIncByScrollUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMFoVDecByScrollUI = this.CreateCustomPlayerAction("PHMFoVDecByScrollUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMHideUI = this.CreateCustomPlayerAction("PHMHideUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMResetUI = this.CreateCustomPlayerAction("PHMResetUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMCloseUI = this.CreateCustomPlayerAction("PHMCloseUI", string.Empty, ControlsActionsCategories.Misc);
		this.PHMSwitchFishDescriptionVisibility = this.CreateCustomPlayerAction("PHMSwitchFishDescriptionVisibility", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOpen = this.CreateCustomPlayerAction("PHMOpenPhotoMode", string.Empty, ControlsActionsCategories.Misc);
		this.PHMOrbitUI = this.CreateCustomTwoAxisPlayerAction(this.PHMOrbitLeftUI, this.PHMOrbitRightUI, this.PHMOrbitDownUI, this.PHMOrbitUpUI, true);
		this.PHMOffsetUI = this.CreateCustomTwoAxisPlayerAction(this.PHMOffsetLeftUI, this.PHMOffsetRightUI, this.PHMOffsetDownUI, this.PHMOffsetUpUI, true);
		this.PHMLabelOffsetUI = this.CreateCustomTwoAxisPlayerAction(this.PHMLabelOffsetLeftUI, this.PHMLabelOffsetRightUI, this.PHMLabelOffsetDownUI, this.PHMLabelOffsetUpUI, true);
		this.MoveMapRight = this.CreateCustomPlayerAction("MoveMapRight", string.Empty, ControlsActionsCategories.Misc);
		this.MoveMapLeft = this.CreateCustomPlayerAction("MoveMapLeft", string.Empty, ControlsActionsCategories.Misc);
		this.MoveMapUp = this.CreateCustomPlayerAction("MoveMapUp", string.Empty, ControlsActionsCategories.Misc);
		this.MoveMapDown = this.CreateCustomPlayerAction("MoveMapDown", string.Empty, ControlsActionsCategories.Misc);
		this.AddMark = this.CreateCustomPlayerAction("AddMark", string.Empty, ControlsActionsCategories.Misc);
		this.SubmitMark = this.CreateCustomPlayerAction("SubmitMark", string.Empty, ControlsActionsCategories.Misc);
		this.ZoomMapIn = this.CreateCustomPlayerAction("ZoomMapIn", string.Empty, ControlsActionsCategories.Misc);
		this.ZoomMapOut = this.CreateCustomPlayerAction("ZoomMapOut", string.Empty, ControlsActionsCategories.Misc);
		this.MoveToPlayer = this.CreateCustomPlayerAction("MoveToPlayer", string.Empty, ControlsActionsCategories.Misc);
		this.ChangeRotationType = this.CreateCustomPlayerAction("ChangeRotationType", string.Empty, ControlsActionsCategories.Misc);
		this.CloseMap = this.CreateCustomPlayerAction("CloseMap", string.Empty, ControlsActionsCategories.Misc);
		this.RenameBuoy = this.CreateCustomPlayerAction("RenameBuoy", string.Empty, ControlsActionsCategories.Misc);
		this.SubmitRename = this.CreateCustomPlayerAction("SubmitRename", string.Empty, ControlsActionsCategories.Misc);
		this.CancelRename = this.CreateCustomPlayerAction("CancelRename", string.Empty, ControlsActionsCategories.Misc);
		this.KeyboardReturn = this.CreateCustomPlayerAction("KeyboardReturn", string.Empty, ControlsActionsCategories.Misc);
		this.KeyboardEscape = this.CreateCustomPlayerAction("KeyboardEscape", string.Empty, ControlsActionsCategories.Misc);
		this.ShareBuoy = this.CreateCustomPlayerAction("ShareBuoy", string.Empty, ControlsActionsCategories.Misc);
		this.OpenMap = this.CreateCustomPlayerAction("OpenMap", string.Empty, ControlsActionsCategories.Misc);
		this.UISellRemove = this.CreateCustomPlayerAction("UISellRemove", string.Empty, ControlsActionsCategories.Misc);
		this.RodStandSubmit = this.CreateCustomPlayerAction("RodStandSubmit", string.Empty, ControlsActionsCategories.Misc);
		this.RodStandCancel = this.CreateCustomPlayerAction("RodStandCancel", string.Empty, ControlsActionsCategories.Misc);
		this.RodStandAddAngle = this.CreateCustomPlayerAction("RodStandAddAngle", string.Empty, ControlsActionsCategories.Misc);
		this.RodStandDecAngle = this.CreateCustomPlayerAction("RodStandDecAngle", string.Empty, ControlsActionsCategories.Misc);
		this.RodStandStandaloneHotkeyModifier = this.CreateCustomPlayerAction("RodStandStandaloneHotkeyModifier", string.Empty, ControlsActionsCategories.Misc);
		this.BoatThrottle = this.CreateCustomPlayerAction("BoatThrottle", string.Empty, ControlsActionsCategories.Misc);
		this.BoatThrottleNegative = this.CreateCustomPlayerAction("BoatThrottleNegative", string.Empty, ControlsActionsCategories.Misc);
		this._menuActions.Add(null);
		this._menuActions.Add(new CustomPlayerAction[] { this.MapAdditional, this.Map });
		this._menuActions.Add(new CustomPlayerAction[] { this.Inventory, this.InventoryAdditional });
		this._menuActions.Add(new CustomPlayerAction[] { this.UICancel });
		this.FlashlightVisibility = this.CreateCustomPlayerAction("FlashlightVisibility", string.Empty, ControlsActionsCategories.Misc);
		this.MapMove = this.CreateCustomTwoAxisPlayerAction(this.MoveMapLeft, this.MoveMapRight, this.MoveMapDown, this.MoveMapUp, false);
	}

	public CustomPlayerTwoAxisAction Move { get; set; }

	public List<string> ExcludeInputList { get; private set; }

	public Dictionary<int, int> IgnoreControlTypes { get; private set; }

	public bool IsBlockedAxis
	{
		get
		{
			return ControlsActions._isBlockedAxis;
		}
	}

	public bool IsBlockedKeyboardInput { get; private set; }

	public void AddIgnoreControlType(int controlCode)
	{
		if (!this.IgnoreControlTypes.ContainsKey(controlCode))
		{
			this.IgnoreControlTypes.Add(controlCode, 0);
		}
		Dictionary<int, int> ignoreControlTypes;
		(ignoreControlTypes = this.IgnoreControlTypes)[controlCode] = ignoreControlTypes[controlCode] + 1;
	}

	public bool ContainsControlInIgnores(int controlCode)
	{
		return this.IgnoreControlTypes.ContainsKey(controlCode) && this.IgnoreControlTypes[controlCode] > 0;
	}

	public void RemoveControlType(int controlCode)
	{
		if (!this.IgnoreControlTypes.ContainsKey(controlCode))
		{
			return;
		}
		Dictionary<int, int> ignoreControlTypes;
		(ignoreControlTypes = this.IgnoreControlTypes)[controlCode] = ignoreControlTypes[controlCode] - 1;
		if (this.IgnoreControlTypes[controlCode] <= 0)
		{
			this.IgnoreControlTypes.Remove(controlCode);
		}
	}

	public static ControlsActions CreateWithDefaultBindings()
	{
		ControlsActions controlsActions = new ControlsActions();
		controlsActions.Fire1.AddDefaultBinding(Key.Space, 1);
		controlsActions.Fire1.AddDefaultBinding(Mouse.LeftButton);
		controlsActions.RodStandSubmit.AddDefaultBinding(Mouse.LeftButton);
		controlsActions.RodStandCancel.AddDefaultBinding(Mouse.RightButton);
		controlsActions.RodStandAddAngle.AddDefaultBinding(Mouse.PositiveScrollWheel);
		controlsActions.RodStandDecAngle.AddDefaultBinding(Mouse.NegativeScrollWheel);
		controlsActions.Fire1.AddDefaultBinding(InputControlType.RightTrigger);
		controlsActions.RodStandSubmit.AddDefaultBinding(InputControlType.Action1);
		controlsActions.RodStandCancel.AddDefaultBinding(InputControlType.Action2);
		controlsActions.RodStandAddAngle.AddDefaultBinding(InputControlType.DPadUp);
		controlsActions.RodStandDecAngle.AddDefaultBinding(InputControlType.DPadDown);
		controlsActions.AddReelClip.AddDefaultBinding(Mouse.MiddleButton);
		controlsActions.AddReelClip.AddDefaultBinding(Key.C, 1);
		controlsActions.Fire2.AddDefaultBinding(Key.Return, 1);
		controlsActions.Fire2.AddDefaultBinding(Key.PadEnter, 2);
		controlsActions.Fire2.AddDefaultBinding(Mouse.RightButton);
		controlsActions.Fire2.AddDefaultBinding(InputControlType.LeftTrigger);
		controlsActions.AddReelClipGamePadPart1.AddDefaultBinding(InputControlType.LeftBumper);
		controlsActions.AddReelClipGamePadPart2.AddDefaultBinding(InputControlType.RightBumper);
		controlsActions.Up.AddDefaultBinding(Key.W, 1);
		controlsActions.Down.AddDefaultBinding(Key.S, 1);
		controlsActions.Left.AddDefaultBinding(Key.A, 1);
		controlsActions.Right.AddDefaultBinding(Key.D, 1);
		controlsActions.Up.AddDefaultBinding(Key.UpArrow, 2);
		controlsActions.Down.AddDefaultBinding(Key.DownArrow, 2);
		controlsActions.Left.AddDefaultBinding(Key.LeftArrow, 2);
		controlsActions.Right.AddDefaultBinding(Key.RightArrow, 2);
		controlsActions.Left.AddDefaultBinding(InputControlType.LeftStickLeft);
		controlsActions.Right.AddDefaultBinding(InputControlType.LeftStickRight);
		controlsActions.Up.AddDefaultBinding(InputControlType.LeftStickUp);
		controlsActions.Down.AddDefaultBinding(InputControlType.LeftStickDown);
		controlsActions.LookUp.AddDefaultBinding(Mouse.PositiveY);
		controlsActions.LookDown.AddDefaultBinding(Mouse.NegativeY);
		controlsActions.LookLeft.AddDefaultBinding(Mouse.NegativeX);
		controlsActions.LookRight.AddDefaultBinding(Mouse.PositiveX);
		controlsActions.PhotoModeLookUp.AddDefaultBinding(Mouse.PositiveY);
		controlsActions.PhotoModeLookDown.AddDefaultBinding(Mouse.NegativeY);
		controlsActions.PhotoModeLookLeft.AddDefaultBinding(Mouse.NegativeX);
		controlsActions.PhotoModeLookRight.AddDefaultBinding(Mouse.PositiveX);
		controlsActions.PHMMoveFishDescriptionUp.AddDefaultBinding(Mouse.PositiveY);
		controlsActions.PHMMoveFishDescriptionDown.AddDefaultBinding(Mouse.NegativeY);
		controlsActions.PHMMoveFishDescriptionLeft.AddDefaultBinding(Mouse.NegativeX);
		controlsActions.PHMMoveFishDescriptionRight.AddDefaultBinding(Mouse.PositiveX);
		controlsActions.LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
		controlsActions.LookRight.AddDefaultBinding(InputControlType.RightStickRight);
		controlsActions.LookUp.AddDefaultBinding(InputControlType.RightStickUp);
		controlsActions.LookDown.AddDefaultBinding(InputControlType.RightStickDown);
		controlsActions.IncFriction.AddDefaultBinding(Key.PadPlus, 1);
		controlsActions.IncFriction.AddDefaultBinding(Key.Equals, 2);
		controlsActions.IncFriction.AddDefaultBinding(InputControlType.DPadRight);
		controlsActions.DecFriction.AddDefaultBinding(Key.PadMinus, 1);
		controlsActions.DecFriction.AddDefaultBinding(Key.Minus, 2);
		controlsActions.DecFriction.AddDefaultBinding(InputControlType.DPadLeft);
		controlsActions.IncSpeed.AddDefaultBinding(Key.L, 1);
		controlsActions.IncSpeed.AddDefaultBinding(InputControlType.DPadUp);
		controlsActions.DecSpeed.AddDefaultBinding(Key.K, 1);
		controlsActions.DecSpeed.AddDefaultBinding(InputControlType.DPadDown);
		for (int i = 0; i < controlsActions.Rods.Length; i++)
		{
			controlsActions.Rods[i].AddDefaultBinding(Key.Key1 + i, 1);
		}
		controlsActions.RodPod.AddDefaultBinding(Key.Key9, 1);
		controlsActions.EmptyHands.AddDefaultBinding(Key.Key0, 1);
		controlsActions.HandThrow.AddDefaultBinding(Key.Key8, 1);
		controlsActions.RodStandSlots[0].AddDefaultBinding(InputControlType.DPadUp);
		controlsActions.RodStandSlots[1].AddDefaultBinding(InputControlType.DPadRight);
		controlsActions.RodStandSlots[2].AddDefaultBinding(InputControlType.DPadDown);
		controlsActions.RodStandSlots[3].AddDefaultBinding(InputControlType.DPadLeft);
		controlsActions.NextHours.AddDefaultBinding(Key.T, 1);
		controlsActions.NextHours.AddDefaultBinding(InputControlType.Action3);
		controlsActions.Chat.AddDefaultBinding(Key.Tab, 1);
		controlsActions.ChatScrollUp.AddDefaultBinding(Key.PageUp, 1);
		controlsActions.ChatScrollDown.AddDefaultBinding(Key.PageDown, 1);
		controlsActions.ChatChangeTabToRight.AddDefaultBinding(Key.LeftControl, 1);
		controlsActions.ChatChangeTabToRight.AddDefaultBinding(Key.RightArrow, 2);
		controlsActions.ChatChangeTabToLeft.AddDefaultBinding(Key.LeftControl, 1);
		controlsActions.ChatChangeTabToLeft.AddDefaultBinding(Key.LeftArrow, 2);
		controlsActions.ShowKeepnetIn3D.AddDefaultBinding(Key.Y, 1);
		controlsActions.ShowKeepnetIn3D.AddDefaultBinding(InputControlType.Action4);
		controlsActions.ChatGamePad.AddDefaultBinding(InputControlType.Back);
		controlsActions.ChatGamePad.AddDefaultBinding(InputControlType.TouchPadButton);
		controlsActions.ChatGamePad.AddDefaultBinding(InputControlType.View);
		controlsActions.ShowHud.AddDefaultBinding(Key.Backquote, 1);
		controlsActions.ShowHud.AddDefaultBinding(InputControlType.LeftStickButton);
		controlsActions.PitchMode.AddDefaultBinding(Key.F11, 1);
		controlsActions.PitchMode.AddDefaultBinding(InputControlType.RightStickButton);
		controlsActions.Help.AddDefaultBinding(Key.F1, 1);
		controlsActions.LineLeashInc.AddDefaultBinding(Key.P, 1);
		controlsActions.LineLeashInc.AddDefaultBinding(InputControlType.DPadUp);
		controlsActions.LineLeashDec.AddDefaultBinding(Key.O, 1);
		controlsActions.LineLeashDec.AddDefaultBinding(InputControlType.DPadDown);
		controlsActions.Map.AddDefaultBinding(Key.H, 1);
		controlsActions.HelpGamepad.AddDefaultBinding(InputControlType.Start);
		controlsActions.HelpGamepad.AddDefaultBinding(InputControlType.Options);
		controlsActions.HelpGamepad.AddDefaultBinding(InputControlType.Menu);
		controlsActions.RunHotkey.AddDefaultBinding(Key.LeftShift, 1);
		controlsActions.RunHotkey.AddDefaultBinding(Key.RightShift, 2);
		controlsActions.RodStandStandaloneHotkeyModifier.AddDefaultBinding(Key.LeftShift, 1);
		controlsActions.RodStandStandaloneHotkeyModifier.AddDefaultBinding(Key.RightShift, 1);
		controlsActions.RodStandStandaloneHotkeyModifier.AddDefaultBinding(Key.LeftCommand, 1);
		controlsActions.RodStandStandaloneHotkeyModifier.AddDefaultBinding(Key.RightCommand, 1);
		controlsActions.RodStandStandaloneHotkeyModifier.AddDefaultBinding(Key.LeftControl, 1);
		controlsActions.RodStandStandaloneHotkeyModifier.AddDefaultBinding(Key.RightControl, 1);
		controlsActions.RunHotkey.AddDefaultBinding(InputControlType.LeftStickButton);
		controlsActions.Inventory.AddDefaultBinding(Key.I, 1);
		controlsActions.InventoryAdditional.AddDefaultBinding(InputControlType.Action2);
		controlsActions.FastestReelMode.AddDefaultBinding(Key.LeftShift, 1);
		controlsActions.FastestReelMode.AddDefaultBinding(InputControlType.Action1);
		controlsActions.BreakLine.AddDefaultBinding(Key.B, 1);
		controlsActions.RodPanel.AddDefaultBinding(Key.Q, 1);
		controlsActions.LurePanel.AddDefaultBinding(Key.Z, 1);
		controlsActions.BreakLine.AddDefaultBinding(InputControlType.Action2);
		controlsActions.RodPanel.AddDefaultBinding(InputControlType.LeftBumper);
		controlsActions.LurePanel.AddDefaultBinding(InputControlType.RightBumper);
		controlsActions.NextRod.AddDefaultBinding(InputControlType.RightBumper);
		controlsActions.PreviousRod.AddDefaultBinding(InputControlType.LeftBumper);
		controlsActions.GetTool.AddDefaultBinding(Key.F, 1);
		controlsActions.GetTool.AddDefaultBinding(InputControlType.Action1);
		controlsActions.NextTool.AddDefaultBinding(Key.L, 1);
		controlsActions.PrevTool.AddDefaultBinding(Key.K, 1);
		controlsActions.InteractObject.AddDefaultBinding(Key.E, 1);
		controlsActions.InteractObject.AddDefaultBinding(InputControlType.Action4);
		controlsActions.ForceMouselook.AddDefaultBinding(Key.X, 1);
		controlsActions.ForceMouselook.AddDefaultBinding(Mouse.RightButton);
		controlsActions.ForceMouselook.AddDefaultBinding(InputControlType.RightBumper);
		controlsActions.ShowCursor.AddDefaultBinding(Key.LeftControl, 1);
		controlsActions.ShowCursor.AddDefaultBinding(Key.RightControl, 2);
		controlsActions.DropDownUp.AddDefaultBinding(Key.UpArrow, 1);
		controlsActions.DropDownDown.AddDefaultBinding(Key.DownArrow, 1);
		controlsActions.Space.AddDefaultBinding(Key.Space, 1);
		controlsActions.UISubmit.AddDefaultBinding(Key.PadEnter, 1);
		controlsActions.UISubmit.AddDefaultBinding(Key.Return, 2);
		controlsActions.MapAdditional.AddDefaultBinding(InputControlType.Start);
		controlsActions.MapAdditional.AddDefaultBinding(InputControlType.Options);
		controlsActions.MapAdditional.AddDefaultBinding(InputControlType.Menu);
		controlsActions.CameraZoom.AddDefaultBinding(Key.Z, 1);
		controlsActions.UseAnchor.AddDefaultBinding(Key.Z, 1);
		controlsActions.StartStopBoatEngine.AddDefaultBinding(Key.E, 1);
		controlsActions.IgnitionForward.AddDefaultBinding(Mouse.LeftButton);
		controlsActions.StartFishing.AddDefaultBinding(Key.R, 1);
		controlsActions.UICancel.AddDefaultBinding(Key.Escape, 1);
		controlsActions.DownLure.AddDefaultBinding(Mouse.NegativeScrollWheel);
		controlsActions.UpLure.AddDefaultBinding(Mouse.PositiveScrollWheel);
		controlsActions.DownRod.AddDefaultBinding(Mouse.NegativeScrollWheel);
		controlsActions.UpRod.AddDefaultBinding(Mouse.PositiveScrollWheel);
		controlsActions.CameraZoom.AddDefaultBinding(InputControlType.RightBumper);
		controlsActions.StartStopBoatEngine.AddDefaultBinding(InputControlType.RightBumper);
		controlsActions.UseAnchor.AddDefaultBinding(InputControlType.LeftBumper);
		controlsActions.IgnitionForward.AddDefaultBinding(InputControlType.RightTrigger);
		controlsActions.StartFishing.AddDefaultBinding(InputControlType.Action1);
		controlsActions.DownLure.AddDefaultBinding(InputControlType.RightStickDown);
		controlsActions.UpLure.AddDefaultBinding(InputControlType.RightStickUp);
		controlsActions.DownRod.AddDefaultBinding(InputControlType.LeftStickDown);
		controlsActions.UpRod.AddDefaultBinding(InputControlType.LeftStickUp);
		controlsActions.DownLureAdditional.AddDefaultBinding(InputControlType.RightStickRight);
		controlsActions.UpLureAdditional.AddDefaultBinding(InputControlType.RightStickLeft);
		controlsActions.DownRodAdditional.AddDefaultBinding(InputControlType.LeftStickRight);
		controlsActions.UpRodAdditional.AddDefaultBinding(InputControlType.LeftStickLeft);
		controlsActions.ResetToDefault.AddDefaultBinding(Key.R, 1);
		controlsActions.PHMChangeModeUpUI.AddDefaultBinding(Key.Q, 1);
		controlsActions.PHMChangeModeDownUI.AddDefaultBinding(Key.E, 1);
		controlsActions.PHMOffsetLeftUI.AddDefaultBinding(Key.LeftArrow, 1);
		controlsActions.PHMOffsetRightUI.AddDefaultBinding(Key.RightArrow, 1);
		controlsActions.PHMOffsetDownUI.AddDefaultBinding(Key.DownArrow, 1);
		controlsActions.PHMOffsetUpUI.AddDefaultBinding(Key.UpArrow, 1);
		controlsActions.PHMLabelOffsetLeftUI.AddDefaultBinding(Key.LeftArrow, 1);
		controlsActions.PHMLabelOffsetRightUI.AddDefaultBinding(Key.RightArrow, 1);
		controlsActions.PHMLabelOffsetDownUI.AddDefaultBinding(Key.DownArrow, 1);
		controlsActions.PHMLabelOffsetUpUI.AddDefaultBinding(Key.UpArrow, 1);
		controlsActions.PHMOrbitLeftUI.AddDefaultBinding(Key.Pad4, 1);
		controlsActions.PHMOrbitRightUI.AddDefaultBinding(Key.Pad6, 1);
		controlsActions.PHMOrbitDownUI.AddDefaultBinding(Key.Pad5, 1);
		controlsActions.PHMOrbitDownUI.AddDefaultBinding(Key.Pad2, 1);
		controlsActions.PHMOrbitUpUI.AddDefaultBinding(Key.Pad8, 1);
		controlsActions.PHMDollyZoomInUI.AddDefaultBinding(Key.PadPlus, 1);
		controlsActions.PHMDollyZoomOutUI.AddDefaultBinding(Key.PadMinus, 1);
		controlsActions.PHMDollyZoomInByScrollUI.AddDefaultBinding(Mouse.PositiveScrollWheel);
		controlsActions.PHMDollyZoomOutByScrollUI.AddDefaultBinding(Mouse.NegativeScrollWheel);
		controlsActions.PHMFoVDecByScrollUI.AddDefaultBinding(Mouse.PositiveScrollWheel);
		controlsActions.PHMFoVIncByScrollUI.AddDefaultBinding(Mouse.NegativeScrollWheel);
		controlsActions.PHMHideUI.AddDefaultBinding(Key.H, 1);
		controlsActions.PHMResetUI.AddDefaultBinding(Key.R, 1);
		controlsActions.PHMCloseUI.AddDefaultBinding(Key.X, 1);
		controlsActions.PHMSwitchFishDescriptionVisibility.AddDefaultBinding(Key.F, 1);
		controlsActions.PHMOpen.AddDefaultBinding(Key.P, 1);
		controlsActions.FlashlightVisibility.AddDefaultBinding(Key.J, 1);
		controlsActions.PHMChangeModeUpUI.AddDefaultBinding(InputControlType.DPadUp);
		controlsActions.PHMChangeModeDownUI.AddDefaultBinding(InputControlType.DPadDown);
		controlsActions.PHMOffsetLeftUI.AddDefaultBinding(InputControlType.LeftStickLeft);
		controlsActions.PHMOffsetRightUI.AddDefaultBinding(InputControlType.LeftStickRight);
		controlsActions.PHMOffsetDownUI.AddDefaultBinding(InputControlType.LeftStickDown);
		controlsActions.PHMOffsetUpUI.AddDefaultBinding(InputControlType.LeftStickUp);
		controlsActions.PHMLabelOffsetLeftUI.AddDefaultBinding(InputControlType.RightStickLeft);
		controlsActions.PHMLabelOffsetRightUI.AddDefaultBinding(InputControlType.RightStickRight);
		controlsActions.PHMLabelOffsetDownUI.AddDefaultBinding(InputControlType.RightStickDown);
		controlsActions.PHMLabelOffsetUpUI.AddDefaultBinding(InputControlType.RightStickUp);
		controlsActions.PHMOrbitLeftUI.AddDefaultBinding(InputControlType.RightStickLeft);
		controlsActions.PHMOrbitRightUI.AddDefaultBinding(InputControlType.RightStickRight);
		controlsActions.PHMOrbitDownUI.AddDefaultBinding(InputControlType.RightStickDown);
		controlsActions.PHMOrbitUpUI.AddDefaultBinding(InputControlType.RightStickUp);
		controlsActions.PHMDollyZoomInUI.AddDefaultBinding(InputControlType.LeftBumper);
		controlsActions.PHMDollyZoomOutUI.AddDefaultBinding(InputControlType.RightBumper);
		controlsActions.PHMRollRightUI.AddDefaultBinding(InputControlType.DPadRight);
		controlsActions.PHMRollLeftUI.AddDefaultBinding(InputControlType.DPadLeft);
		controlsActions.PHMFoVIncUI.AddDefaultBinding(InputControlType.RightBumper);
		controlsActions.PHMFoVDecUI.AddDefaultBinding(InputControlType.LeftBumper);
		controlsActions.PHMHideUI.AddDefaultBinding(InputControlType.Action4);
		controlsActions.PHMResetUI.AddDefaultBinding(InputControlType.Action3);
		controlsActions.PHMCloseUI.AddDefaultBinding(InputControlType.Action2);
		controlsActions.PHMSwitchFishDescriptionVisibility.AddDefaultBinding(InputControlType.LeftBumper);
		controlsActions.PHMOpen.AddDefaultBinding(InputControlType.Action3);
		controlsActions.FlashlightVisibility.AddDefaultBinding(InputControlType.RightStickButton);
		controlsActions.ZoomMapIn.AddDefaultBinding(Key.PadPlus, 1);
		controlsActions.ZoomMapOut.AddDefaultBinding(Key.PadMinus, 1);
		controlsActions.MoveMapRight.AddDefaultBinding(Key.RightArrow, 1);
		controlsActions.MoveMapLeft.AddDefaultBinding(Key.LeftArrow, 1);
		controlsActions.MoveMapUp.AddDefaultBinding(Key.UpArrow, 1);
		controlsActions.MoveMapDown.AddDefaultBinding(Key.DownArrow, 1);
		controlsActions.MoveMapRight.AddDefaultBinding(Key.D, 1);
		controlsActions.MoveMapLeft.AddDefaultBinding(Key.A, 1);
		controlsActions.MoveMapUp.AddDefaultBinding(Key.W, 1);
		controlsActions.MoveMapDown.AddDefaultBinding(Key.S, 1);
		controlsActions.CloseMap.AddDefaultBinding(Key.M, 1);
		controlsActions.CloseMap.AddDefaultBinding(Key.Escape, 1);
		controlsActions.OpenMap.AddDefaultBinding(Key.M, 1);
		controlsActions.RenameBuoy.AddDefaultBinding(Key.Return, 1);
		controlsActions.SubmitRename.AddDefaultBinding(Key.Return, 1);
		controlsActions.CancelRename.AddDefaultBinding(Key.Escape, 1);
		controlsActions.KeyboardReturn.AddDefaultBinding(Key.Return, 1);
		controlsActions.KeyboardEscape.AddDefaultBinding(Key.Escape, 1);
		controlsActions.MoveMapRight.AddDefaultBinding(InputControlType.LeftStickRight);
		controlsActions.MoveMapLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		controlsActions.MoveMapUp.AddDefaultBinding(InputControlType.LeftStickUp);
		controlsActions.MoveMapDown.AddDefaultBinding(InputControlType.LeftStickDown);
		controlsActions.AddMark.AddDefaultBinding(InputControlType.Action3);
		controlsActions.SubmitMark.AddDefaultBinding(InputControlType.Action1);
		controlsActions.ZoomMapIn.AddDefaultBinding(InputControlType.RightStickUp);
		controlsActions.ZoomMapOut.AddDefaultBinding(InputControlType.RightStickDown);
		controlsActions.MoveToPlayer.AddDefaultBinding(InputControlType.LeftStickButton);
		controlsActions.ChangeRotationType.AddDefaultBinding(InputControlType.RightStickButton);
		controlsActions.OpenMap.AddDefaultBinding(InputControlType.Action4);
		controlsActions.ShareBuoy.AddDefaultBinding(InputControlType.Action4);
		controlsActions.CloseMap.AddDefaultBinding(InputControlType.Action2);
		controlsActions.CloseMap.AddDefaultBinding(InputControlType.Action4);
		controlsActions.RenameBuoy.AddDefaultBinding(InputControlType.Action1);
		controlsActions.SubmitRename.AddDefaultBinding(InputControlType.Action1);
		controlsActions.CancelRename.AddDefaultBinding(InputControlType.Action2);
		controlsActions.UISellRemove.AddDefaultBinding(InputControlType.Action2);
		controlsActions.BoatThrottle.AddDefaultBinding(InputControlType.RightTrigger);
		controlsActions.BoatThrottleNegative.AddDefaultBinding(InputControlType.LeftTrigger);
		controlsActions.BoatThrottle.AddDefaultBinding(Key.W, 1);
		controlsActions.BoatThrottleNegative.AddDefaultBinding(Key.S, 1);
		controlsActions.ListenOptions.IncludeUnknownControllers = true;
		controlsActions.ListenOptions.MaxAllowedBindings = 4U;
		controlsActions.ListenOptions.OnBindingFound = delegate(PlayerAction action, BindingSource binding)
		{
			if (binding == new KeyBindingSource(new Key[] { Key.Escape }))
			{
				action.StopListeningForBinding();
				return false;
			}
			return true;
		};
		BindingListenOptions listenOptions = controlsActions.ListenOptions;
		listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(delegate(PlayerAction action, BindingSource binding)
		{
			Debug.Log("Binding added... " + binding.DeviceName + ": " + binding.Name);
		}));
		BindingListenOptions listenOptions2 = controlsActions.ListenOptions;
		listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(delegate(PlayerAction action, BindingSource binding, BindingSourceRejectionType reason)
		{
			Debug.Log("Binding rejected... " + reason);
		}));
		controlsActions.SetNotInFishingZoneMappings();
		return controlsActions;
	}

	public MainMenuPage GetMenuPage()
	{
		for (int i = 1; i < this._menuActions.Count; i++)
		{
			for (int j = 0; j < this._menuActions[i].Length; j++)
			{
				if (this._menuActions[i][j].WasClicked && !this._menuActions[i][j].WasLongPressed)
				{
					return (MainMenuPage)i;
				}
			}
		}
		return MainMenuPage.None;
	}

	public bool IsHudAdditionalPanelActive
	{
		get
		{
			return this.RodPanel != null && this.LurePanel != null && (this.RodPanel.IsPressed || this.LurePanel.IsPressed);
		}
	}

	protected CustomPlayerAction CreateCustomPlayerAction(string name, string localizationKey = "", ControlsActionsCategories category = ControlsActionsCategories.Misc)
	{
		CustomPlayerAction customPlayerAction = new CustomPlayerAction(name, this, localizationKey, category);
		customPlayerAction.Device = base.Device ?? InputManager.ActiveDevice;
		if (this.actionsByName.ContainsKey(name))
		{
			throw new InControlException("Action '" + name + "' already exists in this set.");
		}
		this.actions.Add(customPlayerAction);
		this.actionsByName.Add(name, customPlayerAction);
		return customPlayerAction;
	}

	protected CustomPlayerTwoAxisAction CreateCustomTwoAxisPlayerAction(PlayerAction negativeXAction, PlayerAction positiveXAction, PlayerAction negativeYAction, PlayerAction positiveYAction, bool isUnblockable = false)
	{
		CustomPlayerTwoAxisAction customPlayerTwoAxisAction = new CustomPlayerTwoAxisAction(negativeXAction, positiveXAction, negativeYAction, positiveYAction, this, isUnblockable);
		this.twoAxisActions.Add(customPlayerTwoAxisAction);
		return customPlayerTwoAxisAction;
	}

	public void ChangeToRightHanded(bool isRightHanded)
	{
		if (!isRightHanded)
		{
			this.ChangeBinding(this.Left, InputControlType.LeftStickLeft, InputControlType.RightStickLeft);
			this.ChangeBinding(this.Right, InputControlType.LeftStickRight, InputControlType.RightStickRight);
			this.ChangeBinding(this.Up, InputControlType.LeftStickUp, InputControlType.RightStickUp);
			this.ChangeBinding(this.Down, InputControlType.LeftStickDown, InputControlType.RightStickDown);
			this.ChangeBinding(this.LookLeft, InputControlType.RightStickLeft, InputControlType.LeftStickLeft);
			this.ChangeBinding(this.LookRight, InputControlType.RightStickRight, InputControlType.LeftStickRight);
			this.ChangeBinding(this.LookUp, InputControlType.RightStickUp, InputControlType.LeftStickUp);
			this.ChangeBinding(this.LookDown, InputControlType.RightStickDown, InputControlType.LeftStickDown);
			this.ChangeBinding(this.PitchMode, InputControlType.RightStickButton, InputControlType.LeftStickButton);
			this.ChangeBinding(this.RunHotkey, InputControlType.LeftStickButton, InputControlType.RightStickButton);
		}
		else
		{
			this.ChangeBinding(this.Left, InputControlType.RightStickLeft, InputControlType.LeftStickLeft);
			this.ChangeBinding(this.Right, InputControlType.RightStickRight, InputControlType.LeftStickRight);
			this.ChangeBinding(this.Up, InputControlType.RightStickUp, InputControlType.LeftStickUp);
			this.ChangeBinding(this.Down, InputControlType.RightStickDown, InputControlType.LeftStickDown);
			this.ChangeBinding(this.LookLeft, InputControlType.LeftStickLeft, InputControlType.RightStickLeft);
			this.ChangeBinding(this.LookRight, InputControlType.LeftStickRight, InputControlType.RightStickRight);
			this.ChangeBinding(this.LookUp, InputControlType.LeftStickUp, InputControlType.RightStickUp);
			this.ChangeBinding(this.LookDown, InputControlType.LeftStickDown, InputControlType.RightStickDown);
			this.ChangeBinding(this.PitchMode, InputControlType.LeftStickButton, InputControlType.RightStickButton);
			this.ChangeBinding(this.RunHotkey, InputControlType.RightStickButton, InputControlType.LeftStickButton);
		}
	}

	private void ChangeBinding(CustomPlayerAction action, InputControlType previousControl, InputControlType newControl)
	{
		BindingSource bindingSource = action.Bindings.FirstOrDefault((BindingSource binding) => binding is DeviceBindingSource && ((DeviceBindingSource)binding).Control == previousControl);
		if (bindingSource != null && bindingSource.BoundTo != null)
		{
			action.RemoveBinding(bindingSource);
		}
		BindingSource bindingSource2 = action.Bindings.FirstOrDefault((BindingSource binding) => binding is DeviceBindingSource && ((DeviceBindingSource)binding).Control == newControl);
		if (bindingSource2 == null)
		{
			action.AddDefaultBinding(newControl);
		}
	}

	public void ClearBindings()
	{
		this.Fire1.Activate();
		this.Fire2.Activate();
		this.IncFriction.Activate();
		this.DecFriction.Activate();
		this.IncSpeed.Activate();
		this.DecSpeed.Activate();
		this.LineLeashInc.Activate();
		this.LineLeashDec.Activate();
		this.NextHours.Activate();
		this.Map.Activate();
		this.NextRod.Activate();
		this.PreviousRod.Activate();
		this.LurePanel.Activate();
		this.RodPanel.Activate();
		this.PitchMode.Activate();
		this.InventoryAdditional.Activate();
		this.FastestReelMode.Activate();
		this.BreakLine.Activate();
		this.GetTool.Activate();
		this.InteractObject.Activate();
		this.DownRod.Activate();
		this.UpRod.Activate();
		this.DownRodAdditional.Activate();
		this.UpRodAdditional.Activate();
	}

	public void SetNotInFishingZoneMappings()
	{
		LogHelper.Log("SetNotInFishingZoneMappings()");
		this.ClearBindings();
		this.Fire2.DeactivateSource(typeof(DeviceBindingSource));
		this.IncFriction.DeactivateSource(typeof(DeviceBindingSource));
		this.DecFriction.DeactivateSource(typeof(DeviceBindingSource));
		this.IncSpeed.DeactivateSource(typeof(DeviceBindingSource));
		this.DecSpeed.DeactivateSource(typeof(DeviceBindingSource));
		this.LineLeashInc.DeactivateSource(typeof(DeviceBindingSource));
		this.LineLeashDec.DeactivateSource(typeof(DeviceBindingSource));
		this.NextRod.DeactivateSource(typeof(DeviceBindingSource));
		this.PreviousRod.DeactivateSource(typeof(DeviceBindingSource));
		this.LurePanel.DeactivateSource(typeof(DeviceBindingSource));
		this.RodPanel.DeactivateSource(typeof(DeviceBindingSource));
		this.LurePanel.DeactivateSource(typeof(KeyBindingSource));
		this.RodPanel.DeactivateSource(typeof(KeyBindingSource));
		this.PitchMode.DeactivateSource(typeof(DeviceBindingSource));
		this.FastestReelMode.DeactivateSource(typeof(DeviceBindingSource));
		this.BreakLine.DeactivateSource(typeof(DeviceBindingSource));
	}

	public void SetInFishingZoneMappings()
	{
		LogHelper.Log("SetInFishingZoneMappings()");
		this.ClearBindings();
		this.FastestReelMode.DeactivateSource(typeof(DeviceBindingSource));
		this.BreakLine.DeactivateSource(typeof(DeviceBindingSource));
	}

	public void SetRodSpecificInFishingZoneMappings(bool floatRod)
	{
		LogHelper.Log("SetRodSpecificInFishingZoneMappings({0})", new object[] { floatRod });
		if (floatRod)
		{
			this.IncSpeed.DeactivateSource(typeof(DeviceBindingSource));
			this.DecSpeed.DeactivateSource(typeof(DeviceBindingSource));
			this.LineLeashInc.Activate();
			this.LineLeashDec.Activate();
		}
		else
		{
			this.IncSpeed.Activate();
			this.DecSpeed.Activate();
			this.LineLeashInc.DeactivateSource(typeof(DeviceBindingSource));
			this.LineLeashDec.DeactivateSource(typeof(DeviceBindingSource));
		}
	}

	public void SetFishingMappings()
	{
		LogHelper.Log("SetFishingMappings()");
		this.ClearBindings();
		this.LineLeashInc.DeactivateSource(typeof(DeviceBindingSource));
		this.LineLeashDec.DeactivateSource(typeof(DeviceBindingSource));
		this.NextHours.DeactivateSource(typeof(DeviceBindingSource));
		this.Map.DeactivateSource(typeof(DeviceBindingSource));
		this.NextRod.DeactivateSource(typeof(DeviceBindingSource));
		this.PreviousRod.DeactivateSource(typeof(DeviceBindingSource));
		this.PitchMode.DeactivateSource(typeof(DeviceBindingSource));
		this.InventoryAdditional.DeactivateSource(typeof(DeviceBindingSource));
		this.GetTool.DeactivateSource(typeof(DeviceBindingSource));
		this.InteractObject.DeactivateSource(typeof(DeviceBindingSource));
		this.LineLeashInc.DeactivateSource(typeof(DeviceBindingSource));
		this.LineLeashDec.DeactivateSource(typeof(DeviceBindingSource));
		this.LurePanel.DeactivateSource(typeof(DeviceBindingSource));
		this.LurePanel.DeactivateSource(typeof(KeyBindingSource));
		this.DownRod.DeactivateSource(typeof(DeviceBindingSource));
		this.UpRod.DeactivateSource(typeof(DeviceBindingSource));
		this.DownRodAdditional.DeactivateSource(typeof(DeviceBindingSource));
		this.UpRodAdditional.DeactivateSource(typeof(DeviceBindingSource));
		this.DownRod.DeactivateSource(typeof(MouseBindingSource));
		this.UpRod.DeactivateSource(typeof(MouseBindingSource));
	}

	public void BlockForCtrl()
	{
		this.wasAxisBlocked = ControlsActions._isBlockedAxis;
		this.BlockInput(null);
		this.BlockMouseButtons(true, true, true, true);
	}

	public void UnblockForCtrl()
	{
		this.BlockMouseButtons(false, false, false, false);
		this.UnBlockInput();
		if (ControlsActions._isBlockedAxis && (!(GameFactory.ChatInGameController != null) || !GameFactory.ChatInGameController.IsEditingMode))
		{
			this.UnBlockAxis();
		}
	}

	public void BlockInput(List<string> excludeList = null)
	{
		ControlsActions._isBlockedAxis = true;
		this.BlockKeyboardInput(excludeList);
		LogHelper.Log("BlockInput: " + this.ExcludeStack.Count);
	}

	public void BlockKeyboardInput(List<string> excludeList = null)
	{
		LogHelper.Log("BlockKeyboardInput");
		this.IsBlockedKeyboardInput = true;
		this.ExcludeStack.Push(this.ExcludeInputList);
		this.ExcludeInputList = excludeList ?? new List<string>();
	}

	public float GetAxis(string name)
	{
		if (ControlsActions._isBlockedAxis && !this.ExcludeInputList.Contains(name))
		{
			return 0f;
		}
		if (ControlsActions._isBlockedScrollAxis && name == "Mouse ScrollWheel")
		{
			return 0f;
		}
		return Input.GetAxis(name);
	}

	internal bool GetMouseButton(int button)
	{
		return (button != 0 || !ControlsActions._isBlockedLMB) && (button != 1 || !ControlsActions._isBlockedRMB) && (button != 2 || !ControlsActions._isBlockedSMB) && Input.GetMouseButton(button);
	}

	internal bool GetMouseButtonDownMandatory(int button)
	{
		return Input.GetMouseButtonDown(button);
	}

	public void BlockMouseButtons(bool LMB = true, bool RMB = true, bool SMB = true, bool scroll = true)
	{
		ControlsActions._isBlockedLMB = LMB;
		ControlsActions._isBlockedRMB = RMB;
		ControlsActions._isBlockedSMB = SMB;
		if (LMB)
		{
			this.Fire1.Block();
		}
		else
		{
			this.Fire1.UnBlock();
		}
		if (RMB)
		{
			this.Fire2.Block();
		}
		else
		{
			this.Fire2.UnBlock();
		}
		ControlsActions._isBlockedScrollAxis = scroll;
	}

	public void ResetAndUnblock()
	{
		this.IsBlockedKeyboardInput = false;
		this.ExcludeStack.Clear();
		this.ExcludeInputList = new List<string>();
		this.Fire1.UnBlock();
		this.Fire2.UnBlock();
		ControlsActions._isBlockedLMB = false;
		ControlsActions._isBlockedRMB = false;
		ControlsActions._isBlockedSMB = false;
		ControlsActions._isBlockedAxis = false;
		ControlsActions._isBlockedScrollAxis = false;
		LogHelper.Log("ResetAndUnblock");
	}

	public void UnBlockInput()
	{
		this.IsBlockedKeyboardInput = this.ExcludeStack.Count > 1;
		this.ExcludeInputList = ((this.ExcludeStack.Count <= 0) ? new List<string>() : this.ExcludeStack.Pop());
		if (!this.IsBlockedKeyboardInput)
		{
			this.Fire1.UnBlock();
			this.Fire2.UnBlock();
			ControlsActions._isBlockedLMB = false;
			ControlsActions._isBlockedRMB = false;
			ControlsActions._isBlockedSMB = false;
			ControlsActions._isBlockedAxis = false;
			ControlsActions._isBlockedScrollAxis = false;
		}
		LogHelper.Log("UnBlockInput: " + this.ExcludeStack.Count);
	}

	public void BlockAxis()
	{
		ControlsActions._isBlockedAxis = true;
		LogHelper.Log("BlockAxis");
	}

	public void UnBlockAxis()
	{
		ControlsActions._isBlockedAxis = false;
		ControlsActions._isBlockedScrollAxis = false;
		LogHelper.Log("UnBlockAxis");
	}

	public void UnBlockKeyboard()
	{
		LogHelper.Log("UnBlockKeyboard");
		this.IsBlockedKeyboardInput = false;
		this.ExcludeInputList = ((this.ExcludeStack.Count <= 0) ? new List<string>() : this.ExcludeStack.Pop());
	}

	public const int VERSION = 28;

	public CustomPlayerAction[] Rods = new CustomPlayerAction[7];

	[ControlsAction]
	public CustomPlayerAction RodPod;

	[ControlsAction]
	public CustomPlayerAction EmptyHands;

	[ControlsAction]
	public CustomPlayerAction HandThrow;

	[ControlsAction]
	public CustomPlayerAction Fire1;

	[ControlsAction]
	public CustomPlayerAction Fire2;

	[ControlsAction]
	public CustomPlayerAction Left;

	[ControlsAction]
	public CustomPlayerAction Right;

	[ControlsAction]
	public CustomPlayerAction Up;

	[ControlsAction]
	public CustomPlayerAction Down;

	[ControlsAction]
	public CustomPlayerAction AddReelClip;

	public CustomPlayerAction AddReelClipGamePadPart1;

	public CustomPlayerAction AddReelClipGamePadPart2;

	public CustomPlayerAction LookLeft;

	public CustomPlayerAction LookRight;

	public CustomPlayerAction LookUp;

	public CustomPlayerAction LookDown;

	public CustomPlayerAction PhotoModeLookLeft;

	public CustomPlayerAction PhotoModeLookRight;

	public CustomPlayerAction PhotoModeLookUp;

	public CustomPlayerAction PhotoModeLookDown;

	public CustomPlayerAction PHMMoveFishDescriptionLeft;

	public CustomPlayerAction PHMMoveFishDescriptionRight;

	public CustomPlayerAction PHMMoveFishDescriptionUp;

	public CustomPlayerAction PHMMoveFishDescriptionDown;

	[ControlsAction]
	public CustomPlayerAction IncFriction;

	[ControlsAction]
	public CustomPlayerAction DecFriction;

	[ControlsAction]
	public CustomPlayerAction IncSpeed;

	[ControlsAction]
	public CustomPlayerAction DecSpeed;

	[ControlsAction]
	public CustomPlayerAction NextHours;

	[ControlsAction]
	public CustomPlayerAction Chat;

	[ControlsAction]
	public CustomPlayerAction ChatScrollUp;

	[ControlsAction]
	public CustomPlayerAction ChatScrollDown;

	[ControlsAction]
	public CustomPlayerAction ShowHud;

	[ControlsAction]
	public CustomPlayerAction PitchMode;

	[ControlsAction]
	public CustomPlayerAction Help;

	[ControlsAction]
	public CustomPlayerAction LineLeashInc;

	[ControlsAction]
	public CustomPlayerAction LineLeashDec;

	[ControlsAction]
	public CustomPlayerAction OpenMap;

	public CustomPlayerAction Map;

	[ControlsAction]
	public CustomPlayerAction Inventory;

	[ControlsAction]
	public CustomPlayerAction FastestReelMode;

	[ControlsAction]
	public CustomPlayerAction BreakLine;

	[ControlsAction]
	public CustomPlayerAction GetTool;

	[ControlsAction]
	public CustomPlayerAction NextTool;

	[ControlsAction]
	public CustomPlayerAction PrevTool;

	[ControlsAction]
	public CustomPlayerAction InteractObject;

	[ControlsAction]
	public CustomPlayerAction ForceMouselook;

	[ControlsAction]
	public CustomPlayerAction ShowCursor;

	[ControlsAction]
	public CustomPlayerAction LurePanel;

	[ControlsAction]
	public CustomPlayerAction RodPanel;

	[ControlsAction]
	public CustomPlayerAction FlashlightVisibility;

	public CustomPlayerAction[] RodStandSlots = new CustomPlayerAction[4];

	public CustomPlayerAction DropDownUp;

	public CustomPlayerAction UpLure;

	public CustomPlayerAction UpLureAdditional;

	public CustomPlayerAction DownLure;

	public CustomPlayerAction DownLureAdditional;

	public CustomPlayerAction UpRod;

	public CustomPlayerAction UpRodAdditional;

	public CustomPlayerAction DownRod;

	public CustomPlayerAction DownRodAdditional;

	public CustomPlayerAction DropDownDown;

	public CustomPlayerAction Space;

	public CustomPlayerAction UISubmit;

	public CustomPlayerAction UICancel;

	public CustomPlayerAction ResetToDefault;

	public CustomPlayerAction NextRod;

	public CustomPlayerAction PreviousRod;

	public CustomPlayerAction Jump;

	public CustomPlayerAction ChatChangeTabToRight;

	public CustomPlayerAction ChatChangeTabToLeft;

	public CustomPlayerAction ShowKeepnetIn3D;

	public CustomPlayerAction HelpGamepad;

	[ControlsAction]
	public CustomPlayerAction RunHotkey;

	public CustomPlayerAction InventoryAdditional;

	public CustomPlayerAction MapAdditional;

	public CustomPlayerAction ChatGamePad;

	public CustomPlayerAction CameraZoom;

	[ControlsAction]
	public CustomPlayerAction UseAnchor;

	public CustomPlayerAction StartStopBoatEngine;

	public CustomPlayerAction IgnitionForward;

	[ControlsAction]
	public CustomPlayerAction StartFishing;

	public CustomPlayerAction PHMChangeModeUpUI;

	public CustomPlayerAction PHMChangeModeDownUI;

	public CustomPlayerAction PHMOffsetLeftUI;

	public CustomPlayerAction PHMOffsetRightUI;

	public CustomPlayerAction PHMOffsetDownUI;

	public CustomPlayerAction PHMOffsetUpUI;

	public CustomPlayerAction PHMLabelOffsetLeftUI;

	public CustomPlayerAction PHMLabelOffsetRightUI;

	public CustomPlayerAction PHMLabelOffsetDownUI;

	public CustomPlayerAction PHMLabelOffsetUpUI;

	public CustomPlayerAction PHMOrbitLeftUI;

	public CustomPlayerAction PHMOrbitRightUI;

	public CustomPlayerAction PHMOrbitDownUI;

	public CustomPlayerAction PHMOrbitUpUI;

	public CustomPlayerAction PHMDollyZoomInUI;

	public CustomPlayerAction PHMDollyZoomOutUI;

	public CustomPlayerAction PHMDollyZoomInByScrollUI;

	public CustomPlayerAction PHMDollyZoomOutByScrollUI;

	public CustomPlayerAction PHMRollRightUI;

	public CustomPlayerAction PHMRollLeftUI;

	public CustomPlayerAction PHMFoVIncUI;

	public CustomPlayerAction PHMFoVDecUI;

	public CustomPlayerAction PHMFoVIncByScrollUI;

	public CustomPlayerAction PHMFoVDecByScrollUI;

	public CustomPlayerAction PHMHideUI;

	public CustomPlayerAction PHMResetUI;

	public CustomPlayerAction PHMCloseUI;

	public CustomPlayerAction PHMSwitchFishDescriptionVisibility;

	public CustomPlayerAction PHMOpen;

	public CustomPlayerAction MoveMapRight;

	public CustomPlayerAction MoveMapLeft;

	public CustomPlayerAction MoveMapUp;

	public CustomPlayerAction MoveMapDown;

	public CustomPlayerAction AddMark;

	public CustomPlayerAction SubmitMark;

	public CustomPlayerAction ZoomMapIn;

	public CustomPlayerAction ZoomMapOut;

	public CustomPlayerAction MoveToPlayer;

	public CustomPlayerAction ChangeRotationType;

	public CustomPlayerAction CloseMap;

	public CustomPlayerAction RenameBuoy;

	public CustomPlayerAction CancelRename;

	public CustomPlayerAction SubmitRename;

	public CustomPlayerAction KeyboardEscape;

	public CustomPlayerAction KeyboardReturn;

	public CustomPlayerAction ShareBuoy;

	public CustomPlayerAction UISellRemove;

	public CustomPlayerAction RodStandSubmit;

	public CustomPlayerAction RodStandCancel;

	public CustomPlayerAction RodStandStandaloneHotkeyModifier;

	public CustomPlayerAction RodStandAddAngle;

	public CustomPlayerAction RodStandDecAngle;

	public CustomPlayerAction BoatThrottle;

	public CustomPlayerAction BoatThrottleNegative;

	public CustomPlayerAction TrackShow;

	public CustomPlayerAction FishLine;

	public CustomPlayerAction FishMouth;

	public CustomPlayerAction ChangeWeather;

	public CustomPlayerTwoAxisAction Looks;

	public CustomPlayerTwoAxisAction PhotoModeLooks;

	public CustomPlayerTwoAxisAction PHMMoveFishDescription;

	public CustomPlayerTwoAxisAction PHMOrbitUI;

	public CustomPlayerTwoAxisAction PHMOffsetUI;

	public CustomPlayerTwoAxisAction PHMLabelOffsetUI;

	public CustomPlayerTwoAxisAction MapMove;

	public Stack<List<string>> ExcludeStack = new Stack<List<string>>();

	private static bool _isBlockedAxis = false;

	private static bool _isBlockedLMB = false;

	private static bool _isBlockedRMB = false;

	private static bool _isBlockedSMB = false;

	private static bool _isBlockedScrollAxis = false;

	public static Dictionary<string, string> StandaloneToControllerMappings = new Dictionary<string, string>
	{
		{ "UICancel", "MapAdditional" },
		{ "Inventory", "InventoryAdditional" },
		{ "RodPod", "RodPanel" }
	};

	private List<CustomPlayerAction[]> _menuActions = new List<CustomPlayerAction[]>();

	private bool wasAxisBlocked;
}
