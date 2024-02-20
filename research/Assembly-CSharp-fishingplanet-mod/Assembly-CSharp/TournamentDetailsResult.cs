using System;
using System.Linq;
using Assets.Scripts.Steamwork.NET;
using Assets.Scripts.UI._2D.PlayerProfile;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces.Tournaments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentDetailsResult : MonoBehaviour
{
	private void Awake()
	{
		this._scoreTitle.text = ScriptLocalization.Get("TournamentScoreCaption");
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile -= this.Instance_OnGotOtherPlayerProfile;
	}

	public void Init(TournamentIndividualResults result, UserCompetitionPublic t, string primaryScore, int? teamPlace = null)
	{
		if (t.Format == UserCompetitionFormat.Team)
		{
			this._isTeam = true;
			this._teamPlace = teamPlace;
			string team = t.Players.First((UserCompetitionPlayer p) => p.UserId == PhotonConnectionFactory.Instance.Profile.UserId).Team;
			this._ownerTeam.color = UgcConsts.GetTeamColor(team);
			this._ownerTeam.gameObject.SetActive(true);
		}
		this.Init(result, 4, primaryScore);
	}

	public void Init(TournamentIndividualResults result, Tournament t, string primaryScore)
	{
		this.Init(result, t.KindId, primaryScore);
	}

	public void SetLast()
	{
		this._rullerLast.SetActive(true);
		this._ruller.SetActive(false);
	}

	private void Init(TournamentIndividualResults result, TournamentKinds kindId, string primaryScore)
	{
		this._scoreValue.text = primaryScore;
		this._isInTop = false;
		if (result.Place != null)
		{
			int num = result.Place.Value - 1;
			this._isInTop = num >= 0 && num < this._cups.Length;
			if (this._isInTop)
			{
				this._cupImg.overrideSprite = this._cups[num];
			}
		}
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (result.UserId != profile.UserId)
		{
			this._requestedUserId = result.UserId;
			PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile += this.Instance_OnGotOtherPlayerProfile;
			PhotonConnectionFactory.Instance.RequestOtherPlayerProfile(result.UserId.ToString());
		}
		else
		{
			if (!this._isTeam)
			{
				this._scoreValue.color = this.PremiumColor;
				this._scoreTitle.color = this.OwnerScoreTitleColor;
			}
			this.FillProfileData(profile, true);
			if (!this._isInTop || this._isTeam)
			{
				this._ownerInfo.SetActive(true);
				this._cupImg.gameObject.SetActive(false);
				this._ownerPlace.SetActive(!result.IsDisqualified);
				this._ownerCupImgBan.gameObject.SetActive(result.IsDisqualified);
				this._pos.text = ((result.Place == null) ? "-" : string.Format("#{0}", result.Place));
			}
			if (kindId != 3)
			{
				if (kindId != 2)
				{
					if (kindId == 1)
					{
						this._ownerRatingValue.text = profile.TournamentRating.ToString();
					}
				}
				else
				{
					this._ownerRatingValue.text = profile.EventRating.ToString();
				}
			}
			else
			{
				this._ownerRatingValue.text = profile.CompetitionRating.ToString();
			}
			if (result.Rating != null)
			{
				this._ownerRatingValue.text = string.Format("<color=#FFFFFF99>{0}</color> <color=#FFFFFFFF>{1}</color> <color=#7ED321FF>({2})</color>", ScriptLocalization.Get("TournamentYourPersonalRatingCaption"), this._ownerRatingValue.text, result.Rating);
			}
			else
			{
				this._ownerRating.SetActive(false);
			}
		}
	}

	private void FillProfileData(Profile profile, bool isOwner)
	{
		this._name.text = profile.Name;
		this._lvl.text = profile.Level.ToString();
		if (this._isTeam && this._teamPlace != null && this._teamPlace.Value == 1)
		{
			this._name.color = UgcConsts.WinnerColor;
		}
		if (profile.HasPremium)
		{
			this._lvl.color = this.PremiumColor;
		}
		this._starPrem.SetActive(profile.HasPremium);
		this._star.SetActive(!profile.HasPremium);
		PlayerProfileHelper.SetAvatarIco(profile, this._avatar, SteamAvatarLoader.AvatarTypes.Large);
	}

	private void Instance_OnGotOtherPlayerProfile(Profile profile)
	{
		if (this._requestedUserId != profile.UserId)
		{
			return;
		}
		PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile -= this.Instance_OnGotOtherPlayerProfile;
		this.FillProfileData(profile, false);
	}

	[SerializeField]
	private GameObject _star;

	[SerializeField]
	private GameObject _starPrem;

	[SerializeField]
	private GameObject _ruller;

	[SerializeField]
	private GameObject _rullerLast;

	[SerializeField]
	private Image _cupImg;

	[SerializeField]
	private Image _avatar;

	[SerializeField]
	private TextMeshProUGUI _lvl;

	[SerializeField]
	private TextMeshProUGUI _name;

	[SerializeField]
	private TextMeshProUGUI _scoreTitle;

	[SerializeField]
	private TextMeshProUGUI _scoreValue;

	[SerializeField]
	private GameObject _ownerInfo;

	[SerializeField]
	private TextMeshProUGUI _pos;

	[SerializeField]
	private GameObject _ownerRating;

	[SerializeField]
	private TextMeshProUGUI _ownerRatingValue;

	[SerializeField]
	private GameObject _ownerPlace;

	[SerializeField]
	private Image _ownerCupImgBan;

	[SerializeField]
	private Image _ownerTeam;

	[SerializeField]
	private TextMeshProUGUI _ownerReward;

	[SerializeField]
	private Sprite[] _cups;

	public readonly Color PremiumColor = new Color(1f, 0.8666667f, 0.46666667f);

	public readonly Color OwnerScoreTitleColor = new Color(1f, 0.8666667f, 0.46666667f, 0.3882353f);

	private bool _isInTop;

	private bool _isTeam;

	private int? _teamPlace;

	private Guid _requestedUserId = Guid.Empty;
}
