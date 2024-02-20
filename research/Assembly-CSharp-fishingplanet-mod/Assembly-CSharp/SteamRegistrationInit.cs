using System;
using System.Collections;
using System.Text.RegularExpressions;
using CodeStage.AntiCheat.ObscuredTypes;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SteamRegistrationInit : MonoBehaviour
{
	internal void OnEnable()
	{
		this._passwordGuid = default(Guid).ToString();
		if (this.Email != null)
		{
			EventSystem.current.SetSelectedGameObject(this.Email.gameObject, null);
			this.Email.OnPointerClick(new PointerEventData(EventSystem.current));
		}
		PhotonConnectionFactory.Instance.OnRegisterUser += this.OnRegisterUser;
		PhotonConnectionFactory.Instance.OnPromoCodeInvalid += this.OnPromoCodeInvalid;
		PhotonConnectionFactory.Instance.OnPromoCodeValid += this.OnPromoCodeValid;
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnConnectedToMaster += this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed += this.OnAuthenticationFailed;
		PhotonConnectionFactory.Instance.OnCheckEmailIsUnique += this.OnCheckEmailIsUnique;
		PhotonConnectionFactory.Instance.OnCheckUsernameIsUnique += this.OnCheckUsernameIsUnique;
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnRegisterUser -= this.OnRegisterUser;
		PhotonConnectionFactory.Instance.OnPromoCodeInvalid -= this.OnPromoCodeInvalid;
		PhotonConnectionFactory.Instance.OnPromoCodeValid -= this.OnPromoCodeValid;
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnConnectedToMaster -= this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed -= this.OnAuthenticationFailed;
		PhotonConnectionFactory.Instance.OnCheckEmailIsUnique -= this.OnCheckEmailIsUnique;
		PhotonConnectionFactory.Instance.OnCheckUsernameIsUnique -= this.OnCheckUsernameIsUnique;
	}

	public void OnRegisterUser(Profile profile)
	{
		this._isRegistered = true;
		PhotonConnectionFactory.Instance.SetAddProps(ChangeLanguage.GetCurrentLanguage.Id, null, false);
		StaticUserData.IsSignInToServer = true;
		MeasuringSystemManager.ChangeMeasuringSystem();
		this.LoadCustomizationScene.UnloadedScene += delegate(object e, EventArgs obj)
		{
			SceneController.CallAction(ScenesList.Registration, SceneStatuses.RegisterComplete, this, profile);
		};
		this.LoadCustomizationScene.ShowFirstTime();
	}

	public void OnChange()
	{
		this.Name.text = this.Name.text.ReplaceNonLatin();
	}

	public void Register()
	{
		if (this._isRegistering)
		{
			return;
		}
		this._isRegistering = true;
		this._correctEmail = 0;
		this._correctName = 0;
		this._correctPromoCode = 0;
		this.ClearSavedAuth();
		this.EnterEmail();
		base.StartCoroutine(this.CheckName());
	}

	private IEnumerator CheckName()
	{
		if (this._isRegistered)
		{
			yield break;
		}
		while (this._correctEmail == 0)
		{
			yield return null;
		}
		if (this._correctEmail == 2)
		{
			this._isRegistering = false;
			yield break;
		}
		this.EnterName();
		base.StartCoroutine(this.CheckPromoCode());
		yield break;
	}

	private IEnumerator CheckPromoCode()
	{
		while (this._correctName == 0)
		{
			yield return null;
		}
		if (this._correctName == 2)
		{
			this._isRegistering = false;
			yield break;
		}
		this.EnterPromoCode();
		base.StartCoroutine(this.RegisterCoroutine());
		yield break;
	}

	private IEnumerator RegisterCoroutine()
	{
		while (this._correctEmail == 0 || this._correctName == 0 || this._correctPromoCode == 0)
		{
			yield return null;
		}
		if (this._correctEmail == 2 || this._correctName == 2 || this._correctPromoCode == 2)
		{
			this._isRegistering = false;
			yield break;
		}
		bool nameOk = this.Name.text != string.Empty;
		bool emailOk = this.Email != null && !string.IsNullOrEmpty(this.Email.text) && this.Email.text != "E-mail";
		if (nameOk && emailOk)
		{
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
		yield break;
	}

	private void ClearSavedAuth()
	{
		if (!this._cleared)
		{
			this._cleared = true;
			string text = StaticUserData.SteamId.ToString();
			ObscuredPrefs.DeleteKey("Email" + text);
			ObscuredPrefs.DeleteKey("Password" + text);
			PhotonConnectionFactory.Instance.Email = null;
			PhotonConnectionFactory.Instance.Password = null;
			PhotonConnectionFactory.Instance.BackupSteamAuthTicket();
		}
	}

	private void OnConnectedToMaster()
	{
		StaticUserData.StartConnection = false;
		StaticUserData.IsSignInToServer = true;
		string text = string.Empty;
		text = this.Email.text;
		if (this._isRegistering)
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			this._isRegistering = false;
			if (StaticUserData.ClientType == ClientTypes.SteamWindows || StaticUserData.ClientType == ClientTypes.SteamOsX || StaticUserData.ClientType == ClientTypes.SteamLinux)
			{
				PhotonConnectionFactory.Instance.RegisterNewAccount(this.Name.text, this._passwordGuid, text, ChangeLanguage.GetCurrentLanguage.Id, "Steam", StaticUserData.SteamId.ToString(), this.PromoCode.text);
			}
		}
	}

	public void EnterEmail()
	{
		if (this._isRegistered)
		{
			return;
		}
		if (!string.IsNullOrEmpty(this.Email.text) && this.Email.text != "E-mail" && SteamRegistrationInit.rgxEmail.IsMatch(this.Email.text))
		{
			PhotonConnectionFactory.Instance.CheckEmailIsUnique(this.Email.text);
		}
		else
		{
			this._correctEmail = 2;
			this.InfoText.color = Color.red;
			this.InfoText.text = ScriptLocalization.Get("EmailInvalidFormat");
		}
	}

	private void OnCheckEmailIsUnique(bool unique)
	{
		if (unique)
		{
			this._correctEmail = 1;
			this.InfoText.color = Color.white;
			this.InfoText.text = string.Empty;
		}
		else
		{
			this._correctEmail = 2;
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
			this.InfoText.color = Color.red;
			this.InfoText.text = ScriptLocalization.Get("EmailAlreadyUse");
		}
	}

	public void EnterPromoCode()
	{
		if (string.IsNullOrEmpty(this.PromoCode.text))
		{
			this._correctPromoCode = 1;
			return;
		}
		if (this.PromoCode.text.Length == 10 && SteamRegistrationInit.rgxPromoCode.IsMatch(this.PromoCode.text))
		{
			PhotonConnectionFactory.Instance.CheckPromoCode(this.PromoCode.text);
		}
		else
		{
			this._correctPromoCode = 2;
			this.InfoText.color = Color.red;
			this.InfoText.text = ScriptLocalization.Get("IncorrectPromoCode");
		}
	}

	private void OnPromoCodeValid()
	{
		this._correctPromoCode = 1;
		this.InfoText.color = Color.white;
		this.InfoText.text = string.Empty;
	}

	private void OnPromoCodeInvalid(Failure failure)
	{
		if (!string.IsNullOrEmpty(this.PromoCode.text))
		{
			this._correctPromoCode = 2;
			this.InfoText.color = Color.red;
			this.InfoText.text = ScriptLocalization.Get("IncorrectPromoCode");
		}
		else
		{
			this._correctPromoCode = 1;
			this.InfoText.color = Color.white;
			this.InfoText.text = string.Empty;
		}
	}

	public void EnterName()
	{
		if (!string.IsNullOrEmpty(this.Name.text) && SteamRegistrationInit.rgxUsername.IsMatch(this.Name.text) && !AbusiveWords.HasAbusiveWords(this.Name.text))
		{
			PhotonConnectionFactory.Instance.CheckUsernameIsUnique(this.Name.text);
		}
		else
		{
			this._correctName = 2;
			this.InfoText.color = Color.red;
			this.InfoText.text = ScriptLocalization.Get("UsernameInvalidFormat");
		}
	}

	private void OnCheckUsernameIsUnique(bool unique)
	{
		if (unique)
		{
			this._correctName = 1;
			this.InfoText.color = Color.white;
			this.InfoText.text = string.Empty;
		}
		else
		{
			this._correctName = 2;
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
			this.InfoText.color = Color.red;
			this.InfoText.text = ScriptLocalization.Get("UsernameAlreadyUse");
		}
	}

	private void OnDisconnect()
	{
		if (this._isRegistering)
		{
			PhotonConnectionFactory.Instance.ConnectToMasterNoAuth();
		}
	}

	private void OnAuthenticationFailed(Failure failure)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
	}

	public InputField Email;

	public InputField Name;

	public InputField PromoCode;

	public Text InfoText;

	public Button JoinButton;

	public LoadCustomizationScene LoadCustomizationScene;

	private string _passwordGuid;

	private bool _isRegistering;

	private bool _isRegistered;

	private bool _cleared;

	private static string patternEmail = "\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z";

	protected static Regex rgxEmail = new Regex(SteamRegistrationInit.patternEmail, RegexOptions.IgnoreCase);

	private static string patternUsername = "^(?=.{3,30}$)(?![_.-])(?!.*[_.-]{2})[a-zA-Z0-9._-]+(?<![_.-])$";

	internal static Regex rgxUsername = new Regex(SteamRegistrationInit.patternUsername, RegexOptions.IgnoreCase);

	private static string patternPromoCode = "^[A-Z1-9]*$";

	internal static Regex rgxPromoCode = new Regex(SteamRegistrationInit.patternPromoCode);

	private byte _correctName;

	private byte _correctEmail;

	private byte _correctPromoCode;
}
