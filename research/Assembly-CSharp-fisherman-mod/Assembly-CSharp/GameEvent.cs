using System;
using ExitGames.Client.Photon;
using ObjectModel;
using Photon.Interfaces;

public class GameEvent
{
	public EventCode EventCode { get; set; }

	public Hashtable RawEventData { get; set; }

	public Fish Fish { get; set; }

	public InventoryItem Item { get; set; }

	public int RodSlot { get; set; }
}
