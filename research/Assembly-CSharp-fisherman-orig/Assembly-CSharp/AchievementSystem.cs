using System;
using System.Collections;
using Steamworks;
using UnityEngine;

public class AchievementSystem : MonoBehaviour
{
	public static AchievementSystem Instance { get; private set; }

	private void Awake()
	{
		if (AchievementSystem.Instance == null)
		{
			AchievementSystem.Instance = this;
		}
	}

	private void Update()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (!AchievementSystem._isInited)
		{
			AchievementSystem._isInited = true;
			this.m_GameID = new CGameID(SteamUtils.GetAppID());
			this.m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(new Callback<UserStatsReceived_t>.DispatchDelegate(this.OnUserStatsReceived));
			this.m_UserStatsStored = Callback<UserStatsStored_t>.Create(new Callback<UserStatsStored_t>.DispatchDelegate(this.OnUserStatsStored));
			this.m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(new Callback<UserAchievementStored_t>.DispatchDelegate(this.OnAchievementStored));
			this.m_bRequestedStats = false;
			this.m_bStatsValid = false;
			LogHelper.Log("___kocha AchievementSystem:INIT GameID:{0}", new object[] { this.m_GameID });
			base.StartCoroutine(this.GetStats());
		}
	}

	private IEnumerator GetStats()
	{
		while (!SteamManager.Initialized)
		{
			yield return new WaitForEndOfFrame();
		}
		bool bSuccess = false;
		if (!this.m_bRequestedStats)
		{
			this.m_bRequestedStats = true;
			bSuccess = SteamUserStats.RequestCurrentStats();
			this.m_bRequestedStats = bSuccess;
			LogHelper.Log("___kocha RequestCurrentStats bSuccess:{0}", new object[] { bSuccess });
		}
		yield break;
	}

	private void StoreStats()
	{
		SteamUserStats.StoreStats();
	}

	public bool IsAchievementUnlocked(string achievementId)
	{
		if (SteamManager.Initialized)
		{
			bool flag;
			SteamUserStats.GetAchievement(achievementId, ref flag);
			return flag;
		}
		LogHelper.Log("___kocha IsAchievementUnlocked SteamManager.Initialized = false");
		return false;
	}

	public void ClearAchievement(string achievementId)
	{
		if (SteamManager.Initialized)
		{
			LogHelper.Log("___kocha Try ClearAchievement achievementID:{0}", new object[] { achievementId });
			SteamUserStats.ClearAchievement(achievementId);
			this.StoreStats();
		}
		else
		{
			LogHelper.Log("___kocha ClearAchievement SteamManager.Initialized = false");
		}
	}

	public void UnlockAchievement(string achievementId)
	{
		if (SteamManager.Initialized)
		{
			LogHelper.Log("___kocha Try UnlockAchievement achievementID:{0}", new object[] { achievementId });
			SteamUserStats.SetAchievement(achievementId);
			this.StoreStats();
		}
		else
		{
			LogHelper.Log("___kocha UnlockAchievement SteamManager.Initialized = false");
		}
	}

	public void UpdateStat(string statId, int newValue)
	{
		if (SteamManager.Initialized)
		{
			SteamUserStats.SetStat(statId, newValue);
			this.StoreStats();
		}
		else
		{
			LogHelper.Log("___kocha UpdateStat SteamManager.Initialized = false");
		}
	}

	public int GetStat(string statId)
	{
		if (SteamManager.Initialized)
		{
			int num;
			SteamUserStats.GetStat(statId, ref num);
			return num;
		}
		return 0;
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		LogHelper.Log("___kocha OnUserStatsReceived m_eResult:{0} m_nGameID:{1} m_steamIDUser:{2}", new object[] { pCallback.m_eResult, pCallback.m_nGameID, pCallback.m_steamIDUser });
		if (!SteamManager.Initialized)
		{
			return;
		}
		if ((ulong)this.m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == 1)
			{
				this.m_bStatsValid = true;
			}
			else
			{
				LogHelper.Log("___kocha OnUserStatsReceived - failed");
			}
		}
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		LogHelper.Log("___kocha OnUserStatsStored m_eResult:{0} m_nGameID:{1}", new object[] { pCallback.m_eResult, pCallback.m_nGameID });
		if ((ulong)this.m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult != 1)
			{
				if (pCallback.m_eResult == 8)
				{
					LogHelper.Log("___kocha StorOnUserStatsStoredeStats - some failed to validate");
					UserStatsReceived_t userStatsReceived_t = default(UserStatsReceived_t);
					userStatsReceived_t.m_eResult = 1;
					userStatsReceived_t.m_nGameID = (ulong)this.m_GameID;
					UserStatsReceived_t userStatsReceived_t2 = userStatsReceived_t;
					this.OnUserStatsReceived(userStatsReceived_t2);
				}
				else
				{
					LogHelper.Log("___kocha OnUserStatsStored - failed");
				}
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		LogHelper.Log("___kocha OnAchievementStored m_rgchAchievementName:{0} m_nGameID:{1}", new object[] { pCallback.m_rgchAchievementName, pCallback.m_nGameID });
		if ((ulong)this.m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_nMaxProgress != 0U)
			{
				LogHelper.Log(string.Concat(new object[] { "___kocha Achievement '", pCallback.m_rgchAchievementName, "' progress callback, (", pCallback.m_nCurProgress, ",", pCallback.m_nMaxProgress, ")" }));
			}
		}
	}

	private CGameID m_GameID;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	private static bool _isInited;

	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	protected Callback<UserStatsStored_t> m_UserStatsStored;

	protected Callback<UserAchievementStored_t> m_UserAchievementStored;
}
