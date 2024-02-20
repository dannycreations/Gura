using System;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using ObjectModel.Tournaments;
using UnityEngine;
using UnityEngine.UI;

public class SerieTournamentInit : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ApplyOnClick;

	private void Start()
	{
		this.ApplyText.SetActive(false);
	}

	private void SerieTournamentInit_ApplyOnClick(object sender, EventArgs e)
	{
		if (this.ApplyOnClick != null)
		{
			this.ApplyOnClick(this, new EventArgs());
		}
	}

	public void Init(TournamentSerieInstance serie)
	{
		this._currentSerie = serie;
		if (this._currentSerie.LogoBID != null)
		{
			this.ImageLdbl.Image = this.Image;
			this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", this._currentSerie.LogoBID.Value));
		}
		this.Name.text = serie.Name;
		if (PhotonConnectionFactory.Instance.ServerUtcNow < serie.RegistrationStart)
		{
			this.StartTime.text = string.Format(ScriptLocalization.Get("RegTimeText"), MeasuringSystemManager.DateTimeString(serie.RegistrationStart.ToLocalTime()));
		}
		else
		{
			this.StartTime.text = string.Format(ScriptLocalization.Get("StartTimeText"), MeasuringSystemManager.DateTimeString(serie.RegistrationStart.ToLocalTime()));
		}
		if (PhotonConnectionFactory.Instance.ServerUtcNow > serie.Stages.Min((Tournament x) => x.StartDate))
		{
			this.StartTime.text = ScriptLocalization.Get("StartedCaption");
		}
		if (serie.Stages.All((Tournament x) => x.IsEnded))
		{
			this.StartTime.text = ScriptLocalization.Get("FinishedCaption");
		}
	}

	public void PointerEnter()
	{
		this.ApplyText.SetActive(true);
		if (this._currentSerie == null)
		{
			return;
		}
		if (this._currentSerie.Stages != null && this._currentSerie.Stages.Length > 0)
		{
			this.ApplyTextCaption.text = ((!this._currentSerie.Stages[0].IsRegistered) ? ScriptLocalization.Get("TournamentApplyButton") : ScriptLocalization.Get("TournamentSignedCaption"));
		}
	}

	public void PointerExit()
	{
		this.ApplyText.SetActive(false);
	}

	public virtual void OnClick()
	{
		this._messageBoxDetails = TournamentHelper.ShowingSerieDetails(this._currentSerie);
		this._messageBoxDetails.GetComponent<TournamentSerieDetailsMessage>().ApplyOnClick += this.SerieTournamentInit_ApplyOnClick;
	}

	public Image Image;

	private ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Text StartTime;

	public GameObject ApplyText;

	public Text ApplyTextCaption;

	public Text Name;

	private TournamentSerieInstance _currentSerie;

	private GameObject _messageBoxDetails;
}
