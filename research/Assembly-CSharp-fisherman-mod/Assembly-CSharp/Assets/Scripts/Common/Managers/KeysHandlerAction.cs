using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

namespace Assets.Scripts.Common.Managers
{
	public static class KeysHandlerAction
	{
		public static bool HelpShown
		{
			get
			{
				return KeysHandlerAction._helpCanvas != null || (KeysHandlerAction._helpF1Mouse != null && KeysHandlerAction._helpF1Mouse.IsVisible);
			}
		}

		public static void EscapeHandler(bool is3DActive = false)
		{
			if (TournamentHelper.TOURNAMENT_ENDED_STILL_IN_ROOM && !HudTournamentHandler.IsInHUD)
			{
				return;
			}
			if (KeysHandlerAction._helpCanvas != null)
			{
				KeysHandlerAction.HelpGamepadHandler(false);
				return;
			}
			if (KeysHandlerAction._helpF1Mouse != null && KeysHandlerAction._helpF1Mouse.IsVisible)
			{
				KeysHandlerAction._helpF1Mouse.SetVisible(false);
				return;
			}
			KeysHandlerAction._pondHelpers.PondControllerListRefresh();
			if (KeysHandlerAction._pondHelpers.PondControllerList == null || StaticUserData.CurrentLocation == null || (KeysHandlerAction.helpers.MenuPrefabsList.loadingFormAS.isActive && KeysHandlerAction._pondHelpers.PondControllerList.IsInMenu))
			{
				return;
			}
			if (KeysHandlerAction.FishingInProgress())
			{
				return;
			}
			if (KeysHandlerAction._pondHelpers.PondControllerList.Game3DPond != null)
			{
				if (!CompleteMessage.IsBuyingActive && !is3DActive && !StaticUserData.IS_IN_TUTORIAL)
				{
					KeysHandlerAction.BackHandlerOnPondsScenes(FormsEnum.LocalMap);
				}
				return;
			}
			if ((KeysHandlerAction.helpers.MenuPrefabsList.exitMenuForm == null || KeysHandlerAction.helpers.MenuPrefabsList.currentActiveForm != KeysHandlerAction.helpers.MenuPrefabsList.exitMenuForm) && (KeysHandlerAction.helpers.MenuPrefabsList.optionsForm == null || KeysHandlerAction.helpers.MenuPrefabsList.currentActiveForm != KeysHandlerAction.helpers.MenuPrefabsList.optionsForm) && (KeysHandlerAction.helpers.MenuPrefabsList.startForm == null || KeysHandlerAction.helpers.MenuPrefabsList.currentActiveForm != KeysHandlerAction.helpers.MenuPrefabsList.startForm) && (KeysHandlerAction.helpers.MenuPrefabsList.loginForm == null || KeysHandlerAction.helpers.MenuPrefabsList.currentActiveForm != KeysHandlerAction.helpers.MenuPrefabsList.loginForm) && (KeysHandlerAction.helpers.MenuPrefabsList.playLoggedForm == null || KeysHandlerAction.helpers.MenuPrefabsList.currentActiveForm != KeysHandlerAction.helpers.MenuPrefabsList.playLoggedForm) && (KeysHandlerAction.helpers.MenuPrefabsList.playNotLoggedForm == null || KeysHandlerAction.helpers.MenuPrefabsList.currentActiveForm != KeysHandlerAction.helpers.MenuPrefabsList.playNotLoggedForm) && (KeysHandlerAction.helpers.MenuPrefabsList.registrationForm == null || KeysHandlerAction.helpers.MenuPrefabsList.currentActiveForm != KeysHandlerAction.helpers.MenuPrefabsList.registrationForm) && (KeysHandlerAction.helpers.MenuPrefabsList.loadingForm == null || KeysHandlerAction.helpers.MenuPrefabsList.currentActiveForm != KeysHandlerAction.helpers.MenuPrefabsList.loadingForm))
			{
				if (KeysHandlerAction.oldTimeScale == 0f && Time.timeScale != 0f)
				{
					KeysHandlerAction.oldTimeScale = Time.timeScale;
				}
				ActivityState topMenuFormAS = KeysHandlerAction.helpers.MenuPrefabsList.topMenuFormAS;
				if (topMenuFormAS.isActive)
				{
					MonoBehaviour.print("time scale set to 0");
					Time.timeScale = 0f;
					KeysHandlerAction.helpers.HideForm(topMenuFormAS, false);
				}
				else
				{
					MonoBehaviour.print("time scale set to  " + KeysHandlerAction.oldTimeScale);
					Time.timeScale = KeysHandlerAction.oldTimeScale;
					KeysHandlerAction.helpers.ShowForm(topMenuFormAS, false);
				}
			}
		}

		public static bool FishingInProgress()
		{
			bool flag = KeysHandlerAction.IsFishingInProgress();
			if (flag)
			{
				GameFactory.Message.ShowCantOpenMenuWhenCasted();
			}
			return flag || KeysHandlerAction.IsPhotoMode;
		}

		public static bool IsPhotoMode
		{
			get
			{
				return GameFactory.Player != null && GameFactory.Player.State == typeof(PlayerPhotoMode);
			}
		}

		public static bool IsFishingInProgress()
		{
			return GameFactory.Player != null && GameFactory.Player.CantOpenInventory;
		}

		public static void InGameMapHandler()
		{
			if (GameFactory.Player != null && GameFactory.Player.IsTackleThrown)
			{
				if (!KeysHandlerAction._ignoredByMapStates.Any((Type s) => s == GameFactory.Player.State))
				{
					GameFactory.Message.ShowCantOpenMapWhenCasted();
				}
			}
		}

		public static void LicenseShopHandler()
		{
			if (KeysHandlerAction.FishingInProgress())
			{
				return;
			}
			if (KeysHandlerAction._pondHelpers.PondControllerList != null && !KeysHandlerAction._pondHelpers.PondControllerList.IsInMenu)
			{
				if (KeysHandlerAction._pondHelpers.PondControllerList.Game3DPond.activeSelf)
				{
					KeysHandlerAction.ShowMenu(FormsEnum.Shop);
				}
				if (ShopMainPageHandler.Instance != null)
				{
					ShopMainPageHandler.Instance.OpenLicences();
				}
			}
		}

		public static bool IsMenuActive
		{
			get
			{
				return KeysHandlerAction._pondHelpers.PondControllerList != null && !KeysHandlerAction._pondHelpers.PondControllerList.Game3DPond.activeSelf;
			}
		}

		public static void InventoryHandler()
		{
			KeysHandlerAction.ShowMenu(FormsEnum.Inventory);
		}

		public static void MissionsHandler()
		{
			if (KeysHandlerAction.FishingInProgress())
			{
				return;
			}
			if (KeysHandlerAction._pondHelpers.PondControllerList != null && !KeysHandlerAction._pondHelpers.PondControllerList.IsInMenu && KeysHandlerAction._pondHelpers.PondControllerList.Game3DPond.activeSelf)
			{
				KeysHandlerAction.ShowMenu(FormsEnum.Quests);
			}
		}

		public static void MapHandler()
		{
			KeysHandlerAction.ShowMenu(FormsEnum.LocalMap);
		}

		public static void OnInputTypeChanged()
		{
			if (KeysHandlerAction._helpCanvas != null)
			{
				KeysHandlerAction.HelpGamepadHandler(false);
			}
			if (KeysHandlerAction._helpF1Mouse != null && KeysHandlerAction._helpF1Mouse.IsVisible)
			{
				KeysHandlerAction._helpF1Mouse.SetVisible(false);
			}
		}

		private static void BackHandlerOnPondsScenes(FormsEnum form = FormsEnum.LocalMap)
		{
			if (KeysHandlerAction._pondHelpers.PondControllerList.Game3DPond.activeSelf)
			{
				KeysHandlerAction.ShowMenu(form);
			}
			else
			{
				if (ShowLocationInfo.Instance != null)
				{
					ShowLocationInfo.Instance.RestoreOnLocation();
				}
				TransferToLocation.Instance.ActivateGameView();
			}
		}

		private static void ShowMenu(FormsEnum form)
		{
			if (GameFactory.Player != null)
			{
				KeysHandlerAction.ShowLocationMenu(form, !KeysHandlerAction._pondHelpers.PondControllerList.IsInMenu);
				KeysHandlerAction._pondHelpers.PondControllerList.HideGame();
			}
		}

		private static void ShowLocationMenu(FormsEnum form, bool immediate = false)
		{
			KeysHandlerAction._pondHelpers.PondControllerList.ShowMenu(true, true);
			if (StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == 2)
			{
				immediate = false;
			}
			if (!StaticUserData.IsShowDashboard)
			{
				KeysHandlerAction._pondHelpers.PondPrefabsList.topMenuFormAS.Show(immediate);
				if (KeysHandlerAction._pondHelpers.PondPrefabsList.helpPanelAS != null)
				{
					KeysHandlerAction._pondHelpers.PondPrefabsList.helpPanelAS.Show(immediate);
				}
				StaticUserData.IsShowDashboard = true;
			}
			DashboardTabSetter.SwitchForms(form, immediate);
			PhotonConnectionFactory.Instance.Game.Pause();
		}

		public static void ExitZoneHandler()
		{
			if (!KeysHandlerAction.FishingInProgress())
			{
				KeysHandlerAction.BackHandlerOnPondsScenes(FormsEnum.LocalMap);
				Transform component = GameObject.Find(StaticUserData.CurrentLocation.Asset).GetComponent<Transform>();
				GameFactory.Player.Move(component);
			}
		}

		internal static void HelpHandler(MonoBehaviour sender)
		{
			if (CursorManager.IsModalWindow())
			{
				return;
			}
			NewChatInGameController chatInGameController = GameFactory.ChatInGameController;
			if (KeysHandlerAction._helpF1Mouse == null)
			{
				KeysHandlerAction._helpF1Mouse = Object.Instantiate<GameObject>(MessageBoxList.Instance.HelpF1MousePrefab).GetComponent<NewHelpWindowHandler>();
				KeysHandlerAction._helpF1Mouse.SetVisible(true);
				if (chatInGameController != null)
				{
					chatInGameController.SetActive(false);
				}
			}
			else
			{
				KeysHandlerAction._helpF1Mouse.SetVisible(!KeysHandlerAction._helpF1Mouse.IsVisible);
				if (chatInGameController != null)
				{
					chatInGameController.SetActive(KeysHandlerAction._helpF1Mouse.IsVisible);
				}
			}
		}

		internal static void HelpGamepadHandler(bool show)
		{
			if (MessageBoxList.Instance == null)
			{
				return;
			}
			if (!show && KeysHandlerAction._helpCanvas != null)
			{
				Object.Destroy(KeysHandlerAction._helpCanvas);
				UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.WindowClose, SettingsManager.InterfaceVolume);
				KeysHandlerAction._helpCanvas = null;
			}
			else if (show && KeysHandlerAction._helpCanvas == null)
			{
				GameObject helpConsoleCanvasPrefab = MessageBoxList.Instance.HelpConsoleCanvasPrefab;
				KeysHandlerAction._helpCanvas = Object.Instantiate<GameObject>(helpConsoleCanvasPrefab);
				UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.WindowOpen, SettingsManager.InterfaceVolume);
				KeysHandlerAction._helpCanvas.SetActive(true);
			}
		}

		private static Type[] _ignoredByMapStates = new Type[]
		{
			typeof(PlayerPhotoMode),
			typeof(ShowMap),
			typeof(PlayerShowFishIdle),
			typeof(PlayerShowFishLineIdle)
		};

		private static MenuHelpers helpers = new MenuHelpers();

		private static PondHelpers _pondHelpers = new PondHelpers();

		private static GameObject _helpCanvas;

		private static float oldTimeScale = 0f;

		private static NewHelpWindowHandler _helpF1Mouse;
	}
}
