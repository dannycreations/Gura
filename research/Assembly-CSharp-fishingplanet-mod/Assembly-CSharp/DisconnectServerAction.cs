using System;
using System.Collections;
using System.Diagnostics;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectServerAction : MonoBehaviour
{
	internal void Awake()
	{
		DisconnectServerAction.Instance = this;
		this.DisconnectPanel.SetActive(false);
		this.SteamFailedPanel.SetActive(false);
		this.SteamFailedPanel.SetActive(false);
		this.ConnectionFailedPanel.SetActive(false);
		this.UWPSignOutCompletePanel.SetActive(false);
		this.AuthenticateErrorPanel.SetActive(false);
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect1;
		PhotonConnectionFactory.Instance.OnConnectionFailed += this.OnConnectionFailed1;
		PhotonConnectionFactory.Instance.OnGotProtocolVersion += this.OnGotProtocolVersion;
		PhotonConnectionFactory.Instance.OnConnectedToMaster += this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnDuplicateLogin += this.OnDuplicateLogin;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed += this.OnAuthenticationFailed;
	}

	private void Instance_OnErrorGetingProfile()
	{
		this.PSNProfileError();
	}

	private void Instance_OnNewPatchExists()
	{
		this.PSNewPatchExist.SetActive(true);
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect1;
		PhotonConnectionFactory.Instance.OnConnectionFailed -= this.OnConnectionFailed1;
		PhotonConnectionFactory.Instance.OnGotProtocolVersion -= this.OnGotProtocolVersion;
		PhotonConnectionFactory.Instance.OnConnectedToMaster -= this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnDuplicateLogin -= this.OnDuplicateLogin;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed -= this.OnAuthenticationFailed;
	}

	private bool VerifyPreviewAndCustomizationScenesClosed(Action action)
	{
		if (LoadCustomizationScene.ActiveInstance != null)
		{
			if (LoadCustomizationScene.ActiveInstance.IsLoadingScene && !this.isWaiting)
			{
				InfoMessageController.Instance.StartCoroutine(this.WaitForCharacterCustomizationLoadFinishAndCall(action));
				return false;
			}
			LoadCustomizationScene.ActiveInstance.UnloadScene();
		}
		if (LoadPreviewScene.ActiveInstance != null)
		{
			LoadPreviewScene.ActiveInstance.UnloadScene();
		}
		return true;
	}

	public void ShowConnectionFailedMessage()
	{
		if (this.isWaiting)
		{
			return;
		}
		if (!this.VerifyPreviewAndCustomizationScenesClosed(new Action(this.ShowConnectionFailedMessage)))
		{
			return;
		}
		if (!DisconnectServerAction.IsQuitDisconnect && !this.ConnectionFailedPanel.activeSelf)
		{
			CursorManager.ShowCursor();
			Time.timeScale = 0f;
			this.DisconnectMessage.text = ScriptLocalization.Get("ConnectionLostMessage").Replace("<br>", "\n");
			this._isDuplicateLogin = false;
			DisconnectServerAction._protocolRequested = false;
			this.DisconnectPanel.SetActive(true);
			base.StopAllCoroutines();
			base.StartCoroutine(this.WaitForPress());
		}
	}

	private void OnConnectionFailed1()
	{
		this.ShowConnectionFailedMessage();
	}

	public void UnableInternetConnection()
	{
		this.ConnectionFailedMessage.text = ScriptLocalization.Get("FirstConnectionFailed").Replace("<br>", "\n");
		this.ConnectionFailedPanel.SetActive(true);
		base.StartCoroutine(this.WaitForPress());
	}

	private IEnumerator WaitForCharacterCustomizationLoadFinishAndCall(Action action)
	{
		this.isWaiting = true;
		while (LoadCustomizationScene.ActiveInstance.IsLoadingScene)
		{
			yield return null;
		}
		this.isWaiting = false;
		if (action != null)
		{
			action();
		}
		yield break;
	}

	private void OnDisconnect1()
	{
		if (this.isWaiting)
		{
			return;
		}
		if (!this.VerifyPreviewAndCustomizationScenesClosed(new Action(this.OnDisconnect1)))
		{
			return;
		}
		if (StaticUserData.IsSignInToServer && !DisconnectServerAction.IsQuitDisconnect && !this.ConnectionFailedPanel.activeSelf)
		{
			CursorManager.ShowCursor();
			Time.timeScale = 0f;
			this.DisconnectMessage.text = ScriptLocalization.Get("ConnectionLostMessage").Replace("<br>", "\n");
			if (this._isDuplicateLogin)
			{
				this.DisconnectMessage.text = ScriptLocalization.Get("ConnectionWasAnotherComputerMessage");
			}
			this._isDuplicateLogin = false;
			DisconnectServerAction._protocolRequested = false;
			this.DisconnectPanel.SetActive(true);
			base.StopAllCoroutines();
			base.StartCoroutine(this.WaitForPress());
		}
		if (DisconnectServerAction.IsQuitDisconnect)
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	private void OnDuplicateLogin()
	{
		this._isDuplicateLogin = true;
	}

	private void OnConnectedToMaster()
	{
		Debug.Log("OnConnectedToMaster");
		StaticUserData.StartConnection = false;
		if (!DisconnectServerAction._protocolRequested)
		{
			Debug.Log("GetProtocolVersion");
			DisconnectServerAction._protocolRequested = true;
			PhotonConnectionFactory.Instance.GetProtocolVersion();
		}
		else
		{
			base.StartCoroutine(this.WaitForPress());
		}
	}

	private void OnGotProtocolVersion(bool match)
	{
		if (!match)
		{
			CursorManager.ShowCursor();
			Time.timeScale = 0f;
			this.ObsoleteVersionMessage.text = ScriptLocalization.Get("VersionObsoleteMessage").Replace("<br>", "\n");
			base.StartCoroutine(this.WaitForPress());
			this.ObsoleteVersionPanel.SetActive(true);
		}
	}

	private void OnAuthenticationFailed(Failure failure)
	{
		Debug.LogError("OnAuthenticationFailed");
		if (failure.ErrorCode == 32587 || failure.ErrorCode == 32588 || failure.ErrorCode == 32586)
		{
			CursorManager.ShowCursor();
			Time.timeScale = 0f;
			if (failure.ErrorCode == 32587)
			{
				this.ObsoleteVersionMessage.text = ScriptLocalization.Get("AccountBanned").Replace("<br>", "\n");
			}
			else if (failure.ErrorCode == 32588)
			{
				this.ObsoleteVersionMessage.text = ScriptLocalization.Get("AccountLocked").Replace("<br>", "\n");
			}
			else if (failure.ErrorCode == 32586)
			{
				this.ObsoleteVersionMessage.text = ScriptLocalization.Get("AccountUnapproved").Replace("<br>", "\n");
			}
			this.ObsoleteVersionPanel.SetActive(true);
		}
	}

	public void OnDisconnectContinueClick(bool afterDisconnect)
	{
		bool flag = StaticUserData.ResetStaticUserDate(afterDisconnect);
		TransferToLocation.IsMoving = false;
		BackToLobbyClick.IsLeaving = false;
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect1;
		PhotonConnectionFactory.Instance.OnConnectionFailed -= this.OnConnectionFailed1;
		PhotonConnectionFactory.Clear();
		if (PhotonConnectionFactory.Instance.Peer != null && PhotonConnectionFactory.Instance.Peer.PeerState != 3)
		{
			DebugUtility.Connection.Trace("OnDisconnectContinueClick (soft)", new object[0]);
			PhotonConnectionFactory.Instance.Reset();
			DebugUtility.RoomIdIssue.Trace("Clear RoomId - Reset", new object[0]);
			DebugUtility.RoomIdIssue.Trace("Clear roomToEnter - Reset", new object[0]);
		}
		else
		{
			DebugUtility.Connection.Trace("OnDisconnectContinueClick", new object[0]);
			PhotonConnectionFactory.Instance.Disconnect();
		}
		if (flag)
		{
			PhotonConnectionFactory.Instance.ReuseCaches();
			CacheLibrary.SubscribeAllCaches();
		}
		DisconnectServerAction._protocolRequested = false;
		ManagerScenes.Instance.LoadStartScene();
	}

	public void AuthenticateError(DisconnectServerAction.AuthenticateErrorCodes code)
	{
		this.AuthenticateErrorPanel.SetActive(true);
		this.AuthenticateErrorMessage.text = string.Format("Authentication error code # {0}. \n Please try again later or contact support@fishingplanet.com.", (int)code);
		Debug.LogError("AuthenticateError()----------");
		base.StartCoroutine(this.WaitForPress());
	}

	public void PSNProfileError()
	{
		this.AuthenticateErrorPanel.SetActive(true);
		this.AuthenticateErrorMessage.text = ScriptLocalization.Get("PSNProfileError");
		Debug.LogError("PSNProfileError()----------");
		base.StartCoroutine(this.WaitForPress());
	}

	public void BlockedUserMultiplayerPrivilege()
	{
		this.XboxInvalidUserPanel.SetActive(true);
		this.XboxInvalidUserMessage.text = ScriptLocalization.Get("BlockedMultiplayerPrivilege");
		Debug.LogError("BlockedUserMultiplayerPrivilege()----------");
		base.StartCoroutine(this.WaitForPress());
	}

	private IEnumerator WaitForPress()
	{
		yield return null;
		while (!InputManager.AnyKeyIsPressed && !InputManager.ActiveDevice.AnyButtonIsPressed && !ControlsController.ControlsActions.GetMouseButtonDownMandatory(0) && !ControlsController.ControlsActions.GetMouseButtonDownMandatory(1))
		{
			yield return null;
		}
		if (this.ConnectionFailedPanel.activeSelf || this.DisconnectPanel.activeSelf || this.AuthenticateErrorPanel.activeSelf)
		{
			this.OnDisconnectContinueClick(true);
		}
		if (this.ObsoleteVersionPanel.activeSelf || this.UWPSignOutCompletePanel.activeSelf)
		{
			DisconnectServerAction.IsQuitDisconnect = true;
			GracefulDisconnectHandler.Disconnect();
		}
		yield break;
	}

	public static bool IsQuitDisconnect;

	public GameObject DisconnectPanel;

	public Text DisconnectMessage;

	public GameObject ObsoleteVersionPanel;

	public GameObject SteamFailedPanel;

	public GameObject ConnectionFailedPanel;

	public GameObject PSInvalidUserPanel;

	public GameObject XboxInvalidUserPanel;

	public GameObject PSNewPatchExist;

	public GameObject UWPSignOutCompletePanel;

	public GameObject AuthenticateErrorPanel;

	public Text ConnectionFailedMessage;

	public Text ObsoleteVersionMessage;

	public Text PSInvalidUserMessage;

	public Text XboxInvalidUserMessage;

	public Text UwpSignoutMessage;

	public Text AuthenticateErrorMessage;

	private bool _isDuplicateLogin;

	public static DisconnectServerAction Instance;

	private static bool _protocolRequested;

	private bool isWaiting;

	public enum AuthenticateErrorCodes
	{
		XstsIsNull = 1,
		XstsUnknownException,
		XstsDidNotArrive,
		ProfileWasNotLoaded,
		EncryptionFailedToEstablish,
		ExceptionOnConnect,
		SecurityExceptionOnConnect,
		UnknownError = 99
	}
}
