using System;
using UnityEngine;

public class GotoRegisterForm : MonoBehaviour
{
	public void OnClick()
	{
		if (!PhotonConnectionFactory.Instance.IsConnectedToMaster)
		{
			StaticUserData.ConnectToMasterWrapper();
			this._isRegisterClick = true;
		}
		else
		{
			SceneController.CallAction(ScenesList.Login, SceneStatuses.GotoRegister, this, null);
		}
	}

	internal void Update()
	{
		if (this._isRegisterClick && PhotonConnectionFactory.Instance.IsConnectedToMaster)
		{
			this._isRegisterClick = false;
			SceneController.CallAction(ScenesList.Login, SceneStatuses.GotoRegister, this, null);
		}
	}

	private bool _isRegisterClick;
}
