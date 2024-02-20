using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class NotLoggedSceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			if (status == SceneStatuses.GotoLogin)
			{
				PhotonConnectionFactory.Instance.PinError("Steam player is redirected to forms authentication (2)", Environment.StackTrace);
				ActivityState playNotLoggedFormAS = this.helpers.MenuPrefabsList.playNotLoggedFormAS;
				ActivityState loginFormAS = this.helpers.MenuPrefabsList.loginFormAS;
				this.helpers.ChangeFormAction(playNotLoggedFormAS, loginFormAS, true, false);
			}
		}

		private MenuHelpers helpers = new MenuHelpers();
	}
}
