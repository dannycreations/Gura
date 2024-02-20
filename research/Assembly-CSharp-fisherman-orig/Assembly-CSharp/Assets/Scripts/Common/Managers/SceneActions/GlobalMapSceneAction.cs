using System;
using Assets.Scripts.Common.Exceptions;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class GlobalMapSceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			switch (status)
			{
			case SceneStatuses.GotoBackFromOption:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.optionsFormAS, this.helpers.MenuPrefabsList.globalMapFormAS, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.GlobalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoBackFromExit:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.exitMenuFormAS, this.helpers.MenuPrefabsList.globalMapFormAS, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.GlobalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoPond:
				this.GotoPond(parameters);
				break;
			default:
				switch (status)
				{
				case SceneStatuses.GotoExitMenu:
					this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.globalMapFormAS, this.helpers.MenuPrefabsList.exitMenuFormAS, true, false);
					break;
				default:
					if (status == SceneStatuses.Disconnected)
					{
						ManagerScenes.Instance.LoadStartScene();
					}
					break;
				case SceneStatuses.GotoGameMenuFromTravel:
					Object.Destroy(this.helpers.MenuPrefabsList.travelingForm.GetComponent<LoadPond>());
					this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.travelingFormAS, this.helpers.MenuPrefabsList.globalMapFormAS, false, true);
					UIStatsCollector.ChangeGameScreen(GameScreenType.GlobalMap, GameScreenTabType.Undefined, null, null, null, null, null);
					break;
				case SceneStatuses.GotoOptionMenu:
					this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.globalMapFormAS, this.helpers.MenuPrefabsList.optionsFormAS, true, false);
					break;
				}
				break;
			case SceneStatuses.GotoShopMenu:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.globalMapFormAS, this.helpers.MenuPrefabsList.shopFormAS, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.GlobalShop, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoInventoryMenu:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.globalMapFormAS, this.helpers.MenuPrefabsList.inventoryFormAS, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.Storage, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoGameMenuFromInventory:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.inventoryFormAS, this.helpers.MenuPrefabsList.globalMapFormAS, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.GlobalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			case SceneStatuses.GotoGameMenuFromShop:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.shopFormAS, this.helpers.MenuPrefabsList.globalMapFormAS, true, false);
				UIStatsCollector.ChangeGameScreen(GameScreenType.GlobalMap, GameScreenTabType.Undefined, null, null, null, null, null);
				break;
			}
		}

		private void GotoPond(object parameters)
		{
			PondTransferInfo pondTransferInfo = parameters as PondTransferInfo;
			if (pondTransferInfo != null)
			{
				if (this.helpers.MenuPrefabsList.travelingForm.GetComponent<LoadPond>() == null)
				{
					ActivityState travelingFormAS = this.helpers.MenuPrefabsList.travelingFormAS;
					travelingFormAS.gameObject.AddComponent<LoadPond>().PondInfo = pondTransferInfo;
				}
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.globalMapFormAS, this.helpers.MenuPrefabsList.travelingFormAS, false, false);
				return;
			}
			throw new UninitializedParameterException();
		}

		private MenuHelpers helpers = new MenuHelpers();
	}
}
