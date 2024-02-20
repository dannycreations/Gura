using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public static class PondHelper
{
	public static bool IsOnPond
	{
		get
		{
			return StaticUserData.CurrentPond != null;
		}
	}

	public static int DayByTime(TimeSpan? ts)
	{
		int num = ((PhotonConnectionFactory.Instance == null) ? 5 : PhotonConnectionFactory.Instance.PondDayStart);
		TimeSpan timeSpan = ((ts == null) ? new TimeSpan(0, 0, 0, 0) : ts.Value);
		return timeSpan.Days - ((timeSpan.Hours >= num) ? 0 : 1) + 1;
	}

	public static int CurrentDay
	{
		get
		{
			return PondHelper.DayByTime(TimeAndWeatherManager.CurrentTime);
		}
	}

	public static int AllDays
	{
		get
		{
			int currentDay = PondHelper.CurrentDay;
			return (PhotonConnectionFactory.Instance == null || PhotonConnectionFactory.Instance.Profile == null || PhotonConnectionFactory.Instance.Profile.PondStayTime == null) ? currentDay : Mathf.Max(PhotonConnectionFactory.Instance.Profile.PondStayTime.Value, currentDay);
		}
	}

	public static LevelLockRemoval GetLevelLockForPond(Pond pond)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null)
		{
			return null;
		}
		int pondId = pond.PondId;
		LevelLockRemoval levelLockRemoval = null;
		if (profile.LevelLockRemovals != null)
		{
			for (int i = 0; i < profile.LevelLockRemovals.Count; i++)
			{
				LevelLockRemoval levelLockRemoval2 = profile.LevelLockRemovals[i];
				if (levelLockRemoval2.Ponds != null)
				{
					for (int j = 0; j < levelLockRemoval2.Ponds.Length; j++)
					{
						if (levelLockRemoval2.Ponds[j] == pondId && levelLockRemoval2.Type == LevelLockRemovalType.Unlock)
						{
							return levelLockRemoval2;
						}
						if (levelLockRemoval2.Ponds[j] == pondId)
						{
							if (levelLockRemoval != null)
							{
								DateTime? endDate = levelLockRemoval.EndDate;
								bool flag = endDate != null;
								DateTime? endDate2 = levelLockRemoval2.EndDate;
								if (!(flag & (endDate2 != null)) || !(endDate.GetValueOrDefault() < endDate2.GetValueOrDefault()))
								{
									goto IL_C9;
								}
							}
							levelLockRemoval = levelLockRemoval2;
						}
						IL_C9:;
					}
				}
			}
		}
		return (pond.OriginalMinLevel > profile.Level) ? levelLockRemoval : null;
	}

	public static bool PondLocked(this Pond pond)
	{
		if (pond == null)
		{
			return false;
		}
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null)
		{
			return true;
		}
		LevelLockRemoval levelLockForPond = PondHelper.GetLevelLockForPond(pond);
		return (levelLockForPond == null || (levelLockForPond.Type != LevelLockRemovalType.Unlock && !(levelLockForPond.EndDate > TimeHelper.UtcTime()))) && pond.OriginalMinLevel > profile.Level;
	}

	public static IEnumerable<BoatDesc> GetPondBoatPrices(this IEnumerable<BoatDesc> items, int stateID)
	{
		Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.State.StateId == stateID);
		return items.Where((BoatDesc x) => x.Prices != null && x.Prices.Any((BoatPriceDesc p) => p.PondId == pond.PondId));
	}

	public static DateTime? LockEndTime(Pond pond)
	{
		LevelLockRemoval levelLockForPond = PondHelper.GetLevelLockForPond(pond);
		return (levelLockForPond != null && levelLockForPond.Type != LevelLockRemovalType.Unlock) ? levelLockForPond.EndDate : null;
	}

	public static bool PondPaidLocked(this Pond pond)
	{
		if (pond == null || !pond.IsPaid)
		{
			return false;
		}
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null)
		{
			return true;
		}
		if (profile.PaidLockRemovals != null)
		{
			for (int i = 0; i < profile.PaidLockRemovals.Count; i++)
			{
				PaidLockRemoval paidLockRemoval = profile.PaidLockRemovals[i];
				if (paidLockRemoval.Ponds != null && paidLockRemoval.Ponds.Any((int pId) => pId == pond.PondId))
				{
					return false;
				}
			}
		}
		return true;
	}
}
