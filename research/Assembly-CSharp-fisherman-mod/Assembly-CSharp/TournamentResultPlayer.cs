using System;
using Assets.Scripts.Steamwork.NET;
using Assets.Scripts.UI._2D.PlayerProfile;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentResultPlayer : MonoBehaviour
{
	public TournamentResultPlayer.TournamentResultPlayerData Data { get; private set; }

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile -= this.Instance_OnGotOtherPlayerProfile;
	}

	public void Init(TournamentResultPlayer.TournamentResultPlayerData o)
	{
		this.Data = o;
		if (!string.IsNullOrEmpty(o.Team) && this._teamImg != null)
		{
			this._teamImg.color = UgcConsts.GetTeamColor(o.Team);
		}
		this._userPlace.text = ((o.UserPlace == null) ? "-" : o.UserPlace.ToString());
		this._userName.text = o.UserName;
		this._scorePrimary.text = MeasuringSystemManager.GetTournamentScoreValueToString(o.ScoringType, o.ScorePrimary, o.TotalScoreKind, "3");
		this._scoreSecondary.text = MeasuringSystemManager.GetTournamentScoreValueToString(o.SecondaryScoringType, o.ScoreSecondary, o.TotalScoreKind, "3");
		if (this._cupImg != null)
		{
			this._cupImg.gameObject.SetActive(o.InTop3);
		}
		if (o.InTop3)
		{
			if (this._cupImg != null)
			{
				this._cupImg.overrideSprite = this._cups[o.UserPlace.Value - 1];
			}
			if (!o.IsOwner && o.UserPlace == 1)
			{
				TournamentResultPlayer.SetTextBold(this._userName);
				TournamentResultPlayer.SetTextBold(this._scorePrimary);
				TournamentResultPlayer.SetTextBold(this._scoreSecondary);
			}
		}
		if (o.IsOwner)
		{
			this.FillProfileData(PhotonConnectionFactory.Instance.Profile);
		}
		else
		{
			this._requestedUserId = o.UserId;
			PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile += this.Instance_OnGotOtherPlayerProfile;
			PhotonConnectionFactory.Instance.RequestOtherPlayerProfile(o.UserId.ToString());
		}
	}

	private void Instance_OnGotOtherPlayerProfile(Profile profile)
	{
		if (this._requestedUserId != profile.UserId)
		{
			return;
		}
		this.FillProfileData(profile);
		PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile -= this.Instance_OnGotOtherPlayerProfile;
	}

	private void FillProfileData(Profile profile)
	{
		PlayerProfileHelper.SetAvatarIco(profile, this._userAvatar, SteamAvatarLoader.AvatarTypes.Large);
	}

	private static void SetTextBold(TextMeshProUGUI t)
	{
		t.text = string.Format("<b>{0}</b>", t.text);
	}

	[SerializeField]
	private Image _teamImg;

	[SerializeField]
	private Image _cupImg;

	[SerializeField]
	private Image _userAvatar;

	[SerializeField]
	private TextMeshProUGUI _userPlace;

	[SerializeField]
	private TextMeshProUGUI _userName;

	[SerializeField]
	private TextMeshProUGUI _scorePrimary;

	[SerializeField]
	private TextMeshProUGUI _scoreSecondary;

	[SerializeField]
	private Sprite[] _cups;

	private Guid _requestedUserId = Guid.Empty;

	public class TournamentResultPlayerData
	{
		public int? UserPlace { get; set; }

		public Guid UserId { get; set; }

		public string UserName { get; set; }

		public float? ScorePrimary { get; set; }

		public float? ScoreSecondary { get; set; }

		public bool IsOwner { get; set; }

		public bool InTop3 { get; set; }

		public TournamentScoreType ScoringType { get; set; }

		public TournamentScoreType SecondaryScoringType { get; set; }

		public string Team { get; set; }

		public TournamentTotalScoreKind TotalScoreKind { get; set; }
	}
}
