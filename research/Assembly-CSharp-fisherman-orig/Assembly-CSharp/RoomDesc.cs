using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class RoomDesc
{
	public RoomDesc()
	{
		this.players = new Dictionary<string, Player>();
	}

	public IEnumerable<Player> Players
	{
		get
		{
			return this.players.Values;
		}
	}

	public void Clear()
	{
		this.players.Clear();
		this.RoomId = null;
	}

	public void ClearPlayers()
	{
		this.players.Clear();
	}

	public void Add(Player player)
	{
		if (!this.players.ContainsKey(player.UserId))
		{
			this.players.Add(player.UserId, player);
		}
	}

	public void Remove(Player player)
	{
		Debug.Log(player.UserId);
		if (this.players.ContainsKey(player.UserId))
		{
			this.players.Remove(player.UserId);
		}
	}

	public Player FindPlayer(string playerId)
	{
		return (!this.players.ContainsKey(playerId)) ? null : this.players[playerId];
	}

	public string RoomId;

	private Dictionary<string, Player> players;

	public bool IsOpen;

	public bool IsVisible;

	public int MaxPlayers;

	public int PondId;

	public int LanguageId;

	public bool IsPrivate;

	public bool IsTournament;

	public int TournamentId;
}
