using System;
using ObjectModel;
using UnityEngine;

public class XboxRegistrationHandler : MonoBehaviour
{
	protected virtual void OnEnable()
	{
		PhotonConnectionFactory.Instance.OnRegisterUser += this.OnRegisterUser;
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnConnectedToMaster += this.OnConnectedToMaster;
		if (!this._isRegestring && !this._isRegistered)
		{
			this.Register();
		}
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnRegisterUser -= this.OnRegisterUser;
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnConnectedToMaster -= this.OnConnectedToMaster;
	}

	private void Register()
	{
		Debug.Log("Start register");
		if (!this._isRegestring)
		{
			this._isRegestring = true;
			if (PhotonConnectionFactory.Instance.IsConnectedToMaster)
			{
				StaticUserData.IsSignInToServer = false;
				PhotonConnectionFactory.Instance.Disconnect();
			}
			else
			{
				PhotonConnectionFactory.Instance.ConnectToMasterNoAuth();
			}
		}
	}

	public void OnRegisterUser(Profile profile)
	{
		this._isRegistered = true;
		PhotonConnectionFactory.Instance.SetAddProps(ChangeLanguage.GetCurrentLanguage.Id, null, false);
		StaticUserData.IsSignInToServer = true;
		MeasuringSystemManager.ChangeMeasuringSystem();
		this.EulaScreenInit.gameObject.SetActive(true);
		this.EulaScreenInit.EULAAccepted += this.EulaScreenInit_EULAAccepted;
	}

	private void EulaScreenInit_EULAAccepted(object sender, EventArgs e)
	{
		if (this.EulaScreenInit != null)
		{
			this.EulaScreenInit.gameObject.SetActive(false);
		}
		else
		{
			((EULAScreenInit)sender).gameObject.SetActive(false);
		}
		this.LoadCustomizationScene.UnloadedScene += delegate(object eh, EventArgs obj)
		{
			SceneController.CallAction(ScenesList.Registration, SceneStatuses.RegisterComplete, this, PhotonConnectionFactory.Instance.Profile);
		};
		this.LoadCustomizationScene.ShowFirstTime();
	}

	private void OnDisconnect()
	{
		if (this._isRegestring)
		{
			PhotonConnectionFactory.Instance.ConnectToMasterNoAuth();
		}
	}

	protected virtual void OnConnectedToMaster()
	{
		Debug.Log("OnConnectedToMaster");
		StaticUserData.StartConnection = false;
		StaticUserData.IsSignInToServer = true;
		if (this._isRegestring)
		{
			this._isRegestring = false;
			string text = Guid.NewGuid().ToString();
			string text2 = default(Guid).ToString();
		}
	}

	public LoadCustomizationScene LoadCustomizationScene;

	public EULAScreenInit EulaScreenInit;

	private bool _isRegestring;

	private bool _isRegistered;

	protected string _username;

	protected string _source;
}
