using System;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TournamentInitBase : MonoBehaviour
{
	protected virtual void Start()
	{
	}

	private void Awake()
	{
		this.ApplyText.SetActive(false);
	}

	public virtual void PointerEnter()
	{
		if (this._currentTournament == null)
		{
			return;
		}
		this.ApplyText.SetActive(true);
		if (this._currentTournament.RegistrationStart < PhotonConnectionFactory.Instance.ServerUtcNow && this._currentTournament.StartDate > PhotonConnectionFactory.Instance.ServerUtcNow && !this._currentTournament.IsRegistered)
		{
			this.ApplyTextCaption.text = ScriptLocalization.Get("TournamentApplyDetailsCaption");
		}
		else
		{
			this.ApplyTextCaption.text = ScriptLocalization.Get("TournamentDetailsCaption");
		}
	}

	public void PointerExit()
	{
		this.ApplyText.SetActive(false);
	}

	public virtual void OnClick()
	{
		if (this._currentTournament == null)
		{
			return;
		}
		TournamentHelper.ShowingTournamentDetails(this._currentTournament, false);
	}

	public Image Image;

	protected ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Image InfoImage;

	public Text StartTime;

	public GameObject IsWinnerInfo;

	public GameObject ApplyText;

	public Text ApplyTextCaption;

	protected Tournament _currentTournament;
}
