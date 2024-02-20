using System;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowListCompetitionsItem : WindowListItem
{
	public UserCompetitionPublic Ugc { get; private set; }

	public Tournament Competition { get; private set; }

	public bool IsFinished
	{
		get
		{
			return this.Competition.IsDone || this.Competition.IsCanceled || this.Competition.IsEnded || this.Competition.IsDisqualified || (this.Competition.EndDate < TimeHelper.UtcTime() && this.IsStarted);
		}
	}

	public bool IsStarted
	{
		get
		{
			return (this.Competition.IsStarted && this.Competition.IsApproved) || (this.Ugc != null && this.Ugc.IsActive && this.Ugc.IsApproved);
		}
	}

	public void Init(UserCompetitionPublic ugc, Tournament t)
	{
		this.Ugc = ugc;
		this.Competition = t;
		this.PondValue.text = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == this.Competition.PondId).Name;
		if (StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId != this.Competition.PondId)
		{
			this.PondValue.text = string.Format("{0}\n{1}", this.PondValue.text, UgcConsts.GetYellowTan(ScriptLocalization.Get("TravelRequiredCaption")));
		}
		bool flag = this.Ugc != null;
		this.PlayersValue.text = ((!flag) ? this.Competition.RegistrationsCount.ToString() : string.Format("{0}/{1}", this.Competition.RegistrationsCount, this.Ugc.MaxParticipants));
		if (flag)
		{
			this.Ico.gameObject.SetActive(false);
			this.FormatValue.text = UserCompetitionHelper.GetTypeIco(this.Ugc);
			DateTime dateTime = this.Ugc.StartDate.ToLocalTime();
			if (!this.Ugc.IsStarted)
			{
				if (this.Ugc.SortType == UserCompetitionSortType.Automatic)
				{
					dateTime = this.Ugc.FixedStartDate.Value.ToLocalTime();
				}
				else
				{
					this.StartTimeValue.text = ScriptLocalization.Get("UGC_ManualStart");
				}
			}
			if (this.Ugc.SortType == UserCompetitionSortType.Automatic || this.Ugc.IsStarted)
			{
				if (DateTime.Now.Day < dateTime.Day || DateTime.Now.Month < dateTime.Month)
				{
					this.StartTimeValue.text = string.Format("{0} {1}", MeasuringSystemManager.DateTimeShortWithoutYear(dateTime), MeasuringSystemManager.TimeString(dateTime));
				}
				else
				{
					this.StartTimeValue.text = MeasuringSystemManager.TimeString(dateTime);
				}
			}
		}
		else
		{
			this._thumbnailLdbl.Load(this.Competition.LogoBID, this.Ico, "Textures/Inventory/{0}");
			this.FormatValue.text = string.Empty;
			this.StartTimeValue.text = MeasuringSystemManager.TimeString(t.StartDate.ToLocalTime());
		}
		this.NameValue.text = ((!flag) ? this.Competition.Name.ToUpper() : UserCompetitionHelper.GetDefaultName(this.Ugc).ToUpper());
		Graphic nameValue = this.NameValue;
		Color color = ((!this.IsStarted) ? this.Normal : this.Started);
		this.StartTimeValue.color = color;
		color = color;
		this.PlayersValue.color = color;
		color = color;
		this.PondValue.color = color;
		nameValue.color = color;
	}

	public void Init(UserCompetitionPublic ugc)
	{
		this.Init(ugc, this.Competition);
	}

	[SerializeField]
	protected TextMeshProUGUI FormatValue;

	[SerializeField]
	protected Image Ico;

	[SerializeField]
	protected TextMeshProUGUI NameValue;

	[SerializeField]
	protected TextMeshProUGUI PondValue;

	[SerializeField]
	protected TextMeshProUGUI PlayersValue;

	[SerializeField]
	protected TextMeshProUGUI StartTimeValue;

	private readonly Color Normal = new Color(0.8f, 0.8f, 0.8f);

	private readonly Color Started = Color.white;

	private readonly ResourcesHelpers.AsyncLoadableImage _thumbnailLdbl = new ResourcesHelpers.AsyncLoadableImage();
}
