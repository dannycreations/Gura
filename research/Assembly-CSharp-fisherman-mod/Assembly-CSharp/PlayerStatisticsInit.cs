using System;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Steamwork.NET;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatisticsInit : MonoBehaviour
{
	public void Start()
	{
		this.XBoxLive.gameObject.SetActive(false);
		this._isInited = true;
		if (!this._isAvatarLoaded && this._profile != null)
		{
			this._isAvatarLoaded = true;
			PlayerProfileHelper.SetAvatarIco(this._profile, this._profilePic, SteamAvatarLoader.AvatarTypes.Large);
		}
	}

	public void Init(Profile profile)
	{
		this._profile = profile;
		if (this._isInited)
		{
			this._isAvatarLoaded = true;
			PlayerProfileHelper.SetAvatarIco(profile, this._profilePic, SteamAvatarLoader.AvatarTypes.Large);
		}
		this.UserName.text = profile.Name;
		this.PlaySince.text = profile.CreationDate.ToShortDateString();
		this.LevelText.text = PlayerStatisticsInit.GetLevelRank(profile).ToString();
		if (this._profile.Rank > 0)
		{
			this.LevelIcon.text = "\ue727";
		}
		this._silvers = (int)profile.SilverCoins;
		this._golds = (int)profile.GoldCoins;
		this.GoldCoinsField.text = this._golds.ToString(CultureInfo.InvariantCulture);
		this.SilverCoinsField.text = this._silvers.ToString(CultureInfo.InvariantCulture);
		string text;
		long num;
		PlayerStatisticsInit.ExperienceInfo(profile, out text, out num);
		this._expCaption.text = text;
		this.Expiriences.text = string.Format("{0}/{1}", PlayerStatisticsInit.GetExperience(profile), num);
		this.ToNextLevel.value = PlayerStatisticsInit.GetProgress(profile);
		if (profile.Stats.GenericStats.ContainsKey(StatsCounterType.ActiveDays))
		{
			this.DaysInGame.text = profile.Stats.GenericStats[StatsCounterType.ActiveDays].Count.ToString();
		}
		else
		{
			this.DaysInGame.text = "0";
		}
		if (profile.Stats.GenericStats.ContainsKey(StatsCounterType.TotalFish))
		{
			this.FishCatched.text = profile.Stats.GenericStats[StatsCounterType.TotalFish].Count.ToString();
		}
		if (profile.Stats.GenericStats.ContainsKey(StatsCounterType.TrophyFish))
		{
			this.Trophy.text = profile.Stats.GenericStats[StatsCounterType.TrophyFish].Count.ToString();
		}
		if (profile.Stats.GenericStats.ContainsKey(StatsCounterType.UniqueFish))
		{
			this.Unique.text = profile.Stats.GenericStats[StatsCounterType.UniqueFish].Count.ToString();
		}
		if (profile.Stats.GenericStats.ContainsKey(StatsCounterType.TotalFish))
		{
			int num2 = 0;
			int num3 = 0;
			if (profile.Stats.GenericStats.ContainsKey(StatsCounterType.UniqueFish))
			{
				num2 = profile.Stats.GenericStats[StatsCounterType.UniqueFish].Count;
			}
			if (profile.Stats.GenericStats.ContainsKey(StatsCounterType.TrophyFish))
			{
				num3 = profile.Stats.GenericStats[StatsCounterType.TrophyFish].Count;
			}
			this.CommonFishCatched.text = (profile.Stats.GenericStats[StatsCounterType.TotalFish].Count - num3 - num2).ToString();
		}
		if (profile.HasPremium)
		{
			this.PremiumExpiresIn.text = string.Format("({0} {1})", ScriptLocalization.Get("WillExpireCaption"), MeasuringSystemManager.DateTimeShortString(profile.SubscriptionEndDate.Value));
		}
		else
		{
			this.PremiumIcon.gameObject.SetActive(false);
			this.PremiumExpiresIn.gameObject.SetActive(false);
		}
		this.PersonalSkillRating.text = profile.CompetitionRating.ToString();
		this.CompetitionsGamesPlayed.text = profile.Stats.GetCounterValue(StatsCounterType.CompPartCount, 0).ToString();
		this.FirstPlaceCount.text = profile.Stats.GetCounterValue(StatsCounterType.CompWon, 0).ToString();
		this.SecondPlaceCount.text = profile.Stats.GetCounterValue(StatsCounterType.Comp2nd, 0).ToString();
		this.ThirdPlaceCount.text = profile.Stats.GetCounterValue(StatsCounterType.Comp3rd, 0).ToString();
		if (PhotonConnectionFactory.Instance.Profile.Friends != null && PhotonConnectionFactory.Instance.Profile.Friends.Any((Player x) => x.UserId == profile.UserId.ToString() && (x.Status == FriendStatus.FriendshipRequested || x.Status == FriendStatus.Friend)))
		{
			this.DisableAddFriendButton();
		}
		if (profile.UserId == PhotonConnectionFactory.Instance.Profile.UserId)
		{
			this.AddFriend.gameObject.SetActive(false);
		}
	}

	public void ClickAddToFriend()
	{
		if (PhotonConnectionFactory.Instance.FriendsCount < 100 && this._profile != null)
		{
			PhotonConnectionFactory.Instance.RequestFriendship(PlayerProfileHelper.ProfileToPlayerExt(this._profile));
			this.DisableAddFriendButton();
		}
	}

	public void ClickShowProfile()
	{
	}

	public static int MaxLevel
	{
		get
		{
			return PhotonConnectionFactory.Instance.LevelCap;
		}
	}

	public static void ExperienceInfo(Profile profile, out string expCaption, out long experienceToNext)
	{
		if (profile.Level != PlayerStatisticsInit.MaxLevel)
		{
			expCaption = ScriptLocalization.Get("ExpToNextLevel");
			experienceToNext = profile.ExpToNextLevel;
		}
		else
		{
			expCaption = ScriptLocalization.Get("ExpToNextRank");
			experienceToNext = profile.ExpToNextRank;
		}
	}

	public static long GetExperience(Profile profile)
	{
		return (profile.Level == PlayerStatisticsInit.MaxLevel) ? profile.RankExperience : profile.Experience;
	}

	public static float GetProgress(Profile profile)
	{
		return (profile.Level == PlayerStatisticsInit.MaxLevel) ? profile.RankProgress : profile.Progress;
	}

	public static int GetLevelRank(Profile profile)
	{
		return (profile.Rank > 0) ? profile.Rank : profile.Level;
	}

	private void DisableAddFriendButton()
	{
		this.AddFriend.transform.Find("Addfriend/Label").GetComponent<Text>().color = new Color(0.16470589f, 0.16470589f, 0.16470589f, 1f);
		this.AddFriend.transform.Find("Addfriend/hotkey").GetComponent<Text>().color = new Color(0.16470589f, 0.16470589f, 0.16470589f, 1f);
		this.AddFriend.interactable = false;
	}

	[SerializeField]
	private Image _profilePic;

	[SerializeField]
	private Text _expCaption;

	public Text UserName;

	public Text PlaySince;

	public Text DaysInGame;

	public Text Expiriences;

	public Text FishCatched;

	public Text Trophy;

	public Text Unique;

	public Text CommonFishCatched;

	public Text PremiumExpiresIn;

	public Text PremiumIcon;

	public Text LevelText;

	public Text LevelIcon;

	public Text GoldCoinsField;

	public Text SilverCoinsField;

	public Text FirstPlaceCount;

	public Text SecondPlaceCount;

	public Text ThirdPlaceCount;

	public Text CompetitionsGamesPlayed;

	public Text PersonalSkillRating;

	public Slider ToNextLevel;

	public Button AddFriend;

	public Button XBoxLive;

	private int _silvers;

	private int _golds;

	private Profile _profile;

	private bool _isInited;

	private bool _isAvatarLoaded;
}
