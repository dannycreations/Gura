using System;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;

public class LoadProfileAndGotoGame : MonoBehaviour
{
	internal void Start()
	{
		this._starTime = DateTime.Now.Ticks;
		PhotonConnectionFactory.Instance.OnGotProfile += this.OnGotProfile;
		if (PhotonConnectionFactory.Instance.Profile == null)
		{
			PhotonConnectionFactory.Instance.RequestProfile();
		}
		else
		{
			this._profile = PhotonConnectionFactory.Instance.Profile;
		}
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotProfile -= this.OnGotProfile;
	}

	private void OnGotProfile(Profile profile)
	{
		this._profile = profile;
	}

	internal void Update()
	{
		if (!this._inited && (float)((DateTime.Now.Ticks - this._starTime) / 10000L) < this._duration * 1000f)
		{
			return;
		}
		if (!this._inited && this._profile != null && this.BaseForm != null && this.BaseForm.GetComponent<ActivityState>().isActive)
		{
			this._inited = true;
			this.GotoGame(this._profile);
		}
	}

	private void GotoGame(Profile parameters)
	{
		MeasuringSystemManager.ChangeMeasuringSystem();
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile.PondId != null && profile.PondId > 0)
		{
			if (this.BaseForm.GetComponent<LoadLocation>() == null)
			{
				LoadLocation loadLocation = this.BaseForm.AddComponent<LoadLocation>();
				loadLocation.PondId = profile.PondId.Value;
				loadLocation.Action();
			}
		}
		else if (this.BaseForm.GetComponent<ConnectToLobbyAndGotoGame>() == null)
		{
			this.BaseForm.AddComponent<ConnectToLobbyAndGotoGame>();
			PhotonConnectionFactory.Instance.MoveToBase();
		}
	}

	public GameObject BaseForm;

	private float _duration = 1.5f;

	private long _starTime;

	private bool _inited;

	private Profile _profile;

	private MenuHelpers helpers = new MenuHelpers();
}
