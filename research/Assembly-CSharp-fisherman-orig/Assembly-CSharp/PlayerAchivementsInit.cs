using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAchivementsInit : MonoBehaviour
{
	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotStats -= this.OnGotStatsOwner;
	}

	public void Init(Profile playerProfile)
	{
		if (playerProfile.UserId == PhotonConnectionFactory.Instance.Profile.UserId)
		{
			PhotonConnectionFactory.Instance.OnGotStats += this.OnGotStatsOwner;
			PhotonConnectionFactory.Instance.RequestStats();
		}
		else
		{
			this.OnGetStats(playerProfile.Stats);
		}
	}

	private void AddItemsToList(List<StatAchievement> achievments)
	{
		for (int i = 0; i < achievments.Count; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.AchivPrefab);
			Image component = gameObject.GetComponent<Image>();
			component.enabled = i % 2 == 0;
			component.color = this.ListLineColor;
			gameObject.GetComponent<AchivInit>().Refresh(achievments[i]);
		}
	}

	internal void OnGetStats(PlayerStats stats)
	{
		List<StatAchievement> list = (from x in stats.Achievements
			where x.CurrentStage != null
			select x into p
			orderby p.Name
			select p).ToList<StatAchievement>();
		this.AchivCount.text = string.Format("({0})", list.Count);
		this.AddItemsToList(list);
	}

	public void Clear()
	{
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	private void OnGotStatsOwner(PlayerStats stats)
	{
		PhotonConnectionFactory.Instance.OnGotStats -= this.OnGotStatsOwner;
		this.OnGetStats(stats);
	}

	public GameObject ContentPanel;

	public GameObject AchivPrefab;

	public Color ListLineColor;

	public Text AchivCount;

	public GameObject AchivementsList;
}
