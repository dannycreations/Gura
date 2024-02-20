using System;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using ObjectModel;
using ObjectModel.Tournaments;
using UnityEngine;
using UnityEngine.UI;

public class TournamentPlaceItemInit : MonoBehaviour
{
	private void Awake()
	{
		this.ProIcon.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile -= this.OnGotOtherPlayerProfile;
	}

	public void Init(int place)
	{
		if (place != 1)
		{
			if (place != 2)
			{
				if (place == 3)
				{
					this.PlaceIcon.sprite = this.Place3;
				}
			}
			else
			{
				this.PlaceIcon.sprite = this.Place2;
			}
		}
		else
		{
			this.PlaceIcon.sprite = this.Place1;
		}
	}

	public void Init(TournamentIndividualResults result, Tournament currentTournament)
	{
		this._currentResultUserId = result.UserId;
		this.Score.text = ScriptLocalization.Get("TournamentScoreCaption") + " " + MeasuringSystemManager.GetTournamentPrimaryScoreValueToString(currentTournament, result);
		this.PlaceIconLdbl.Image = this.PlaceIcon;
		this.PlaceIconLdbl.Load("ART/UI/Icons/tournament-cup-" + result.Place);
		int? place = result.Place;
		if (place != null)
		{
			int value = place.Value;
			if (value != 1)
			{
				if (value != 2)
				{
					if (value == 3)
					{
						this.PlaceIcon.sprite = this.Place3;
					}
				}
				else
				{
					this.PlaceIcon.sprite = this.Place2;
				}
			}
			else
			{
				this.PlaceIcon.sprite = this.Place1;
			}
		}
		if (result.UserId != PhotonConnectionFactory.Instance.Profile.UserId)
		{
			PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile += this.OnGotOtherPlayerProfile;
			PhotonConnectionFactory.Instance.RequestOtherPlayerProfile(result.UserId.ToString());
		}
		else
		{
			this.FillProfileData(PhotonConnectionFactory.Instance.Profile);
		}
	}

	public void Init(TournamentSecondaryResult reward, Guid userId)
	{
		this._currentResultUserId = userId;
		this.RewardIcon.SetActive(true);
		this.PlaceIcon.gameObject.SetActive(false);
		this.Score.text = string.Concat(new string[]
		{
			ScriptLocalization.Get("BiggestFishCaption"),
			" ",
			reward.SecondaryRewardFishName,
			", ",
			MeasuringSystemManager.FishWeight((float)reward.Score).ToString("n1"),
			MeasuringSystemManager.FishWeightSufix()
		});
		if (userId != PhotonConnectionFactory.Instance.Profile.UserId)
		{
			PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile += this.OnGotOtherPlayerProfile;
			PhotonConnectionFactory.Instance.RequestOtherPlayerProfile(userId.ToString());
		}
		else
		{
			this.FillProfileData(PhotonConnectionFactory.Instance.Profile);
		}
	}

	private void OnGotOtherPlayerProfile(Profile profile)
	{
		if (profile.UserId == this._currentResultUserId)
		{
			this.FillProfileData(profile);
			PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile -= this.OnGotOtherPlayerProfile;
			string gender = profile.Gender;
			if (gender != null)
			{
				if (gender == "M")
				{
					this.Avatar.sprite = this.maleUserpicSprite;
					return;
				}
				if (gender == "F")
				{
					this.Avatar.sprite = this.femaleUserpicSprite;
					return;
				}
			}
			this.Avatar.sprite = this.notProvidedUserpicSprite;
		}
	}

	private void FillProfileData(Profile profile)
	{
		this.Name.text = profile.Name;
		this.Level.text = PlayerProfileHelper.GetPlayerLevelRank(profile);
	}

	public Text Name;

	public Text Level;

	public Image Avatar;

	public Text Score;

	public Text ProIcon;

	public Image PlaceIcon;

	private ResourcesHelpers.AsyncLoadableImage PlaceIconLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public GameObject RewardIcon;

	public Sprite femaleUserpicSprite;

	public Sprite maleUserpicSprite;

	public Sprite notProvidedUserpicSprite;

	public Sprite Place1;

	public Sprite Place2;

	public Sprite Place3;

	private Guid _currentResultUserId;

	public readonly Color PremiumColor = new Color(0.92941177f, 0.78431374f, 0.3882353f);
}
