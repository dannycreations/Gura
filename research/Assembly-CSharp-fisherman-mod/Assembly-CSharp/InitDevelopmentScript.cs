using System;
using ObjectModel;
using UnityEngine;

public class InitDevelopmentScript : MonoBehaviour
{
	internal void Start()
	{
		if (PhotonConnectionFactory.Instance.Email == null)
		{
			PhotonConnectionFactory.Instance.MasterServerAddress = StaticUserData.ServerConnectionString;
			PhotonConnectionFactory.Instance.Protocol = StaticUserData.ServerConnectionProtocol;
			PhotonConnectionFactory.Instance.AppName = "Editor";
			PhotonConnectionFactory.Instance.Email = "test2@test.com";
			PhotonConnectionFactory.Instance.Password = "123456";
			StaticUserData.ConnectToMasterWrapper();
		}
		else
		{
			foreach (GameObject gameObject in this.DevelopmentForms)
			{
				gameObject.SetActive(true);
			}
		}
		PhotonConnectionFactory.Instance.OnGotProfile += this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed += this.OnMoveFailed;
		PhotonConnectionFactory.Instance.OnConnectedToMaster += this.OnConnectedToMaster;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotProfile -= this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed -= this.OnMoveFailed;
		PhotonConnectionFactory.Instance.OnConnectedToMaster -= this.OnConnectedToMaster;
	}

	private void OnConnectedToMaster()
	{
		StaticUserData.StartConnection = false;
		if (PhotonConnectionFactory.Instance.IsConnectedToMaster && PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			PhotonConnectionFactory.Instance.MoveToBase();
		}
	}

	private void OnMoved(TravelDestination destination)
	{
		PhotonConnectionFactory.Instance.RequestProfile();
	}

	private void OnMoveFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void OnGotProfile(Profile profile)
	{
		foreach (GameObject gameObject in this.DevelopmentForms)
		{
			gameObject.SetActive(true);
		}
	}

	public GameObject[] DevelopmentForms;
}
