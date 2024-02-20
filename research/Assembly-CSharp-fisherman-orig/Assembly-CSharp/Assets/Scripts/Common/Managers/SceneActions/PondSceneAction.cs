using System;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class PondSceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			PondSceneAction._pondHelpers.PondControllerListRefresh();
			switch (status)
			{
			case SceneStatuses.GotoBackFromOption:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.optionsForm, PondSceneAction._pondHelpers.PondControllerList.PondFirstSceneForm, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.LocalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoBackFromExit:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.exitMenuForm, PondSceneAction._pondHelpers.PondControllerList.PondFirstSceneForm, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.LocalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			default:
				if (status != SceneStatuses.GotoExitMenu)
				{
					if (status == SceneStatuses.GotoOptionMenu)
					{
						this.helpers.ChangeFormAction(PondSceneAction._pondHelpers.PondControllerList.PondFirstSceneForm, this.helpers.MenuPrefabsList.optionsForm, true, false);
					}
				}
				else
				{
					this.helpers.ChangeFormAction(PondSceneAction._pondHelpers.PondControllerList.PondFirstSceneForm, this.helpers.MenuPrefabsList.exitMenuForm, true, false);
				}
				break;
			case SceneStatuses.GotoLobby:
				this.GotoLobby(false);
				break;
			case SceneStatuses.GotoLobbyFromGame:
				this.GotoLobby(true);
				break;
			case SceneStatuses.GotoShopMenu:
				this.helpers.ChangeFormAction(PondSceneAction._pondHelpers.PondControllerList.PondFirstSceneForm, this.helpers.MenuPrefabsList.shopForm, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.LocalShop, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoInventoryMenu:
				this.helpers.ChangeFormAction(PondSceneAction._pondHelpers.PondControllerList.PondFirstSceneForm, this.helpers.MenuPrefabsList.inventoryForm, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.Storage, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoBackFromInventory:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.inventoryForm, PondSceneAction._pondHelpers.PondControllerList.PondFirstSceneForm, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.LocalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoBackFromShop:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.shopForm, PondSceneAction._pondHelpers.PondControllerList.PondFirstSceneForm, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.LocalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			}
		}

		private void GotoLobby(bool isFromGame = false)
		{
			if (this.helpers.MenuPrefabsList.loadingForm != null && this.helpers.MenuPrefabsList.loadingForm.GetComponent<BackFromPond>() == null)
			{
				ActivityState travelingFormAS = this.helpers.MenuPrefabsList.travelingFormAS;
				travelingFormAS.gameObject.AddComponent<BackFromPond>();
			}
			PondSceneAction._pondHelpers.PondControllerListRefresh();
			if (PondSceneAction._pondHelpers.PondControllerList == null)
			{
				return;
			}
			if (PondSceneAction._pondHelpers.PondControllerList.IsInMenu)
			{
				this.helpers.ChangeFormAction(StaticUserData.CurrentForm, this.helpers.MenuPrefabsList.travelingFormAS, false, false);
			}
			else
			{
				PondSceneAction._pondHelpers.PondControllerList.HideGame();
				PondSceneAction._pondHelpers.PondControllerList.ShowMenu(true, true);
				this.helpers.ChangeFormAction(null, this.helpers.MenuPrefabsList.travelingFormAS, false, false);
			}
		}

		private MenuHelpers helpers = new MenuHelpers();

		private static PondHelpers _pondHelpers = new PondHelpers();
	}
}
