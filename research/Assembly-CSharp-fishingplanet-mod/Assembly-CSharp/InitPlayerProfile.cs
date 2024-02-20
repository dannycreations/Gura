using System;
using ObjectModel;
using UnityEngine;

public class InitPlayerProfile : MonoBehaviour
{
	public void Init(Profile playerProfile, bool is3DInit = false)
	{
		if (this.PlayerAchievmentsInit != null)
		{
			this.PlayerAchievmentsInit.Init(playerProfile);
		}
		if (this.PlayerTrophiesInit != null)
		{
			this.PlayerTrophiesInit.Init(playerProfile.Stats.FishStats, is3DInit);
		}
		if (this.PlayerStatisticsInit != null)
		{
			this.PlayerStatisticsInit.Init(playerProfile);
		}
		if (this.PlayerEquipmentInit != null)
		{
			this.PlayerEquipmentInit.Init(playerProfile);
		}
	}

	public PlayerStatisticsInit PlayerStatisticsInit;

	public PlayerAchivementsInit PlayerAchievmentsInit;

	public PlayerTrophiesInit PlayerTrophiesInit;

	public PlayerEquipmentInit PlayerEquipmentInit;
}
