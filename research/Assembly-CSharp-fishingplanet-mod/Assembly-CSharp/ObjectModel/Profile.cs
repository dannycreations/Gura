using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ObjectModel.Common;
using ObjectModel.Tournaments;
using Photon.Interfaces;
using SharedLib.Game;

namespace ObjectModel
{
	[JsonObject(1)]
	public class Profile
	{
		public static int LevelCap { get; set; }

		[JsonIgnore]
		public bool SerializationIsInProgress { get; set; }

		public Guid UserId { get; set; }

		public string Name { get; set; }

		public string Email { get; set; }

		public string Password { get; set; }

		public string PasswordQuestion { get; set; }

		public string PasswordAnswer { get; set; }

		public string PhoneNumber { get; set; }

		public string PlaceOfResidence { get; set; }

		public int AvatarBID { get; set; }

		public string AvatarUrl { get; set; }

		public int LanguageId { get; set; }

		public int HomeState { get; set; }

		[JsonProperty]
		public string HomeStateName { get; set; }

		public int? PondId
		{
			get
			{
				return this._PondId;
			}
			set
			{
				int? pondId = this._PondId;
				this._PondId = value;
				this.OnDependencyChanged("PondId", DependencyChange.Updated(pondId, value));
			}
		}

		public int Level
		{
			get
			{
				return this._Level;
			}
			set
			{
				int level = this._Level;
				this._Level = value;
				this.OnDependencyChanged("Level", DependencyChange.Updated(level, value));
			}
		}

		public int Rank
		{
			get
			{
				return this._Rank;
			}
			set
			{
				int rank = this._Rank;
				this._Rank = value;
				this.OnDependencyChanged("Rank", DependencyChange.Updated(rank, value));
			}
		}

		public long Experience { get; set; }

		public long RankExperience { get; set; }

		public DateTime? Birthday { get; set; }

		public string Gender { get; set; }

		public double SilverCoins { get; set; }

		public double GoldCoins { get; set; }

		public double SilverBages { get; set; }

		public double GoldBages { get; set; }

		public string Role { get; set; }

		public bool IsTempUser { get; set; }

		public DateTime CreationDate { get; set; }

		[JsonProperty]
		public string FavoriteFish { get; set; }

		[JsonProperty]
		public string FavoritePond { get; set; }

		[JsonProperty]
		public string FavoriteTackle { get; set; }

		[JsonProperty]
		public string FavoriteFishingMethod { get; set; }

		[JsonProperty]
		public int Karma { get; set; }

		[JsonProperty]
		public int Mood { get; set; }

		[JsonProperty]
		public int Power { get; set; }

		[JsonProperty]
		public int Reputation { get; set; }

		public int? TournamentRating { get; set; }

		public int? EventRating { get; set; }

		public int? CompetitionRating { get; set; }

		public int? DuelRating { get; set; }

		[JsonProperty]
		public List<Player> Friends { get; set; }

		[JsonProperty]
		public List<Player> FriendsUnfriended { get; set; }

		[JsonProperty]
		public Inventory Inventory { get; set; }

		[JsonProperty]
		public int? AdditionalInventoryCapacity
		{
			get
			{
				return this._AdditionalInventoryCapacity;
			}
			set
			{
				int? additionalInventoryCapacity = this._AdditionalInventoryCapacity;
				this._AdditionalInventoryCapacity = value;
				if (((additionalInventoryCapacity == null) ? 0 : additionalInventoryCapacity.Value) != ((value == null) ? 0 : value.Value))
				{
					this.OnDependencyChanged("AdditionalInventoryCapacity", DependencyChange.Updated(additionalInventoryCapacity, value));
					if (this.MissionsContext != null)
					{
						this.MissionsContext.ForcePropertiesToRecalculate(new string[] { "AvailableSlots" });
					}
				}
			}
		}

		[JsonProperty]
		public InventoryRodSetups InventoryRodSetups { get; set; }

		[JsonProperty]
		public ChumRecipes ChumRecipes { get; set; }

		[JsonProperty]
		public int? AdditionalRodSetupCapacity { get; set; }

		[JsonProperty]
		public int? AdditionalBuoyCapacity { get; set; }

		[JsonProperty]
		public int? AdditionalChumRecipesCapacity { get; set; }

		[JsonProperty]
		public List<BuoySetting> Buoys { get; set; }

		[JsonProperty]
		public List<BuoySetting> BuoyShareRequests { get; set; }

		public MissionsContext MissionsContext { get; set; }

		[JsonProperty]
		public Dictionary<string, string> Settings { get; set; }

		[JsonProperty]
		public DateTime? PondStartTime { get; set; }

		[JsonProperty]
		public int? PondStayTime { get; set; }

		[JsonProperty]
		public TimeSpan? PondTimeSpent { get; set; }

		[JsonProperty]
		public bool? IsTravelingByCar { get; set; }

		[JsonProperty]
		public List<PlayerLicense> Licenses { get; set; }

		[JsonProperty]
		public float Debt { get; set; }

		[JsonProperty]
		public long ExpToThisLevel { get; set; }

		[JsonProperty]
		public long ExpToNextLevel { get; set; }

		[JsonProperty]
		public long ExpToThisRank { get; set; }

		[JsonProperty]
		public long ExpToNextRank { get; set; }

		[JsonIgnore]
		public PlayerStats Stats { get; set; }

		[JsonProperty]
		public FishCageContents FishCage { get; set; }

		public TournamentTitles Title { get; set; }

		[JsonProperty]
		public DateTime TitleTimestamp { get; set; }

		[JsonProperty]
		public ProfileTournament Tournament { get; set; }

		[JsonProperty]
		public bool? IsUgcHost { get; set; }

		[JsonProperty]
		public bool? IsInfluencer { get; set; }

		[JsonProperty]
		public int? SubscriptionId { get; set; }

		[JsonProperty]
		public float? ExpMultiplier { get; set; }

		[JsonProperty]
		public float? MoneyMultiplier { get; set; }

		[JsonProperty]
		public DateTime? SubscriptionEndDate { get; set; }

		[JsonProperty]
		public bool IsIncognito { get; set; }

		public List<int> StartersOwned { get; set; }

		public List<int> StartersGiven { get; set; }

		public string Source { get; set; }

		public string ExternalId { get; set; }

		[JsonProperty]
		public string PaymentCurrency { get; set; }

		public bool IsTutorialFinished { get; set; }

		[JsonProperty]
		public bool IsLatestEulaSigned { get; set; }

		[JsonProperty]
		public List<LevelLockRemoval> LevelLockRemovals { get; set; }

		[JsonProperty]
		public List<PaidLockRemoval> PaidLockRemovals { get; set; }

		[JsonProperty]
		public string OwnPromoCode { get; set; }

		[JsonProperty]
		public string InvitedByPromoCode { get; set; }

		[JsonIgnore]
		public int CurrentDayOfStay
		{
			get
			{
				return (this.PondTimeSpent != null) ? (InGameTimeHelper.EnvelopeStayTime(this.PondTimeSpent.Value).Days + 1) : 181;
			}
		}

		public DateTime? ChatBanEndDate { get; set; }

		[JsonIgnore]
		public IEnumerable<PlayerLicense> ActiveLicenses
		{
			get
			{
				DateTime now = TimeHelper.UtcTime();
				return this.Licenses.Where((PlayerLicense l) => l.Term == 0 || l.End > now);
			}
		}

		[JsonProperty]
		public PersistentData PersistentData { get; set; }

		[JsonProperty]
		public int Flags { get; set; }

		public void Init()
		{
			if (this.Inventory != null)
			{
				this.Inventory.Profile = this;
			}
			if (this.InventoryRodSetups == null)
			{
				this.InventoryRodSetups = new InventoryRodSetups();
			}
			if (this.ChumRecipes == null)
			{
				this.ChumRecipes = new ChumRecipes();
			}
			foreach (RodSetup rodSetup in this.InventoryRodSetups)
			{
				rodSetup.Profile = this;
			}
		}

		public void InitMissionsContext()
		{
			if (this.MissionsContext == null)
			{
				this.MissionsContext = new MissionsContext();
			}
			this.MissionsContext.SetProfile(this);
			this.MissionsContext.InitCalculatedProperties();
		}

		public void OnDependencyChanged(string dependency, IDependencyChange change = null)
		{
			if (this.MissionsContext != null)
			{
				this.MissionsContext.OnDependencyChanged(dependency, change, true);
			}
		}

		public bool ValidateBuyLicense(int licenseId)
		{
			if (this.Licenses == null)
			{
				this.Licenses = new List<PlayerLicense>();
			}
			PlayerLicense playerLicense = this.Licenses.FirstOrDefault((PlayerLicense l) => l.LicenseId == licenseId);
			return playerLicense == null || (playerLicense.Term != 0 && playerLicense.End != null && playerLicense.CanExpire);
		}

		public PlayerLicense AddLicense(PlayerLicense license)
		{
			PlayerLicense playerLicense = null;
			if (this.Licenses == null)
			{
				this.Licenses = new List<PlayerLicense>();
			}
			else
			{
				playerLicense = this.Licenses.FirstOrDefault((PlayerLicense i) => i.LicenseId == license.LicenseId);
			}
			if (playerLicense != null && playerLicense.End < TimeHelper.UtcTime())
			{
				this.Licenses.Remove(playerLicense);
				playerLicense = null;
			}
			if (playerLicense != null)
			{
				if (license.Term == 0)
				{
					playerLicense.Term = 0;
					playerLicense.CanExpire = false;
					playerLicense.End = null;
					playerLicense.Cost = license.Cost;
					playerLicense.Currency = license.Currency;
					playerLicense.PenaltyCurrency = license.PenaltyCurrency;
				}
				else if (playerLicense.Term > 0)
				{
					playerLicense.Term += license.Term;
					if (playerLicense.End == null)
					{
						playerLicense.End = new DateTime?(TimeHelper.UtcTime().AddDays((double)license.Term));
					}
					else
					{
						playerLicense.End = new DateTime?(playerLicense.End.Value.AddDays((double)license.Term));
					}
				}
			}
			else
			{
				this.Licenses.Add(license);
			}
			return playerLicense ?? license;
		}

		public static PlayerLicense CloneLicense(PlayerLicense license)
		{
			PlayerLicense playerLicense = new PlayerLicense();
			playerLicense.MakeCloneOf(license, true);
			return playerLicense;
		}

		[JsonIgnore]
		public bool HasPremium
		{
			get
			{
				if (this.SubscriptionId != null && this.SubscriptionEndDate != null)
				{
					DateTime? subscriptionEndDate = this.SubscriptionEndDate;
					DateTime dateTime = TimeHelper.UtcTime();
					DateTime? dateTime2 = subscriptionEndDate;
					if (dateTime2 != null)
					{
						dateTime < dateTime2.GetValueOrDefault();
					}
				}
				return true;
			}
		}

		[JsonIgnore]
		public float Progress
		{
			get
			{
				if (this.ExpToNextLevel - this.ExpToThisLevel == 0L)
				{
					return 1f;
				}
				if (this.Experience < this.ExpToThisLevel)
				{
					return 0f;
				}
				if (this.Experience > this.ExpToNextLevel)
				{
					return 1f;
				}
				return (float)(this.Experience - this.ExpToThisLevel) / (float)(this.ExpToNextLevel - this.ExpToThisLevel);
			}
		}

		[JsonIgnore]
		public float RankProgress
		{
			get
			{
				if (this.ExpToNextRank - this.ExpToThisRank == 0L)
				{
					return 1f;
				}
				if (this.RankExperience < this.ExpToThisRank)
				{
					return 0f;
				}
				if (this.RankExperience > this.ExpToNextRank)
				{
					return 1f;
				}
				return (float)(this.RankExperience - this.ExpToThisRank) / (float)(this.ExpToNextRank - this.ExpToThisRank);
			}
		}

		public float PrevProgress(float expGained)
		{
			float num = (float)this.Experience - expGained;
			if (this.ExpToNextLevel - this.ExpToThisLevel == 0L)
			{
				return 1f;
			}
			if (num < (float)this.ExpToThisLevel)
			{
				return 0f;
			}
			if (num > (float)this.ExpToNextLevel)
			{
				return 1f;
			}
			return (num - (float)this.ExpToThisLevel) / (float)(this.ExpToNextLevel - this.ExpToThisLevel);
		}

		public List<TournamentPrecondition> CheckTournamentRegisterPreconditions(Tournament tournament)
		{
			if (tournament.Preconditions == null)
			{
				return null;
			}
			List<TournamentPrecondition> list = null;
			foreach (TournamentPrecondition tournamentPrecondition in tournament.Preconditions)
			{
				if (!tournamentPrecondition.Match(new TournamentTitles?(this.Title), this.Stats.GetBestTournamentPlace(tournamentPrecondition.TournamentTemplateId), new int?(this.Level)))
				{
					if (list == null)
					{
						list = new List<TournamentPrecondition>();
					}
					list.Add(tournamentPrecondition);
				}
			}
			return list;
		}

		public bool StartTournamentTime()
		{
			if (this.Tournament != null && this.PondId == this.Tournament.PondId)
			{
				this.Tournament.IsStarted = true;
				if (this.FishCage != null)
				{
					this.FishCage.Clear();
				}
				if (this.Tournament.StartTime != null)
				{
					if (this.Tournament.IsSynchronous)
					{
						TimeSpan timeSpan = TimeHelper.UtcTime().Subtract(this.Tournament.StartDate).Multiply(4f);
						this.PondTimeSpent = new TimeSpan?(this.Tournament.StartTime.Value.Add(timeSpan));
					}
					else
					{
						this.PondTimeSpent = this.Tournament.StartTime;
					}
				}
				else
				{
					this.Tournament.StartTime = this.PondTimeSpent;
				}
				this.Tournament.EndTime = new TimeSpan?(this.Tournament.StartTime.Value.AddHours((double)this.Tournament.InGameDuration + (double)this.Tournament.InGameDurationMinute / 60.0));
				return true;
			}
			return false;
		}

		public void EndTournamentTime(bool isIdle)
		{
			if (isIdle)
			{
				this.Tournament = null;
			}
			else
			{
				this.Tournament.IsEnded = true;
			}
		}

		public void ApplyTournamentResult(TournamentTitles titleGiven, TournamentTitles titleProlonged, IEnumerable<Reward> rewards, int tournamnetKind, int? rating)
		{
			if (titleGiven > this.Title)
			{
				this.Title = titleGiven;
				this.TitleTimestamp = DateTime.Now;
			}
			else if (titleProlonged == this.Title)
			{
				this.TitleTimestamp = DateTime.Now;
			}
			foreach (Reward reward in rewards)
			{
				if (reward.Money1 != null && reward.Money1 > 0.0)
				{
					this.IncrementBalance(reward.Currency1, reward.Money1.Value);
				}
				if (reward.Money2 != null && reward.Money2 > 0.0)
				{
					this.IncrementBalance(reward.Currency2, reward.Money2.Value);
				}
			}
			if (rating != null)
			{
				if (tournamnetKind == 1)
				{
					int? num;
					if (rating != null)
					{
						int? tournamentRating = this.TournamentRating;
						num = new int?(((tournamentRating == null) ? 0 : tournamentRating.Value) + rating.GetValueOrDefault());
					}
					else
					{
						num = null;
					}
					this.TournamentRating = num;
				}
				else if (tournamnetKind == 2)
				{
					int? num2;
					if (rating != null)
					{
						int? eventRating = this.EventRating;
						num2 = new int?(((eventRating == null) ? 0 : eventRating.Value) + rating.GetValueOrDefault());
					}
					else
					{
						num2 = null;
					}
					this.EventRating = num2;
				}
				else if (tournamnetKind == 3)
				{
					int? num3;
					if (rating != null)
					{
						int? competitionRating = this.CompetitionRating;
						num3 = new int?(((competitionRating == null) ? 0 : competitionRating.Value) + rating.GetValueOrDefault());
					}
					else
					{
						num3 = null;
					}
					this.CompetitionRating = num3;
				}
			}
		}

		public double GetBalance(string currency)
		{
			if (currency == "SC")
			{
				return this.SilverCoins;
			}
			if (currency == "GC")
			{
				return this.GoldCoins;
			}
			return 0.0;
		}

		public void IncrementBalance(Amount amount)
		{
			this.IncrementBalance(amount.Currency, (double)amount.Value);
		}

		public void IncrementBalance(string currency, double value)
		{
			if (currency == "SC")
			{
				this.SilverCoins += value;
			}
			if (currency == "GC")
			{
				this.GoldCoins += value;
			}
		}

		public bool RefreshTitle(int validityDays)
		{
			if (this.Title == TournamentTitles.None)
			{
				return false;
			}
			if (this.ValidateTitle(validityDays))
			{
				return false;
			}
			this.Title = (TournamentTitles)(this.Title - TournamentTitles.Amateur);
			return true;
		}

		private bool ValidateTitle(int validityDays)
		{
			return DateTime.Now.AddDays((double)validityDays) > this.TitleTimestamp;
		}

		public void PutProductToProfile(ProfileProduct product)
		{
			List<LevelLockRemoval> list;
			this.PutProductToProfile(product, out list, null);
		}

		public void PutProductToProfile(ProfileProduct product, out List<LevelLockRemoval> outdateLevelLockRemovals, DateTime? expireDate = null)
		{
			outdateLevelLockRemovals = null;
			if (product.Silver != null)
			{
				this.IncrementBalance("SC", (double)product.Silver.Value);
			}
			if (product.Gold != null)
			{
				this.IncrementBalance("GC", (double)product.Gold.Value);
			}
			if (product.HasSubscription)
			{
				this.SubscriptionId = new int?(product.ProductId);
				this.ExpMultiplier = new float?((float)product.ExpMultiplier.Value);
				this.MoneyMultiplier = new float?((float)product.MoneyMultiplier.Value);
				if (expireDate != null)
				{
					this.SubscriptionEndDate = expireDate;
				}
				else if (this.SubscriptionEndDate != null && this.SubscriptionEndDate > TimeHelper.UtcTime())
				{
					this.SubscriptionEndDate = new DateTime?(this.SubscriptionEndDate.Value.AddDays((double)product.Term.Value));
				}
				else
				{
					this.SubscriptionEndDate = new DateTime?(TimeHelper.UtcTime().AddDays((double)product.Term.Value));
				}
			}
			if (product.PondsUnlocked != null)
			{
				this.OutdateLockRemoval(out outdateLevelLockRemovals);
				if (this.LevelLockRemovals == null)
				{
					this.LevelLockRemovals = new List<LevelLockRemoval>();
				}
				int[] pondsUnlocked = product.PondsUnlocked;
				for (int i = 0; i < pondsUnlocked.Length; i++)
				{
					int pondId2 = pondsUnlocked[i];
					LevelLockRemoval levelLockRemoval = this.LevelLockRemovals.FirstOrDefault((LevelLockRemoval r) => r.EndDate != null && r.AccessibleLevel != null && r.Ponds.Length == 1 && r.Ponds[0] == pondId2);
					LevelLockRemoval levelLockRemoval2 = this.LevelLockRemovals.FirstOrDefault((LevelLockRemoval r) => r.EndDate == null && r.AccessibleLevel != null && r.Ponds.Length == 1 && r.Ponds[0] == pondId2);
					if (product.Term == null)
					{
						if (levelLockRemoval2 == null)
						{
							if (levelLockRemoval != null)
							{
								levelLockRemoval.EndDate = null;
							}
							else
							{
								this.LevelLockRemovals.Add(new LevelLockRemoval
								{
									StartDate = DateTime.UtcNow,
									Ponds = new int[] { pondId2 },
									AccessibleLevel = product.AccessibleLevel
								});
							}
						}
					}
					else if (levelLockRemoval2 == null)
					{
						if (levelLockRemoval == null)
						{
							this.LevelLockRemovals.Add(new LevelLockRemoval
							{
								StartDate = DateTime.UtcNow,
								EndDate = new DateTime?((expireDate == null) ? DateTime.UtcNow.AddDays((double)product.Term.Value) : expireDate.Value),
								Ponds = new int[] { pondId2 },
								AccessibleLevel = product.AccessibleLevel
							});
						}
						else
						{
							levelLockRemoval.EndDate = new DateTime?((expireDate == null) ? levelLockRemoval.EndDate.Value.AddDays((double)product.Term.Value) : expireDate.Value);
						}
					}
				}
			}
			if (product.PaidPondsUnlocked != null)
			{
				if (this.PaidLockRemovals == null)
				{
					this.PaidLockRemovals = new List<PaidLockRemoval>();
				}
				int[] paidPondsUnlocked = product.PaidPondsUnlocked;
				for (int j = 0; j < paidPondsUnlocked.Length; j++)
				{
					int pondId = paidPondsUnlocked[j];
					if (this.PaidLockRemovals.FirstOrDefault((PaidLockRemoval unlock) => unlock.Ponds[0] == pondId) == null)
					{
						this.PaidLockRemovals.Add(new PaidLockRemoval
						{
							Ponds = new int[] { pondId },
							EntitlementId = ((product.PlatformId != 2) ? null : product.ForeignProductId)
						});
					}
				}
			}
			if (product.Items != null && product.Items.Length > 0)
			{
				foreach (InventoryItem inventoryItem in product.Items)
				{
					this.Inventory.JoinItem(inventoryItem, null);
				}
			}
			if (product.Licenses != null && product.Licenses.Length > 0)
			{
				this.PutLicenses(product.Licenses, null);
			}
			if (product.RodSetups != null && product.RodSetups.Length > 0)
			{
				foreach (RodSetup rodSetup in product.RodSetups)
				{
					if (string.IsNullOrEmpty(rodSetup.Name))
					{
						string text = string.Format("{0} - {1}", product.Name, Inventory.GetRodSubtypeName(rodSetup.Rod));
						int nextIndexForRodSetup = this.Inventory.GetNextIndexForRodSetup(text);
						rodSetup.Name = string.Format("{0} #{1}", text, nextIndexForRodSetup);
					}
					this.Inventory.AddSetup(rodSetup);
				}
			}
			if (product.InventoryExt != null)
			{
				int? additionalInventoryCapacity = this.AdditionalInventoryCapacity;
				this.AdditionalInventoryCapacity = new int?(((additionalInventoryCapacity == null) ? 0 : additionalInventoryCapacity.Value) + product.InventoryExt.Value);
				if (this.AdditionalInventoryCapacity > Inventory.MaxInventoryCapacity - Inventory.DefaultInventoryCapacity)
				{
					this.AdditionalInventoryCapacity = new int?(Math.Max(Inventory.MaxInventoryCapacity - Inventory.DefaultInventoryCapacity, 0));
				}
			}
			if (product.RodSetupExt != null)
			{
				int? additionalRodSetupCapacity = this.AdditionalRodSetupCapacity;
				this.AdditionalRodSetupCapacity = new int?(((additionalRodSetupCapacity == null) ? 0 : additionalRodSetupCapacity.Value) + product.RodSetupExt.Value);
				if (this.AdditionalRodSetupCapacity > Inventory.MaxRodSetupCapacity - Inventory.DefaultRodSetupCapacity)
				{
					this.AdditionalRodSetupCapacity = new int?(Math.Max(Inventory.MaxRodSetupCapacity - Inventory.DefaultRodSetupCapacity, 0));
				}
			}
			if (product.BuoyExt != null)
			{
				int? additionalBuoyCapacity = this.AdditionalBuoyCapacity;
				this.AdditionalBuoyCapacity = new int?(((additionalBuoyCapacity == null) ? 0 : additionalBuoyCapacity.Value) + product.BuoyExt.Value);
				if (this.AdditionalBuoyCapacity > Inventory.MaxBuoyCapacity - Inventory.DefaultBuoyCapacity)
				{
					this.AdditionalBuoyCapacity = new int?(Math.Max(Inventory.MaxBuoyCapacity - Inventory.DefaultBuoyCapacity, 0));
				}
			}
			if (product.ChumRecipesExt != null)
			{
				int? additionalChumRecipesCapacity = this.AdditionalChumRecipesCapacity;
				this.AdditionalChumRecipesCapacity = new int?(((additionalChumRecipesCapacity == null) ? 0 : additionalChumRecipesCapacity.Value) + product.ChumRecipesExt.Value);
				if (this.AdditionalChumRecipesCapacity > Inventory.MaxChumRecipesCapacity - Inventory.DefaultChumRecipesCapacity)
				{
					this.AdditionalChumRecipesCapacity = new int?(Math.Max(Inventory.MaxChumRecipesCapacity - Inventory.DefaultChumRecipesCapacity, 0));
				}
			}
		}

		public bool UpdateSubscriptionEndDate(ProfileProduct product, DateTime expireDate, out List<LevelLockRemoval> outdateLevelLockRemovals)
		{
			bool flag = false;
			outdateLevelLockRemovals = null;
			if (product.HasSubscription)
			{
				if (this.SubscriptionId == null)
				{
					this.SubscriptionId = new int?(product.ProductId);
					flag = true;
				}
				if (this.ExpMultiplier == null)
				{
					this.ExpMultiplier = new float?((float)product.ExpMultiplier.Value);
					flag = true;
				}
				if (this.MoneyMultiplier == null)
				{
					this.MoneyMultiplier = new float?((float)product.MoneyMultiplier.Value);
					flag = true;
				}
				if (this.SubscriptionEndDate != expireDate)
				{
					this.SubscriptionEndDate = new DateTime?(expireDate);
					flag = true;
				}
			}
			if (product.PondsUnlocked != null && product.Term != null)
			{
				this.OutdateLockRemoval(out outdateLevelLockRemovals);
				if (this.LevelLockRemovals == null)
				{
					this.LevelLockRemovals = new List<LevelLockRemoval>();
				}
				int[] pondsUnlocked = product.PondsUnlocked;
				for (int i = 0; i < pondsUnlocked.Length; i++)
				{
					int pondId = pondsUnlocked[i];
					LevelLockRemoval levelLockRemoval = this.LevelLockRemovals.FirstOrDefault((LevelLockRemoval r) => r.EndDate != null && r.AccessibleLevel != null && r.Ponds.Length == 1 && r.Ponds[0] == pondId);
					if (levelLockRemoval == null)
					{
						this.LevelLockRemovals.Add(new LevelLockRemoval
						{
							StartDate = TimeHelper.UtcTime(),
							EndDate = new DateTime?(expireDate),
							Ponds = new int[] { pondId },
							AccessibleLevel = product.AccessibleLevel
						});
						flag = true;
					}
					else if (levelLockRemoval.EndDate != expireDate)
					{
						levelLockRemoval.EndDate = new DateTime?(expireDate);
						flag = true;
					}
				}
			}
			return flag;
		}

		public void DeleteSubscriptions(IEnumerable<Subscription> subscriptionsToDelete)
		{
			using (IEnumerator<Subscription> enumerator = subscriptionsToDelete.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Subscription s = enumerator.Current;
					if (s.IsPremium)
					{
						this.SubscriptionId = null;
						this.SubscriptionEndDate = null;
						this.ExpMultiplier = null;
						this.MoneyMultiplier = null;
					}
					else
					{
						this.LevelLockRemovals.RemoveAll((LevelLockRemoval r) => r.EndDate != null && r.Ponds != null && r.Ponds.Length == 1 && r.Ponds[0] == s.PondId);
					}
				}
			}
		}

		public bool OutdateLockRemoval(out List<LevelLockRemoval> outdateLevelLockRemovals)
		{
			outdateLevelLockRemovals = new List<LevelLockRemoval>();
			if (this.LevelLockRemovals == null)
			{
				return false;
			}
			DateTime now = DateTime.UtcNow;
			List<LevelLockRemoval> list = this.LevelLockRemovals.Where((LevelLockRemoval r) => r.EndDate != null && r.EndDate < now).ToList<LevelLockRemoval>();
			if (list.Any<LevelLockRemoval>())
			{
				outdateLevelLockRemovals.AddRange(list);
				list.ForEach(delegate(LevelLockRemoval r)
				{
					this.LevelLockRemovals.Remove(r);
				});
				if (this.LevelLockRemovals.Count == 0)
				{
					this.LevelLockRemovals = null;
				}
				return true;
			}
			return false;
		}

		private void PutItems(InventoryItem[] items)
		{
			foreach (InventoryItem inventoryItem in items)
			{
				if (inventoryItem is Rod)
				{
					this.AddRod(inventoryItem as Rod);
				}
				else if (inventoryItem is OutfitItem)
				{
					this.AddOutfitItem(inventoryItem as OutfitItem);
				}
				else if (inventoryItem is ToolItem)
				{
					this.AddToolItem(inventoryItem as ToolItem);
				}
				else
				{
					this.Inventory.JoinItem(inventoryItem, null);
				}
			}
			this.MoveItemExcessFromEquipmentToStorage(items);
		}

		public void MoveItemExcessFromEquipmentToStorage(InventoryItem[] itemsToPreserve)
		{
			InventoryItem[] array = this.Inventory.RodsAndReelsOutOfStorage.ToArray<InventoryItem>();
			if (array.Length > 0)
			{
				foreach (InventoryItem inventoryItem in array)
				{
					this.Inventory.MoveItem(inventoryItem, null, StoragePlaces.Storage, true, true);
				}
				this.Inventory.RenumberSlots();
			}
			foreach (InventoryItem inventoryItem2 in this.Inventory.GetAllTackleOutOfStorage(itemsToPreserve))
			{
				this.Inventory.MoveItem(inventoryItem2, null, StoragePlaces.Storage, true, true);
			}
			foreach (InventoryItem inventoryItem3 in this.Inventory.GetAllChumOutOfStorage(itemsToPreserve))
			{
				this.Inventory.MoveItem(inventoryItem3, null, StoragePlaces.Storage, true, true);
			}
		}

		private void AddRod(Rod rod)
		{
			if (rod.Slot != 0 && (rod.Storage == StoragePlaces.Doll || rod.Storage == StoragePlaces.Hands))
			{
				InventoryItem inventoryItem = this.Inventory.FirstOrDefault((InventoryItem i) => i.Slot == rod.Slot);
				if (inventoryItem != null)
				{
					inventoryItem.Slot = 0;
					this.Inventory.MoveItem(inventoryItem, null, StoragePlaces.Storage, true, true);
				}
			}
			this.Inventory.AddItem(rod);
		}

		private void AddOutfitItem(OutfitItem outfitItem)
		{
			if (outfitItem.Storage == StoragePlaces.Doll)
			{
				InventoryItem inventoryItem = this.Inventory.FirstOrDefault((InventoryItem i) => i.ItemType == outfitItem.ItemType && i.ItemSubType == outfitItem.ItemSubType && i.Storage == StoragePlaces.Doll);
				if (inventoryItem != null)
				{
					inventoryItem.Storage = StoragePlaces.Storage;
				}
			}
			this.Inventory.AddItem(outfitItem);
		}

		private void AddToolItem(ToolItem toolItem)
		{
			InventoryItem inventoryItem;
			if (toolItem is FishCage)
			{
				inventoryItem = this.Inventory.FirstOrDefault((InventoryItem i) => i is FishCage && i.Storage == StoragePlaces.Doll);
			}
			else
			{
				inventoryItem = this.Inventory.FirstOrDefault((InventoryItem i) => i.ItemType == toolItem.ItemType && i.ItemSubType == toolItem.ItemSubType && i.Storage == StoragePlaces.Doll);
			}
			if (inventoryItem != null)
			{
				inventoryItem.Storage = StoragePlaces.Storage;
			}
			this.Inventory.AddItem(toolItem);
		}

		private void PutLicenses(PlayerLicense[] licenses, DateTime? expireDate = null)
		{
			foreach (PlayerLicense playerLicense in licenses)
			{
				PlayerLicense playerLicense2 = this.AddLicense(Profile.CloneLicense(playerLicense));
				if (playerLicense2.Term > 0 && playerLicense2.End != null && expireDate != null && playerLicense2.End < expireDate)
				{
					playerLicense2.End = expireDate;
				}
			}
		}

		public bool ChekcInteractionIsOnTime(InteractiveObject obj)
		{
			DateTime dateTime = TimeHelper.UtcTime();
			return dateTime >= obj.InteractionStart && dateTime <= obj.InteractionEnd;
		}

		public bool CanInteractWithObject(InteractiveObject obj)
		{
			if (obj.Config.CustomInteractionRule == CustomInteractionRules.Always)
			{
				if (obj.Config.Frequency == InteractFreq.Daily)
				{
					return this.CanInteractWithDailyObject(obj.ObjectId);
				}
				if (obj.Config.Frequency == InteractFreq.OneTime || obj.Config.Frequency == InteractFreq.OneTimeAndHide)
				{
					return this.CanInteractWithOneTimeObject(obj.ObjectId);
				}
			}
			else if (obj.Config.CustomInteractionRule == CustomInteractionRules.ZeroCredsNoRewardItem)
			{
				if (obj.Config.ItemRefs.Length != 1)
				{
					throw new InvalidOperationException("Configuration error. Object with ZeroCredsNoRewardItem custom interactionRule has wrong number of items configured, should be 1.");
				}
				if (obj.Config.ItemRefs.First<ItemRef>().Id == null)
				{
					throw new InvalidOperationException("Configuration error. Object with ZeroCredsNoRewardItem custom interactionRule has item without ItemId configured.");
				}
				int rewardItemId = obj.Config.ItemRefs.First<ItemRef>().Id.Value;
				return this.SilverCoins <= 0.0 && this.Inventory.All((InventoryItem i) => i.ItemId != rewardItemId);
			}
			return false;
		}

		public bool CanShowObject(InteractiveObject obj)
		{
			DateTime dateTime = TimeHelper.UtcTime();
			return !(dateTime < obj.ShowStart) && !(dateTime > obj.ShowEnd) && (obj.Config.Frequency != InteractFreq.OneTimeAndHide || this.CanInteractWithOneTimeObject(obj.ObjectId));
		}

		private bool CanInteractWithOneTimeObject(int objId)
		{
			return this.Stats.GetObjectInteractions(objId) == 0;
		}

		private bool CanInteractWithDailyObject(int objId)
		{
			return this.Stats.GetObjectDailyInteractions(objId) == 0;
		}

		public bool CanBuyItemWithoutSendingToStorage(InventoryItem item)
		{
			if (this.Tournament != null && !this.Tournament.IsEnded)
			{
				ErrorCode errorCode = this.CheckTournamentStartPrerequisitesRodStands(this.Tournament.EquipmentAllowed, item as RodStand);
				if (errorCode != null)
				{
					return false;
				}
				if (item is Rod && this.Inventory.GetDefaultStorage(item) == StoragePlaces.Doll)
				{
					List<Rod> list = (from i in this.Inventory.OfType<Rod>()
						where i.IsRodOnDoll
						select i).ToList<Rod>();
					ErrorCode errorCode2 = this.CheckTournamentStartPrerequisitesRodCountRodTypesCount(this.Tournament.EquipmentAllowed, list.Union(new Rod[] { (Rod)item }));
					if (errorCode2 != null)
					{
						return false;
					}
				}
			}
			bool flag;
			if (this.Inventory.GetDefaultStorage(item) == null && (item.IsUnstockable || !this.Inventory.Any((InventoryItem i) => item.CanCombineWith(i) && i.Storage == StoragePlaces.Equipment)))
			{
				if (item is Boat)
				{
					flag = this.Inventory.OfType<Boat>().All((Boat i) => i.Storage != StoragePlaces.Doll);
				}
				else
				{
					flag = false;
				}
			}
			else
			{
				flag = true;
			}
			return flag;
		}

		public ErrorCode CheckTournamentStartPrerequisitesEquipment(UserCompetitionPublic competition)
		{
			Tournament tournament = new Tournament
			{
				PondId = competition.PondId,
				PrimaryScoring = new TournamentScoring
				{
					ScoringFishSource = competition.FishSource
				},
				TournamentType = competition.GetTournamentType(),
				EquipmentAllowed = competition.TournamentEquipment
			};
			return this.CheckTournamentStartPrerequisitesEquipment(tournament);
		}

		public ErrorCode CheckTournamentStartPrerequisitesRods(UserCompetitionPublic competition)
		{
			Tournament tournament = new Tournament
			{
				PondId = competition.PondId,
				PrimaryScoring = new TournamentScoring
				{
					ScoringFishSource = competition.FishSource
				},
				TournamentType = competition.GetTournamentType(),
				EquipmentAllowed = competition.TournamentEquipment
			};
			return this.CheckTournamentStartPrerequisitesRods(tournament);
		}

		public ErrorCode CheckTournamentStartPrerequisites(TournamentBase tournament)
		{
			if (this.Tournament != null)
			{
				return 32644;
			}
			if (this.PondId != tournament.PondId)
			{
				return 32643;
			}
			ErrorCode errorCode = this.CheckTournamentStartPrerequisitesEquipment(tournament);
			if (errorCode != null)
			{
				return errorCode;
			}
			ErrorCode errorCode2 = this.CheckTournamentStartPrerequisitesRods(tournament);
			if (errorCode2 != null)
			{
				return errorCode2;
			}
			return 0;
		}

		public ErrorCode CheckTournamentStartPrerequisitesEquipment(TournamentBase tournament)
		{
			InventoryItem inventoryItem = this.Inventory.FirstOrDefault((InventoryItem i) => i is FishCage && i.Storage == StoragePlaces.Doll);
			if (tournament.PrimaryScoring.ScoringFishSource == TournamentFishSource.Cage && inventoryItem == null)
			{
				return 32638;
			}
			if (tournament.EquipmentAllowed == null)
			{
				return 0;
			}
			if (tournament.EquipmentAllowed.FishCageTypes != null && tournament.EquipmentAllowed.FishCageTypes.Length > 0)
			{
				if (inventoryItem == null)
				{
					if (!UserCompetitionLogic.IsCustomCompetition(tournament.TournamentType))
					{
						return 32638;
					}
				}
				else if (!tournament.EquipmentAllowed.FishCageTypes.Contains(inventoryItem.ItemSubType))
				{
					return 32579;
				}
			}
			RodStand rodStand = this.Inventory.OfType<RodStand>().FirstOrDefault((RodStand s) => s.Storage == StoragePlaces.Doll);
			ErrorCode errorCode = this.CheckTournamentStartPrerequisitesRodStands(tournament.EquipmentAllowed, rodStand);
			if (errorCode != null)
			{
				return errorCode;
			}
			return 0;
		}

		public ErrorCode CheckTournamentStartPrerequisitesRods(TournamentBase tournament)
		{
			List<Rod> list = (from i in this.Inventory.OfType<Rod>()
				where i.IsRodOnDoll
				select i).ToList<Rod>();
			ErrorCode errorCode = this.CheckTournamentStartPrerequisitesRodCountRodTypesCount(tournament.EquipmentAllowed, list);
			if (errorCode != null)
			{
				return errorCode;
			}
			bool flag = false;
			bool flag2 = !UserCompetitionLogic.IsCustomCompetition(tournament.TournamentType);
			foreach (Rod rod in list)
			{
				RodTemplate rodTemplate = this.Inventory.GetRodTemplate(rod);
				if (!flag2 || rodTemplate != RodTemplate.UnEquiped)
				{
					if (this.RodMatchesTournament(rod, rodTemplate, tournament.EquipmentAllowed, true) == null)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				return 0;
			}
			if (UserCompetitionLogic.IsCustomCompetition(tournament.TournamentType))
			{
				return 32556;
			}
			return 32557;
		}

		private ErrorCode CheckTournamentStartPrerequisitesRodCountRodTypesCount(TournamentEquipment tournamentEquipment, IEnumerable<Rod> rods)
		{
			int num = 0;
			List<ItemSubTypes> list = new List<ItemSubTypes>();
			foreach (Rod rod in rods)
			{
				num++;
				if (!list.Contains(rod.ItemSubType))
				{
					list.Add(rod.ItemSubType);
				}
			}
			if (tournamentEquipment.MaxRods != null && num > tournamentEquipment.MaxRods)
			{
				return 32581;
			}
			if (tournamentEquipment.MaxRodTypes != null)
			{
				int? maxRodTypes = tournamentEquipment.MaxRodTypes;
				if (list.Count > maxRodTypes)
				{
					return 32580;
				}
			}
			return 0;
		}

		private ErrorCode CheckTournamentStartPrerequisitesRodStands(TournamentEquipment tournamentEquipment, RodStand rodStand)
		{
			return 0;
		}

		private ErrorCode RodMatchesTournament(Rod rod, RodTemplate rodBuild, TournamentEquipment tournamentEquipment, bool checkTackleLevelLimits = true)
		{
			if (tournamentEquipment == null)
			{
				return 0;
			}
			List<TournamentRodEquipment> list = new List<TournamentRodEquipment>();
			if (tournamentEquipment != null)
			{
				list.Add(tournamentEquipment);
				if (tournamentEquipment.Alternatives != null)
				{
					list.AddRange(tournamentEquipment.Alternatives);
				}
			}
			list = list.Where((TournamentRodEquipment rodEquipment) => rodEquipment.HasRodCondition || rodEquipment.HasLineCondition || rodEquipment.HasSinkerCondition || rodEquipment.HasFeederCondition || rodEquipment.HasLeaderCondition || rodEquipment.HasTerminalTackleCondition || rodEquipment.HasHookCondition || rodEquipment.HasBaitCondition || rodEquipment.HasChumCondition).ToList<TournamentRodEquipment>();
			if (list.Any<TournamentRodEquipment>())
			{
				bool flag = false;
				foreach (TournamentRodEquipment tournamentRodEquipment in list)
				{
					if (this.RodMatchesTournamentSingleRod(rod, rodBuild, tournamentRodEquipment) == null)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return 32557;
				}
			}
			if (checkTackleLevelLimits && tournamentEquipment.TackleLevelLimits != null)
			{
				InventoryItem[] array = this.Inventory.Items.Where(delegate(InventoryItem i)
				{
					Guid? parentItemInstanceId = i.ParentItemInstanceId;
					bool flag2 = parentItemInstanceId != null;
					Guid? instanceId = rod.InstanceId;
					return flag2 == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
				}).ToArray<InventoryItem>();
				foreach (TackleLevelLimit tackleLevelLimit in tournamentEquipment.TackleLevelLimits)
				{
					if (rod.ItemSubType == tackleLevelLimit.TackleType)
					{
						if (tackleLevelLimit.MinLevel != null)
						{
							int? minLevel = tackleLevelLimit.MinLevel;
							if (rod.MinLevel < minLevel)
							{
								return 32573;
							}
						}
						if (tackleLevelLimit.MaxLevel != null)
						{
							int? maxLevel = tackleLevelLimit.MaxLevel;
							if (rod.MinLevel > maxLevel)
							{
								return 32573;
							}
						}
					}
					foreach (InventoryItem inventoryItem in array)
					{
						if (inventoryItem.ItemSubType == tackleLevelLimit.TackleType)
						{
							if (tackleLevelLimit.MinLevel != null)
							{
								int? minLevel2 = tackleLevelLimit.MinLevel;
								if (inventoryItem.MinLevel < minLevel2)
								{
									return 32573;
								}
							}
							if (tackleLevelLimit.MaxLevel != null)
							{
								int? maxLevel2 = tackleLevelLimit.MaxLevel;
								if (inventoryItem.MinLevel > maxLevel2)
								{
									return 32573;
								}
							}
						}
					}
				}
			}
			return 0;
		}

		public ErrorCode RodMatchesTournamentSingleRod(Rod rod, RodTemplate rodBuild, TournamentRodEquipment rodEquipment)
		{
			if (rodEquipment == null)
			{
				return 0;
			}
			if (rodEquipment.RodTypes != null && !rodEquipment.RodTypes.Contains(rod.ItemSubType))
			{
				return 32578;
			}
			if (rodEquipment.RodIds != null && !rodEquipment.RodIds.Contains(rod.ItemId))
			{
				return 32578;
			}
			if (rodEquipment.RodBuilds != null && !rodEquipment.RodBuilds.Contains(rodBuild))
			{
				return 32577;
			}
			InventoryItem[] array = this.Inventory.Items.Where(delegate(InventoryItem i)
			{
				Guid? parentItemInstanceId = i.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = rod.InstanceId;
				return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
			}).ToArray<InventoryItem>();
			Func<InventoryItem, bool> func = (InventoryItem item) => item.ItemType == ItemTypes.Hook || item.ItemType == ItemTypes.Lure || item.ItemType == ItemTypes.JigHead;
			Func<InventoryItem, bool> func2 = (InventoryItem item) => item.ItemType == ItemTypes.Bait || item.ItemType == ItemTypes.JigBait;
			if (rodEquipment.HasLineCondition || rodEquipment.HasSinkerCondition || rodEquipment.HasFeederCondition || rodEquipment.HasLeaderCondition || rodEquipment.HasTerminalTackleCondition || rodEquipment.HasHookCondition || rodEquipment.HasBaitCondition || rodEquipment.HasChumCondition)
			{
				foreach (InventoryItem inventoryItem in array)
				{
					if (!(inventoryItem is Bobber))
					{
						if (rodEquipment.HasLineCondition && inventoryItem.ItemType == ItemTypes.Line)
						{
							if (rodEquipment.LineTypes != null && !rodEquipment.LineTypes.Contains(inventoryItem.ItemSubType) && !rodEquipment.LineTypes.Contains((ItemSubTypes)inventoryItem.ItemType))
							{
								return 32576;
							}
							if (rodEquipment.LineIds != null && !rodEquipment.LineIds.Contains(inventoryItem.ItemId))
							{
								return 32576;
							}
						}
						if (rodEquipment.HasSinkerCondition && inventoryItem.ItemType == ItemTypes.Sinker)
						{
							if (rodEquipment.SinkerTypes != null && !rodEquipment.SinkerTypes.Contains(inventoryItem.ItemSubType) && !rodEquipment.SinkerTypes.Contains((ItemSubTypes)inventoryItem.ItemType))
							{
								return 32575;
							}
							if (rodEquipment.SinkerIds != null && !rodEquipment.SinkerIds.Contains(inventoryItem.ItemId))
							{
								return 32575;
							}
						}
						if (rodEquipment.HasFeederCondition && inventoryItem.ItemType == ItemTypes.Feeder)
						{
							if (rodEquipment.FeederTypes != null && !rodEquipment.FeederTypes.Contains(inventoryItem.ItemSubType) && !rodEquipment.FeederTypes.Contains((ItemSubTypes)inventoryItem.ItemType))
							{
								return 32575;
							}
							if (rodEquipment.FeederIds != null && !rodEquipment.FeederIds.Contains(inventoryItem.ItemId))
							{
								return 32575;
							}
						}
						if (rodEquipment.HasLeaderCondition && inventoryItem.ItemType == ItemTypes.Leader)
						{
							if (rodEquipment.LeaderTypes != null && !rodEquipment.LeaderTypes.Contains(inventoryItem.ItemSubType) && !rodEquipment.LeaderTypes.Contains((ItemSubTypes)inventoryItem.ItemType))
							{
								return 32575;
							}
							if (rodEquipment.LeaderIds != null && !rodEquipment.LeaderIds.Contains(inventoryItem.ItemId))
							{
								return 32575;
							}
						}
						if (rodEquipment.HasHookCondition && inventoryItem.ItemType == ItemTypes.Hook)
						{
							if (rodEquipment.HookTypes != null && !rodEquipment.HookTypes.Contains(inventoryItem.ItemSubType) && !rodEquipment.HookTypes.Contains((ItemSubTypes)inventoryItem.ItemType))
							{
								return 32575;
							}
							if (rodEquipment.HookIds != null && !rodEquipment.HookIds.Contains(inventoryItem.ItemId))
							{
								return 32575;
							}
						}
						if (rodEquipment.HasBaitCondition && func2(inventoryItem))
						{
							BoilBait boilBait = inventoryItem as BoilBait;
							Func<BoilBaitForm, bool> hasBoilBaitForm = (BoilBaitForm f) => boilBait != null && boilBait.Form == f;
							if (rodEquipment.BaitTypes != null && !rodEquipment.BaitTypes.Contains(inventoryItem.ItemSubType) && !rodEquipment.BaitTypes.Contains((ItemSubTypes)inventoryItem.ItemType) && !rodEquipment.BaitTypes.Any(delegate(ItemSubTypes t)
							{
								if (t != ItemSubTypes.BoilBait_Boils)
								{
									return t == ItemSubTypes.BoilBait_Pellets && hasBoilBaitForm(BoilBaitForm.Pellets);
								}
								return hasBoilBaitForm(BoilBaitForm.Boils);
							}))
							{
								return 32574;
							}
							if (rodEquipment.BaitIds != null && !rodEquipment.BaitIds.Contains(inventoryItem.ItemId))
							{
								return 32574;
							}
							if (rodEquipment.BoilBaitForms != null && !rodEquipment.BoilBaitForms.Any(hasBoilBaitForm))
							{
								return 32574;
							}
						}
						if (rodEquipment.HasTerminalTackleCondition && func(inventoryItem))
						{
							if (rodEquipment.TerminalTackleTypes != null && !rodEquipment.TerminalTackleTypes.Contains(inventoryItem.ItemSubType) && !rodEquipment.TerminalTackleTypes.Contains((ItemSubTypes)inventoryItem.ItemType))
							{
								return 32575;
							}
							if (rodEquipment.TerminalTackleIds != null && !rodEquipment.TerminalTackleIds.Contains(inventoryItem.ItemId))
							{
								return 32575;
							}
						}
						if (rodEquipment.HasChumCondition && inventoryItem is Chum)
						{
							Chum chum = (Chum)inventoryItem;
							ItemSubTypes chumSubType = Chum.ChumSubtype(chum);
							Func<ChumCarpbaitsForm, bool> hasChumCarpbaitsForm = (ChumCarpbaitsForm f) => chumSubType == ItemSubTypes.ChumCarpbaits && chum.ChumBase.OfType<ChumCarpbaits>().Any((ChumCarpbaits i) => i.Form == f);
							if (rodEquipment.ChumTypes != null && !rodEquipment.ChumTypes.Any(delegate(ItemSubTypes t)
							{
								if (t == ItemSubTypes.ChumCarpbaits_ChumBoils)
								{
									return hasChumCarpbaitsForm(ChumCarpbaitsForm.ChumBoils);
								}
								if (t == ItemSubTypes.ChumCarpbaits_ChumPellets)
								{
									return hasChumCarpbaitsForm(ChumCarpbaitsForm.ChumPellets);
								}
								if (t != ItemSubTypes.ChumCarpbaits_ChumSpodMix)
								{
									return chumSubType == t;
								}
								return hasChumCarpbaitsForm(ChumCarpbaitsForm.ChumSpodMix);
							}))
							{
								return 32558;
							}
							if (rodEquipment.ChumIds != null && !chum.Ingredients.Any((ChumIngredient i) => rodEquipment.ChumIds.Contains(i.ItemId)))
							{
								return 32558;
							}
							if (rodEquipment.ChumCarpbaitsForms != null && !rodEquipment.ChumCarpbaitsForms.Any(hasChumCarpbaitsForm))
							{
								return 32558;
							}
						}
					}
				}
			}
			if (rodEquipment.HasSinkerCondition)
			{
				if (!array.Any((InventoryItem i) => i.ItemType == ItemTypes.Sinker))
				{
					return 32575;
				}
			}
			if (rodEquipment.HasFeederCondition)
			{
				if (!array.Any((InventoryItem i) => i.ItemType == ItemTypes.Feeder))
				{
					return 32575;
				}
			}
			if (rodEquipment.HasLeaderCondition)
			{
				if (!array.Any((InventoryItem i) => i.ItemType == ItemTypes.Leader))
				{
					return 32575;
				}
			}
			if (rodEquipment.HasHookCondition)
			{
				if (!array.Any((InventoryItem i) => i.ItemType == ItemTypes.Hook))
				{
					return 32575;
				}
			}
			if (rodEquipment.HasTerminalTackleCondition && !array.Any(func))
			{
				return 32575;
			}
			if (rodEquipment.HasBaitCondition && !array.Any(func2))
			{
				return 32574;
			}
			if (rodEquipment.HasChumCondition)
			{
				if (!array.Any((InventoryItem i) => i.ItemType == ItemTypes.Chum))
				{
					return 32558;
				}
			}
			return 0;
		}

		public InventoryItem[] AddBoatRent(InventoryItem[] boatBoatInventory)
		{
			if (this.PondId == null)
			{
				throw new InvalidOperationException("Not on pond to rent a boat!");
			}
			InventoryItem rentBoat = boatBoatInventory.First((InventoryItem i) => i is Boat);
			InventoryItem inventoryItem = this.Inventory.FirstOrDefault((InventoryItem i) => i is Boat && i.Storage == StoragePlaces.Doll && i.ItemSubType == rentBoat.ItemSubType && i.Rent == null);
			if (inventoryItem != null && inventoryItem.Rent == null)
			{
				throw new InvalidOperationException("There is already a boat #" + inventoryItem.InstanceId + " equipped, can't rent more!");
			}
			InventoryItem sameTypeRentBoat = this.Inventory.FirstOrDefault((InventoryItem i) => i is Boat && i.Storage == StoragePlaces.Rent && i.ItemId == rentBoat.ItemId && i.Rent != null);
			if (sameTypeRentBoat == null)
			{
				rentBoat.Storage = StoragePlaces.Rent;
				foreach (InventoryItem inventoryItem2 in boatBoatInventory)
				{
					Inventory.InitItem(inventoryItem2);
					inventoryItem2.Rent = new RentProps
					{
						PondId = this.PondId.Value,
						Duration = rentBoat.Rent.Duration,
						StartDay = rentBoat.Rent.StartDay,
						EndDay = rentBoat.Rent.EndDay
					};
					if (inventoryItem2 != rentBoat)
					{
						inventoryItem2.ParentItem = rentBoat;
						inventoryItem2.Storage = StoragePlaces.ParentItem;
					}
					this.Inventory.JoinItem(inventoryItem2, null);
				}
				return boatBoatInventory;
			}
			sameTypeRentBoat.Rent.Duration = rentBoat.Rent.Duration;
			sameTypeRentBoat.Rent.EndDay = rentBoat.Rent.EndDay;
			List<InventoryItem> list = this.Inventory.Where(delegate(InventoryItem item)
			{
				Guid? parentItemInstanceId = item.ParentItemInstanceId;
				bool flag = parentItemInstanceId != null;
				Guid? instanceId = sameTypeRentBoat.InstanceId;
				return flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault()) && item.Rent != null;
			}).ToList<InventoryItem>();
			foreach (InventoryItem inventoryItem3 in list)
			{
				inventoryItem3.Rent.Duration = rentBoat.Rent.Duration;
				inventoryItem3.Rent.EndDay = rentBoat.Rent.EndDay;
			}
			return new InventoryItem[] { sameTypeRentBoat }.Union(list).ToArray<InventoryItem>();
		}

		public void AddBuoy(BuoySetting buoy, bool generateId = true)
		{
			if (this.Buoys == null)
			{
				this.Buoys = new List<BuoySetting>();
			}
			if (generateId)
			{
				int num;
				if (this.Buoys.Count == 0)
				{
					num = 0;
				}
				else
				{
					num = this.Buoys.Max((BuoySetting i) => i.BuoyId);
				}
				int num2 = num;
				buoy.BuoyId = num2 + 1;
			}
			buoy.SenderId = null;
			buoy.Sender = null;
			buoy.CreatedTime = new DateTime?(DateTime.UtcNow);
			this.Buoys.Add(buoy);
		}

		public void AddBuoyShareRequest(BuoySetting buoy, bool generateId = true)
		{
			if (this.BuoyShareRequests == null)
			{
				this.BuoyShareRequests = new List<BuoySetting>();
			}
			if (generateId)
			{
				int num;
				if (this.BuoyShareRequests.Count == 0)
				{
					num = 0;
				}
				else
				{
					num = this.BuoyShareRequests.Max((BuoySetting i) => i.BuoyId);
				}
				int num2 = num;
				buoy.BuoyId = num2 + 1;
			}
			buoy.CreatedTime = new DateTime?(DateTime.UtcNow);
			this.BuoyShareRequests.Add(buoy);
		}

		public BuoySetting FindBuoyShareRequest(int buoyId)
		{
			if (this.BuoyShareRequests == null)
			{
				return null;
			}
			return this.BuoyShareRequests.FirstOrDefault((BuoySetting b) => b.BuoyId == buoyId);
		}

		public void RemoveBuoyShareRequest(BuoySetting buoy)
		{
			if (this.BuoyShareRequests != null)
			{
				this.BuoyShareRequests.Remove(buoy);
			}
		}

		public void RemoveBuoyShareRequestsOfPlayer(Guid playerId)
		{
			if (this.BuoyShareRequests != null)
			{
				this.BuoyShareRequests.RemoveAll((BuoySetting r) => r.SenderId == playerId);
			}
		}

		public void RemoveOutdatedShareRequests()
		{
			if (this.Buoys != null)
			{
				DateTime utcNow = DateTime.UtcNow;
				this.Buoys.ForEach(delegate(BuoySetting b)
				{
					if (b.CreatedTime == null)
					{
						b.CreatedTime = new DateTime?(DateTime.UtcNow);
					}
				});
			}
			if (this.BuoyShareRequests != null)
			{
				DateTime now = DateTime.UtcNow;
				this.BuoyShareRequests.ForEach(delegate(BuoySetting b)
				{
					if (b.CreatedTime == null)
					{
						b.CreatedTime = new DateTime?(DateTime.UtcNow);
					}
				});
				this.BuoyShareRequests.RemoveAll((BuoySetting b) => now.Subtract(b.CreatedTime.Value).TotalDays > 7.0);
			}
		}

		public List<BoatRent> ActiveBoatRents
		{
			get
			{
				List<BoatRent> list = null;
				if (this.Inventory == null)
				{
					return list;
				}
				IEnumerable<Boat> enumerable = this.Inventory.OfType<Boat>();
				foreach (Boat boat in enumerable)
				{
					if (boat.Rent != null)
					{
						BoatRent boatRent = new BoatRent
						{
							BoatType = boat.ItemSubType,
							StartDay = boat.Rent.StartDay,
							EndDay = boat.Rent.EndDay,
							Duration = boat.Rent.Duration
						};
						if (list == null)
						{
							list = new List<BoatRent>();
						}
						list.Add(boatRent);
					}
				}
				return list;
			}
		}

		private int? _PondId;

		private int _Level;

		private int _Rank;

		private int? _AdditionalInventoryCapacity;
	}
}
