using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class LoginSceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			if (status != SceneStatuses.ConnectedToMaster)
			{
				if (status != SceneStatuses.GotoRegister)
				{
					if (status == SceneStatuses.Back)
					{
						PhotonConnectionFactory.Instance.PinError("Steam player is redirected to forms authentication (1)", Environment.StackTrace);
						ActivityState activityState = this.helpers.MenuPrefabsList.loginFormAS;
						ActivityState activityState2 = this.helpers.MenuPrefabsList.playNotLoggedFormAS;
						this.helpers.ChangeFormAction(activityState, activityState2, true, false);
					}
				}
				else
				{
					ActivityState activityState = this.helpers.MenuPrefabsList.loginFormAS;
					ActivityState activityState2 = this.helpers.MenuPrefabsList.registrationFormAS;
					this.helpers.ChangeFormAction(activityState, activityState2, true, false);
				}
			}
			else
			{
				this.loginActions.LoadingPlayerStartGameAction(this.helpers.MenuPrefabsList.loginFormAS);
			}
		}

		private MenuHelpers helpers = new MenuHelpers();

		private LoginActions loginActions = new LoginActions();
	}
}
