using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class LoggedSceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			if (status != SceneStatuses.Disconnected)
			{
				if (status == SceneStatuses.ToGame)
				{
					this.ToGame();
				}
			}
			else
			{
				this.helpers.ChangeFormAction(this.helpers.MenuPrefabsList.playLoggedFormAS, this.helpers.MenuPrefabsList.loginFormAS, true, false);
			}
		}

		private void ToGame()
		{
			this.loginActions.StartGameAction();
		}

		private MenuHelpers helpers = new MenuHelpers();

		private LoginActions loginActions = new LoginActions();
	}
}
