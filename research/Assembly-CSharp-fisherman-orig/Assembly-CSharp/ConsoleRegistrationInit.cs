using System;
using System.Collections;
using System.Text.RegularExpressions;
using I2.Loc;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleRegistrationInit : MonoBehaviour
{
	protected virtual void OnEnable()
	{
		this.InfoText.text = string.Format("{0} {1}", "\ue710", ScriptLocalization.Get("EnterText"));
		this._passwordGuid = default(Guid).ToString();
		EventSystem.current.firstSelectedGameObject = this.Email.gameObject;
		PhotonConnectionFactory.Instance.OnRegisterUser += this.OnRegisterUser;
		PhotonConnectionFactory.Instance.OnPromoCodeInvalid += this.OnPromoCodeInvalid;
		PhotonConnectionFactory.Instance.OnPromoCodeValid += this.OnPromoCodeValid;
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnConnectedToMaster += this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnCheckEmailIsUnique += this.OnCheckEmailIsUnique;
	}

	internal void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnRegisterUser -= this.OnRegisterUser;
		PhotonConnectionFactory.Instance.OnPromoCodeInvalid -= this.OnPromoCodeInvalid;
		PhotonConnectionFactory.Instance.OnPromoCodeValid -= this.OnPromoCodeValid;
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnConnectedToMaster -= this.OnConnectedToMaster;
		PhotonConnectionFactory.Instance.OnCheckEmailIsUnique -= this.OnCheckEmailIsUnique;
	}

	private void Update()
	{
		if (!this._registered)
		{
			if (this._canRegister && InputManager.ActiveDevice.GetControl(InputControlType.Action3).WasPressed)
			{
				this.InfoText.text = ScriptLocalization.Get("RegistrationInfoText");
				this.Register();
			}
			if (InputManager.ActiveDevice.GetControl(InputControlType.Action1).WasPressed && !ScreenKeyboard.IsOpened)
			{
				this.Email.GetComponent<ScreenKeyboard>().OnInputFieldSelect(this.Email.text.TrimEnd(new char[0]), false, ScreenKeyboard.VirtualKeyboardScope.EmailSmtpAddress);
			}
		}
	}

	public virtual void Register()
	{
		if (!this._isRegestring)
		{
			this._isRegestring = true;
			this._correctEmail = 0;
			this._correctPromoCode = 0;
			this.EnterEmail();
			base.StartCoroutine(this.CheckPromoCode());
		}
	}

	protected IEnumerator CheckPromoCode()
	{
		while (this._correctEmail == 0)
		{
			yield return null;
		}
		if (this._correctEmail == 2)
		{
			this._isRegestring = false;
			yield break;
		}
		Object.Destroy(this.Email.GetComponent<ScreenKeyboard>());
		this.EnterPromoCode();
		base.StartCoroutine(this.RegisterCoroutine());
		yield break;
	}

	private IEnumerator RegisterCoroutine()
	{
		while (this._correctEmail == 0 || this._correctPromoCode == 0)
		{
			yield return null;
		}
		if (this._correctEmail == 2 || this._correctPromoCode == 2)
		{
			this._isRegestring = false;
			yield break;
		}
		this.InfoText.text = ScriptLocalization.Get("RegistrationInfoText");
		if (PhotonConnectionFactory.Instance.IsConnectedToMaster)
		{
			StaticUserData.IsSignInToServer = false;
			PhotonConnectionFactory.Instance.Disconnect();
		}
		else
		{
			PhotonConnectionFactory.Instance.ConnectToMasterNoAuth();
		}
		yield break;
	}

	public void OnRegisterUser(Profile profile)
	{
		this._registered = true;
		PhotonConnectionFactory.Instance.SetAddProps(ChangeLanguage.GetCurrentLanguage.Id, null, false);
		StaticUserData.IsSignInToServer = true;
		MeasuringSystemManager.ChangeMeasuringSystem();
		this.PromoCode.GetComponent<ScreenKeyboard>().enabled = false;
		this.Panel.SetActive(false);
		this.EulaScreenInit.gameObject.SetActive(true);
		this.EulaScreenInit.EULAAccepted += this.EulaScreenInit_EULAAccepted;
	}

	private void EulaScreenInit_EULAAccepted(object sender, EventArgs e)
	{
		this.Panel.SetActive(true);
		this.EulaScreenInit.gameObject.SetActive(false);
		this.LoadCustomizationScene.UnloadedScene += delegate(object eh, EventArgs obj)
		{
			SceneController.CallAction(ScenesList.Registration, SceneStatuses.RegisterComplete, this, PhotonConnectionFactory.Instance.Profile);
		};
		this.LoadCustomizationScene.ShowFirstTime();
	}

	public void EnterPromoCode()
	{
		if (string.IsNullOrEmpty(this.PromoCode.text))
		{
			this._correctPromoCode = 1;
			return;
		}
		if (this.PromoCode.text.Length == 10 && ConsoleRegistrationInit.rgxPromoCode.IsMatch(this.PromoCode.text))
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

	public void EnterEmail()
	{
		this._canRegister = false;
		if (!string.IsNullOrEmpty(this.Email.text.TrimEnd(new char[0])) && this.Email.text != "Name" && this.Email.text.TrimEnd(new char[0]) != Localization.Get("Name") && ConsoleRegistrationInit.rgx.IsMatch(this.Email.text.TrimEnd(new char[0])))
		{
			PhotonConnectionFactory.Instance.CheckEmailIsUnique(this.Email.text.TrimEnd(new char[0]));
		}
		else
		{
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
			this._canRegister = true;
		}
		else
		{
			this.InfoText.color = Color.red;
			this.InfoText.text = ScriptLocalization.Get("EmailAlreadyUse");
		}
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
		}
	}

	public InputField Email;

	public InputField PromoCode;

	public Text InfoText;

	public LoadCustomizationScene LoadCustomizationScene;

	public EULAScreenInit EulaScreenInit;

	public GameObject Panel;

	protected bool _isRegestring;

	private bool _registered;

	protected string _passwordGuid;

	protected string _username;

	private bool _canRegister;

	protected byte _correctEmail;

	protected byte _correctPromoCode;

	private static string pattern = "\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z";

	protected static Regex rgx = new Regex(ConsoleRegistrationInit.pattern, RegexOptions.IgnoreCase);

	private static string patternPromoCode = "^[A-Z1-9]*$";

	internal static Regex rgxPromoCode = new Regex(ConsoleRegistrationInit.patternPromoCode);
}
