using System;
using System.Globalization;
using Assets.Scripts.Steamwork.NET;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class InitCommonInfo : MonoBehaviour
{
	private void Awake()
	{
		this._levelExpCaption.text = string.Format("{0} {1}:", ScriptLocalization.Get("LevelMenuCaption"), ScriptLocalization.Get("ExpShortCaption"));
		this._rankExpCaption.text = string.Format("{0} {1}:", string.Format(ScriptLocalization.Get("LockedRank"), string.Empty).TrimEnd(new char[0]), ScriptLocalization.Get("ExpShortCaption"));
	}

	public void Refresh(Profile profile, PlayerStats stats)
	{
		this._levelExpValue.text = profile.Experience.ToString();
		this._rankExpValue.text = profile.RankExperience.ToString();
		this.UserName.text = profile.Name;
		PlayerProfileHelper.SetAvatarIco(profile, this.UserPic, SteamAvatarLoader.AvatarTypes.Large);
		this._profileCaption.text = ScriptLocalization.Get("ProfileButton").ToUpper(CultureInfo.InvariantCulture);
		this.UserLevel.text = ((profile.Rank > 0) ? string.Format("{0} {1}", "\ue727", profile.Rank) : string.Format("{0} {1}", "\ue62d", profile.Level));
		long experience = PlayerStatisticsInit.GetExperience(profile);
		string text;
		long num;
		PlayerStatisticsInit.ExperienceInfo(profile, out text, out num);
		this.ExpiriencesCaption.text = text;
		this.Expiriences.text = string.Format("{0}/{1}", experience, num);
		this.LevelProgress.value = PlayerStatisticsInit.GetProgress(profile);
		this.TournamnetsTitle.text = this.TournamnetsTitle.text.ToUpper();
		this.PlaySince.text = profile.CreationDate.ToShortDateString();
		this.GetStatsCounter(stats, StatsCounterType.ActiveDays, this.DaysInGame);
		int statsCounter = this.GetStatsCounter(stats, StatsCounterType.TotalFish, this.FishCatched);
		int statsCounter2 = this.GetStatsCounter(stats, StatsCounterType.UniqueFish, this.Unique);
		int statsCounter3 = this.GetStatsCounter(stats, StatsCounterType.TrophyFish, this.Trophy);
		if (statsCounter > 0)
		{
			this.CommonFishCatched.text = (statsCounter - statsCounter3 - statsCounter2).ToString();
		}
		this.PremiumIcon.gameObject.SetActive(profile.HasPremium);
		if (profile.HasPremium && profile.SubscriptionEndDate != null)
		{
			this.PremiumExpiresIn.text = string.Format("({0} {1})", ScriptLocalization.Get("WillExpireCaption"), MeasuringSystemManager.DateTimeShortString(profile.SubscriptionEndDate.Value));
		}
		this.PersonalSkillRating.text = profile.CompetitionRating.ToString();
		this.SetCompStatsCounter(profile.Stats, StatsCounterType.CompPartCount, this.CompetitionsGamesPlayed);
		this.SetCompStatsCounter(profile.Stats, StatsCounterType.CompWon, this.FirstPlaceCount);
		this.SetCompStatsCounter(profile.Stats, StatsCounterType.Comp2nd, this.SecondPlaceCount);
		this.SetCompStatsCounter(profile.Stats, StatsCounterType.Comp3rd, this.ThirdPlaceCount);
	}

	private int GetStatsCounter(PlayerStats stats, StatsCounterType type, Text t)
	{
		int num = ((!stats.GenericStats.ContainsKey(type)) ? 0 : stats.GenericStats[type].Count);
		t.text = num.ToString();
		return num;
	}

	private void SetCompStatsCounter(PlayerStats stats, StatsCounterType type, Text t)
	{
		t.text = stats.GetCounterValue(type, 0).ToString();
	}

	[SerializeField]
	private Text _profileCaption;

	[SerializeField]
	private Text _levelExpCaption;

	[SerializeField]
	private Text _rankExpCaption;

	[SerializeField]
	private Text _levelExpValue;

	[SerializeField]
	private Text _rankExpValue;

	public Text UserName;

	public Text UserLevel;

	public Image UserPic;

	public Text PlaySince;

	public Text DaysInGame;

	public Text ExpiriencesCaption;

	public Text Expiriences;

	public Text FishCatched;

	public Text CommonFishCatched;

	public Text CompetitionsGamesPlayed;

	public Text PersonalSkillRating;

	public Text FirstPlaceCount;

	public Text SecondPlaceCount;

	public Text ThirdPlaceCount;

	public Text Trophy;

	public Text Unique;

	public Text PremiumIcon;

	public Text TournamnetsTitle;

	public Text PremiumExpiresIn;

	public Text ExpPercent;

	public Slider LevelProgress;

	public GameObject Content;

	public GameObject GeneralStat;

	public GameObject PerformanceStat;

	public GameObject CompetitionsStat;
}
