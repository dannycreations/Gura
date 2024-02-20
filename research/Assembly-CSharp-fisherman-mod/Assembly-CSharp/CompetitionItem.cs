using System;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionItem : UesrCompetitionItem
{
	public Tournament Tournament { get; protected set; }

	public virtual void Init(Tournament t)
	{
		this.Tournament = t;
		this.Init();
		this.UpdateStatus();
	}

	protected override void UpdateStatus()
	{
		this.StatusIco.text = this.StatesIco[UesrCompetitionItem.States.None];
		this.StatusIco.color = this.NormalColor;
		this._statusTime.text = string.Format("{0}-{1}", MeasuringSystemManager.TimeString(this.Tournament.RegistrationStart.ToLocalTime()), MeasuringSystemManager.TimeString(this.Tournament.StartDate.ToLocalTime()));
		if (this.Tournament.IsEnded)
		{
			this.StatusIco.text = this.StatesIco[UesrCompetitionItem.States.Finished];
			Graphic statusTime = this._statusTime;
			Color color = this.FinishedColor;
			this.StatusIco.color = color;
			statusTime.color = color;
			this._statusTime.text = ScriptLocalization.Get((!this.Tournament.IsCanceled) ? "FinishedCaption" : "CanceledStatusText");
		}
		else if (this.Tournament.IsRegistered)
		{
			this.StatusIco.text = this.StatesIco[UesrCompetitionItem.States.Registered];
			Graphic statusTime2 = this._statusTime;
			Color color = this.RegColor;
			this.StatusIco.color = color;
			statusTime2.color = color;
			this._statusTime.text = ScriptLocalization.Get("JoinedCaption").ToUpper();
		}
		else if (TimeHelper.UtcTime() >= this.Tournament.RegistrationStart && TimeHelper.UtcTime() <= this.Tournament.StartDate)
		{
			this.StatusIco.text = this.StatesIco[UesrCompetitionItem.States.RegistrationOpened];
			Graphic statusTime3 = this._statusTime;
			Color color = this.RegColor;
			this.StatusIco.color = color;
			statusTime3.color = color;
		}
	}

	protected virtual void Init()
	{
		this.Name.text = this.Tournament.Name.ToUpper();
		this.ImageLdbl.Image = this.Image;
		this.ImageLdbl.Load((this.Tournament.LogoBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", this.Tournament.LogoBID.Value));
		this.SetPondName(this.Tournament.PondId);
		this._date.text = this.Tournament.StartDate.ToString("dd/MM").Replace('.', '/').Replace(',', '/');
	}

	[SerializeField]
	private TextMeshProUGUI _date;

	[SerializeField]
	private TextMeshProUGUI _statusTime;
}
