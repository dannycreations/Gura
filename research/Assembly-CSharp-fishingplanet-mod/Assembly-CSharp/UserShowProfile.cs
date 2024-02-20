using System;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserShowProfile : MonoBehaviour
{
	private void Subscribe()
	{
		if (!this._subscribed)
		{
			PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile += this.OnGotOtherPlayerProfile;
			PhotonConnectionFactory.Instance.OnProfileOperationFailed += this.OnProfileOperationFailed;
			this._subscribed = true;
		}
	}

	private void Unsubscribe()
	{
		if (this._subscribed)
		{
			PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile -= this.OnGotOtherPlayerProfile;
			PhotonConnectionFactory.Instance.OnProfileOperationFailed -= this.OnProfileOperationFailed;
			this._subscribed = false;
		}
	}

	internal void Awake()
	{
		this.Subscribe();
	}

	internal void OnDestroy()
	{
		this.Unsubscribe();
	}

	public virtual void ShowPlayerProfile()
	{
		if (this._clicked == null && this.CurrentPlayer != null && this.CurrentPlayer.UserId != PhotonConnectionFactory.Instance.Profile.UserId.ToString())
		{
			this.RequestById(this.CurrentPlayer.UserId);
		}
	}

	public void RequestById(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return;
		}
		UIHelper.Waiting(true, null);
		this._clicked = id;
		if (id == PhotonConnectionFactory.Instance.Profile.UserId.ToString())
		{
			this.InitProfile(PhotonConnectionFactory.Instance.Profile);
		}
		else
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			PhotonConnectionFactory.Instance.RequestOtherPlayerProfile(this._clicked);
		}
	}

	private void OnGotOtherPlayerProfile(Profile profile)
	{
		if (this._clicked == profile.UserId.ToString())
		{
			this.InitProfile(profile);
		}
	}

	private void OnProfileOperationFailed(ProfileFailure failure)
	{
		if (failure.SubOperation == 250 && !string.IsNullOrEmpty(this._clicked))
		{
			UIHelper.Waiting(false, null);
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
			MenuHelpers.Instance.ShowMessage(ScriptLocalization.Get("GetProfileErrorText"), failure.FullErrorInfo, true, new Action(this.Cleanup), false);
		}
	}

	private void InitProfile(Profile profile)
	{
		UIHelper.Waiting(false, null);
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		this._messageBox = MenuHelpers.Instance.ShowPlayerProfile(profile, new Action(this.Cleanup));
	}

	private void Cleanup()
	{
		this._clicked = null;
	}

	[HideInInspector]
	public Player CurrentPlayer;

	private MessageBox _messageBox;

	protected string _clicked;

	private bool _subscribed;
}
