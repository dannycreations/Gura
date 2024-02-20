using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionViewItemThreeBtns : CompetitionViewItem
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<UserCompetitionFormat> OnSelectFormat = delegate(UserCompetitionFormat f)
	{
	};

	public override void Init(string title, string hint, string value, ToggleGroup tg, bool separActive)
	{
		base.Init(title, hint, value, tg, separActive);
		for (int i = 0; i < this.CompetitionFormats.Length; i++)
		{
			CompetitionViewItemThreeBtns.UserCompetitionFormats o = this.CompetitionFormats[i];
			if (this._normalFontMaterial == null)
			{
				this._normalFontMaterial = o.Ico.fontMaterial;
			}
			o.Toggle.group = tg;
			o.Toggle.onValueChanged.AddListener(delegate(bool b)
			{
				if (b && SettingsManager.InputType != InputModuleManager.InputType.GamePad)
				{
					this.OnSelectFormat(o.Format);
				}
			});
			o.Btn.onClick.AddListener(delegate
			{
				if (o.Toggle.isOn)
				{
					this.OnSelectFormat(o.Format);
				}
			});
		}
	}

	public void SetActive(UserCompetitionFormat f)
	{
		for (int i = 0; i < this.CompetitionFormats.Length; i++)
		{
			CompetitionViewItemThreeBtns.UserCompetitionFormats userCompetitionFormats = this.CompetitionFormats[i];
			userCompetitionFormats.ActiveObj.SetActive(userCompetitionFormats.Format == f);
		}
	}

	public override void SetBlocked(bool flag)
	{
		base.SetBlocked(flag);
		for (int i = 0; i < this.CompetitionFormats.Length; i++)
		{
			this.CompetitionFormats[i].Toggle.interactable = !flag;
		}
	}

	public void SetBlocked(UserCompetitionFormat f, bool flag)
	{
		for (int i = 0; i < this.CompetitionFormats.Length; i++)
		{
			if (this.CompetitionFormats[i].Format == f)
			{
				this.CompetitionFormats[i].Toggle.interactable = !flag;
				this.CompetitionFormats[i].Ico.color = ((!flag) ? this.NormalIco : this.BlockedText);
				break;
			}
		}
	}

	public void SetSponsored(bool isSponsored)
	{
		for (int i = 0; i < this.CompetitionFormats.Length; i++)
		{
			CompetitionViewItemThreeBtns.UserCompetitionFormats userCompetitionFormats = this.CompetitionFormats[i];
			if (isSponsored)
			{
				UserCompetitionHelper.SetSponsoredMaterial(userCompetitionFormats.Ico, "Angler_icon_v3 SDF_Sponsored");
			}
			else
			{
				userCompetitionFormats.Ico.fontMaterial = this._normalFontMaterial;
			}
		}
	}

	[SerializeField]
	protected CompetitionViewItemThreeBtns.UserCompetitionFormats[] CompetitionFormats;

	private Material _normalFontMaterial;

	[Serializable]
	public class UserCompetitionFormats
	{
		[SerializeField]
		public UserCompetitionFormat Format;

		[SerializeField]
		public GameObject ActiveObj;

		[SerializeField]
		public Toggle Toggle;

		[SerializeField]
		public Button Btn;

		[SerializeField]
		public TextMeshProUGUI Ico;
	}
}
