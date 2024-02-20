using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class GameMenuAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			switch (status)
			{
			case SceneStatuses.GotoExitMenu:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.topMenuFormAS, this.helpers.MenuPrefabsList.exitMenuFormAS, true, false);
				break;
			default:
				if (status == SceneStatuses.Disconnected)
				{
					ManagerScenes.Instance.LoadStartScene();
				}
				break;
			case SceneStatuses.GotoGameMenuFromExit:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.exitMenuFormAS, this.helpers.MenuPrefabsList.topMenuFormAS, true, false);
				break;
			case SceneStatuses.GotoOptionMenu:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.topMenuFormAS, this.helpers.MenuPrefabsList.optionsFormAS, true, false);
				break;
			case SceneStatuses.GotoGameMenuFromOption:
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.optionsFormAS, this.helpers.MenuPrefabsList.topMenuFormAS, true, false);
				break;
			}
		}

		private MenuHelpers helpers = new MenuHelpers();
	}
}
