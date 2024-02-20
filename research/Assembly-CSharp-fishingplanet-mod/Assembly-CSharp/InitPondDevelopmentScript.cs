using System;
using ObjectModel;
using UnityEngine;

public class InitPondDevelopmentScript : MonoBehaviour
{
	internal void Start()
	{
		StaticUserData.CurrentPond = new Pond
		{
			PondId = this.PondId
		};
		if (PhotonConnectionFactory.Instance.Email == null)
		{
			PhotonConnectionFactory.Instance.MasterServerAddress = StaticUserData.ServerConnectionString;
			PhotonConnectionFactory.Instance.Protocol = StaticUserData.ServerConnectionProtocol;
			PhotonConnectionFactory.Instance.AppName = "Editor";
			PhotonConnectionFactory.Instance.Email = this.UserEmail;
			PhotonConnectionFactory.Instance.Password = this.UserPassword;
			StaticUserData.ConnectToMasterWrapper();
		}
		else
		{
			this.ShowAll();
		}
		PhotonConnectionFactory.Instance.OnGotProfile += this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed += this.OnMoveFailed;
		PhotonConnectionFactory.Instance.OnConnectedToMaster += this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnGotLocationInfo += this.OnGotLocationInfo;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotProfile -= this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed -= this.OnMoveFailed;
		PhotonConnectionFactory.Instance.OnConnectedToMaster -= this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnGotLocationInfo -= this.OnGotLocationInfo;
	}

	private void OnConnectedToMaster()
	{
		StaticUserData.StartConnection = false;
		if (PhotonConnectionFactory.Instance.IsConnectedToMaster && PhotonConnectionFactory.Instance.IsAuthenticated && PhotonConnectionFactory.Instance.Profile == null)
		{
			PhotonConnectionFactory.Instance.RequestProfile();
		}
	}

	private void OnMoved(TravelDestination destination)
	{
		if (destination == TravelDestination.Room)
		{
			PhotonConnectionFactory.Instance.GetLocationInfo(this.LocationId);
		}
		else
		{
			PhotonConnectionFactory.Instance.MoveToRoom(this.PondId, null);
		}
	}

	private void OnMoveFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void OnGotLocationInfo(Location location)
	{
		if (location != null)
		{
			StaticUserData.CurrentLocation = location;
			this.ShowAll();
			PhotonConnectionFactory.Instance.Game.Resume(true);
			PhotonConnectionFactory.Instance.GetPondWeather(StaticUserData.CurrentPond.PondId, 1);
		}
	}

	private void ShowAll()
	{
		foreach (GameObject gameObject in this.DevelopmentForms)
		{
			gameObject.SetActive(true);
		}
	}

	private void OnGotProfile(Profile profile)
	{
		Profile profile2 = PhotonConnectionFactory.Instance.Profile;
		if (profile2.PondId == null || profile2.PondId == 0 || profile2.PondId != this.PondId)
		{
			PhotonConnectionFactory.Instance.MoveToPond(this.PondId, new int?(30), null);
		}
		else if (profile2.PondTimeSpent != null && profile2.PondTimeSpent.Value.TotalDays > 20.0)
		{
			PhotonConnectionFactory.Instance.MoveToBase();
			PhotonConnectionFactory.Instance.MoveToPond(this.PondId, new int?(30), null);
		}
		else
		{
			this.OnMoved(TravelDestination.Pond);
		}
	}

	public string UserEmail;

	public string UserPassword;

	public int PondId;

	public int LocationId;

	public GameObject[] DevelopmentForms;
}
