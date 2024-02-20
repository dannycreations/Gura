using System;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentTimeEndHandler : MessageBoxBase
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.UnsubscribeGetUserCompetition();
	}

	public void Init(TournamentResult result)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		bool flag = profile.Tournament.IsUgc();
		this._fishRule.text = ((!flag) ? ScriptLocalization.Get("TournamentAllCaughtFish") : string.Empty);
		this._result = result as TournamentIndividualResults;
		if (this._result == null)
		{
			LogHelper.Error("TournamentTimeEndHandler:Init - result is null", new object[0]);
			return;
		}
		if (profile.Tournament.IsUgc())
		{
			this.Name.text = string.Empty;
			PhotonConnectionFactory.Instance.OnGetUserCompetition += this.Instance_OnGetUserCompetition;
			PhotonConnectionFactory.Instance.OnFailureGetUserCompetition += this.Instance_OnFailureGetUserCompetition;
			PhotonConnectionFactory.Instance.GetUserCompetition(profile.Tournament.TournamentId);
		}
		else
		{
			this.Name.text = profile.Tournament.Name;
		}
		if (profile.Tournament.ImageBID != null)
		{
			this.LogoLdbl.Image = this.Logo;
			this.LogoLdbl.Load(string.Format("Textures/Inventory/{0}", profile.Tournament.ImageBID.Value));
		}
		this.PlaceValue.text = ((this._result.Place == null) ? "-" : this._result.Place.ToString());
		this.Score1Value.text = MeasuringSystemManager.GetTournamentPrimaryScoreValueToString(profile.Tournament, this._result);
		this.Score2Value.text = MeasuringSystemManager.GetTournamentSecondaryScoreValueToString(profile.Tournament, this._result);
		this._userName.text = profile.Name;
		if (this._result.TeamResult == null)
		{
			bool flag2 = this._result.Place == 1;
			this._winner.SetActive(flag2);
			Color color = ((!flag2) ? UgcConsts.YellowTan : UgcConsts.WinnerColor);
			Graphic userName = this._userName;
			Color color2 = color;
			this._scoreTitle.color = color2;
			userName.color = color2;
			if (flag2)
			{
				this.PlaceValue.color = color;
			}
		}
		else
		{
			bool flag3 = this._result.TeamResult.Place == 1;
			this.UpdateTeam(this._result.TeamResult.Team, flag3);
		}
	}

	public void FinishedTournament()
	{
		base.GetComponent<AlphaFade>().HidePanel();
	}

	private void Instance_OnGetUserCompetition(UserCompetitionPublic competition)
	{
		this.UnsubscribeGetUserCompetition();
		UserCompetitionHelper.GetDefaultName(this.Name, competition);
		this._fishRule.text = ScriptLocalization.Get((!competition.IsSponsored) ? "UGC_TournamentAllReleasedFish" : "TournamentAllCaughtFish");
		if (competition.Format == UserCompetitionFormat.Team)
		{
			this._teamScore1Value.text = UserCompetitionHelper.GetScoreString(competition.ScoringType, this._result.TeamResult.Score, competition.TotalScoreKind);
			this._teamScore2Value.text = UserCompetitionHelper.GetScoreString(competition.SecondaryScoringType, this._result.TeamResult.SecondaryScore, competition.TotalScoreKind);
		}
	}

	private void Instance_OnFailureGetUserCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeGetUserCompetition();
	}

	private void UnsubscribeGetUserCompetition()
	{
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetition -= this.Instance_OnFailureGetUserCompetition;
		PhotonConnectionFactory.Instance.OnGetUserCompetition -= this.Instance_OnGetUserCompetition;
	}

	private void UpdateTeam(string team, bool isWin)
	{
		this._teamScoreCaption.text = UgcConsts.GetTeamScoreLoc(team);
		for (int i = 0; i < this._teamImgs.Length; i++)
		{
			this._teamImgs[i].color = UgcConsts.GetTeamColor(team);
		}
		this._winner.SetActive(isWin);
		Color color = ((!isWin) ? UgcConsts.YellowTan : UgcConsts.WinnerColor);
		Graphic userName = this._userName;
		Color color2 = color;
		this._teamScoreCaption.color = color2;
		userName.color = color2;
		if (isWin)
		{
			Graphic teamScore1Value = this._teamScore1Value;
			color2 = color;
			this._teamScore2Value.color = color2;
			teamScore1Value.color = color2;
		}
	}

	[SerializeField]
	private Text _teamScoreCaption;

	[SerializeField]
	private Image[] _teamImgs;

	[SerializeField]
	private Text _teamScore1Value;

	[SerializeField]
	private Text _teamScore2Value;

	[Space(15f)]
	[SerializeField]
	private Text _userName;

	[SerializeField]
	private Text _scoreTitle;

	[SerializeField]
	private GameObject _winner;

	[SerializeField]
	private Text _fishRule;

	public AlphaFade Panel;

	public TextMeshProUGUI Name;

	private ResourcesHelpers.AsyncLoadableImage LogoLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public Image Logo;

	public Text PlaceValue;

	public Text Score1Caption;

	public Text Score1Value;

	public Text Score2Caption;

	public Text Score2Value;

	private TournamentIndividualResults _result;

	private bool _isPlayingTwice;
}
