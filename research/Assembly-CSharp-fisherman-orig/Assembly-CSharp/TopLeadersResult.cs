using System;
using System.Collections.Generic;
using ObjectModel;
using Photon.Interfaces.LeaderBoards;

public class TopLeadersResult
{
	public TopTournamentKind? TournamentKind { get; set; }

	public IEnumerable<TopTournamentPlayers> Tournaments { get; set; }

	public TopKind Kind;

	public IEnumerable<TopPlayers> Players;

	public IEnumerable<TopFish> Fish;
}
