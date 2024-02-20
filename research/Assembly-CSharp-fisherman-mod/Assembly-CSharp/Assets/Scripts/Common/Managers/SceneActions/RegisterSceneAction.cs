using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class RegisterSceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			switch (status)
			{
			case SceneStatuses.RegisterComplete:
				this.ShowSettingsOnStart(this.helpers.MenuPrefabsList.registrationFormAS);
				break;
			case SceneStatuses.StartSettingChanged:
				this.SuccesfullRegister(parameters);
				break;
			default:
				if (status == SceneStatuses.Back)
				{
					ActivityState registrationFormAS = this.helpers.MenuPrefabsList.registrationFormAS;
					ActivityState loginFormAS = this.helpers.MenuPrefabsList.loginFormAS;
					this.helpers.ChangeFormAction(registrationFormAS, loginFormAS, true, false);
				}
				break;
			case SceneStatuses.ToGame:
				this.ShowSettingsOnStart(this.helpers.MenuPrefabsList.startFormAS);
				break;
			}
		}

		private void SuccesfullRegister(object parameters)
		{
			MeasuringSystemManager.ChangeMeasuringSystem();
			ActivityState settingsOnStartFormAS = this.helpers.MenuPrefabsList.settingsOnStartFormAS;
			this.loginActions.LoadingPlayerStartGameAction(settingsOnStartFormAS);
		}

		private void SuccesfullRegisterConsole(object parameters)
		{
			MeasuringSystemManager.ChangeMeasuringSystem();
			ActivityState registrationFormAS = this.helpers.MenuPrefabsList.registrationFormAS;
			this.loginActions.LoadingPlayerStartGameAction(registrationFormAS);
		}

		private void ShowSettingsOnStart(ActivityState currentForm)
		{
			ActivityState settingsOnStartFormAS = this.helpers.MenuPrefabsList.settingsOnStartFormAS;
			this.helpers.ChangeFormAction(currentForm, settingsOnStartFormAS, true, false);
		}

		private MenuHelpers helpers = new MenuHelpers();

		private LoginActions loginActions = new LoginActions();
	}
}
