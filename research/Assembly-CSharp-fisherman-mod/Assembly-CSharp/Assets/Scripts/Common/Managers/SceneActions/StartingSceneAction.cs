using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class StartingSceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			ActivityState startFormAS = this.helpers.MenuPrefabsList.startFormAS;
			switch (status)
			{
			case SceneStatuses.ConnectedToMaster:
			{
				ActivityState activityState = this.helpers.MenuPrefabsList.playLoggedFormAS;
				this.helpers.ChangeFormAction(startFormAS, activityState, true, false);
				break;
			}
			default:
				if (status == SceneStatuses.GotoRegister)
				{
					ActivityState activityState = this.helpers.MenuPrefabsList.registrationFormAS;
					this.helpers.ChangeFormAction(startFormAS, activityState, true, false);
				}
				break;
			case SceneStatuses.FailedAuthentication:
			{
				PhotonConnectionFactory.Instance.PinError("Steam player is redirected to forms authentication (4)", Environment.StackTrace);
				ActivityState activityState = this.helpers.MenuPrefabsList.playNotLoggedFormAS;
				this.helpers.ChangeFormAction(startFormAS, activityState, true, false);
				break;
			}
			case SceneStatuses.NotRememberedUser:
			{
				PhotonConnectionFactory.Instance.PinError("Steam player is redirected to forms authentication (3)", Environment.StackTrace);
				ActivityState activityState = this.helpers.MenuPrefabsList.playNotLoggedFormAS;
				this.helpers.ChangeFormAction(startFormAS, activityState, true, false);
				break;
			}
			}
		}

		private MenuHelpers helpers = new MenuHelpers();
	}
}
