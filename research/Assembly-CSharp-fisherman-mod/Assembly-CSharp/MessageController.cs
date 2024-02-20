using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour
{
	public bool IgnoreMessages { get; set; }

	public bool IsDisplaying
	{
		get
		{
			return this._messagePanel != null;
		}
	}

	private void Awake()
	{
		this.IgnoreMessages = false;
		GameFactory.Message = this;
	}

	private void Update()
	{
		if (this._messagePanel == null || this.PanelPrefab == null)
		{
			return;
		}
		if (this._messagePanel.activeSelf && Time.time - this._startTime > this._lifeTime)
		{
			this._hideLerpValue += Time.deltaTime * 3f;
			this.HideMessagePanel();
		}
		if (!this._messagePanel.activeSelf)
		{
			Object.Destroy(this._messagePanel);
			this._messagePanel = null;
		}
	}

	private void HideMessagePanel()
	{
		if (this._messagePanel != null)
		{
			if (this._messagePanel.GetComponent<CanvasGroup>() == null || this._messagePanel.GetComponent<CanvasGroup>().alpha < 0.01f)
			{
				this._messagePanel.SetActive(false);
				this._hideLerpValue = 0f;
			}
			else
			{
				this._messagePanel.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1f, 0f, this._hideLerpValue);
			}
		}
		this._currentOrderWeight = int.MaxValue;
	}

	public void HideMessage()
	{
		if (this._isPersistent)
		{
			return;
		}
		this._lifeTime = 0f;
	}

	private void ShowingMessage(string message, int orderWeight, GameObject parent, float lifeTime = 3f, bool isNotAlphaPanel = false)
	{
		if (this.IgnoreMessages)
		{
			return;
		}
		if (this.PanelPrefab == null || this._currentOrderWeight < orderWeight)
		{
			return;
		}
		if (this._messagePanel == null)
		{
			GameObject gameObject = InfoMessageController.Instance.gameObject;
			if (gameObject == null)
			{
				if (parent == null || parent.GetComponent<Canvas>() == null)
				{
					parent = this._menuHelpers.MenuPrefabsList.MessagePanel;
				}
			}
			else
			{
				parent = gameObject;
			}
			this._messagePanel = GUITools.AddChild(parent, this.PanelPrefab);
			this._messagePanel.GetComponent<RectTransform>().anchoredPosition = this.PanelPrefab.GetComponent<RectTransform>().anchoredPosition;
			if (StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == 2)
			{
				this._messagePanel.transform.localPosition = new Vector3(0f, -165f);
			}
			if (isNotAlphaPanel)
			{
				this._messagePanel.GetComponent<Image>().color = new Color(this._messagePanel.GetComponent<Image>().color.r, this._messagePanel.GetComponent<Image>().color.g, this._messagePanel.GetComponent<Image>().color.b, 1f);
			}
		}
		this._startTime = Time.time;
		this._lifeTime = lifeTime;
		this._currentOrderWeight = orderWeight;
		this._messagePanel.GetComponent<MessageInfo>().Message = message;
		Debug.LogWarning(string.Concat(new object[]
		{
			"UI HINT: ",
			message,
			" [",
			Time.time,
			"]"
		}));
	}

	public void KillLastMessage()
	{
		if (this._messagePanel == null)
		{
			return;
		}
		Object.Destroy(this._messagePanel);
		this._messagePanel = null;
	}

	public void ShowLowerMessage(string message, GameObject parent, float lifeTime = 3f, bool isNotAlphaPanel = false)
	{
		this.ShowingMessage(message, 1, parent, lifeTime, isNotAlphaPanel);
		this._messagePanel.transform.localPosition = new Vector2(0f, -340f);
	}

	public void ShowMessage(string message, GameObject parent, float lifeTime = 3f, bool isNotAlphaPanel = false)
	{
		this.ShowingMessage(message, 1, parent, lifeTime, isNotAlphaPanel);
	}

	public void ShowBoatsMoved()
	{
		this.ShowingMessage(ScriptLocalization.Get("BoatInitialPosition"), 2, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRodBroken()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("ShowRodBroken1"), ScriptLocalization.Get("ShowRodBroken2")), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowQuiverBroken()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("QuiverTipBroken"), ScriptLocalization.Get("QuiverTipBroken2")), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowReelBroken()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("ShowReelBroken1"), ScriptLocalization.Get("ShowReelBroken2")), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowLineBroken()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("ShowLineBroken1"), ScriptLocalization.Get("ShowLineBroken2")), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowLineBrokenCut()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("ShowLineBrokenCut1"), ScriptLocalization.Get("ShowLineBroken2")), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowBobberIsBroken()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("ShowBobberBroken1"), ScriptLocalization.Get("ShowBobberBroken2")), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowSinkerIsBroken()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("ShowSinkerBroken1"), ScriptLocalization.Get("ShowSinkerBroken2")), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowLeaderIsBroken()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("LeaderBroken"), ScriptLocalization.Get("LeaderBroken2")), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowLeaderIsBrokenCut()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("LeaderBrokenCut"), ScriptLocalization.Get("LeaderBroken2")), 5, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowLureBroken()
	{
		this.ShowingMessage(string.Format("{0} \n {1}", ScriptLocalization.Get("ShowLureBroken1"), ScriptLocalization.Get("ShowLureBroken2")), 5, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowFishCageBroken()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowFishCageBroken"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowNoAssembledRods()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowNoAssembledRods"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowPitchIsTooShort()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowPitchIsTooShort"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowTackleHitTheGround()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleHitTheGround"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRodIsNotAssembled(string slot)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowRodIsNotAssembled"), slot), 9, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRodHasNoReel(string slot)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowRodHasNoReel"), slot), 30, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRodHasNoLine(string slot)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowRodHasNoLine"), slot), 31, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRodHasNoTackle(string rodName)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowRodHasNoTackle"), rodName), 32, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowHaventRodInSlot(string slotName)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowHaventRodInSlot"), slotName), 9, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRodIsBroken(GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowRodIsBroken"), new object[0]), 10, parent, 3f, false);
	}

	public void ShowReelIsBroken(GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowReelIsBroken"), new object[0]), 11, parent, 3f, false);
	}

	public void ShowRodIsDamaged(GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowRodIsDamaged"), new object[0]), 20, parent, 3f, false);
	}

	public void ShowReelIsDamaged(GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowReelIsDamaged"), new object[0]), 1, parent, 3f, false);
	}

	public void ShowReelDoesntMatchRod(GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowReelDoesntMatchRod"), new object[0]), 1, parent, 3f, false);
	}

	public void ShowLineDoesntMatchRod(GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowLineDoesntMatchRod"), new object[0]), 1, parent, 3f, false);
	}

	public void ShowLineDoesntMatchReel(GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowLineDoesntMatchReel"), new object[0]), 2, parent, 3f, false);
	}

	public void ShowTackleWeightOptimal(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleWeightOptimal"), 1, parent, 3f, false);
	}

	public void ShowTackleLightweightForRods(GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowTackleLightweightForRods"), new object[0]), 1, parent, 3f, false);
	}

	public void ShowTackleHavyForRods(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleHavyForRods"), 1, parent, 3f, false);
	}

	public void ShowTackleCanBreakRod(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleCanBreakRod"), 1, parent, 3f, false);
	}

	public void ShowTackleHavyForLine(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleHavyForLine"), 1, parent, 3f, false);
	}

	public void ShowNoFishingLine(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowNoFishingLine"), 1, parent, 3f, false);
	}

	public void ShowTackleHavyForBobber(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleHavyForBobber"), 1, parent, 3f, false);
	}

	public void ShowTackleLightweightForReel(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleLightweightForReel"), 1, parent, 3f, false);
	}

	public void ShowTackleHavyForReel(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleHavyForReel"), 1, parent, 3f, false);
	}

	public void ShowReelsMustBeSetup(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowReelsMustBeSetup"), 1, parent, 3f, false);
	}

	public void ShowRodMustBeSetup(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowRodMustBeSetup"), 1, parent, 3f, false);
	}

	public void ShowLineMustBeSetup(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowLineMustBeSetup"), 1, parent, 3f, false);
	}

	public void ShowTackleMustBeSetup(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleMustBeSetup"), 1, parent, 3f, false);
	}

	public void ShowHookMustBeSetup(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowHookMustBeSetup"), 1, parent, 3f, false);
	}

	private void ShowMessageForRod(Rod rod, string message, int orderWeight, float lifeTime = 3f)
	{
		AssembledRod rodInHand = StaticUserData.RodInHand;
		bool flag2;
		if (rodInHand != null && rodInHand.Rod != null)
		{
			Guid? instanceId = rodInHand.Rod.InstanceId;
			bool flag = instanceId != null;
			Guid? instanceId2 = rod.InstanceId;
			flag2 = flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
		}
		else
		{
			flag2 = false;
		}
		bool flag3 = flag2;
		MonoBehaviour.print("ShowMessageForRod called. isRodInHands: " + flag3);
		this.ShowingMessage((!flag3) ? string.Format("<color=#FFDD77FF>[{0}]:</color> {1}", rod.Slot, message) : message, orderWeight, this._menuHelpers.MenuPrefabsList.MessagePanel, lifeTime, false);
	}

	public void ShowEarlyStrike(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowEarlyStrike"), 1, 3f);
	}

	public void ShowIncorrectReeling(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowIncorrectReeling"), 1, 3f);
	}

	public void ShowTackleWasPulledAwayFromFish(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowTackleWasPulledAwayFromFish"), 1, 3f);
	}

	public void ShowStrikeTimeoutExpired(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowStrikeTimeoutExpired"), 1, 3f);
	}

	public void ShowBaitLost(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowBaitLost"), 3, 3f);
	}

	public void ShowBaitDepleted(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowBaitDepleted"), 2, 6f);
	}

	public void ShowChumLost(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowChumLost"), 2, 6f);
	}

	public void ShowPVABagLost(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowPVALost"), 2, 6f);
	}

	public void ShowBaitReplenished(Rod Rod)
	{
		this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowBaitReplenished"), 2, 3f);
	}

	public void ShowFishLostIncorrectHook(Rod Rod)
	{
		if (PhotonConnectionFactory.Instance.Profile.Level < 5)
		{
			this.ShowMessageForRod(Rod, ScriptLocalization.Get("ShowFishLostIncorrectHook"), 1, 3f);
		}
	}

	public void ShowLureOnSurface(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowLureOnSurface"), 1, parent, 3f, false);
	}

	public void ShowLureNearSurface(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowLureNearSurface"), 1, parent, 3f, false);
	}

	public void ShowLureInMiddleWater(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowLureInMiddleWater"), 1, parent, 3f, false);
	}

	public void ShowLureNearBottom(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowLureNearBottom"), 1, parent, 3f, false);
	}

	public void ShowLureOnBottom(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowLureOnBottom"), 1, parent, 3f, false);
	}

	public void ShowDragStyle(string name, GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowDragStyle"), name), 2, parent, 3f, false);
	}

	public void ShowReelOverloaded(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowReelOverloaded"), 1, parent, 3f, false);
	}

	public void ShowRodAndReelOverloaded(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowRodAndReelOverloaded"), 1, parent, 3f, false);
	}

	public void ShowRodDamage(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowRodDamage"), 1, parent, 3f, false);
	}

	public void ShowReelDamage(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowReelDamage"), 1, parent, 3f, false);
	}

	public void ShowRodAndReelDamage(GameObject parent)
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowRodAndReelDamage"), 1, parent, 3f, false);
	}

	public void ShowTackleHitched(Rod Rod)
	{
		string text;
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.Mouse)
		{
			BindingSource bindingSource = ControlsController.ControlsActions.BreakLine.Bindings.FirstOrDefault((BindingSource b) => b.GetType() == typeof(KeyBindingSource));
			text = string.Format(ScriptLocalization.Get("ShowTackleHitched"), (!(bindingSource == null) || (bindingSource as KeyBindingSource).Control.Count <= 0) ? (bindingSource as KeyBindingSource).Control.Get(0).ToString() : null);
		}
		else
		{
			text = string.Format(ScriptLocalization.Get("ShowTackleHitchedGamePad"), HotkeyIcons.KeyMappings[InputControlType.Action2]);
		}
		this.ShowingMessage(string.Format("<color=#FFDD77FF>[{0}]:</color> {1}", Rod.Slot, text), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 10f, false);
	}

	public void ShowTackleUnhitched()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowTackleUnhitched"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRodAndReelDamage()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowRodAndReelDamage"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowFishHooked()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowFishHooked"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowFishEscaped()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowFishEscaped"), 2, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowFishEscapedLowTension()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowFishEscapedLowTension"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowFishEscapedHighTension()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowFishEscapedHighTension"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCantOpenMenuWhenCasted()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowCantOpenMenuWhenCasted"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowChumFillingCancelled()
	{
		this.ShowingMessage(ScriptLocalization.Get("ChumFillingCancelled"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCantOpenMapWhenCasted()
	{
		this.ShowingMessage(ScriptLocalization.Get("CantOpenMap"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCantUseRodStand(bool competition)
	{
		this.ShowingMessage(ScriptLocalization.Get((!competition) ? "UseRodStandMessageTournament" : "UseRodStandMessage"), 4, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCantForwardTimeInTournament(bool competition)
	{
		this.ShowingMessage(ScriptLocalization.Get((!competition) ? "FastForwardForbiddenInTournament" : "FastForwardForbiddenInCompetition"), 4, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRecievedAchivement(AchivementInfo achivement)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowRecievedAchivement"), achivement.AchivementName, achivement.AchivementStageName), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCanNotMove(string errorMessage, GameObject parent)
	{
		if (errorMessage.StartsWith("Player does not have car, but traveling on car"))
		{
			this.ShowingMessage(ScriptLocalization.Get("PlayerHasNoCar"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't make self parented item"))
		{
			this.ShowingMessage(ScriptLocalization.Get("CantParentToSelf"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Items for replacement can't be null"))
		{
			this.ShowingMessage(ScriptLocalization.Get("ReplacementCantBeNull"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't replace item with itself"))
		{
			this.ShowingMessage(ScriptLocalization.Get("CantReplaceWithSelf"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Item can be replaced only with item of the same type"))
		{
			this.ShowingMessage(ScriptLocalization.Get("ReplacementIsOfTheWrongType"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Storage is not available at pond"))
		{
			this.ShowingMessage(ScriptLocalization.Get("StorageIsNotAvailable"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("No car - no car equipment"))
		{
			this.ShowingMessage(ScriptLocalization.Get("NoCar"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("No lodge - no lodge equipment"))
		{
			this.ShowingMessage(ScriptLocalization.Get("NoLodge"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't equip item on broken item"))
		{
			this.ShowingMessage(ScriptLocalization.Get("ParentIsBroken"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't equip broken item"))
		{
			this.ShowingMessage(ScriptLocalization.Get("ItemIsBroken"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("No constraint - item of this type can't be equiped at all"))
		{
			this.ShowingMessage(ScriptLocalization.Get("ItemCantBeEquiped"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't equip any more items"))
		{
			this.ShowingMessage(ScriptLocalization.Get("NoMorePlaceToEquip"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Rod can not be modified when casted"))
		{
			this.ShowingMessage(ScriptLocalization.Get("RodCantBeModifiedWhenCasted"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't equip item"))
		{
			this.ShowingMessage(ScriptLocalization.Get("EquipmentRulesBreached"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't move items into overloaded storage"))
		{
			this.ShowingMessage(ScriptLocalization.Get("CantMoveIntoOverloadedStorage"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't move items from exceeded storage"))
		{
			this.ShowingMessage(ScriptLocalization.Get("CantMoveFromExceededStorage"), 1, parent, 3f, true);
		}
		if (errorMessage.StartsWith("Can't save setup - empty Rod in slot"))
		{
			this.ShowingMessage(ScriptLocalization.Get("CantSaveSetupEmptyRod"), 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't save setup - Rod is UnEquiped"))
		{
			this.ShowingMessage(ScriptLocalization.Get("CantSaveSetupRodUnEquiped"), 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't save setup - no slots available"))
		{
			this.ShowingMessage(ScriptLocalization.Get("CantSaveSetupNoSlotsAvailable"), 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't save setup - empty name"))
		{
			this.ShowingMessage(ScriptLocalization.Get("CantSaveSetupEmptyName"), 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't equip setup - Storage is not available"))
		{
			this.ShowingMessage("Can't equip setup - Storage is not available", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't equip empty setup"))
		{
			this.ShowingMessage("Can't equip empty setup", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't equip setup - wrong slot"))
		{
			this.ShowingMessage("Can't equip setup - wrong slot", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't equip setup - Rod is in Hands"))
		{
			this.ShowingMessage("Can't equip setup - Rod is in Hands", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't equip setup - no Rod in slot"))
		{
			this.ShowingMessage("Can't equip setup - no Rod in slot", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't equip setup - Rod in slot is of wrong type"))
		{
			this.ShowingMessage("Can't equip setup - Rod in slot is of wrong type", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't equip setup - some items missing in Inventory"))
		{
			this.ShowingMessage("Can't equip setup - some items missing in Inventory", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't equip setup - not enough capacity to unequip current Rod"))
		{
			this.ShowingMessage("Can't equip setup - not enough capacity to unequip current Rod", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't clear setup - non existing setup"))
		{
			this.ShowingMessage("Can't clear setup - non existing setup", 1, parent, 3f, false);
		}
		if (errorMessage.StartsWith("Can't rename setup - non existing setup"))
		{
			this.ShowingMessage("Can't rename setup - non existing setup", 1, parent, 3f, false);
		}
	}

	public void ShowLeashLineChanged(int value, GameObject parent)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowLeashLineChanged"), value, MeasuringSystemManager.LineLeashLengthSufix()), 1, (!(parent == null)) ? parent : this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, parent != null);
	}

	public void ShowCantTakeFish()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowCantTakeFish"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCantTakeFishTooBig()
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("FishIsTooBigForKeepnet"), MeasuringSystemManager.FishWeight(PhotonConnectionFactory.Instance.Profile.FishCage.Cage.MaxFishWeight).ToString("n3"), MeasuringSystemManager.FishWeightSufix()), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 7f, false);
	}

	public void ShowHaventFishkeeper()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowHaventFishkeeper"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCantSetupFirework()
	{
		this.ShowingMessage(ScriptLocalization.Get("CantSetupFirework"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRoomIsFull()
	{
		this.ShowingMessage(ScriptLocalization.Get("RoomIsFullMessage"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
		this._isPersistent = true;
	}

	public void ShowUnableToEnterRoom()
	{
		this.ShowingMessage(ScriptLocalization.Get("JoinRoomFailedMessage"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
		this._isPersistent = true;
	}

	public void ShowCantChangeRoomWhenCasted()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowCantChangeRoomWhenCasted"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowLengthTooShort()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowLengthTooShort"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCageWasBroken()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowCageWasBroken"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCageSoonBrake()
	{
		this.ShowingMessage(ScriptLocalization.Get("ShowCageSoonBrake"), 2, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowEntryTimeIsOver()
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("EntryTimeIsOverMessage"), "\n"), 2, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCantSetupFireworkInTournament()
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("CannotUseFireworkInTournamentMessage"), "\n"), 2, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowCantSetupFireworkOutEvent()
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("ShowCantSetupFireworkOutEvent"), "\n"), 2, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public void ShowRebootMessage(TimeSpan rebootIn, int expectedDownTime)
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("RebootMessage"), rebootIn.Minutes, expectedDownTime), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 10f, false);
	}

	public void ShowRebootCanceledMessage()
	{
		this.ShowingMessage(string.Format(ScriptLocalization.Get("RebootCanceledMessage"), "\n"), 1, this._menuHelpers.MenuPrefabsList.MessagePanel, 3f, false);
	}

	public GameObject PanelPrefab;

	private float _lifeTime;

	private float _startTime;

	private bool _isPersistent;

	private const float StandardMessageLifetime = 3f;

	private const float InfiniteMessageLifetime = 3.4028235E+38f;

	private int _currentOrderWeight = int.MaxValue;

	private float _hideLerpValue;

	private GameObject _messagePanel;

	private MenuHelpers _menuHelpers = new MenuHelpers();

	private const string rodSlotFormatMessage = "<color=#FFDD77FF>[{0}]:</color> {1}";
}
