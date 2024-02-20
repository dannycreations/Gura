using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using ObjectModel;

public class PlayerStats
{
	[JsonIgnore]
	public static IDictionary<int, FishCategory> FishCategoryCache { get; private set; }

	public static void InitFishCategoryCache(Func<IDictionary<int, FishCategory>> getCacheFunc)
	{
		object cacheLock = PlayerStats.CacheLock;
		lock (cacheLock)
		{
			if (PlayerStats.FishCategoryCache == null)
			{
				PlayerStats.FishCategoryCache = getCacheFunc();
			}
		}
	}

	public TotalFishFormStat TotalFishFormStat
	{
		get
		{
			TotalFishFormStat totalFishFormStat;
			if ((totalFishFormStat = this.totalFishFormStat) == null)
			{
				totalFishFormStat = (this.totalFishFormStat = new TotalFishFormStat());
			}
			return totalFishFormStat;
		}
	}

	public IDictionary<int, FishStat> FishStats
	{
		get
		{
			IDictionary<int, FishStat> dictionary;
			if ((dictionary = this.fishStats) == null)
			{
				dictionary = (this.fishStats = new Dictionary<int, FishStat>());
			}
			return dictionary;
		}
	}

	public IDictionary<int, BaitStat> BaitStats
	{
		get
		{
			IDictionary<int, BaitStat> dictionary;
			if ((dictionary = this.baitStats) == null)
			{
				dictionary = (this.baitStats = new Dictionary<int, BaitStat>());
			}
			return dictionary;
		}
	}

	public IDictionary<ItemSubTypes, ConsumedItemStat> ConsumeStats
	{
		get
		{
			IDictionary<ItemSubTypes, ConsumedItemStat> dictionary;
			if ((dictionary = this.consumeStats) == null)
			{
				dictionary = (this.consumeStats = new Dictionary<ItemSubTypes, ConsumedItemStat>());
			}
			return dictionary;
		}
	}

	public IDictionary<StatsCounterType, StatsCounter> GenericStats
	{
		get
		{
			IDictionary<StatsCounterType, StatsCounter> dictionary;
			if ((dictionary = this.genericStats) == null)
			{
				dictionary = (this.genericStats = new Dictionary<StatsCounterType, StatsCounter>());
			}
			return dictionary;
		}
	}

	public IDictionary<int, InteractStat> InteractStats
	{
		get
		{
			IDictionary<int, InteractStat> dictionary;
			if ((dictionary = this.interactStats) == null)
			{
				dictionary = (this.interactStats = new Dictionary<int, InteractStat>());
			}
			return dictionary;
		}
	}

	public IDictionary<int, DailyInteractStat> DailyInteractStats
	{
		get
		{
			IDictionary<int, DailyInteractStat> dictionary;
			if ((dictionary = this.dailyInteractStats) == null)
			{
				dictionary = (this.dailyInteractStats = new Dictionary<int, DailyInteractStat>());
			}
			return dictionary;
		}
	}

	public IList<PlayerAchievementId> AchivementIds
	{
		get
		{
			IList<PlayerAchievementId> list;
			if ((list = this.achivementIds) == null)
			{
				list = (this.achivementIds = new List<PlayerAchievementId>());
			}
			return list;
		}
	}

	public StatAchievement[] Achievements { get; set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event CheckAchivement OnCheckAchivement;

	public List<StatAchievement> DailyAchievements { get; set; }

	public List<StatAchievement> MissionAchievements { get; set; }

	public List<StatLevel> DailyLevels { get; set; }

	public List<StatLevel> MissionLevels { get; set; }

	public List<StatPenalty> DailyPenalties { get; set; }

	public List<StatPenalty> MissionPenalties { get; set; }

	public IList<TournamentStat> TournamentStats
	{
		get
		{
			IList<TournamentStat> list;
			if ((list = this.tournamentStats) == null)
			{
				list = (this.tournamentStats = new List<TournamentStat>());
			}
			return list;
		}
		set
		{
			this.tournamentStats = value;
		}
	}

	public int GetCounterValue(StatsCounterType counterType, int entityId = 0)
	{
		if (counterType == StatsCounterType.TotalFishForm)
		{
			return this.TotalFishFormStat.Count;
		}
		if (counterType == StatsCounterType.FishType)
		{
			return this.GetFishCountByCategory(entityId);
		}
		if (counterType == StatsCounterType.BaitType)
		{
			return this.GetCatchCountByBait(entityId);
		}
		if (counterType == StatsCounterType.ConsumedItemType)
		{
			return this.GetConsumesCountByBait(entityId);
		}
		StatsCounter statsCounter;
		this.GenericStats.TryGetValue(counterType, out statsCounter);
		if (statsCounter != null)
		{
			return statsCounter.Count;
		}
		return 0;
	}

	private int GetFishCountByCategory(int categoryId)
	{
		FishStat fishStat;
		this.FishStats.TryGetValue(categoryId, out fishStat);
		if (fishStat != null)
		{
			return fishStat.Count;
		}
		return 0;
	}

	private int GetCatchCountByBait(int baitId)
	{
		BaitStat baitStat;
		this.BaitStats.TryGetValue(baitId, out baitStat);
		if (baitStat != null)
		{
			return baitStat.Count;
		}
		return 0;
	}

	private int GetConsumesCountByBait(int itemSubtype)
	{
		ConsumedItemStat consumedItemStat;
		this.ConsumeStats.TryGetValue((ItemSubTypes)itemSubtype, out consumedItemStat);
		if (consumedItemStat != null)
		{
			return consumedItemStat.Count;
		}
		return 0;
	}

	private StatsCounter CreateCounter(StatsCounterType type)
	{
		string name = Enum.GetName(typeof(StatsCounterType), type);
		if (name == null)
		{
			throw new InvalidOperationException("Error creating counter. TypeName is not defined!");
		}
		if (name.EndsWith("RewardDays"))
		{
			return new RewardDaysCounter
			{
				Type = type,
				LastUpdated = DateTime.Today.AddDays(-1.0)
			};
		}
		if (name.EndsWith("PerDay"))
		{
			return new TimedCounter
			{
				Type = type,
				LastUpdated = DateTime.Now
			};
		}
		if (name.EndsWith("Days"))
		{
			return new DaysCounter
			{
				Type = type,
				LastUpdated = DateTime.Now
			};
		}
		if (name.StartsWith("Max"))
		{
			return new MaxCounter
			{
				Type = type
			};
		}
		if (name.EndsWith("BreaksInARow"))
		{
			return new AchievemntResetCounter
			{
				Type = type
			};
		}
		return new StatsCounter
		{
			Type = type
		};
	}

	public int? GetBestTournamentPlace(int? tournamentTemplateId)
	{
		if (tournamentTemplateId == null)
		{
			return null;
		}
		TournamentStat tournamentStat = this.TournamentStats.FirstOrDefault((TournamentStat s) => s.Id == tournamentTemplateId.GetValueOrDefault() && tournamentTemplateId != null);
		if (tournamentStat == null)
		{
			return null;
		}
		return tournamentStat.Place;
	}

	public int GetObjectInteractions(int objId)
	{
		InteractStat interactStat;
		this.InteractStats.TryGetValue(objId, out interactStat);
		if (interactStat != null)
		{
			return interactStat.Count;
		}
		return 0;
	}

	public int GetObjectDailyInteractions(int objId)
	{
		DailyInteractStat dailyInteractStat;
		this.DailyInteractStats.TryGetValue(objId, out dailyInteractStat);
		if (dailyInteractStat != null && dailyInteractStat.LastUpdated.Date == TimeHelper.UtcTime().Date)
		{
			return dailyInteractStat.Count;
		}
		return 0;
	}

	public void UpdateInteractStats(int objId, bool daily)
	{
		InteractStat interactStat;
		this.InteractStats.TryGetValue(objId, out interactStat);
		if (interactStat == null)
		{
			interactStat = new InteractStat
			{
				ObjectId = objId
			};
		}
		interactStat.Count++;
		this.InteractStats[objId] = interactStat;
		if (daily)
		{
			DailyInteractStat dailyInteractStat;
			this.DailyInteractStats.TryGetValue(objId, out dailyInteractStat);
			if (dailyInteractStat == null)
			{
				dailyInteractStat = new DailyInteractStat
				{
					ObjectId = objId
				};
			}
			else if (dailyInteractStat.LastUpdated.Date != TimeHelper.UtcTime().Date)
			{
				dailyInteractStat.Count = 0;
			}
			dailyInteractStat.Count++;
			dailyInteractStat.LastUpdated = TimeHelper.UtcTime().Date;
			this.DailyInteractStats[objId] = dailyInteractStat;
		}
	}

	public void OverrideCounterValue(StatsCounterType type, int value)
	{
		StatsCounter statsCounter;
		this.GenericStats.TryGetValue(type, out statsCounter);
		if (statsCounter == null)
		{
			statsCounter = this.CreateCounter(type);
		}
		statsCounter.Count = value;
	}

	private static readonly object CacheLock = new object();

	private TotalFishFormStat totalFishFormStat;

	private IDictionary<int, FishStat> fishStats;

	private IDictionary<int, BaitStat> baitStats;

	private IDictionary<ItemSubTypes, ConsumedItemStat> consumeStats;

	private IDictionary<StatsCounterType, StatsCounter> genericStats;

	private IDictionary<int, InteractStat> interactStats;

	private IDictionary<int, DailyInteractStat> dailyInteractStats;

	private IList<PlayerAchievementId> achivementIds;

	private IList<TournamentStat> tournamentStats;
}
