using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using ObjectModel.Common;
using UnityEngine;
using UnityEngine.UI;

public class TournamentDetailsRewards : MonoBehaviour
{
	private void OnDestroy()
	{
		this.UnsubscribeGetUserCompetitionReward();
		if (CacheLibrary.ItemsCache != null)
		{
			CacheLibrary.ItemsCache.OnGotItems -= this.Instance_OnGotItems;
		}
	}

	public void Init(List<Reward> rewards, TournamentDetailsRewards.BiggestFishData bfd)
	{
		this._rewards = rewards;
		this._biggestFish = bfd;
		if ((this._tournament != null || this._ugc != null) && this._results != null && !this._isActive)
		{
			this.FillData(null);
		}
	}

	public void Init(UserCompetitionPublic ugc, List<TournamentIndividualResults> results, int num)
	{
		this._ugc = ugc;
		this._results = results;
		if (this._rewards != null && !this._isActive)
		{
			this.FillData(null);
		}
	}

	public void Init(Tournament tournament, List<TournamentIndividualResults> results, int num)
	{
		this._tournament = tournament;
		this._results = results;
		if (this._rewards != null && !this._isActive)
		{
			this.FillData(null);
		}
	}

	public void Init(UserCompetitionPublic ugc, List<TournamentTeamResult> results, int totalParticipants)
	{
		List<TournamentIndividualResults> iResult = new List<TournamentIndividualResults>();
		results.ForEach(delegate(TournamentTeamResult p)
		{
			iResult.AddRange(p.IndividualResults);
		});
		this.Init(ugc, iResult, totalParticipants);
	}

	public void Init(Tournament tournament, List<TournamentTeamResult> results, int totalParticipants)
	{
	}

	private void FillData(Reward reward = null)
	{
		this._rewardsList.SetActive(false);
		this.ClearMoneyAndExp();
		this._isActive = true;
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		TournamentIndividualResults tournamentIndividualResults = this._results.FirstOrDefault((TournamentIndividualResults r) => r.UserId == profile.UserId);
		if (tournamentIndividualResults != null)
		{
			if (this._ugc != null)
			{
				if (reward == null)
				{
					PhotonConnectionFactory.Instance.OnGetUserCompetitionReward += this.Instance_OnGetUserCompetitionReward;
					PhotonConnectionFactory.Instance.OnFailureGetUserCompetitionReward += this.Instance_OnFailureGetUserCompetitionReward;
					PhotonConnectionFactory.Instance.GetUserCompetitionReward(this._ugc.TournamentId);
				}
			}
			else
			{
				int? currentUserResultPlace = tournamentIndividualResults.Place;
				TournamentPlace tournamentPlace = this._tournament.Places.FirstOrDefault((TournamentPlace p) => p.PlaceId == currentUserResultPlace.GetValueOrDefault() && currentUserResultPlace != null);
				string currentUserRewardName = ((tournamentPlace == null) ? "-" : tournamentPlace.RewardName);
				reward = this._rewards.FirstOrDefault((Reward r) => r.Name == currentUserRewardName);
			}
			if (reward != null)
			{
				ItemReward[] itemRewards = reward.GetItemRewards();
				if (itemRewards != null)
				{
					CacheLibrary.ItemsCache.OnGotItems += this.Instance_OnGotItems;
					CacheLibrary.ItemsCache.GetItems(itemRewards.Select((ItemReward x) => x.ItemId).ToArray<int>(), 823323);
				}
				ProductReward[] productRewards = reward.GetProductRewards();
				if (productRewards != null)
				{
					productRewards.ToList<ProductReward>().ForEach(delegate(ProductReward p)
					{
						StoreProduct storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == p.ProductId);
						if (storeProduct != null)
						{
							RewardItem.SpawnItem(this._itemsRoot, this._itemPrefab, storeProduct);
						}
						this._rewardsList.SetActive(this._itemsRoot.transform.childCount > 0);
					});
				}
				LicenseRef[] licenseRewards = reward.GetLicenseRewards();
				if (licenseRewards != null)
				{
					licenseRewards.ToList<LicenseRef>().ForEach(delegate(LicenseRef p)
					{
						ShopLicense shopLicense = CacheLibrary.MapCache.AllLicenses.FirstOrDefault((ShopLicense x) => x.LicenseId == p.LicenseId);
						if (shopLicense != null)
						{
							RewardItem.SpawnItem(this._itemsRoot, this._itemPrefab, shopLicense);
						}
						this._rewardsList.SetActive(this._itemsRoot.transform.childCount > 0);
					});
				}
				this.SetMoneyAndExp(reward);
			}
			else
			{
				this.SetMoneyAndExp(new Reward
				{
					Currency1 = "SC",
					Experience = 0
				});
			}
		}
		else
		{
			this.SetMoneyAndExp(new Reward
			{
				Currency1 = "SC",
				Experience = 0
			});
		}
	}

	private void Instance_OnGotItems(List<InventoryItem> items, int sunscriberId)
	{
		if (sunscriberId == 823323)
		{
			CacheLibrary.ItemsCache.OnGotItems -= this.Instance_OnGotItems;
			if (items != null)
			{
				items.ForEach(delegate(InventoryItem p)
				{
					RewardItem.SpawnItem(this._itemsRoot, this._itemPrefab, p);
				});
			}
			this._rewardsList.SetActive(this._itemsRoot.transform.childCount > 0);
		}
	}

	private void SetMoneyAndExp(Reward reward)
	{
		Amount amount = new Amount
		{
			Currency = reward.Currency1,
			Value = ((reward.Money1 == null) ? 0 : ((int)reward.Money1.Value))
		};
		Amount amount2 = new Amount
		{
			Currency = reward.Currency2,
			Value = ((reward.Money2 == null) ? 0 : ((int)reward.Money2.Value))
		};
		int num = 0;
		int num2 = 0;
		UIHelper.ParseAmount(amount, ref num2, ref num);
		UIHelper.ParseAmount(amount2, ref num2, ref num);
		this._goldGo.SetActive(true);
		this._gold.text = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon("GC"), num);
		this._silverGo.SetActive(true);
		this._silver.text = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon("SC"), num2);
		this._xpGo.SetActive(reward.Experience > 0);
		if (this._xpGo.activeSelf)
		{
			this._xp.text = string.Format("<b>{0}</b> {1}", ScriptLocalization.Get("ExpCaption"), reward.Experience);
		}
	}

	private void ClearMoneyAndExp()
	{
		this._goldGo.SetActive(false);
		this._silverGo.SetActive(false);
		this._xpGo.SetActive(false);
	}

	private void Instance_OnFailureGetUserCompetitionReward(UserCompetitionFailure failure)
	{
		this.UnsubscribeGetUserCompetitionReward();
		Debug.LogErrorFormat("UGC FailureGetUserCompetitionReward FullErrorInfo:{0}", new object[] { failure.FullErrorInfo });
	}

	private void Instance_OnGetUserCompetitionReward(int tournamentId, Reward reward)
	{
		if (this._ugc != null && this._ugc.TournamentId == tournamentId)
		{
			this.UnsubscribeGetUserCompetitionReward();
			this.FillData(reward);
		}
	}

	private void UnsubscribeGetUserCompetitionReward()
	{
		PhotonConnectionFactory.Instance.OnGetUserCompetitionReward -= this.Instance_OnGetUserCompetitionReward;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetitionReward -= this.Instance_OnFailureGetUserCompetitionReward;
	}

	[SerializeField]
	private GameObject _rewardsList;

	[SerializeField]
	private GameObject _itemPrefab;

	[SerializeField]
	private GameObject _itemsRoot;

	[SerializeField]
	private Text _xp;

	[SerializeField]
	private Text _gold;

	[SerializeField]
	private Text _silver;

	[SerializeField]
	private GameObject _xpGo;

	[SerializeField]
	private GameObject _goldGo;

	[SerializeField]
	private GameObject _silverGo;

	private const int ItemSubscriberId = 823323;

	private TournamentDetailsRewards.BiggestFishData _biggestFish;

	private List<Reward> _rewards;

	private Tournament _tournament;

	private UserCompetitionPublic _ugc;

	private List<TournamentIndividualResults> _results;

	private bool _isActive;

	public class BiggestFishData
	{
		public BiggestFishData(double silvers, double golds, int productId)
		{
			this.Silvers = silvers;
			this.Golds = golds;
			this.ProductId = productId;
		}

		public double Silvers { get; private set; }

		public double Golds { get; private set; }

		public int ProductId { get; private set; }
	}
}
