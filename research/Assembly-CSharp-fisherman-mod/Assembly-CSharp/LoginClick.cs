using System;
using System.Security.Authentication;
using CodeStage.AntiCheat.ObscuredTypes;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class LoginClick : MonoBehaviour
{
	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnGotProfile += this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed += this.OnAuthenticationFailed;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotProfile -= this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed -= this.OnAuthenticationFailed;
	}

	public void OnClick()
	{
		this._isAuthenticating = false;
		string text = this.EmailControl.GetComponent<InputField>().text;
		string text2 = this.PasswordControl.GetComponent<InputField>().text;
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2) || text == "Email" || text2 == "Password")
		{
			GUITools.SetActive(this.EnterMessageControl, true);
		}
		else
		{
			GUITools.SetActive(this.EnterMessageControl, false);
			if (PhotonConnectionFactory.Instance.IsConnectedToMaster)
			{
				StaticUserData.IsSignInToServer = false;
				PhotonConnectionFactory.Instance.Disconnect();
			}
			PhotonConnectionFactory.Instance.Email = text;
			PhotonConnectionFactory.Instance.Password = text2;
			StaticUserData.ConnectToMasterWrapper();
			StaticUserData.IsSignInToServer = true;
		}
	}

	public void Update()
	{
		if (this.LoginForm.GetComponent<ActivityState>().isActive && PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.IsConnectedToMaster && PhotonConnectionFactory.Instance.IsAuthenticated && !this._isAuthenticating)
		{
			this._isAuthenticating = true;
			PhotonConnectionFactory.Instance.SetAddProps(ChangeLanguage.GetCurrentLanguage.Id, null, false);
			PhotonConnectionFactory.Instance.RequestProfile();
			StaticUserData.IsSignInToServer = true;
			string text = this.EmailControl.GetComponent<InputField>().text;
			string text2 = this.PasswordControl.GetComponent<InputField>().text;
			GUITools.SetActive(this.IncorrectMessageControl, false);
			Toggle component = this.RememberMeControl.GetComponent<Toggle>();
			if (component != null && component.isOn)
			{
				try
				{
					ObscuredPrefs.SetString("Email", text);
					ObscuredPrefs.SetString("Password", text2);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void OnGotProfile(Profile profile)
	{
		SceneController.CallAction(ScenesList.Login, SceneStatuses.ConnectedToMaster, this, null);
	}

	private void OnAuthenticationFailed(Failure failure)
	{
		StaticUserData.IsSignInToServer = false;
		if (this.LoginForm.GetComponent<ActivityState>().isActive)
		{
			GUITools.SetActive(this.IncorrectMessageControl, true);
		}
		throw new AuthenticationException(failure.ErrorMessage);
	}

	public GameObject EnterMessageControl;

	public GameObject IncorrectMessageControl;

	public InputField EmailControl;

	public InputField PasswordControl;

	public Toggle RememberMeControl;

	public Canvas LoginForm;

	private bool _isAuthenticating;
}
