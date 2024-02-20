using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SupportSubmitionForm : SupportBaseForm
{
	private void Awake()
	{
		this._placeholderDescription.text = ScriptLocalization.Get("EnterTextCaption");
	}

	protected override void ClearForm()
	{
		this._subCategory.ClearOptions();
		SupportSubmitionForm.SubcategoryRecord[] array = this._subCategories[this._current];
		for (int i = 0; i < array.Length; i++)
		{
			this._subCategory.AddOption(new Dropdown.OptionData(LocalizationManager.GetTermTranslation(array[i].LocLabel)));
		}
		this._subCategory.value = 0;
		this._subCategory.RefreshShownValue();
		this._title.text = string.Empty;
		this._inputFieldDescription.text = string.Empty;
		this._attachments.text = string.Empty;
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void OnAttachClick()
	{
	}

	public void OnCancelClick()
	{
		this.ClearForm();
	}

	public void OnSendClick()
	{
		if (this._title.text == string.Empty)
		{
			MenuHelpers.Instance.ShowMessage(null, LocalizationManager.GetTermTranslation("SupportFormError"), LocalizationManager.GetTermTranslation("SupportFormErrorNoSummary"), false, true, false, null);
			return;
		}
		if (this._inputFieldDescription.text == string.Empty)
		{
			MenuHelpers.Instance.ShowMessage(null, LocalizationManager.GetTermTranslation("SupportFormError"), LocalizationManager.GetTermTranslation("SupportFormErrorNoText"), false, true, false, null);
			return;
		}
		string text = string.Format("{0}.{1}: {2}", this._current, this._subCategories[this._current][this._subCategory.value].ShortName, this._title.text);
		PhotonConnectionFactory.Instance.CreateSupportTicket(this.AdjustStringLength(text, 256), this.AdjustStringLength(this._inputFieldDescription.text, 4000), null);
		this.ClearForm();
	}

	private void Start()
	{
		this._summaryText.text = LocalizationManager.GetTermTranslation("SupportFormSummary").ToUpper();
		this._descriptionText.text = LocalizationManager.GetTermTranslation("SupportFormDescription").ToUpper();
		this._inputFieldDescription.characterLimit = 4000;
		this._title.characterLimit = 200;
		this.UpdateDescMaxLen();
		this._inputFieldDescription.onValueChanged.AddListener(delegate
		{
			this.UpdateDescMaxLen();
		});
		PhotonConnectionFactory.Instance.OnCreateSupportTicketFailed += this.Instance_OnCreateSupportTicketFailed;
		PhotonConnectionFactory.Instance.OnSupportTicketCreated += this.Instance_OnSupportTicketCreated;
	}

	private void Instance_OnCreateSupportTicketFailed(Failure failure)
	{
		Debug.LogErrorFormat("SupportForm:CreateSupportTicketFailed ErrorMessage:{0}", new object[] { failure.ErrorMessage });
		MenuHelpers.Instance.ShowMessage(null, LocalizationManager.GetTermTranslation("MessageCaption"), LocalizationManager.GetTermTranslation("SupportFormError"), false, true, false, null);
	}

	private void Instance_OnSupportTicketCreated()
	{
		MenuHelpers.Instance.ShowMessage(null, LocalizationManager.GetTermTranslation("SupportFormSuccess"), string.Format(LocalizationManager.GetTermTranslation("SupportFormReportSent"), PhotonConnectionFactory.Instance.Profile.Email), false, true, false, null);
	}

	private void UpdateDescMaxLen()
	{
		this._descMaxLen.text = string.Format(LocalizationManager.GetTermTranslation("SupportFormDescMaxLen"), 4000 - this._inputFieldDescription.text.Length);
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnCreateSupportTicketFailed -= this.Instance_OnCreateSupportTicketFailed;
		PhotonConnectionFactory.Instance.OnSupportTicketCreated -= this.Instance_OnSupportTicketCreated;
		this._inputFieldDescription.onValueChanged.RemoveAllListeners();
	}

	private string AdjustStringLength(string s, int maxSize)
	{
		return (s.Length <= maxSize) ? s : s.Substring(0, maxSize);
	}

	private const int TITLE_MAX_LENGTH_WITHOUT_TECHNICAL_INFO = 200;

	private const int TITLE_MAX_LENGTH = 256;

	private const int DESCRIPTION_MAX_LENGTH = 4000;

	[SerializeField]
	private DropDownList _subCategory;

	[SerializeField]
	private InputField _title;

	[SerializeField]
	private Text _attachments;

	[SerializeField]
	private Text _descMaxLen;

	[SerializeField]
	private Text _summaryText;

	[SerializeField]
	private Text _descriptionText;

	[Space(6f)]
	[SerializeField]
	private TMP_InputField _inputFieldDescription;

	[SerializeField]
	private TextMeshProUGUI _placeholderDescription;

	private readonly Dictionary<SupportCategory, SupportSubmitionForm.SubcategoryRecord[]> _subCategories = new Dictionary<SupportCategory, SupportSubmitionForm.SubcategoryRecord[]>
	{
		{
			SupportCategory.Technical,
			new SupportSubmitionForm.SubcategoryRecord[]
			{
				new SupportSubmitionForm.SubcategoryRecord("SupportFormGraphicsHeader", "Graphics"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormCrashesHeader", "Crashes"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormPerformanceHeader", "Performance"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormTechnicalOtherHeader", "Other")
			}
		},
		{
			SupportCategory.Payments,
			new SupportSubmitionForm.SubcategoryRecord[]
			{
				new SupportSubmitionForm.SubcategoryRecord("SupportFormPaymentIssuesHeader", "PaymentIssues"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormMissingItemsHeader", "MissingItems"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormMissingCurrencyHeader", "MissingCurrency"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormPaymentsOtherHeader", "Other")
			}
		},
		{
			SupportCategory.GamePlay,
			new SupportSubmitionForm.SubcategoryRecord[]
			{
				new SupportSubmitionForm.SubcategoryRecord("SupportFormProfileIssuesHeader", "ProfileIssues"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormGameplayBugsHeader", "GameplayBugs"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormLicensesHeader", "Licenses"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormCompetitiveIssuesHeader", "CompetitiveIssues"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormGameplayOtherHeader", "Other")
			}
		},
		{
			SupportCategory.Feedback,
			new SupportSubmitionForm.SubcategoryRecord[]
			{
				new SupportSubmitionForm.SubcategoryRecord("SupportFormWriteAFeedback", "Feedback"),
				new SupportSubmitionForm.SubcategoryRecord("SupportFormSuggestions", "Suggestions")
			}
		}
	};

	private class SubcategoryRecord
	{
		public SubcategoryRecord(string locLabel, string shortName)
		{
			this.LocLabel = locLabel;
			this.ShortName = shortName;
		}

		public string LocLabel { get; private set; }

		public string ShortName { get; private set; }
	}
}
