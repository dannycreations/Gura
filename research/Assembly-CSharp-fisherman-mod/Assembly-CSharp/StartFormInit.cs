using System;
using System.Collections;
using Assets.Scripts.Common.Managers.Helpers;
using CodeStage.AntiCheat.ObscuredTypes;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class StartFormInit : MonoBehaviour
{
	internal void Init()
	{
		ChangeLanguage.InitLanguage();
		Shader.SetGlobalFloat("UWIntensityRT", 0f);
		PhotonConnectionFactory.Instance.OnGotProfile += this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed += this.OnAuthenticationFailed;
		base.Invoke("StartFormInit_OnStart", 0.5f);
	}

	internal void OnDestroy()
	{
		base.StopAllCoroutines();
		PhotonConnectionFactory.Instance.OnGotProfile -= this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed -= this.OnAuthenticationFailed;
	}

	private void StartFormInit_OnStart()
	{
		this._starTime = DateTime.Now.Ticks;
		if (StaticUserData.ClientType == ClientTypes.SteamWindows || StaticUserData.ClientType == ClientTypes.SteamLinux || StaticUserData.ClientType == ClientTypes.SteamOsX || StaticUserData.ClientType == ClientTypes.MailRuWindows || StaticUserData.ClientType == ClientTypes.MailRuLinux || StaticUserData.ClientType == ClientTypes.TencentWindows)
		{
			StaticUserData.ConnectToMasterWrapper();
		}
		else
		{
			string text = null;
			string text2 = null;
			try
			{
				text = ObscuredPrefs.GetString("Email");
				text2 = ObscuredPrefs.GetString("Password");
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message);
			}
			if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
			{
				PhotonConnectionFactory.Instance.Email = text;
				PhotonConnectionFactory.Instance.Password = text2;
				StaticUserData.ConnectToMasterWrapper();
				StaticUserData.IsSignInToServer = true;
			}
		}
		base.StartCoroutine(this.WaitForConnection());
	}

	public void Update()
	{
		if (base.GetComponent<ActivityState>().CanRun)
		{
			if (!this._inited && (float)(DateTime.Now.Ticks - this._starTime) / 10000f < this._duration * 1000f)
			{
				return;
			}
			if (this.BaseForm.GetComponent<ActivityState>().isActive)
			{
				string text = null;
				string text2 = null;
				try
				{
					text = ObscuredPrefs.GetString("Email");
					text2 = ObscuredPrefs.GetString("Password");
				}
				catch (Exception)
				{
				}
				if (!this._inited && string.IsNullOrEmpty(text) && string.IsNullOrEmpty(text2) && (StaticUserData.ClientType == ClientTypes.StandaloneWindows || StaticUserData.ClientType == ClientTypes.StandaloneLinux || StaticUserData.ClientType == ClientTypes.StandaloneOsX || StaticUserData.ClientType == ClientTypes.WindowsUWP))
				{
					this._inited = true;
					this.messageBox = this.helpers.ShowMessage(base.gameObject.transform.root.gameObject, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("SteamNotRun"), ScriptLocalization.Get("Quit"), true);
					this.messageBox.GetComponent<EventAction>().ActionCalled += this.CompleteMessage_ActionCalled;
				}
				else if (!this._inited && PhotonConnectionFactory.Instance.IsConnectedToMaster && PhotonConnectionFactory.Instance.IsAuthenticated)
				{
					this._inited = true;
					PhotonConnectionFactory.Instance.SetAddProps(ChangeLanguage.GetCurrentLanguage.Id, null, true);
				}
			}
		}
	}

	private void OnAuthenticationFailed(Failure failure)
	{
		if (this.BaseForm.GetComponent<ActivityState>().isActive && failure.ErrorCode == 32592)
		{
			SceneController.CallAction(ScenesList.Starting, SceneStatuses.GotoRegister, this, null);
			return;
		}
		if (!this.BaseForm.GetComponent<ActivityState>().isActive || failure.ErrorCode == 32595)
		{
		}
	}

	private void OnGotProfile(Profile profile)
	{
		StaticUserData.IsSignInToServer = true;
		Debug.Log("GetProfile responce");
		this.CheckTutorialFinishedAndContinue(profile);
	}

	private void EulaScreenInit_EULAAccepted(object sender, EventArgs e)
	{
		this.Panel.SetActive(true);
		this.EulaScreenInit.gameObject.SetActive(false);
		this.CheckTutorialFinishedAndContinue(PhotonConnectionFactory.Instance.Profile);
	}

	private void CheckTutorialFinishedAndContinue(Profile profile)
	{
		if (profile.IsTutorialFinished)
		{
			SceneController.CallAction(ScenesList.Logged, SceneStatuses.ToGame, this, null);
		}
		else
		{
			this.LoadCustomizationScene.UnloadedScene += delegate(object e, EventArgs obj)
			{
				SceneController.CallAction(ScenesList.Registration, SceneStatuses.ToGame, this, profile);
			};
			this.LoadCustomizationScene.ShowFirstTime();
		}
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		DisconnectServerAction.IsQuitDisconnect = true;
		GracefulDisconnectHandler.Disconnect();
	}

	private IEnumerator WaitForConnection()
	{
		float timeStarted = Time.realtimeSinceStartup;
		while (StaticUserData.StartConnection && Time.realtimeSinceStartup - timeStarted < 25f)
		{
			yield return null;
		}
		if (StaticUserData.StartConnection)
		{
			DisconnectServerAction.Instance.UnableInternetConnection();
		}
		yield break;
	}

	public GameObject BaseForm;

	public LoadCustomizationScene LoadCustomizationScene;

	public EULAScreenInit EulaScreenInit;

	public GameObject Panel;

	private float _duration = 1f;

	private long _starTime;

	private bool _inited;

	private MenuHelpers helpers = new MenuHelpers();

	private MessageBox messageBox;

	private const float timeToWait = 25f;
}
