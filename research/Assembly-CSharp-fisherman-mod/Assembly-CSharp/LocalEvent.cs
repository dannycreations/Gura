using System;
using ObjectModel;
using UnityEngine;

public class LocalEvent
{
	public LocalEventType EventType { get; set; }

	public Player Player { get; set; }

	public Fish Fish { get; set; }

	public BrokenTackleType TackleType { get; set; }

	public int Level { get; set; }

	public int Rank { get; set; }

	public string Achivement { get; set; }

	public string AchivementStage { get; set; }

	public int AchivementStageCount { get; set; }

	public int ItemId { get; set; }

	public Vector3 Position { get; set; }

	public string Quest { get; set; }

	public string QuestStage { get; set; }

	public BuoySetting Buoy { get; set; }

	public InventoryItem CaughtItem { get; set; }

	public string Receiver { get; set; }
}
