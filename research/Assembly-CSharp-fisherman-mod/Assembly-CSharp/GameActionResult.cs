using System;
using ExitGames.Client.Photon;
using Photon.Interfaces.Game;

public class GameActionResult : Failure
{
	public GameActionResult(OperationResponse response)
		: base(response)
	{
	}

	public GameActionCode ActionCode { get; set; }

	public Hashtable ActionData { get; set; }

	public bool AnyBreaks { get; set; }

	public WearInfo WearInfo { get; set; }

	public int RodSlot { get; set; }
}
