using System;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class LoadingSceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			GameState.State = GameStates.LoadingState;
			if (status != SceneStatuses.GotProfileAndGotoGame)
			{
				if (status != SceneStatuses.GotoGame)
				{
					if (status == SceneStatuses.ConnectedToGlobalMap || status == SceneStatuses.GotoLobby)
					{
						this.GotoGlobalMap(sender);
					}
				}
				else
				{
					this.GotoGame();
				}
			}
			else
			{
				this.LoadedProfileAndGotoGame(parameters);
			}
		}

		private void GotoGlobalMap(MonoBehaviour sender)
		{
			if (ManagerScenes.Instance.CanLoadGlobalMap)
			{
				this.helpers.MenuPrefabsList.currentActiveForm = this.helpers.MenuPrefabsList.globalMapForm;
				ManagerScenes.Instance.LoadGlobalMap();
			}
		}

		private void GotoGame()
		{
			Debug.Log("GotoGame");
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			if (profile.PondId != null && profile.PondId > 0)
			{
				if (this.helpers.MenuPrefabsList.loadingForm.GetComponent<LoadLocation>() == null)
				{
					LoadLocation loadLocation = this.helpers.MenuPrefabsList.loadingForm.AddComponent<LoadLocation>();
					loadLocation.PondId = profile.PondId.Value;
					loadLocation.Action();
				}
			}
			else if (this.helpers.MenuPrefabsList.loadingForm.GetComponent<ConnectToLobbyAndGotoGame>() == null)
			{
				this.helpers.MenuPrefabsList.loadingForm.AddComponent<ConnectToLobbyAndGotoGame>();
				PhotonConnectionFactory.Instance.MoveToBase();
			}
		}

		private void LoadedProfileAndGotoGame(object parameters)
		{
			if (parameters is Profile)
			{
				MeasuringSystemManager.ChangeMeasuringSystem();
				this.GotoGame();
				return;
			}
			throw new ArgumentNullException();
		}

		private MenuHelpers helpers = new MenuHelpers();
	}
}
