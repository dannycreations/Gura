using System;
using CodeStage.AntiCheat.ObscuredTypes;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegisterClick : MonoBehaviour
{
	internal void OnEnable()
	{
		PhotonConnectionFactory.Instance.OnConnectedToMaster += this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnRegisterUser += this.OnRegisterUser;
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed += this.OnProfileOperationFailed;
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnConnectedToMaster -= this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnRegisterUser -= this.OnRegisterUser;
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed -= this.OnProfileOperationFailed;
	}

	public void OnClick()
	{
		InputCheckingEmail component = this.EmailLabel.GetComponent<InputCheckingEmail>();
		InputCheckingName component2 = this.NameLabel.GetComponent<InputCheckingName>();
		InputCheckingPassword component3 = this.PasswordLabel.GetComponent<InputCheckingPassword>();
		InputCheckingConfirmPassword component4 = this.ConfirmPasswordLabel.GetComponent<InputCheckingConfirmPassword>();
		if (component.isCorrect && component2.isCorrect && component3.isCorrect && component4.isCorrect)
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			Debug.Log("All register fields is correct");
			this.isRegistering = true;
			this.Register();
		}
	}

	private void Register()
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		if (PhotonConnectionFactory.Instance.IsConnectedToMaster)
		{
			StaticUserData.IsSignInToServer = false;
			PhotonConnectionFactory.Instance.Disconnect();
		}
		else
		{
			StaticUserData.ConnectToMasterWrapper();
		}
	}

	public void OnRegisterUser(Profile profile)
	{
		if (this.RegisterForm.GetComponent<ActivityState>().isActive)
		{
			PhotonConnectionFactory.Instance.SetAddProps(ChangeLanguage.GetCurrentLanguage.Id, null, false);
			StaticUserData.IsSignInToServer = true;
			MeasuringSystemManager.ChangeMeasuringSystem();
			if (StaticUserData.ClientType != ClientTypes.StandaloneWindows && StaticUserData.ClientType != ClientTypes.StandaloneLinux)
			{
				if (StaticUserData.ClientType != ClientTypes.StandaloneOsX)
				{
					goto IL_A9;
				}
			}
			try
			{
				ObscuredPrefs.SetString("Email", this.EmailLabel.GetComponent<InputField>().text);
				ObscuredPrefs.SetString("Password", this.PasswordLabel.GetComponent<InputField>().text);
			}
			catch (Exception)
			{
			}
			IL_A9:
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
			this.LoadCustomizationScene.UnloadedScene += delegate(object e, EventArgs obj)
			{
				SceneController.CallAction(ScenesList.Registration, SceneStatuses.RegisterComplete, this, profile);
			};
			this.LoadCustomizationScene.ShowFirstTime();
		}
	}

	private void OnProfileOperationFailed(ProfileFailure failure)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		Debug.LogWarning(failure.FullErrorInfo);
	}

	private void OnDisconnect()
	{
		if (this.isRegistering)
		{
			StaticUserData.ConnectToMasterWrapper();
		}
	}

	private void OnConnectedToMaster()
	{
		StaticUserData.StartConnection = false;
		StaticUserData.IsSignInToServer = true;
		if (this.isRegistering)
		{
			this.isRegistering = false;
			if (StaticUserData.ClientType == ClientTypes.SteamWindows || StaticUserData.ClientType == ClientTypes.SteamOsX || StaticUserData.ClientType == ClientTypes.SteamLinux)
			{
				PhotonConnectionFactory.Instance.RegisterNewAccount(this.NameLabel.GetComponent<InputField>().text, this.PasswordLabel.GetComponent<InputField>().text, this.EmailLabel.GetComponent<InputField>().text, ChangeLanguage.GetCurrentLanguage.Id, "Steam", StaticUserData.SteamId.ToString(), null);
			}
			else
			{
				PhotonConnectionFactory.Instance.RegisterNewAccount(this.NameLabel.GetComponent<InputField>().text, this.PasswordLabel.GetComponent<InputField>().text, this.EmailLabel.GetComponent<InputField>().text, ChangeLanguage.GetCurrentLanguage.Id, null, null, null);
			}
		}
	}

	public GameObject NameLabel;

	public GameObject EmailLabel;

	public GameObject PasswordLabel;

	public GameObject ConfirmPasswordLabel;

	public Canvas RegisterForm;

	public LoadCustomizationScene LoadCustomizationScene;

	private bool isRegistering;
}
