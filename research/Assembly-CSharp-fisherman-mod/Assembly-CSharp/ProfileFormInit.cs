using System;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using CodeStage.AntiCheat.ObscuredTypes;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileFormInit : MonoBehaviour
{
	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnGotProfile += this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnProfileUpdated += this.OnProfileUpdated;
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed += this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnChangePassword += this.OnChangePassword;
		this.changePasswordForm.SetActive(false);
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotProfile -= this.OnGotProfile;
		PhotonConnectionFactory.Instance.OnProfileUpdated -= this.OnProfileUpdated;
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnProfileOperationFailed -= this.OnProfileOperationFailed;
		PhotonConnectionFactory.Instance.OnChangePassword -= this.OnChangePassword;
	}

	public void SaveProfile()
	{
		if (!this.nameLabel.GetComponent<CheckUniqueUsername>().IsCorrect)
		{
			return;
		}
		this.SaveButton.interactable = false;
		this.CancelButton.interactable = false;
		PhotonConnectionFactory.Instance.Profile.Name = this.nameLabel.text;
		PhotonConnectionFactory.Instance.Profile.PlaceOfResidence = this.placeOfResidenceLabel.text;
		PhotonConnectionFactory.Instance.Profile.FavoriteFish = this.fishLabel.text;
		PhotonConnectionFactory.Instance.Profile.FavoritePond = this.pondLabel.text;
		PhotonConnectionFactory.Instance.Profile.FavoriteFishingMethod = this.fishingMethodLabel.text;
		PhotonConnectionFactory.Instance.Profile.FavoriteTackle = this.tackleLabel.text;
		PhotonConnectionFactory.Instance.Profile.Birthday = new DateTime?(this.dateTimePicker.SelectedDate);
		PhotonConnectionFactory.Instance.Profile.Gender = ((!this.maleToggle.isOn) ? "F" : "M");
		PhotonConnectionFactory.Instance.UpdateProfile(PhotonConnectionFactory.Instance.Profile);
	}

	private void Confirm_ActionCalled(object sender, EventArgs e)
	{
		DisconnectServerAction.IsQuitDisconnect = true;
		GracefulDisconnectHandler.Disconnect();
	}

	private void OnDisconnect()
	{
		if (DisconnectServerAction.IsQuitDisconnect)
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	public void ShowChangePassword()
	{
		this.incorrectOldPasswordMessage.SetActive(false);
		this.oldPasswordMatchMessage.SetActive(false);
		this.newPassword.text = string.Empty;
		this.confirmedPassword.text = string.Empty;
		this.oldPassword.text = string.Empty;
		this.newPassword.GetComponent<InputCheckingPassword>().ClearChecks();
		this.confirmedPassword.GetComponent<InputCheckingConfirmPassword>().ClearChecks();
		this.oldPassword.GetComponent<InputCheckingOldPassword>().ClearChecks();
		this.changePasswordForm.SetActive(true);
	}

	public void HideChangePassword()
	{
		this.changePasswordForm.SetActive(false);
	}

	public void ChangePassword()
	{
		this.incorrectOldPasswordMessage.SetActive(false);
		InputCheckingPassword component = this.newPassword.GetComponent<InputCheckingPassword>();
		InputCheckingConfirmPassword component2 = this.confirmedPassword.GetComponent<InputCheckingConfirmPassword>();
		InputCheckingOldPassword component3 = this.oldPassword.GetComponent<InputCheckingOldPassword>();
		component3.OnChange();
		component.OnChange();
		component2.OnChange();
		if (component3.isCorrect && component.isCorrect && component2.isCorrect)
		{
			PhotonConnectionFactory.Instance.ChangePassword(this.oldPassword.text, this.newPassword.text);
		}
	}

	private void OnProfileUpdated()
	{
		if (this._isLanguageChanged)
		{
			EventAction component = this._menuHelpers.ShowMessage(this._menuHelpers.MenuPrefabsList.profileForm, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("RestartGameCaption"), false, false, false, null).GetComponent<EventAction>();
			component.ActionCalled += this.Confirm_ActionCalled;
			PhotonConnectionFactory.Instance.GetAvailablePonds(StaticUserData.CountryId);
		}
		PhotonConnectionFactory.Instance.RequestProfile();
	}

	private void OnGotProfile(Profile profile)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		this.SaveButton.interactable = true;
		this.CancelButton.interactable = true;
		if (!this._isLanguageChanged)
		{
			this.buttonSave.Change();
		}
	}

	private void OnChangePassword()
	{
		try
		{
			ObscuredPrefs.SetString("Password", this.newPassword.text);
		}
		catch (Exception)
		{
		}
		this.changePasswordForm.SetActive(false);
	}

	private void OnProfileOperationFailed(ProfileFailure failure)
	{
		if (failure.SubOperation == 247)
		{
			this.oldPasswordMatchMessage.SetActive(false);
			this.incorrectOldPasswordMessage.SetActive(true);
		}
	}

	public void OnEnable()
	{
		KeysInterceptor.IsEnterText = true;
		if (PhotonConnectionFactory.Instance.Profile != null)
		{
			this.emailLabel.text = PhotonConnectionFactory.Instance.Profile.Email;
			this.nameLabel.text = PhotonConnectionFactory.Instance.Profile.Name;
			this.placeOfResidenceLabel.text = PhotonConnectionFactory.Instance.Profile.PlaceOfResidence;
			this.pondLabel.text = PhotonConnectionFactory.Instance.Profile.FavoritePond;
			this.fishLabel.text = PhotonConnectionFactory.Instance.Profile.FavoriteFish;
			this.fishingMethodLabel.text = PhotonConnectionFactory.Instance.Profile.FavoriteFishingMethod;
			this.tackleLabel.text = PhotonConnectionFactory.Instance.Profile.FavoriteTackle;
			if (PhotonConnectionFactory.Instance.Profile.Gender == "F")
			{
				this.maleToggle.isOn = false;
				this.femaleToggle.isOn = true;
			}
			else
			{
				this.femaleToggle.isOn = false;
				this.maleToggle.isOn = true;
			}
			if (PhotonConnectionFactory.Instance.Profile.Birthday != null)
			{
				this.dateTimePicker.SelectedDate = PhotonConnectionFactory.Instance.Profile.Birthday.Value;
			}
		}
	}

	public void OnDisable()
	{
		KeysInterceptor.IsEnterText = false;
	}

	public Text emailLabel;

	public InputField nameLabel;

	public DatePickerInit dateTimePicker;

	public InputField placeOfResidenceLabel;

	public InputField fishLabel;

	public InputField pondLabel;

	public InputField fishingMethodLabel;

	public InputField tackleLabel;

	public GameObject changePasswordForm;

	public InputField oldPassword;

	public InputField newPassword;

	public InputField confirmedPassword;

	public GameObject incorrectOldPasswordMessage;

	public GameObject oldPasswordMatchMessage;

	public ChangeFormByName buttonSave;

	public Toggle maleToggle;

	public Toggle femaleToggle;

	public Button SaveButton;

	public Button CancelButton;

	private MenuHelpers _menuHelpers = new MenuHelpers();

	private bool _isLanguageChanged;
}
