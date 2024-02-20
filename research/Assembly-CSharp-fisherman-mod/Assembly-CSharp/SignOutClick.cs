using System;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class SignOutClick : MonoBehaviour
{
	internal void Start()
	{
		if (StaticUserData.ClientType == ClientTypes.SteamWindows || StaticUserData.ClientType == ClientTypes.SteamLinux || StaticUserData.ClientType == ClientTypes.SteamOsX)
		{
			base.GetComponent<ControlStateChanges>().Disable();
		}
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
	}

	public void OnClick()
	{
		if (PhotonConnectionFactory.Instance.IsConnectedToMaster)
		{
			StaticUserData.IsSignInToServer = false;
			GracefulDisconnectHandler.Disconnect();
			try
			{
				ObscuredPrefs.DeleteKey("Email");
				ObscuredPrefs.DeleteKey("Password");
			}
			catch (Exception)
			{
			}
		}
	}

	private void OnDisconnect()
	{
		if (this.BaseForm.GetComponent<ActivityState>() && !DisconnectServerAction.IsQuitDisconnect)
		{
			SceneController.CallAction(ScenesList.Logged, SceneStatuses.Disconnected, this, null);
		}
	}

	public GameObject BaseForm;
}
