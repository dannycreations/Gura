using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DashboardMenuInfo : MonoBehaviour
{
	private void Start()
	{
		this.BackToHome.onValueChanged.AddListener(new UnityAction<bool>(this.BackToHomeChangeValue));
		this.Map.onValueChanged.AddListener(new UnityAction<bool>(this.MapChangeValue));
		this.Inventory.onValueChanged.AddListener(new UnityAction<bool>(this.InventoryChangeValue));
		this.Shop.onValueChanged.AddListener(new UnityAction<bool>(this.ShopChangeValue));
		this.Statistics.onValueChanged.AddListener(new UnityAction<bool>(this.StatisticsChangeValue));
		this.FishKeepNet.onValueChanged.AddListener(new UnityAction<bool>(this.FishKeepNetChangeValue));
		this.Tournaments.onValueChanged.AddListener(new UnityAction<bool>(this.TournamentsChangeValue));
		this.Leaderboard.onValueChanged.AddListener(new UnityAction<bool>(this.LeaderboardChangeValue));
		this.Friends.onValueChanged.AddListener(new UnityAction<bool>(this.FriendsChangeValue));
		this.Club.onValueChanged.AddListener(new UnityAction<bool>(this.ClubChangeValue));
	}

	private void BackToHomeChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.BackToHome);
		}
	}

	private void MapChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.Map);
		}
	}

	private void InventoryChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.Inventory);
		}
	}

	private void ShopChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.Shop);
		}
	}

	private void StatisticsChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.Statistics);
		}
	}

	private void FishKeepNetChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.FishKeepNet);
		}
	}

	private void TournamentsChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.Tournaments);
		}
	}

	private void LeaderboardChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.Leaderboard);
		}
	}

	private void FriendsChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.Friends);
		}
	}

	private void ClubChangeValue(bool value)
	{
		if (value)
		{
			this.ChangeValue(this.Club);
		}
	}

	private void ChangeValue(Toggle toggle)
	{
		this.PreviouslyToggle = this.CurrenToggle;
		this.CurrenToggle = toggle;
	}

	public Toggle BackToHome;

	public Toggle Map;

	public Toggle Inventory;

	public Toggle Shop;

	public Toggle Statistics;

	public Toggle FishKeepNet;

	public Toggle Tournaments;

	public Toggle Leaderboard;

	public Toggle Friends;

	public Toggle Club;

	public Toggle CurrenToggle;

	public Toggle PreviouslyToggle;
}
