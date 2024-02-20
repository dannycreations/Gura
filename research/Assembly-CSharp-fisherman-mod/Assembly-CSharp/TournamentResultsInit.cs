using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using Newtonsoft.Json;
using ObjectModel;
using ObjectModel.Tournaments;
using UnityEngine;
using UnityEngine.UI;

public class TournamentResultsInit : MonoBehaviour
{
	public void Init(Tournament tournament)
	{
		this._currentTournament = tournament;
		this._isInited = false;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentFinalResult -= this.OnGotTournamentFinalResult;
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult -= this.PhotonServerGetSecondaryGotTournamentResult;
		PhotonConnectionFactory.Instance.OnGotTournamentRewards -= this.PhotonOnGotTournamentRewards;
		PhotonConnectionFactory.Instance.OnGettingTournamentRewardsFailed -= this.InstanceOnGettingTournamentRewardsFailed;
	}

	internal void Update()
	{
		if (!this._isInited && base.gameObject.activeSelf)
		{
			this._isInited = true;
			if (this._currentTournament.ImageBID != null)
			{
				this.TournamentLogoLoadable.Image = this.TournamentLogo;
				this.TournamentLogoLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentTournament.ImageBID));
			}
			PhotonConnectionFactory.Instance.OnGotTournamentFinalResult += this.OnGotTournamentFinalResult;
			PhotonConnectionFactory.Instance.GetFinalTournamentResult(this._currentTournament.TournamentId);
			if (this._currentTournament.KindId == 1)
			{
				this.CurrentUserRatingContainer.SetActive(false);
			}
		}
	}

	private void PhotonServerGetSecondaryGotTournamentResult(List<TournamentSecondaryResult> results)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult -= this.PhotonServerGetSecondaryGotTournamentResult;
		this._biggestFishReward = results.FirstOrDefault((TournamentSecondaryResult r) => r.RewardType == 1);
		this._results = results;
		if (this._biggestFishReward != null)
		{
			GameObject gameObject = GUITools.AddChild(this.WinnersContent, this.TournamentPlaceItemPrefab);
			gameObject.GetComponent<TournamentPlaceItemInit>().Init(this._biggestFishReward, this._biggestFishReward.UserId);
			this.SetSizeForWinnersContent();
		}
	}

	private void OnGotTournamentFinalResult(List<TournamentIndividualResults> results, int num)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentFinalResult -= this.OnGotTournamentFinalResult;
		PhotonConnectionFactory.Instance.OnGotTournamentSecondaryResult += this.PhotonServerGetSecondaryGotTournamentResult;
		PhotonConnectionFactory.Instance.GetSecondaryTournamentResult(this._currentTournament.TournamentId);
		List<TournamentIndividualResults> list = (from r in results
			where r.Place != null
			where r.Place <= 3
			select r).ToList<TournamentIndividualResults>();
		int i;
		for (i = 1; i <= 3; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.WinnersContent, this.TournamentPlaceItemPrefab);
			TournamentIndividualResults tournamentIndividualResults = list.Where((TournamentIndividualResults r) => r.Place == i).FirstOrDefault<TournamentIndividualResults>();
			if (tournamentIndividualResults != null)
			{
				gameObject.GetComponent<TournamentPlaceItemInit>().Init(tournamentIndividualResults, this._currentTournament);
			}
			else
			{
				gameObject.GetComponent<TournamentPlaceItemInit>().Init(i);
			}
			this.SetSizeForWinnersContent();
		}
		TournamentIndividualResults tournamentIndividualResults2 = results.Where((TournamentIndividualResults r) => r.UserId == PhotonConnectionFactory.Instance.Profile.UserId).FirstOrDefault<TournamentIndividualResults>();
		if (tournamentIndividualResults2 != null)
		{
			int? currentUserResultPlace = tournamentIndividualResults2.Place;
			int kindId = this._currentTournament.KindId;
			if (kindId != 3)
			{
				if (kindId != 2)
				{
					if (kindId == 1)
					{
						this.CurrentUserRating.text = PhotonConnectionFactory.Instance.Profile.TournamentRating.ToString();
					}
				}
				else
				{
					this.CurrentUserRating.text = PhotonConnectionFactory.Instance.Profile.EventRating.ToString();
				}
			}
			else
			{
				this.CurrentUserRating.text = PhotonConnectionFactory.Instance.Profile.CompetitionRating.ToString();
			}
			if (tournamentIndividualResults2.Rating != null)
			{
				this.CurrentUserRating.text = string.Concat(new object[]
				{
					this.CurrentUserRating.text,
					"(",
					tournamentIndividualResults2.Rating,
					")"
				});
			}
			this.CurrentUserPlace.text = ((currentUserResultPlace == null) ? "-" : currentUserResultPlace.ToString());
			this.CurrentUserScore.text = MeasuringSystemManager.GetTournamentPrimaryScoreValueToString(this._currentTournament, tournamentIndividualResults2);
			this.CurrentUserRating.text = tournamentIndividualResults2.Rating.ToString();
			TournamentPlace tournamentPlace = this._currentTournament.Places.FirstOrDefault((TournamentPlace p) => p.PlaceId == currentUserResultPlace.GetValueOrDefault() && currentUserResultPlace != null);
			this._currentUserRewardName = ((tournamentPlace == null) ? "-" : tournamentPlace.RewardName);
			if (tournamentIndividualResults2.IsDisqualified)
			{
				this.CurrentUserPlace.text = ScriptLocalization.Get("DisqualifiedText");
			}
			PhotonConnectionFactory.Instance.OnGotTournamentRewards += this.PhotonOnGotTournamentRewards;
			PhotonConnectionFactory.Instance.OnGettingTournamentRewardsFailed += this.InstanceOnGettingTournamentRewardsFailed;
			PhotonConnectionFactory.Instance.GetTournamentRewards(this._currentTournament.TournamentId);
		}
		else
		{
			this.currentUserResultContent.SetActive(false);
		}
	}

	private void InstanceOnGettingTournamentRewardsFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentRewards -= this.PhotonOnGotTournamentRewards;
		PhotonConnectionFactory.Instance.OnGettingTournamentRewardsFailed -= this.InstanceOnGettingTournamentRewardsFailed;
		Debug.LogError(failure.FullErrorInfo);
	}

	private IEnumerator WaitForResults(List<Reward> rewards)
	{
		while (this._results == null)
		{
			yield return null;
		}
		Reward currentUserReward = rewards.FirstOrDefault((Reward r) => r.Name == this._currentUserRewardName);
		if (currentUserReward != null)
		{
			if (!string.IsNullOrEmpty(currentUserReward.Items))
			{
				ItemReward[] array = JsonConvert.DeserializeObject<ItemReward[]>(currentUserReward.Items, SerializationHelper.JsonSerializerSettings);
				int[] array2 = array.Select((ItemReward i) => i.ItemId).ToArray<int>();
				PhotonConnectionFactory.Instance.OnGotItems += this.OnGotItems;
				PhotonConnectionFactory.Instance.GetItemsByIds(array2, 3, true);
			}
			Reward reward = null;
			if (this._currentTournament.SecondaryRewards != null)
			{
				SecondaryReward biggestFishSecondaryReward = this._currentTournament.SecondaryRewards.FirstOrDefault((SecondaryReward x) => x.RewardType == SecondaryRewardType.BiggestFish);
				if (biggestFishSecondaryReward != null)
				{
					reward = rewards.FirstOrDefault((Reward x) => x.Name == biggestFishSecondaryReward.RewardName);
				}
			}
			if (currentUserReward.Money1 != null)
			{
				GameObject gameObject = GUITools.AddChild(this.CurrentUserRewardsContent, this.TournamentRewardItemPrefab);
				if (reward != null && this._biggestFishReward != null && this._biggestFishReward.UserId == PhotonConnectionFactory.Instance.Profile.UserId)
				{
					TournamentRewardItemInit component = gameObject.GetComponent<TournamentRewardItemInit>();
					double? money = currentUserReward.Money1;
					bool flag = money != null;
					double? money2 = reward.Money1;
					component.Init((!(flag & (money2 != null))) ? null : new double?(money.GetValueOrDefault() + money2.GetValueOrDefault()), currentUserReward.Currency1);
				}
				else
				{
					gameObject.GetComponent<TournamentRewardItemInit>().Init(currentUserReward.Money1, currentUserReward.Currency1);
				}
			}
			if (currentUserReward.Money2 != null)
			{
				GameObject gameObject2 = GUITools.AddChild(this.CurrentUserRewardsContent, this.TournamentRewardItemPrefab);
				if (reward != null && reward.Money2 != null && this._biggestFishReward != null && this._biggestFishReward.UserId == PhotonConnectionFactory.Instance.Profile.UserId)
				{
					TournamentRewardItemInit component2 = gameObject2.GetComponent<TournamentRewardItemInit>();
					double? money3 = currentUserReward.Money2;
					bool flag2 = money3 != null;
					double? money4 = reward.Money2;
					component2.Init((!(flag2 & (money4 != null))) ? null : new double?(money3.GetValueOrDefault() + money4.GetValueOrDefault()), currentUserReward.Currency2);
				}
				else
				{
					gameObject2.GetComponent<TournamentRewardItemInit>().Init(currentUserReward.Money2, currentUserReward.Currency2);
				}
			}
		}
		yield break;
	}

	private void PhotonOnGotTournamentRewards(List<Reward> rewards)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentRewards -= this.PhotonOnGotTournamentRewards;
		PhotonConnectionFactory.Instance.OnGettingTournamentRewardsFailed -= this.InstanceOnGettingTournamentRewardsFailed;
		if (base.gameObject != null && base.gameObject.activeSelf)
		{
			base.StartCoroutine(this.WaitForResults(rewards));
		}
	}

	private void OnGotItems(List<InventoryItem> items, int subscriberId)
	{
		if (subscriberId != 3)
		{
			return;
		}
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
		for (int i = 0; i < items.Count<InventoryItem>(); i++)
		{
			GameObject gameObject = GUITools.AddChild(this.CurrentUserRewardsContent, this.TournamentRewardItemPrefab);
			gameObject.GetComponent<TournamentRewardItemInit>().Init(items[i]);
		}
		this.SetSizeForRewardContent();
	}

	private void SetSizeForRewardContent()
	{
		int childCount = this.CurrentUserRewardsContent.GetComponent<RectTransform>().childCount;
		this.CurrentUserRewardsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(this.CurrentUserRewardsContent.GetComponent<RectTransform>().rect.width, this.TournamentRewardItemPrefab.GetComponent<LayoutElement>().preferredHeight * (float)childCount + this.CurrentUserRewardsContent.GetComponent<VerticalLayoutGroup>().spacing + (float)this.CurrentUserRewardsContent.GetComponent<VerticalLayoutGroup>().padding.bottom + (float)this.CurrentUserRewardsContent.GetComponent<VerticalLayoutGroup>().padding.top);
	}

	public void Close()
	{
		Object.Destroy(base.gameObject);
	}

	private void SetSizeForWinnersContent()
	{
		int childCount = this.WinnersContent.GetComponent<RectTransform>().childCount;
		this.WinnersContent.GetComponent<LayoutElement>().minHeight = this.TournamentPlaceItemPrefab.GetComponent<LayoutElement>().preferredHeight * (float)childCount + this.CurrentUserRewardsContent.GetComponent<VerticalLayoutGroup>().spacing + (float)this.CurrentUserRewardsContent.GetComponent<VerticalLayoutGroup>().padding.bottom + (float)this.CurrentUserRewardsContent.GetComponent<VerticalLayoutGroup>().padding.top;
	}

	private const int ItemSubscriberId = 3;

	public Text TournamentNameText;

	public Image TournamentLogo;

	private ResourcesHelpers.AsyncLoadableImage TournamentLogoLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public GameObject currentUserResultContent;

	public GameObject CurrentUserRewardsContent;

	public GameObject WinnersContent;

	public GameObject ResultsContent;

	public GameObject TournamentRewardItemPrefab;

	public GameObject TournamentPlaceItemPrefab;

	public Text CurrentUserPlace;

	public Text CurrentUserScore;

	public Text CurrentUserRating;

	public GameObject CurrentUserRatingContainer;

	private Tournament _currentTournament;

	private int _currentTournamentID;

	private TournamentIndividualResults place1;

	private TournamentIndividualResults place2;

	private TournamentIndividualResults place3;

	private string _currentUserRewardName;

	private TournamentSecondaryResult _biggestFishReward;

	private List<TournamentSecondaryResult> _results;

	private bool _isInited;
}
