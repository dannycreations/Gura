using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class BackToLobbyClick : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ShowingMessageBox;

	public void OnClick()
	{
		if (StaticUserData.IS_IN_TUTORIAL && !TutorialController.EnableBackToLobby)
		{
			Debug.LogError("___kocha Can't back to lobby in Tutorial state!");
			return;
		}
		if (BackToLobbyClick.IsLeaving || TransferToLocation.IsMoving)
		{
			return;
		}
		BackToLobbyClick.IsLeaving = true;
		if (StaticUserData.CurrentPond.PondId == 2)
		{
			this.SendToHome_ActionCalled(this, null);
		}
		else
		{
			string text = string.Empty;
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			if (profile.Tournament != null && !profile.Tournament.IsEnded)
			{
				text = ScriptLocalization.Get("TournamentIsNotOverText");
			}
			else
			{
				text = ScriptLocalization.Get("LeavePondMessage");
				string text2 = this.CheckChums(profile);
				if (!string.IsNullOrEmpty(text2))
				{
					text += text2;
				}
			}
			this.MessageBox = this._helpers.ShowMessageSelectable(StaticUserData.CurrentForm.gameObject, ScriptLocalization.Get("MessageCaption"), text, false);
			EventConfirmAction component = this.MessageBox.GetComponent<EventConfirmAction>();
			component.ConfirmActionCalled += this.SendToHome_ActionCalled;
			component.CancelActionCalled += this.CancelMessage_ActionCalled;
			if (this.ShowingMessageBox != null)
			{
				this.ShowingMessageBox(this, new EventArgs());
			}
		}
	}

	private void CancelMessage_ActionCalled(object sender, EventArgs e)
	{
		BackToLobbyClick.IsLeaving = false;
		if (this.MessageBox != null)
		{
			this.MessageBox.Close();
		}
	}

	private void SendToHome_ActionCalled(object sender, EventArgs e)
	{
		if (PhotonConnectionFactory.Instance.Profile.Tournament != null && !PhotonConnectionFactory.Instance.Profile.Tournament.IsEnded)
		{
			PhotonConnectionFactory.Instance.EndTournament(true);
		}
		this.CallEndMission();
		if (this.MessageBox != null)
		{
			this.MessageBox.Close();
		}
	}

	private void CallEndMission()
	{
		StaticUserData.CurrentLocation = null;
		PhotonConnectionFactory.Instance.RequestEndOfMissionResult();
		WearTackleController.BackFromTravel = true;
	}

	private void OnDestroy()
	{
		BackToLobbyClick.IsLeaving = false;
	}

	private string CheckChums(Profile profile)
	{
		string empty = string.Empty;
		List<IGrouping<int, InventoryItem>> list = (from p in profile.Inventory.FindAll((InventoryItem p) => p is Chum && p.Storage != StoragePlaces.Storage && (p.IsHidden == null || !p.IsHidden.Value))
			group p by p.ItemId).ToList<IGrouping<int, InventoryItem>>();
		if (list.Count > 0)
		{
			return "\n<b><color=#FFEE44FF>" + ScriptLocalization.Get("ChumsLeavePondMessage") + "</color></b>";
		}
		return empty;
	}

	public static void GoToLobby()
	{
		GameFactory.ClearPondLocationsInfo();
		StaticUserData.CurrentLocation = null;
		StaticUserData.CurrentPond = null;
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoLobby, null, null);
		GameFactory.Clear(true);
	}

	private readonly MenuHelpers _helpers = new MenuHelpers();

	[HideInInspector]
	public MessageBox MessageBox;

	private GameObject _messageFishSold;

	private GameObject _messageEndTournamentTime;

	public static bool IsLeaving;
}
