# NOTES
1. If there is only 1 rod and it is damaged, it causes a death loop (not fixed yet)


# RAW
ExtensionsTools
ShowPondInfo
ShowLocationInfo


# USEFUL
var pondTimeSpent = PhotonConnectionFactory.Instance.Profile.PondTimeSpent.Value.Days.ToString();
var currentDayOfStay = PhotonConnectionFactory.Instance.Profile.CurrentDayOfStay.ToString();
var pondStayTime = PhotonConnectionFactory.Instance.Profile.PondStayTime.Value.ToString();
UIHelper.Label($"Pond: {pondTimeSpent} / {currentDayOfStay} / {pondStayTime}");

ClientMissionsManager.Instance
ControlsController.ControlsActions.Fire1
ControlsController.ControlsActions.Fire2
ControlsController.ControlsActions.NextHours
ControlsController.ControlsActions.RodStandStandaloneHotkeyModifier
GameFactory.Message
GameFactory.Player.CurrentRodPod
GameFactory.Player.HudFishingHandler
InGameMap.FishInfo
InventoryHelper.IsBlocked2Equip
PhotonConnectionFactory.Instance
PhotonConnectionFactory.Instance.Profile.Settings
PhotonConnectionFactory.Instance.Profile.Inventory
PhotonConnectionFactory.Travel
RodHelper.IsRodEquipped
SettingsManager.FishingIndicator
ShowHudElements.Instance
StaticUserData.RodInHand.Rod

CatchedFishInfoHandler
InfoServerMessagesHandler
PlayerStateBase
ScriptLocalization
TimeAndWeatherManager
UIHelper


# USELESS
rod.Rod.GetInstanceID()
PhotonServerConnection.ReplaceItem()
